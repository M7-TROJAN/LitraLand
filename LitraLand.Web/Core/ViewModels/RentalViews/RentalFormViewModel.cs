namespace LitraLand.Web.Core.ViewModels.RentalViews
{
    public class RentalFormViewModel
    {
        public int? Id { get; set; } // repersent the rental id (This is just for the Edit form view)
        public string SubscriberKey { get; set; } = null!;
        public IList<int> SelectedCopies { get; set; } = new List<int>();
        public int? MaxAllowedCopies { get; set; }
        public IEnumerable<BookCopyViewModel> CurrentCopies { get; set; } = new List<BookCopyViewModel>(); // represent the current copies that the subscriber has rented (This is just for the Edit form view)
    }
}