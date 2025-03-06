namespace LitraLand.Web.Services
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string GetEmailBody(string template, Dictionary<string, string> placeholders)
        {
            var templeatePath = $"{_webHostEnvironment.WebRootPath}/templates/{template}.html";
            StreamReader streamReader = new StreamReader(templeatePath);
            var templateContent = streamReader.ReadToEnd();
            streamReader.Close();

            foreach (var placeholder in placeholders)
                templateContent = templateContent.Replace($"[{placeholder.Key}]", placeholder.Value);

            return templateContent;
        }

        // old implementation
        //public string GetEmailBody(string imageUrl, string header, string body, string url, string linkTitle)
        //{
        //    var templeatePath = $"{_webHostEnvironment.WebRootPath}/templates/email.html";
        //    StreamReader streamReader = new StreamReader(templeatePath);
        //    var template = streamReader.ReadToEnd();
        //    streamReader.Close();

        //    return template
        //        .Replace("[[mediaUrl]]", imageUrl)
        //        .Replace("[[header]]", header)
        //        .Replace("[[body]]", body)
        //        .Replace("[[url]]", url)
        //        .Replace("[[linkTitle]]", linkTitle);
        //}
    }
}
