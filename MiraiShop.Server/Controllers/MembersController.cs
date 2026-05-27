using Microsoft.AspNetCore.Mvc;
using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;
using System.Diagnostics;

namespace MiraiShop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _service;

    public MembersController(IMemberService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public ActionResult<MemberDto> Register([FromBody] RegisterMemberRequest request)
    {
        try
        {
            var result = _service.Register(request);
            Debug.WriteLine("result===" + result);
            return CreatedAtAction(nameof(Register), result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
