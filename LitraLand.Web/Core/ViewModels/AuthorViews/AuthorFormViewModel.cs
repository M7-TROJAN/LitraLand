namespace LitraLand.Web.Core.ViewModels.AuthorViews
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Author")]
        [Required(ErrorMessage = "Author name is required.")]
        [Remote("AllowItem", null, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string Name { get; set; } = null!;
    }
}
