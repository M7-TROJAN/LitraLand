using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LitraLand.Web.Helpers
{
    // 1. we need to know the html tag that we are going to apply this tag helper to
    // and the attribute that we are going to use
    [HtmlTargetElement("a", Attributes = "active-when")]
    public class ActiveTag : TagHelper
    {
        // 2. we need to know the value of the attribute that we are going to use
        // note that the name of the property should be the same as the attribute name but in pascal case (ActiveWhen)
        // the value of the attribute will be assigned to this property automatically
        public string ActiveWhen { get; set; }

        // 3. we need to know the view context (the current controller, action, etc.)
        [ViewContext]
        [HtmlAttributeNotBound] // this attribute is not bound to any html attribute meaning the value of this property will not be assigned by any html attribute but by the view context itself
        public ViewContext? ViewContextData { get; set; }

        // 4. we need to override the Process method
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // context: is the context of the tag helper
            // output: is the output of the tag helper (the html tag itself) in this case the <a> tag

            // 5. we need to check if the ActiveWhen property is null or empty
            if (string.IsNullOrEmpty(ActiveWhen))
                return;

            // 6. we need to check if the current controller is the same as the ActiveWhen property
            var currentController = ViewContextData?.RouteData.Values["controller"]?.ToString() ?? string.Empty;

            if (ActiveWhen == currentController)
            {
                if (output.Attributes.TryGetAttribute("class", out var classAttribute))
                    output.Attributes.SetAttribute("class", $"{classAttribute.Value} active");
                else
                    output.Attributes.SetAttribute("class", "active");
            }
        }
    }
}