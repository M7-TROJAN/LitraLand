using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LitraLand.Web.Data.Configuration
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(c => c.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.LastUpdatedOn)
                .HasDefaultValue(null);

            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_Categories_Name");

            builder.ToTable("Categories");
        }
    }
}
