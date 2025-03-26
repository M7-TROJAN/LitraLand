namespace LitraLand.Web.Data.Configuration
{
    public class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
    {
        public void Configure(EntityTypeBuilder<BookCopy> builder)
        {
            builder.Property(b => b.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber"); // SQL Server sequence for serial number

            builder.Property(b => b.CreatedOn)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}