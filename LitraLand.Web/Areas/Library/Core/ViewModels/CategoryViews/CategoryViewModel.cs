namespace LitraLand.Web.Areas.Library.Core.ViewModels.CategoryViews
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

        public override string ToString() => Name;
    }
}
