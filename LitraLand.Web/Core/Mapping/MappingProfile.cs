namespace LitraLand.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
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
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title));

            CreateMap<BookCopy, BookCopyFormViewModel>();
        }
    }
}
