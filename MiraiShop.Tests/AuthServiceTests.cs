using System.IdentityModel.Tokens.Jwt;
using Moq;
using MiraiShop.Application.DTOs;
using MiraiShop.Application.Services;
using MiraiShop.Domain.Entities;
using MiraiShop.Domain.Interfaces;

namespace MiraiShop.Tests;

public class AuthServiceTests
{
    private readonly Mock<IMemberRepository> _repositoryMock;
    private readonly AuthService _service;
    private readonly JwtSettings _jwtSettings;

    private const string TestEmail = "test@example.com";
    private const string TestPassword = "MyP@ssword123";
    private const string TestSalt = "testsalt123";

  
    private Member CreateMember(string password, string? salt = TestSalt) => new()
    {
        Id = Guid.NewGuid(),
        Email = TestEmail,
        Name = "測試用戶",
        PasswordSalt = salt,
        PasswordHash = MemberService.HashPassword(password, salt),
        MailingAddress = "台北市",
        ResidentialAddress = "台北市",
        CreatedAt = DateTime.Now
    };

    // --- 正常登入 ---

    [Fact]
    public void Login_ValidCredentials_ReturnsLoginResponse()
    {
        _repositoryMock.Setup(r => r.GetByEmail(TestEmail)).Returns(CreateMember(TestPassword));

        var result = _service.Login(new LoginRequest(TestEmail, TestPassword));

        Assert.NotNull(result);
        Assert.NotEmpty(result!.Token);
        Assert.True(result.Expiry > DateTime.UtcNow);
        Assert.NotEqual(Guid.Empty, result.MemberId);
    }

    [Fact]
    public void Login_ValidCredentials_TokenContainsCorrectClaims()
    {
        var member = CreateMember(TestPassword);
        _repositoryMock.Setup(r => r.GetByEmail(TestEmail)).Returns(member);

        var result = _service.Login(new LoginRequest(TestEmail, TestPassword));

        var handler = new JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(result!.Token);

        Assert.Equal(member.Id.ToString(), parsed.Subject);
        Assert.Equal(TestEmail, parsed.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }

    // --- 錯誤密碼 ---

    [Fact]
    public void Login_WrongPassword_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByEmail(TestEmail)).Returns(CreateMember(TestPassword));

        var result = _service.Login(new LoginRequest(TestEmail, "WrongPassword"));

        Assert.Null(result);
    }

    // --- 未知 Email ---

    [Fact]
    public void Login_UnknownEmail_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByEmail(It.IsAny<string>())).Returns((Member?)null);

        var result = _service.Login(new LoginRequest("unknown@example.com", TestPassword));

        Assert.Null(result);
    }

    // --- 向後相容：無鹽舊帳號 ---

    [Fact]
    public void Login_LegacyMemberWithoutSalt_ValidatesWithUnsaltedHash()
    {
        var legacyMember = CreateMember(TestPassword, salt: null);
        _repositoryMock.Setup(r => r.GetByEmail(TestEmail)).Returns(legacyMember);

        var result = _service.Login(new LoginRequest(TestEmail, TestPassword));

        Assert.NotNull(result);
    }

    [Fact]
    public void Login_LegacyMemberWithoutSalt_WrongPassword_ReturnsNull()
    {
        var legacyMember = CreateMember(TestPassword, salt: null);
        _repositoryMock.Setup(r => r.GetByEmail(TestEmail)).Returns(legacyMember);

        var result = _service.Login(new LoginRequest(TestEmail, "wrong"));

        Assert.Null(result);
    }
}
