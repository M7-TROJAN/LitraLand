namespace LitraLand.Domain.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Length > maxLength ? input.Substring(0, maxLength) + "..." : input;
        }
    }
}