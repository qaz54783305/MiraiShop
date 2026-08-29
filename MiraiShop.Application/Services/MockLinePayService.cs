using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;

namespace MiraiShop.Application.Services;

public class MockLinePayService : ILinePayService
{
    public Task<LinePayRequestResponse> RequestPaymentAsync(LinePayRequestRequest request)
    {
        var response = new LinePayRequestResponse(
            TransactionId: "999999999999999999",
            PaymentUrl: "https://sandbox-web-pay.line.me/web/payment/wait?transactionReserveId=mock");

        return Task.FromResult(response);
    }

    public Task<LinePayConfirmResponse> ConfirmPaymentAsync(string transactionId, string orderId)
    {
        var response = new LinePayConfirmResponse(
            TransactionId: transactionId,
            OrderId: orderId,
            Status: "PAID");

        return Task.FromResult(response);
    }
}