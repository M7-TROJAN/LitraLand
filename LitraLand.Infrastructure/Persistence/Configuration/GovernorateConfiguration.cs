namespace LitraLand.Web.Data.Configuration
{
    public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
    {
        public void Configure(EntityTypeBuilder<Governorate> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name)
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(g => g.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(g => g.Name)
                .IsUnique()
                .HasDatabaseName("IX_Governorates_Name");

            builder.ToTable("Governorates");
        }
    }
}
