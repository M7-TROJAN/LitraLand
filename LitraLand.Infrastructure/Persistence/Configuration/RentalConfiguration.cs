namespace LitraLand.Infrastructure.Persistence.Configuration
{
    internal class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedOnAdd();

            builder.Property(r => r.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(r => r.StartDate)
                .HasDefaultValueSql("CAST(GETDATE() AS DATE)");

            builder.HasOne(r => r.Subscriber)
                .WithMany(s => s.Rentals)
                .HasForeignKey(r => r.SubscriberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.RentalCopies)
                .WithOne(rc => rc.Rental)
                .HasForeignKey(rc => rc.RentalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(r => !r.IsDeleted);

            builder.ToTable("Rentals");
        }
    }
}
