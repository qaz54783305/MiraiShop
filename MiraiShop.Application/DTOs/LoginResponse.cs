namespace MiraiShop.Application.DTOs;

public record LoginResponse(
    string Token,
    DateTime Expiry,
    Guid MemberId);
