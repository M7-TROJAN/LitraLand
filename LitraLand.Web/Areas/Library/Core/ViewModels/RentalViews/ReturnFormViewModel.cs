namespace LitraLand.Web.Areas.Library.Core.ViewModels.RentalViews
{
    public class ReturnFormViewModel
    {
        public int Id { get; set; } // Rental Id

        [Display(Name = "Penalty Paid?")]
        [AssertThat("(TotalDelayInDays == 0 && PenaltyPaid == false) || PenaltyPaid == true", ErrorMessage = Errors.PenaltyShouldBePaid)]
        public bool PenaltyPaid { get; set; }

        public IList<RentalCopyViewModel> Copies { get; set; } = new List<RentalCopyViewModel>();

        public List<ReturnCopyViewModel> SelectedCopies { get; set; } = new(); // repersent the 

        public bool AllowExtend { get; set; }

        public int TotalDelayInDays
        {
            get
            {
                return Copies.Sum(c => c.DelayInDays);
            }
        }
    }
}
