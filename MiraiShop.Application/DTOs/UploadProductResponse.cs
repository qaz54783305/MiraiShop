namespace MiraiShop.Application.DTOs;

public record UploadProductResponse(
    int SuccessCount,
    int FailCount,
    IReadOnlyList<string> Errors
);
