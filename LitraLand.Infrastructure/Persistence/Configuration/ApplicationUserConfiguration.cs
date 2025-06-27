using LitraLand.Domain.Entities.Common;

namespace LitraLand.Infrastructure.Persistence.Configuration
{
    internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FullName)
                .HasMaxLength(100)
                .IsRequired();

            // prevent duplicated email and username
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.UserName).IsUnique();

            // prevent duplicated phone number if not null
            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique()
                .HasDatabaseName("IX_ApplicationUser_PhoneNumber")
                .HasFilter("[PhoneNumber] IS NOT NULL"); // only unique if not null (e.g. for users who don't have a phone number)

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(u => u.DateOfBirth)
                .HasColumnType("date");

            builder.HasOne(u => u.Area)
                .WithMany(a => a.Users)
                .HasForeignKey(u => u.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Governorate)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(u => u.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

        }
    }
}
