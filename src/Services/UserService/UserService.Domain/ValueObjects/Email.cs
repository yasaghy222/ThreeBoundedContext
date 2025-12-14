using ErrorHandling.Core.Exceptions;

namespace UserService.Domain.ValueObjects;

public record Email
{
	public string Value { get; }

	private Email(string value)
	{
		Value = value;
	}

	public static Email Create(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
			throw new BadRequestException("Email cannot be empty", "email_empty");

		if (!email.Contains('@'))
			throw new BadRequestException("Invalid email format", "email_invalid_format");

		return new Email(email.ToLowerInvariant());
	}

	public override string ToString() => Value;
}
