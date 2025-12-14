using UserService.Domain.Common;
using UserService.Domain.Events;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public class User : Entity
{
    public Email Email { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private User() { } // EF Core

    private User(Guid id, Email email, string fullName, string passwordHash) : base(id)
    {
        Email = email;
        FullName = fullName;
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(string email, string fullName, string passwordHash)
    {
        var emailVo = Email.Create(email);
        var user = new User(Guid.NewGuid(), emailVo, fullName, passwordHash);
        
        user.AddDomainEvent(new UserCreatedDomainEvent(
            user.Id,
            user.Email.Value,
            user.FullName,
            user.CreatedAt
        ));

        return user;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string fullName)
    {
        FullName = fullName;
        UpdatedAt = DateTime.UtcNow;
    }
}
