namespace MiraiShop.Application.DTOs;

public record LinePayRequestRequest(
    string OrderId,
    int Amount,
    IReadOnlyList<LinePayProductItem> Products);

public record LinePayProductItem(
    string Name,
    int Quantity,
    int Price);

public record LinePayRequestResponse(
    string TransactionId,
    string PaymentUrl);

public record LinePayConfirmResponse(
    string TransactionId,
    string OrderId,
    string Status);