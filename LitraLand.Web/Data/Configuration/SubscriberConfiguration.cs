using LitraLand.Web.Core.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LitraLand.Web.Data.Configuration
{
    public class SubscriberConfiguration : IEntityTypeConfiguration<Subscriber>
    {
        public void Configure(EntityTypeBuilder<Subscriber> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedOnAdd(); // Explicitly specifies auto-increment behavior

            builder.Property(s => s.FirstName)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(s => s.LastName)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(s => s.NationalId)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(s => s.NationalId)
                .IsUnique();

            builder.Property(s => s.PhoneNumber)
                .HasColumnType("varchar(15)")
                .HasMaxLength(15)
                .IsRequired();

            builder.HasIndex(s => s.PhoneNumber)
                .IsUnique();

            builder.Property(s => s.Email)
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .IsRequired();

            builder.HasIndex(s => s.Email)
                .IsUnique();

            builder.Property(s => s.ImageUrl)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(s => s.ImageThumbnailUrl)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(s => s.Address)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(s => s.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(s => s.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(s => s.LastUpdatedOn)
                .HasDefaultValue(null);

            builder.HasOne(s => s.Area)
                .WithMany(a => a.Subscribers)
                .HasForeignKey(s => s.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Governorate)
                .WithMany(g => g.Subscribers)
                .HasForeignKey(s => s.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Subscribers");
        }
    }
}