using LitraLand.Domain.Entities.Common;

namespace LitraLand.Domain.Dtos;
public record ManageUserResponseDto(
    bool IsSucceeded,
    ApplicationUser? User,
    string? VerificationCode,
    IEnumerable<string>? Errors
);