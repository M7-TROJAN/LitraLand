using Hangfire;
using HashidsNet;
using LitraLand.Domain.Entities.Common;
using LitraLand.Web.Core.Mapping;
using LitraLand.Web.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using WhatsAppCloudApi.Extensions;

namespace LitraLand.Web
{
    public static class ConfigureServices
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
        public static IServiceCollection AddWebServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            // this line is used to add the developer exception page to the application in development mode (it will show detailed error information)
            services.AddDatabaseDeveloperPageExceptionFilter();

            // Add Identity services
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                // add the email token provider to the identity options (this is used to send the email confirmation token to the user)
                options.Tokens.ProviderMap.Add("Email", new TokenProviderDescriptor(typeof(EmailTokenProvider<ApplicationUser>)));
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultUI()
            .AddDefaultTokenProviders()
            .AddSignInManager<SignInManager<ApplicationUser>>();

            // Add External Authentication Providers
            // Bind GoogleAuthSettings
            var googleAuthSettings = builder.Configuration
                .GetSection("Authentication:Google")
                .Get<GoogleAuthSettings>() ?? throw new InvalidOperationException("Google authentication settings are missing.");

            // Bind FacebookAuthSettings
            var facebookAuthSettings = builder.Configuration
                .GetSection("Authentication:Facebook")
                .Get<FacebookAuthSettings>() ?? throw new InvalidOperationException("Facebook authentication settings are missing.");

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = googleAuthSettings.ClientId;
                    options.ClientSecret = googleAuthSettings.ClientSecret;

                    options.Events.OnRemoteFailure = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Identity/Account/LoginCommunity");
                        return Task.CompletedTask;
                    };
                })
                .AddFacebook(options =>
                {
                    options.AppId = facebookAuthSettings.AppId;
                    options.AppSecret = facebookAuthSettings.AppSecret;

                    // Request additional permissions (اختياري)
                    options.Scope.Add("email");

                    // Get extra fields like email and name
                    options.Fields.Add("email");
                    options.Fields.Add("name");

                    // يضمن اننا ناخد الإيميل (أحياناً بيبقى مخفي)
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");

                    //  التعامل مع فشل المصادقة (زي لما المستخدم يعمل Cancel)
                    options.Events.OnRemoteFailure = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Identity/Account/LoginCommunity");
                        return Task.CompletedTask;
                    };
                });

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

            // Add ClaimsPrincipalFactory to add custom claims to the user (e.g. FullName)
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

            // Force immediate re-validation of the security stamp upon any user-related changes (e.g., password change, role update).
            // This ensures that if a user updates their password or role, they must re-authenticate immediately. 
            services.Configure<SecurityStampValidatorOptions>(option => option.ValidationInterval = TimeSpan.Zero);

            // Configure cookie settings 20/1/2025
            /*
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.LogoutPath = "/Identity/Account/Logout";

                // (returnurl) توجيه المستخدمين إلى صفحة تسجيل الدخول الخاصة بهم مع الحفاظ علي
                options.Events.OnRedirectToLogin = context =>
                {
                    var returnUrl = context.Request.Path + context.Request.QueryString;

                    string loginPath = context.Request.Path.StartsWithSegments("/Community")
                        ? "/Identity/Account/LoginCommunity"
                        : "/Identity/Account/Login";

                    var redirectUri = loginPath + "?returnUrl=" + Uri.EscapeDataString(returnUrl);

                    context.Response.Redirect(redirectUri);
                    return Task.CompletedTask;
                };
            });
            */

            // Configure cookie settings 15/2/2025
            /*
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.LogoutPath = "/Identity/Account/Logout";

                options.Events.OnRedirectToLogin = context =>
                {
                    var returnUrl = context.Request.Path + context.Request.QueryString;

                    string loginPath = "/Home/Index";

                    if (context.Request.Path.StartsWithSegments("/Community"))
                    {   // redirect to the home page if the user is in the Community area
                        loginPath = "/Identity/Account/LoginCommunity";
                    }
                    else if (context.Request.Path.StartsWithSegments("/Library"))
                    {   // redirect to the home page if the user is in the Admin area
                        loginPath = "/Identity/Account/Login";
                    }
                    else
                    {   // redirect to the home page if the user is in the Identity area
                        loginPath = "/Home/Index";
                        returnUrl = string.Empty;
                    }

                    var redirectUri = loginPath + "?returnUrl=" + Uri.EscapeDataString(returnUrl);

                    context.Response.Redirect(redirectUri);
                    return Task.CompletedTask;
                };
            });
            */

            // Configure cookie settings 12/4/2025
            builder.Services.ConfigureApplicationCookie(options =>
            {
                // Set default paths for login, access denied, and logout
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.LogoutPath = "/Identity/Account/Logout";

                // Define the redirect behavior when the user is not authenticated
                options.Events.OnRedirectToLogin = context =>
                {
                    // Construct the returnUrl to redirect the user back to the original page they were trying to access
                    var returnUrl = context.Request.Path + context.Request.QueryString;

                    // Determine the login path based on the request path or ApplicationArea
                    string loginPath = context.Request.Path.StartsWithSegments("/Community") ?
                                       "/Identity/Account/LoginCommunity" :
                                       context.Request.Path.StartsWithSegments("/Library") ?
                                       "/Identity/Account/Login" :
                                       GetLoginPathFromClaims(context);

                    // Append the returnUrl as a query parameter to the redirect URL
                    var redirectUri = $"{loginPath}?returnUrl={Uri.EscapeDataString(returnUrl)}";

                    // Perform the redirect to the appropriate login path
                    context.Response.Redirect(redirectUri);
                    return Task.CompletedTask;
                };
            });

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
                // Retrieve the database connection string from configuration
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                config.UseSqlServerStorage(connectionString);

            });
            // Add Hangfire server
            builder.Services.AddHangfireServer();


            // Add Authorization policy for AdminsOnly (Admins and SuperAdmins)
            services.Configure<AuthorizationOptions>(options =>
            options.AddPolicy("AdminsOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AppRoles.SuperAdmin, AppRoles.LibraryAdmin);
            }));

            // Add MVC services
            services.AddMvc(options =>
            {
                // Add Antiforgery token attribute to the application (will be added automatically to all POST requests)
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            return services;
        }

        // a helper Method to determine the login path based on the ApplicationArea claim
        private static string GetLoginPathFromClaims(RedirectContext<CookieAuthenticationOptions> context)
        {
            var applicationArea = context.HttpContext.User.FindFirstValue(CustomClaimTypes.UserAppllicationArea);

            return applicationArea switch
            {
                AppConstants.LibraryStaffArea => "/Identity/Account/Login",
                AppConstants.CommunityArea => "/Identity/Account/LoginCommunity",
                _ => "/Home/Index" // this means that the session is expired or the user is not authenticated so redirect him to the home page
            };
        }
    }
}