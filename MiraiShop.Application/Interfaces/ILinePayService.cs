using MiraiShop.Application.DTOs;

namespace MiraiShop.Application.Interfaces;
public interface ILinePayService
{
  //請款＆確款
  Task<LinePayRequestResponse> RequestPaymentAsync(LinePayRequest request);
  Task<LinePayConfirmResponse> ConfirmPaymentAsync(string transactionId, LinePayConfirmRequest request);
}