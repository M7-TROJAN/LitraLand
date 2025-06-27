namespace LitraLand.Infrastructure.Persistence.Configuration.Library
{
    internal class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .ValueGeneratedOnAdd();

            builder.Property(b => b.Title)
                .HasColumnType("nvarchar")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(b => b.Publisher)
                .HasColumnType("nvarchar")
                .HasMaxLength(200);

            builder.Property(b => b.PublishedDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(b => b.ImageUrl)
                .HasColumnType("nvarchar")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(b => b.ImageThumbnailUrl)
                .HasColumnType("nvarchar")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(b => b.Hall)
                .HasColumnType("nvarchar")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(b => b.IsAvailableForRental)
                .HasDefaultValue(true);

            builder.Property(b => b.Description)
                .HasColumnType("nvarchar")
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(c => c.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => new { b.Title, b.AuthorId })
               .IsUnique()
               .HasDatabaseName("IX_Books_Title_AuthorId");

            // Add the table configuration and check constraint together
            builder.ToTable("Books", t =>
            {
                // add a check constraint to ensure the published date is not in the future
                t.HasCheckConstraint("CK_PublishedDate_NotInFuture", "PublishedDate <= GETDATE()");
            });
        }
    }
}
