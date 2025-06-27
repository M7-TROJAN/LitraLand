namespace LitraLand.Domain.Dtos.Community
{
    public class BookRequestDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public string? Filter { get; set; }
    }
}
