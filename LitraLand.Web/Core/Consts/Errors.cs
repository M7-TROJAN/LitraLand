namespace LitraLand.Web.Core.Consts
{
    public static class Errors
    {
        public const string MaxLength = "Length cannot be more than {1} characters";
        public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
        public const string Duplicated = "Another record with the same {0} is already exists!"; // note that the {0} is replaced with the property name
        public const string DuplicatedBook = "Book with the same title is already exists with the same author!";
        public const string NotAllowedExtension = "Invalid Image format. Only .jpg, .jpeg, .png and .webp are allowed.";
        public const string MaxSize = "Image size should not exceed 2MB.";
        public const string Required = "{0} is required.";
        public const string NotAllowFutureDates = "{0} cannot be in the future!";
        public const string InvalidRange = "{0} should be between {1} and {2}.!"; // note that the {0}, {1}, and {2} are replaced with the property name, min, and max values respectively
        public const string ConfirmPasswordNotMatch = "The password and confirmation password do not match.";
        public const string PasswordComplexity = "Password must contain at least 8 characters long, one uppercase letter, one lowercase letter, one digit and one non-alphanumeric character.";
        public const string InvalidUsername = "Username can only contain letters, digits, and the following characters: -._@";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";
        public const string InvalidMobileNumber = "Invalid mobile number.";
        public const string InvalidNationalId = "Invalid national ID.";
        public const string InvalidSerialNumber = "Invalid serial number.";
        public const string NotAvilableRental = "This book/copy is not available for rental.";
        public const string EmptyImage = "Please select an image.";
        public const string BlackListedSubscriber = "This subscriber is blacklisted.";
        public const string InactiveSubscriber = "This subscriber is inactive.";
        public const string InvalidCopiesCount = "The selected copies count is more than the allowed count.";
        public const string MaxCopiesReached = "This subscriber has reached the max number for rentals.";
        public const string CopyIsInRental = "This copy is already rentaled.";
        public const string RentalNotAllowedForBlacklisted = "Rental cannot be extended for blacklisted subscribers.";
        public const string RentalNotAllowedForInactive = "Rental cannot be extended for this subscriber before renwal.";
        public const string ExtendNotAllowed = "Rental cannot be extended.";
        public const string PenaltyShouldBePaid = "Penalty should be paid.";
        public const string InvalidStartDate = "Invalid start date.";
        public const string InvalidEndDate = "Invalid end date.";
    }
}
