using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Commands;

public record RegisterUserCommand(
    string Email,
    string FullName,
    string Password
) : IRequest<RegisterUserResponse>;
