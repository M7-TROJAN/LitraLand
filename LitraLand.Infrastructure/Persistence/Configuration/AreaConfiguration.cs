namespace LitraLand.Web.Data.Configuration
{
    public class AreaConfiguration : IEntityTypeConfiguration<Area>
    {
        public void Configure(EntityTypeBuilder<Area> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name)
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(a => new { a.Name, a.GovernorateId })
                .IsUnique()
                .HasDatabaseName("IX_Areas_Name_GovernorateId");

            builder.HasOne(a => a.Governorate)
                .WithMany(g => g.Areas)
                .HasForeignKey(a => a.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Areas");
        }
    }
}
