namespace LitraLand.Infrastructure.Persistence.Configuration.Community
{
    internal class CommunityBookConfiguration : IEntityTypeConfiguration<CommunityBook>
    {
        public void Configure(EntityTypeBuilder<CommunityBook> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .ValueGeneratedOnAdd();

            builder.Property(b => b.Title)
                .HasColumnType("nvarchar")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(b => b.Author)
                .HasColumnType("nvarchar")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(b => b.Description)
                .HasColumnType("nvarchar")
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(b => b.ImageUrl)
                .HasColumnType("nvarchar")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(b => b.ImageThumbnailUrl)
                .HasColumnType("nvarchar")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(b => b.ImagePublicId)
                .HasColumnType("nvarchar")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(b => b.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired(true);

            builder.Property(b => b.IsForExchange)
                .HasDefaultValue(false);

            builder.Property(c => c.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(b => b.Owner)
                .WithMany(u => u.CommunityBooks)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); // restrict means we cannot delete the owner if they have books

            builder.ToTable("CommunityBooks");
        }
    }
}