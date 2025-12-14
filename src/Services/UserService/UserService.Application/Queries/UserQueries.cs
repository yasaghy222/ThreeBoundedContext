using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;

public record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;

public record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;
