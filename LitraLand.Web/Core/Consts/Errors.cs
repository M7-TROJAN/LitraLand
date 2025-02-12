namespace LitraLand.Web.Core.Consts
{
    public class Errors
    {
        public const string MaxLength = "Length cannot be more than {1} characters";
        public const string Duplicated = "{0} with the same name already exists!"; // note that the {0} is replaced with the property name
        public const string DuplicatedBook = "Book with the same title is already exists with the same author!";
        public const string NotAllowedExtension = "Invalid Image format. Only .jpg, .jpeg, .png are allowed.";
        public const string MaxSize = "Image size should not exceed 2MB.";
        public const string Required = "{0} is required.";
        public const string NotAllowFutureDates = "{0} cannot be in the future!";
        public const string InvalidRange = "{0} should be between {1} and {2}.!"; // note that the {0}, {1}, and {2} are replaced with the property name, min, and max values respectively
    }
}
