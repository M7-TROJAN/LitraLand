namespace LitraLand.Web.Data.Configuration
{
    public class AuthorConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd(); // Explicitly specifies auto-increment behavior

            builder.Property(a => a.Name)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(a => a.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.LastUpdatedOn)
                .HasDefaultValue(null);

            builder.HasIndex(a => a.Name)
                .IsUnique()
                .HasDatabaseName("IX_Authors_Name");

            builder.ToTable("Authors");
        }
    }
}
