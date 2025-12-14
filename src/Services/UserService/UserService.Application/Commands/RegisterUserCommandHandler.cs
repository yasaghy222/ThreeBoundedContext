using MediatR;
using UserService.Application.DTOs;
using UserService.Domain.Common;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
	private readonly IUserRepository _userRepository;
	private readonly IDomainEventDispatcher _domainEventDispatcher;

	public RegisterUserCommandHandler(
	    IUserRepository userRepository,
	    IDomainEventDispatcher domainEventDispatcher)
	{
		_userRepository = userRepository;
		_domainEventDispatcher = domainEventDispatcher;
	}

	public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
	{
		// Check if user already exists
		var exists = await _userRepository.ExistsAsync(request.Email, cancellationToken);
		if (exists)
		{
			throw new InvalidOperationException($"User with email {request.Email} already exists");
		}

		// Hash password (simplified - in production use proper hashing)
		var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

		// Create user entity
		var user = User.Create(request.Email, request.FullName, passwordHash);

		// Save to repository
		await _userRepository.AddAsync(user, cancellationToken);

		// Dispatch domain events (publishes to RabbitMQ after commit)
		await _domainEventDispatcher.DispatchAsync(user.DomainEvents, cancellationToken);
		user.ClearDomainEvents();

		return new RegisterUserResponse(user.Id, user.Email.Value, user.FullName);
	}
}
