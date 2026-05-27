namespace MiraiShop.Domain.Entities;

public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PasswordSalt { get; set; }
    public string MailingAddress { get; set; } = string.Empty;
    public string ResidentialAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
