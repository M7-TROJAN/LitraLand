namespace LitraLand.Web.Controllers
{
    public class LocationsController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LocationsController(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult GetAreas(int governorateId)
        {
            var areas = _context.Areas
                .Where(a => a.GovernorateId == governorateId && !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToList();

            return Ok(_mapper.Map<IEnumerable<SelectListItem>>(areas));
        }
    }
}
