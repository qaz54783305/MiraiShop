using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;

namespace MiraiShop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LinePayController : ControllerBase
{
    private readonly ILinePayService _linePayService;
    private readonly IOrderService _orderService;

    public LinePayController(ILinePayService linePayService, IOrderService orderService)
    {
        _linePayService = linePayService;
        _orderService = orderService;
    }

    [HttpPost("request")]
    [Authorize]
    public async Task<ActionResult<LinePayRequestResponse>> RequestPayment( [FromBody] LinePayRequest request)
    {
        try
        {
            var result = await _linePayService.RequestPaymentAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("confirm")]
    public async Task<IActionResult> Confirm([FromQuery] string transactionId, [FromQuery] string orderId)
    {
        if (string.IsNullOrWhiteSpace(transactionId) || string.IsNullOrWhiteSpace(orderId))
            return BadRequest(new { error = "缺少 transactionId或orderId " });
        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return BadRequest(new { error = "找不到訂單" });

            var confirmRequest = new LinePayConfirmRequest(
                Amount: order.TotalAmount,
                Currency: order.Currency);
            var result = await _linePayService.ConfirmPaymentAsync(transactionId, confirmRequest);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("cancel")]
    public IActionResult Cancel([FromQuery] string? orderId)
    {
        return Ok(new { message = "已取消付款", orderId });
    }
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        return StatusCode(StatusCodes.Status200OK, new
        {
            statusCode = 200,
            message = "LinePay API 已接通"
        });
    }
}