using Hangfire;
using Hangfire.Dashboard;
using HashidsNet;
using LitraLand.Infrastructure;
using LitraLand.Web.Seeds;
using LitraLand.Web.Tasks;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Context;

namespace LitraLand.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services
                .AddInfrastructureServices(builder.Configuration) // from LitraLand.Infrastructure layer
                .AddWebServices(builder); // from LitraLand.Web layer (this project)

            // add serilog to the application
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
            builder.Host.UseSerilog();

            var app = builder.Build();

            // Add middleware to prevent the application from being embedded in an iframe
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                await next();
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                // Add StatusCodePagesWithReExecute middleware to handle errors and show a custom error page
                app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // make sure that all cookies are secure
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.Always
            });


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Seed the database with default roles and users if they do not exist
            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>(); // Get a scope factory

            using var scope = scopeFactory.CreateScope(); // Create a scope (means a new service provider is created)

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await DefaultRols.SeedRolsAsync(roleManager);
            await DefaultUsers.SeedAdminUserAsync(userManager);
            // end seed the database

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "LitraLand Dashboard",
                //IsReadOnlyFunc = (DashboardContext context) => true,
                Authorization = new IDashboardAuthorizationFilter[]
                {
                    new HangfireAuthorizationFilter("AdminsOnly")
                }
            });

            // Add Hangfire tasks (jobs that run in the background in a scheduled manner)
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var whatsAppClient = scope.ServiceProvider.GetRequiredService<IWhatsAppClient>();
            var emailBodyBuilder = scope.ServiceProvider.GetRequiredService<IEmailBodyBuilder>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var hangfireTasks = new HangfireTasks(dbContext, webHostEnvironment, whatsAppClient,
                emailBodyBuilder, emailSender);

            RecurringJob.AddOrUpdate(
                "PrepareExpirationAlerts", // Recurring Job ID (Unique Identifier)
                () => hangfireTasks.PrepareExpirationAlerts(),
                "0 14 * * *", // Cron Expression (Run every day at 2:00 PM)
                new RecurringJobOptions()
            );

            RecurringJob.AddOrUpdate(
                "RentalsExpirationAlert",
                () => hangfireTasks.RentalsExpirationAlert(),
                "0 14 * * *", // Cron Expression (Run every day at 2:00 PM)
                new RecurringJobOptions()
            );

            // Add middleware to log the user id and user name for each request
            app.Use(async (context, next) =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var userName = context.User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
                using (LogContext.PushProperty("UserId", userId))
                using (LogContext.PushProperty("UserName", userName))
                {
                    await next();
                }
            });

            app.UseSerilogRequestLogging(); // Add Serilog to log the request

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}