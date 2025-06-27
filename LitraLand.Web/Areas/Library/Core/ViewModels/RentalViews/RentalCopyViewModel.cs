namespace LitraLand.Web.Areas.Library.Core.ViewModels.RentalViews
{
    public class RentalCopyViewModel
    {
        public BookCopyViewModel? BookCopy { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }

        public int DelayInDays
        {
            get
            {
                var delay = 0;

                if (ReturnDate.HasValue && ReturnDate.Value > EndDate)
                    delay = (int)(ReturnDate.Value - EndDate).TotalDays;

                else if (!ReturnDate.HasValue && DateTime.Today > EndDate)
                    delay = (int)(DateTime.Today - EndDate).TotalDays;

                // delay = Math.Max((int)((ReturnDate ?? DateTime.Today) - EndDate).TotalDays, 0); // this line is the same as the previous two lines but more concise and readable

                return delay;

            }
        }
    }
}