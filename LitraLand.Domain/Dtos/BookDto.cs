namespace LitraLand.Domain.Dtos;
public record BookDto(
    int Id,
    string Title,
    string? ImageThumbnailUrl,
    string Author
);