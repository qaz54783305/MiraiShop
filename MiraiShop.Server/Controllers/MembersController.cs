using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;

namespace MiraiShop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IAuthService _authService;

    public MembersController(IMemberService memberService, IAuthService authService)
    {
        _memberService = memberService;
        _authService = authService;
    }

    [HttpPost("register")]
    public ActionResult<MemberDto> Register([FromBody] RegisterMemberRequest request)
    {
        try
        {
            var result = _memberService.Register(request);
            return CreatedAtAction(nameof(Register), result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var result = _authService.Login(request);
        if (result is null)
            return Unauthorized(new { error = "電子信箱或密碼錯誤" });

        return Ok(result);
    }
}
