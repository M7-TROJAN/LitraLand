using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LitraLand.Web.Data.Configuration
{
    public class RentalCopyConfiguration : IEntityTypeConfiguration<RentalCopy>
    {
        public void Configure(EntityTypeBuilder<RentalCopy> builder)
        {

            builder.HasKey(rc => new { rc.RentalId, rc.BookCopyId }); // Composite key of RentalId and BookCopyId

            builder.ToTable("RentalCopies");
        }
    }
}
