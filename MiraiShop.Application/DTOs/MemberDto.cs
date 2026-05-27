namespace MiraiShop.Application.DTOs;

public record MemberDto(
    Guid Id,
    string Name,
    string Email,
    string MailingAddress,
    string ResidentialAddress,
    DateTime CreatedAt);
