using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;

namespace MiraiShop.Application.Services;

public class MockLinePayService : ILinePayService
{
    public Task<LinePayRequestResponse> RequestPaymentAsync(LinePayRequest request)
    {
        var response = new LinePayRequestResponse(
            ReturnCode: "0000",
            ReturnMessage: "Success",
            Info: new LinePayInfo(
                TransactionId: 1234567890123456789,
                PaymentUrl: new LinePayUrl(
                    Web: "https://sandbox-web-pay.line.me/web/payment/wait?transactionReserveId=mock123456",
                    App: "line://pay/payment/mock123456"
                ),
                PaymentAccessToken: "mock_payment_access_token_123456"
            )
        );

        return Task.FromResult(response);
    }

    public Task<LinePayConfirmResponse> ConfirmPaymentAsync(string transactionId, LinePayConfirmRequest request)
    {
        var response = new LinePayConfirmResponse(
            ReturnCode: "0000",
            ReturnMessage: "Success",
            Info: new LinePayConfirmInfo(
                OrderId: "mock_order_id",
                TransactionId: long.Parse(transactionId),
                PayInfo: new List<LinePayPayInfo>
                {
                    new LinePayPayInfo(
                        Method: "CREDIT_CARD",
                        Amount: request.Amount
                    )
                }.AsReadOnly()
            )
        );

        return Task.FromResult(response);
    }
}