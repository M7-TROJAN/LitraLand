using LitraLand.Domain.Dtos.Community;
using LitraLand.Domain.Dtos.Library;
using LitraLand.Domain.Entities.Common;
using LitraLand.Domain.Entities.Community;
namespace LitraLand.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Library management system
            // Category
            CreateMap<Category, CategoryViewModel>(); // Category -> CategoryViewModel

            CreateMap<CategoryFormViewModel, Category>().ReverseMap(); // CategoryFormViewModel -> Category and vice versa

            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name)); // Category -> SelectListItem

            CreateMap<Category, string>().ConvertUsing(c => c.Name); // Category -> string (Name)

            // Author
            CreateMap<Author, AuthorViewModel>();

            CreateMap<AuthorFormViewModel, Author>().ReverseMap();

            CreateMap<Author, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            // Book
            CreateMap<BookFormViewModel, Book>()
                .ReverseMap()
                .ForMember(dest => dest.Categories, opt => opt.Ignore()); // Ignore mapping of Categories property

            CreateMap<Book, BookViewModel>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => c.Category!.Name).ToList()));


            // BookCopy
            CreateMap<BookCopy, BookCopyViewModel>()
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Book!.Id))
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title))
                .ForMember(dest => dest.BookThumbnailUrl, opt => opt.MapFrom(src => src.Book!.ImageThumbnailUrl));


            CreateMap<BookCopy, BookCopyFormViewModel>();

            // User
            CreateMap<ApplicationUser, UserViewModel>()
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area!.Name))
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate!.Name));

            CreateMap<UserFormViewModel, ApplicationUser>()
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ReverseMap();


            // subscriber
            CreateMap<SubscriberFormViewModel, Subscriber>()
                .ReverseMap()
                .ForMember(dest => dest.Areas, opt => opt.Ignore())
                .ForMember(dest => dest.Governorates, opt => opt.Ignore());

            CreateMap<Subscriber, SubscriberViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area!.Name))
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate!.Name));

            CreateMap<Subscriber, SubscriberSearchResultViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName));

            // subscription
            CreateMap<Subscription, SubscriptionViewModel>();

            // governorates
            CreateMap<Governorate, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            // areas
            CreateMap<Area, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Rentals
            CreateMap<Rental, RentalViewModel>();
            CreateMap<RentalCopy, RentalCopyViewModel>();

            CreateMap<RentalCopy, CopyHistoryViewModel>()
                .ForMember(dest => dest.SubscriberName, opt => opt.MapFrom(src => src.Rental!.Subscriber!.FirstName + " " + src.Rental!.Subscriber!.LastName))
                .ForMember(dest => dest.SubscriberMobile, opt => opt.MapFrom(src => src.Rental!.Subscriber!.PhoneNumber));

            // stord Procedures
            CreateMap<MostPopularBookDTO, BookViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BookId)); // MostPopularBook -> BookViewModel

            // Community
            // MemberViewModel
            CreateMap<ApplicationUser, MemberViewModel>()
                .ForMember(dest => dest.MembershipDate, opt => opt.MapFrom(src => src.CreatedOn))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area != null ? src.Area.Name : string.Empty))
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate != null ? src.Governorate.Name : string.Empty)); // ApplicationUser -> MemberViewModel

            // CommunityBook
            CreateMap<CommunityBook, CommunityBookViewModel>(); // CommunityBook -> CommunityBookViewModel

            CreateMap<CommunityBookFormViewModel, CommunityBook>()
                .ReverseMap(); // CommunityBookFormViewModel -> CommunityBook and vice versa

            // stord Procedures
            CreateMap<TopMemberDto, TopMemberViewModel>()
                .ForMember(dest => dest.NumberOfBooks, opt => opt.MapFrom(src => src.BookCount))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageThumbnailUrl)); // TopMemberDto -> TopMemberViewModel

        }
    }
}
