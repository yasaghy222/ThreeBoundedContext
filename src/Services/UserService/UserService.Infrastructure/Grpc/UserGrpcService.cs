using Grpc.Core;
using UserService.Domain.Repositories;

namespace UserService.Infrastructure.Grpc;

public class UserGrpcService : UserGrpc.UserGrpcBase
{
    private readonly IUserRepository _userRepository;

    public UserGrpcService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public override async Task<UserResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            return new UserResponse { Found = false };
        }

        var user = await _userRepository.GetByIdAsync(userId, context.CancellationToken);
        
        if (user == null)
        {
            return new UserResponse { Found = false };
        }

        return new UserResponse
        {
            Found = true,
            UserId = user.Id.ToString(),
            Email = user.Email.Value,
            FullName = user.FullName,
            IsActive = user.IsActive
        };
    }

    public override async Task<UserResponse> GetUserByEmail(GetUserByEmailRequest request, ServerCallContext context)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, context.CancellationToken);
        
        if (user == null)
        {
            return new UserResponse { Found = false };
        }

        return new UserResponse
        {
            Found = true,
            UserId = user.Id.ToString(),
            Email = user.Email.Value,
            FullName = user.FullName,
            IsActive = user.IsActive
        };
    }

    public override async Task<ValidateUserResponse> ValidateUser(ValidateUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            return new ValidateUserResponse
            {
                IsValid = false,
                IsActive = false,
                Message = "Invalid user ID format"
            };
        }

        var user = await _userRepository.GetByIdAsync(userId, context.CancellationToken);
        
        if (user == null)
        {
            return new ValidateUserResponse
            {
                IsValid = false,
                IsActive = false,
                Message = "User not found"
            };
        }

        return new ValidateUserResponse
        {
            IsValid = true,
            IsActive = user.IsActive,
            Message = user.IsActive ? "User is valid and active" : "User exists but is not active"
        };
    }
}
