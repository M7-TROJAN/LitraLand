namespace LitraLand.Domain.Entities
{
    public class Rental : BaseEntity
    {
        public int Id { get; set; }
        public int SubscriberId { get; set; } // FK
        public Subscriber? Subscriber { get; set; } // Navigation property
        public DateTime StartDate { get; set; }
        public bool PenaltyPaid { get; set; }
        public ICollection<RentalCopy> RentalCopies { get; set; } = new List<RentalCopy>();
    }
}
