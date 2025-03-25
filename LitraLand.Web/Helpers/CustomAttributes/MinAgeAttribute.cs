namespace LitraLand.Web.Helpers.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MinAgeAttribute : ValidationAttribute
    {
        private readonly int _minAge;

        public MinAgeAttribute(int minAge, string? errorMessage = null)
        {
            _minAge = minAge;
            ErrorMessage = errorMessage ?? $"You must be at least {_minAge} years old.";
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return new ValidationResult($"{validationContext.DisplayName} is required.");
            }

            if (value is not DateTime date)
            {
                throw new InvalidOperationException($"{nameof(MinAgeAttribute)} can only be applied to properties of type DateTime.");
            }

            if (date > DateTime.Today)
            {
                return new ValidationResult("Date of birth cannot be in the future.");
            }

            if (date.AddYears(_minAge) > DateTime.Today)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success!;
        }
    }
}