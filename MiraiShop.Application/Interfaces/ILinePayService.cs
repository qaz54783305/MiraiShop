using MiraiShop.Application.DTOs;
namespace MiraiShop.Application.Interfaces;

public interface ILinePayService
{
    Task<LinePayRequestResponse> RequestPaymentAsync(LinePayRequestRequest request);
    Task<LinePayConfirmResponse> ConfirmPaymentAsync(string transactionId, string orderId);
}