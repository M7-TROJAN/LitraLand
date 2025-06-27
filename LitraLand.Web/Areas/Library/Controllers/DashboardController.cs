namespace LitraLand.Web.Areas.Library.Controllers
{
    [Area("Library")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DashboardController(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<IActionResult> Index()
        {
            var numberOfCopies = await _context.BookCopies.CountAsync(c => !c.IsDeleted) + 200; // 200 is just to make the number more realistic for the demo
            numberOfCopies = numberOfCopies <= 10 ? numberOfCopies : (numberOfCopies / 10) * 10; // round to the nearest 10 for better visualization (e.g. 23 -> 20, 27 -> 30)

            var numberOfSubscribers = await _context.Subscribers
                .CountAsync(s => !s.IsDeleted);

            var lastAddedBooks = await _context.Books
                .Include(b => b.Author)
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.Id)
                .Take(8)
                .ToListAsync();

            //var mostPopularBooks = _context.RentalCopies
            //    .Include(rc => rc.BookCopy)
            //    .ThenInclude(c => c!.Book)
            //    .ThenInclude(b => b!.Author)
            //    .GroupBy(rc => new
            //    {
            //        BookId = rc.BookCopy!.BookId,
            //        Title = rc.BookCopy!.Book!.Title,
            //        ImageThumbnailUrl = rc.BookCopy!.Book!.ImageThumbnailUrl,
            //        AuthorName = rc.BookCopy!.Book!.Author!.Name
            //    })
            //    .Select(b => new
            //    {
            //        BookId = b.Key.BookId,
            //        Title = b.Key.Title,
            //        ImageThumbnailUrl = b.Key.ImageThumbnailUrl,
            //        AuthorName = b.Key.AuthorName,
            //        Count = b.Count()
            //    })
            //    .OrderByDescending(b => b.Count)
            //    .Take(8)
            //    .Select(b => new BookViewModel
            //    {
            //        Id = b.BookId,
            //        Title = b.Title,
            //        ImageThumbnailUrl = b.ImageThumbnailUrl,
            //        Author = b.AuthorName
            //    })
            //    .ToList();

            var mostPopularBooks = await _context.MostPopularBooksView
                .FromSqlRaw("EXEC GetMostPopularBooks @TopN = {0}", 7)
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                NumberOfCopies = numberOfCopies,
                NumberOfSubscribers = numberOfSubscribers,
                LastAddedBooks = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks),
                MostPopularBooks = _mapper.Map<IEnumerable<BookViewModel>>(mostPopularBooks)
            };

            return View(viewModel);
        }


        [AjaxOnly]
        public IActionResult GetRentalsPerDay(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-29);
            endDate ??= DateTime.Today;

            var data = _context.RentalCopies
                .Where(rc => rc.RentalDate >= startDate && rc.RentalDate <= endDate)
                .GroupBy(rc => new
                {
                    Date = rc.RentalDate
                })
                .Select(g => new ChartItemViewModel
                {
                    Label = g.Key.Date.ToString("dd MMM"),
                    Value = g.Count().ToString()
                })
                .ToList();

            // the below code ensures that we have data for each day in the range, even if there are no rentals on that day (for testing purposes) 
            // remove it if you don't need it

            List<ChartItemViewModel> figures = new ();

            for (var day = startDate; day <= endDate; day = day.Value.AddDays(1))
            {
                var dayData = data.SingleOrDefault(d => d.Label == day.Value.ToString("d MMM"));

                ChartItemViewModel item = new()
                {
                    Label = day.Value.ToString("d MMM"),
                    Value = dayData is null ? "0" : dayData.Value
				};

                figures.Add(item);
            }

            return Ok(data);
        }

        [AjaxOnly]
        public IActionResult GetSubscribersPerCity()
        {
            var data = _context.Subscribers
                .Include(s => s.Governorate)
                .Where(s => !s.IsDeleted)
                .GroupBy(s => new { GovernorateName = s.Governorate!.Name })
                .Select(g => new ChartItemViewModel
                {
                    Label = g.Key.GovernorateName,
                    Value = g.Count().ToString()
                })
                .ToList();

            return Ok(data);
        }
    }
}