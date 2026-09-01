namespace MiraiShop.Application.DTOs;

public record LinePayRequest(
    string OrderId,
    int Amount,
    string Currency,
    IReadOnlyList<LinePayPackage> Packages,
    LinePayRedirectUrls RedirectUrls);

public record LinePayPackage(
    string Id,
    int Amount,
    IReadOnlyList<LinePayProducts> Products);

public record LinePayProducts(
    string Id,
    string Name,
    string ImageUrl,
    int Quantity,
    int Price);


public record LinePayRedirectUrls(
    string ConfirmUrl,
    string CancelUrl);


public record LinePayRequestResponse(
    string ReturnCode,
    string ReturnMessage,
    LinePayInfo Info);

public record LinePayInfo(
    long TransactionId,
    LinePayUrl PaymentUrl,
    string PaymentAccessToken);

public record LinePayUrl(
    string Web,
    string App);
    
public record LinePayConfirmRequest(int Amount, string Currency);

public record LinePayConfirmResponse(
    string ReturnCode,
    string ReturnMessage,
    LinePayConfirmInfo Info);

public record LinePayConfirmInfo(
    string OrderId,
    long TransactionId,
    IReadOnlyList<LinePayPayInfo> PayInfo);

public record LinePayPayInfo(string Method, int Amount);