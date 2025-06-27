using LitraLand.Domain.Dtos.Community;
using LitraLand.Domain.Entities.Community;
using Microsoft.AspNetCore.Identity;

namespace LitraLand.Application.Common.Interfaces;
public interface IApplicationDbContext
{
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityUserRole<string>> UserRoles { get; set; }
    public DbSet<IdentityUserClaim<string>> UserClaims { get; set; }
    public DbSet<IdentityUserLogin<string>> UserLogins { get; set; }
    public DbSet<IdentityUserToken<string>> UserTokens { get; set; }
    public DbSet<IdentityRoleClaim<string>> RoleClaims { get; set; }

    public DbSet<Area> Areas { get; set; }
    public DbSet<Author> Authors { get; set; }
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

    //stored procedures
    public DbSet<MostPopularBookDTO> MostPopularBooksView { get; set; }
    public DbSet<TopMemberDto> TopMembersDto { get; set; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}