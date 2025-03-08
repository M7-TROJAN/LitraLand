using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LitraLand.Web.Data.Configuration
{
    public class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedOnAdd();

            builder.Property(c => c.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(c => c.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.LastUpdatedOn)
                .HasDefaultValue(null);

            builder.HasOne(r => r.Subscriber)
                .WithMany(s => s.Rentals)
                .HasForeignKey(r => r.SubscriberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.RentalCopies)
                .WithOne(rc => rc.Rental)
                .HasForeignKey(rc => rc.RentalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Rentals");
        }
    }
}
