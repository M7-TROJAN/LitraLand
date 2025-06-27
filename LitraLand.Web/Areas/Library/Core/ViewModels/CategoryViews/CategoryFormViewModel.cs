namespace LitraLand.Web.Areas.Library.Core.ViewModels.CategoryViews
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Category")]
        [Required(ErrorMessage = "Category name is required.")]
        [Remote("AllowItem", null /*"Categories"*/, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string Name { get; set; } = null!;
    }

    // in remote validation, we need to create a method in the controller that will be called by the remote attribute
    // if you make the controller null in the remote attribute, it will take the current controller that is being used
}
