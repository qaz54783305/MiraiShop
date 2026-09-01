namespace MiraiShop.Application.DTOs;

public record OrderDto(
    string Id,
    int TotalAmount,
    string Currency
    );