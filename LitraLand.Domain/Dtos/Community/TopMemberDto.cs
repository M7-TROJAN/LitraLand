namespace LitraLand.Domain.Dtos.Community
{
    public class TopMemberDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? ImageThumbnailUrl { get; set; }
        public int BookCount { get; set; }
    }

}