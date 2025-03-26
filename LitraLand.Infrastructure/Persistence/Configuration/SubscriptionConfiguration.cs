namespace LitraLand.Web.Data.Configuration
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.StartDate)
                .IsRequired();

            builder.Property(s => s.EndDate)
                .IsRequired();

            builder.Property(s => s.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(s => s.Subscriber)
                .WithMany(s => s.Subscriptions)
                .HasForeignKey(s => s.SubscriberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Subscriptions");
        }
    }
}
