namespace LitraLand.Infrastructure.Persistence.Configuration
{
    internal class AuthorConfiguration : IEntityTypeConfiguration<Author>
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

            builder.HasIndex(a => a.Name)
                .IsUnique()
                .HasDatabaseName("IX_Authors_Name");

            builder.Property(a => a.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.ToTable("Authors");
        }
    }
}
