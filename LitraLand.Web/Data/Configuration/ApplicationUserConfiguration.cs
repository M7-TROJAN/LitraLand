using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LitraLand.Web.Data.Configuration
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FullName)
                .HasMaxLength(100)
                .IsRequired();

            // prevent duplicated email and username
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.UserName).IsUnique();


            builder.Property(u => u.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(u => u.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(u => u.LastUpdatedOn)
                .HasDefaultValue(null);

        }
    }
}
