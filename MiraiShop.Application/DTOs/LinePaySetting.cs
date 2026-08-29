namespace MiraiShop.Application.DTOs;

public record LinePaySettings(
    string BaseUrl,
    string ChannelId,
    string ChannelSecret,
    string Currency,
    string ConfirmUrl,
    string CancelUrl,
    int TimeoutSeconds);