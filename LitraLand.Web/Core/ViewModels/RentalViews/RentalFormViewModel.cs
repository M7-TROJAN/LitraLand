namespace LitraLand.Web.Core.ViewModels.RentalViews
{
    public class RentalFormViewModel
    {
        public string SubscriberKey { get; set; } = null!;
        public IList<int> SelectedCopies { get; set; } = new List<int>();
    }
}