using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace LitraLand.Web.filters
{
    public class AjaxOnlyAttribute : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            var request = routeContext.HttpContext.Request;

            var isAjax = request.Headers["X-Requested-With"] == "XMLHttpRequest";

            return isAjax;
        }
    }
}
