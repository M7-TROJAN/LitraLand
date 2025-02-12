using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace LitraLand.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasSequence<int>("SerialNumber", schema: "shared")
                .StartsAt(1000001)
                .IncrementsBy(1);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(builder);
        }
    }
}
