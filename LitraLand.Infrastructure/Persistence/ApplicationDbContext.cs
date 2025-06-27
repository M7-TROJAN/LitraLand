using LitraLand.Domain.Dtos.Community;
using LitraLand.Domain.Dtos.Library;
using LitraLand.Domain.Entities.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;

namespace LitraLand.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RentalCopy> RentalCopies { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<CommunityBook> CommunityBooks { get; set; }
        public DbSet<MostPopularBookDTO> MostPopularBooksView { get; set; }
        public DbSet<TopMemberDto> TopMembersDto { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            // this is required to use the stored procedures (we tell EF Core that MostPopularBook is a view with no key)
            builder.Entity<MostPopularBookDTO>().HasNoKey().ToView(null);

            // Config DTO returned from Stored Procedure (not mapped to table)
            builder.Entity<TopMemberDto>().HasNoKey().ToView(null);

            builder.HasSequence<int>("SerialNumber", schema: "shared")
                .StartsAt(1000001)
                .IncrementsBy(1);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); // Applies all configurations in the assembly (LitraLand.Infrastructure)

            // Change cascade delete behavior to restrict for all relationships 
            var cascadeFKs = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            base.OnModelCreating(builder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}