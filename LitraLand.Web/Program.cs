using LitraLand.Web.Core.Mapping;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
namespace LitraLand.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Begin Add services to the container.
            // Add DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add Identity services
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            // Add AutoMapper
            builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

            // Add ExpressiveAnnotations
            builder.Services.AddExpressiveAnnotations();

            // Add Cloudinary settings
            var cloudinarySettings = builder.Configuration.GetSection(nameof(CloudinarySettings)) ?? throw new InvalidOperationException("CloudinarySettings section not found.");
            builder.Services.Configure<CloudinarySettings>(cloudinarySettings);
            // End Add services to the container.

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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}