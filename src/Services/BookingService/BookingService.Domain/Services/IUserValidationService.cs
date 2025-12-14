namespace BookingService.Domain.Services;

public record UserInfo(
    Guid UserId,
    string Email,
    string FullName,
    bool IsActive
);

public interface IUserValidationService
{
	Task<UserInfo?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<bool> ValidateUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
