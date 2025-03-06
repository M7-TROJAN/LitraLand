using LitraLand.Web.Core.Mapping;
using LitraLand.Web.Helpers;
using LitraLand.Web.Seeds;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using WhatsAppCloudApi.Extensions;
namespace LitraLand.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Begin Add services to the container.
            // Add DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add Identity services
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequiredLength = 8;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
                options.User.RequireUniqueEmail = true;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 2;
                options.Lockout.AllowedForNewUsers = true;

                // visit the below link for more information about Identity configuration
                // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-10.0
            });

            // Force immediate re-validation of the security stamp upon any user-related changes (e.g., password change, role update).
            // This ensures that if a user updates their password or role, they must re-authenticate immediately. 
            builder.Services.Configure<SecurityStampValidatorOptions>(option => option.ValidationInterval = TimeSpan.Zero);


            // Add DataProtection services to the container
            builder.Services.AddDataProtection()
                .SetApplicationName(nameof(LitraLand));

            // Add Cloudinary settings
            var cloudinarySettings = builder.Configuration.GetSection(nameof(CloudinarySettings)) ?? throw new InvalidOperationException("CloudinarySettings section not found.");
            builder.Services.Configure<CloudinarySettings>(cloudinarySettings);

            // Add Mail settings
            var mailSettings = builder.Configuration.GetSection(nameof(MailSettings)) ?? throw new InvalidOperationException("MailSettings section not found.");
            builder.Services.Configure<MailSettings>(mailSettings);
            // End Add services to the container.

            // Add ClaimsPrincipalFactory to add custom claims to the user (e.g. FullName)
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

            // Add ImageService to the container of services
            builder.Services.AddTransient<IImageServices, ImageService>();

            // Add CloudinaryService to the container of services
            builder.Services.AddTransient<ICloudinaryService, CloudinaryService>();

            // Add EmailSender to the container of services
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            // Add EmailBodyBuilder to the container of services
            builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

            builder.Services.AddControllersWithViews();

            // Add AutoMapper
            builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

            // Add ExpressiveAnnotations
            builder.Services.AddExpressiveAnnotations();

            // Add WhatsApp API Client
            builder.Services.AddWhatsAppApiClient(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}