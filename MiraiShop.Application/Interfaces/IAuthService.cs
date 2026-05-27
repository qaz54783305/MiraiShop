using MiraiShop.Application.DTOs;

namespace MiraiShop.Application.Interfaces;

public interface IAuthService
{
    LoginResponse? Login(LoginRequest request);
}
