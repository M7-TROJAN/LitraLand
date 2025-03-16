namespace LitraLand.Web.Core.ViewModels.RentalViews
{
    public class RentalFormViewModel
    {
        public string SubscriberKey { get; set; } = null!;
        public IList<int> SelectedCopies { get; set; } = new List<int>();
        public int? MaxAllowedCopies { get; set; }
        public int? Id { get; set; } // repercent the rental id (This is for the Edit form view)
        public IEnumerable<BookCopyViewModel> CurrentCopies { get; set; } = new List<BookCopyViewModel>(); // This is for the Edit form view
    }
}