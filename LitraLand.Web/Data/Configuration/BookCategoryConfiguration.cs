using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LitraLand.Web.Data.Configuration
{
    public class BookCategoryConfiguration : IEntityTypeConfiguration<BookCategory>
    {
        public void Configure(EntityTypeBuilder<BookCategory> builder)
        {
            builder.HasKey(bc => new { bc.BookId, bc.CategoryId });

            builder.ToTable("BookCategories");
        }
    }
}
