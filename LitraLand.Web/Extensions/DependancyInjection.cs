using Hangfire;
using HashidsNet;
using LitraLand.Web.Core.Mapping;
using LitraLand.Web.Helpers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using WhatsAppCloudApi.Extensions;

namespace LitraLand.Web.Extensions
{
    public static class DependancyInjection
    {
        /// <summary>
        /// Add LitraLand services to the container of services
        /// this method is used to add all the services that are used in the application to the container of services
        /// like DbContext, Identity, AutoMapper, Cloudinary, EmailSender, etc.
        /// </summary>
        /// <param name="services"></param>"
        /// <param name="builder"></param>
        /// <returns> IServiceCollection that contains all the services that are used in the application</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IServiceCollection AddLitraLandServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            // Add DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // remove the above line and uncomment the below lines if you want to use the options for SQL Server (CommandTimeout, EnableRetryOnFailure)
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(connectionString, sqlOptions =>
            //    {
            //        sqlOptions.CommandTimeout(60);
            //        sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
            //    }));


            // this line is used to add the developer exception page to the application in development mode (it will show detailed error information)
            services.AddDatabaseDeveloperPageExceptionFilter();

            // Add Identity services
            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddSignInManager<SignInManager<ApplicationUser>>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequiredLength = 8;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
                options.User.RequireUniqueEmail = true;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // visit the below link for more information about Identity configuration
                // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-10.0
            });

            // Force immediate re-validation of the security stamp upon any user-related changes (e.g., password change, role update).
            // This ensures that if a user updates their password or role, they must re-authenticate immediately. 
            services.Configure<SecurityStampValidatorOptions>(option => option.ValidationInterval = TimeSpan.Zero);

            // Add ClaimsPrincipalFactory to add custom claims to the user (e.g. FullName)
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

            // Add HashIdes services to the container
            services.AddSingleton<IHashids>(new Hashids("my_unique_salt_value_mahmoud", 11));

            // Add DataProtection services to the container
            services.AddDataProtection()
                .SetApplicationName(nameof(LitraLand));

            // Add ImageService to the container of services
            services.AddTransient<IImageServices, ImageService>();

            // Add CloudinaryService to the container of services
            services.AddTransient<ICloudinaryService, CloudinaryService>();

            // Add EmailSender to the container of services
            services.AddTransient<IEmailSender, EmailSender>();

            // Add EmailBodyBuilder to the container of services
            services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

            // is used to add the MVC services to the container of services (it will add the controllers and views to the application) 
            services.AddControllersWithViews();

            // Add AutoMapper
            services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

            // Add Cloudinary settings
            var cloudinarySettings = builder.Configuration.GetSection(nameof(CloudinarySettings)) ?? throw new InvalidOperationException("CloudinarySettings section not found.");
            services.Configure<CloudinarySettings>(cloudinarySettings);

            // Add Mail settings
            var mailSettings = builder.Configuration.GetSection(nameof(MailSettings)) ?? throw new InvalidOperationException("MailSettings section not found.");
            services.Configure<MailSettings>(mailSettings);

            // Add ExpressiveAnnotations
            services.AddExpressiveAnnotations();

            // Add WhatsApp API Client
            services.AddWhatsAppApiClient(builder.Configuration);

            // Add Hangfire service
            builder.Services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(connectionString);

            });
            // Add Hangfire server
            builder.Services.AddHangfireServer();


            // Add Authorization policy for AdminsOnly (Admins and SuperAdmins)
            services.Configure<AuthorizationOptions>(options =>
            options.AddPolicy("AdminsOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AppRoles.SuperAdmin, AppRoles.Admin);
            }));

            // Add MVC services
            services.AddMvc(options =>
            {
                // Add Antiforgery token attribute to the application (will be added automatically to all POST requests)
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            return services;
        }
    }
}