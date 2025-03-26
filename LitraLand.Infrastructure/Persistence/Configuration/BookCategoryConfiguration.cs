namespace LitraLand.Infrastructure.Persistence.Configuration
{
    internal class BookCategoryConfiguration : IEntityTypeConfiguration<BookCategory>
    {
        public void Configure(EntityTypeBuilder<BookCategory> builder)
        {
            builder.HasKey(bc => new { bc.BookId, bc.CategoryId });

            builder.ToTable("BookCategories");
        }
    }
}
