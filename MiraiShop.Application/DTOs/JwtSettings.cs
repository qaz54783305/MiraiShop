namespace MiraiShop.Application.DTOs;

public record JwtSettings(
    string SecretKey,
    string Issuer,
    string Audience,
    int ExpiryMinutes,
    string[] AdminEmails);
