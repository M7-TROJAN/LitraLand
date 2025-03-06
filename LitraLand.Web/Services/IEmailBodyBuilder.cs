namespace LitraLand.Web.Services
{
    public interface IEmailBodyBuilder
    {
        string GetEmailBody(string template, Dictionary<string, string> placeHolders);

        // old segnature 
        //public string GetEmailBody(string imageUrl, string header, string body, string url, string linkTitle)
    }
}
