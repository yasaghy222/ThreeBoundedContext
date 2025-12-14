namespace UserService.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    DateTime CreatedAt
);

public record RegisterUserRequest(
    string Email,
    string FullName,
    string Password
);

public record RegisterUserResponse(
    Guid UserId,
    string Email,
    string FullName
);
