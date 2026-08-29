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

    public LinePayController(ILinePayService linePayService)
    {
        _linePayService = linePayService;
    }

    [HttpPost("request")]
    [Authorize]
    public async Task<ActionResult<LinePayRequestResponse>> RequestPayment(
        [FromBody] LinePayRequestRequest request)
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
    public async Task<IActionResult> Confirm(
        [FromQuery] string transactionId,
        [FromQuery] string orderId)
    {
        if (string.IsNullOrWhiteSpace(transactionId) || string.IsNullOrWhiteSpace(orderId))
            return BadRequest(new { error = "缺少 transactionId 或 orderId" });

        try
        {
            var result = await _linePayService.ConfirmPaymentAsync(transactionId, orderId);
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