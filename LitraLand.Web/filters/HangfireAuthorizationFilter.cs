using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace LitraLand.Web.filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private string _policyName;

        public HangfireAuthorizationFilter(string policyName)
        {
            _policyName = policyName;
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var authService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            var isAuthorized = authService.AuthorizeAsync(httpContext.User, _policyName)
                                        .ConfigureAwait(false)
                                        .GetAwaiter()
                                        .GetResult()
                                        .Succeeded;

            return isAuthorized;
        }
    }
}
