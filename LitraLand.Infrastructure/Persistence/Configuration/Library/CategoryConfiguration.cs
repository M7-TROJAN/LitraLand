namespace LitraLand.Infrastructure.Persistence.Configuration.Library
{
    internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(c => c.Name)
               .IsUnique()
               .HasDatabaseName("IX_Categories_Name");

            builder.Property(c => c.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.ToTable("Categories");
        }
    }
}
