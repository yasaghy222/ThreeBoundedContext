using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using BookingService.Domain.Services;
using UserService.Infrastructure.Grpc;

namespace BookingService.Infrastructure.Grpc;

public class GrpcSettings
{
	public string UserServiceUrl { get; set; } = "http://localhost:5002";
}

public class UserGrpcClient : IUserValidationService, IAsyncDisposable
{
	private readonly GrpcSettings _settings;
	private GrpcChannel? _channel;
	private UserGrpc.UserGrpcClient? _client;
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public UserGrpcClient(IOptions<GrpcSettings> settings)
	{
		_settings = settings.Value;
	}

	private async Task EnsureChannelAsync()
	{
		if (_channel != null && _client != null)
			return;

		await _semaphore.WaitAsync();
		try
		{
			if (_channel != null && _client != null)
				return;

			_channel = GrpcChannel.ForAddress(_settings.UserServiceUrl);
			_client = new UserGrpc.UserGrpcClient(_channel);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task<UserInfo?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		await EnsureChannelAsync();

		var request = new GetUserByIdRequest { UserId = userId.ToString() };
		var response = await _client!.GetUserByIdAsync(request, cancellationToken: cancellationToken);

		if (!response.Found)
			return null;

		return new UserInfo(
		    Guid.Parse(response.UserId),
		    response.Email,
		    response.FullName,
		    response.IsActive
		);
	}

	public async Task<bool> ValidateUserAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		await EnsureChannelAsync();

		var request = new ValidateUserRequest { UserId = userId.ToString() };
		var response = await _client!.ValidateUserAsync(request, cancellationToken: cancellationToken);

		return response.IsValid && response.IsActive;
	}

	public async ValueTask DisposeAsync()
	{
		if (_channel != null)
		{
			await _channel.ShutdownAsync();
			_channel.Dispose();
		}

		_semaphore.Dispose();
		GC.SuppressFinalize(this);
	}
}
