using MiraiShop.Application.DTOs;

namespace MiraiShop.Application.Interfaces;

public interface IMemberService
{
    MemberDto Register(RegisterMemberRequest request);
}
