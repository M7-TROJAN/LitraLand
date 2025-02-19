namespace LitraLand.Web.Core.ViewModels.UserViews
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Full Name")]
        [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string FullName { get; set; } = null!;

        [MaxLength(20, ErrorMessage = Errors.MaxLength), Display(Name = "Username")]
        [Remote("AllowUserName", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        [RegularExpression(RegexPatterns.Username, ErrorMessage = Errors.InvalidUsername)]
        public string UserName { get; set; } = null!;

        [EmailAddress]
        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowEmail", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8)]
        [RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.PasswordComplexity)]
        [RequiredIf("Id == null", ErrorMessage = Errors.Required)]
        public string? Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = Errors.ConfirmPasswordNotMatch)]
        [RequiredIf("Id == null", ErrorMessage = Errors.Required)]
        public string? ConfirmPassword { get; set; } = null!;

        [Display(Name = "Roles")]
        [Required(ErrorMessage = Errors.Required)]
        public IList<String> SelectedRoles { get; set; } = new List<string>();

        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}