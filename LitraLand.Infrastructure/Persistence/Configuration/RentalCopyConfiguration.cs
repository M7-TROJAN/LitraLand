namespace LitraLand.Infrastructure.Persistence.Configuration
{
    internal class RentalCopyConfiguration : IEntityTypeConfiguration<RentalCopy>
    {
        public void Configure(EntityTypeBuilder<RentalCopy> builder)
        {

            builder.HasKey(rc => new { rc.RentalId, rc.BookCopyId }); // Composite key of RentalId and BookCopyId

            builder.HasQueryFilter(rc => !rc.Rental!.IsDeleted);

            builder.Property(rc => rc.RentalDate)
                .HasDefaultValueSql("CAST(GETDATE() AS DATE)");

            builder.ToTable("RentalCopies");
        }
    }
}
