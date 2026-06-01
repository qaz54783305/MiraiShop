using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;
using MiraiShop.Domain.Interfaces;

namespace MiraiShop.Application.Services;

public class AuthService : IAuthService
{
    private readonly IMemberRepository _repository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IMemberRepository repository, JwtSettings jwtSettings)
    {
        _repository = repository;
        _jwtSettings = jwtSettings;
    }

    public LoginResponse? Login(LoginRequest request)
    {
        var member = _repository.GetByEmail(request.Email);
        if (member is null)
            return null;

        var inputHash = MemberService.HashPassword(request.Password, member.PasswordSalt);
        if (inputHash != member.PasswordHash)
            return null;

        var expiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
        var isAdmin = _jwtSettings.AdminEmails.Contains(member.Email, StringComparer.OrdinalIgnoreCase);
        var token = GenerateToken(member.Id, member.Email, member.Name, isAdmin, expiry);

        return new LoginResponse(token, expiry, member.Id);
    }

    private string GenerateToken(Guid memberId, string email, string name, bool isAdmin, DateTime expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, memberId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name, name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
