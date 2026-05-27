using System.Security.Cryptography;
using System.Text;
using Moq;
using MiraiShop.Application.DTOs;
using MiraiShop.Application.Services;
using MiraiShop.Domain.Entities;
using MiraiShop.Domain.Interfaces;

namespace MiraiShop.Tests;

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _repositoryMock;
    private readonly MemberService _service;

    private static readonly RegisterMemberRequest ValidRequest = new(
        Name: "王小明",
        Email: "test@example.com",
        Password: "MyP@ssword123",
        MailingAddress: "台北市信義區信義路五段7號",
        ResidentialAddress: "新北市板橋區文化路一段1號");

    public MemberServiceTests()
    {
        _repositoryMock = new Mock<IMemberRepository>();
        _repositoryMock.Setup(r => r.ExistsByEmail(It.IsAny<string>())).Returns(false);
        _repositoryMock
            .Setup(r => r.ChkResidentialAddress(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((residential, mailing) =>
                string.IsNullOrEmpty(residential) ? mailing : residential);
        _service = new MemberService(_repositoryMock.Object);
    }

    // --- US1: 正常註冊流程 ---

    [Fact]
    public void Register_ValidRequest_ReturnsMemberDto()
    {
        var result = _service.Register(ValidRequest);

        Assert.Equal(ValidRequest.Name, result.Name);
        Assert.Equal(ValidRequest.Email, result.Email);
        Assert.Equal(ValidRequest.MailingAddress, result.MailingAddress);
        Assert.Equal(ValidRequest.ResidentialAddress, result.ResidentialAddress);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public void Register_ValidRequest_PasswordIsHashed()
    {
        Member? savedMember = null;
        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Member>()))
            .Callback<Member>(m => savedMember = m);

        _service.Register(ValidRequest);

        Assert.NotNull(savedMember);
        Assert.NotEqual(ValidRequest.Password, savedMember!.PasswordHash);

        var expectedHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(ValidRequest.Password))).ToLowerInvariant();
        Assert.Equal(expectedHash, savedMember.PasswordHash);
    }

    [Fact]
    public void Register_ValidRequest_CallsRepositoryAdd()
    {
        _service.Register(ValidRequest);

        _repositoryMock.Verify(r => r.Add(It.IsAny<Member>()), Times.Once);
    }

    [Fact]
    public void Register_ValidRequest_MemberDtoDoesNotContainPassword()
    {
        var result = _service.Register(ValidRequest);
        var resultJson = System.Text.Json.JsonSerializer.Serialize(result);

        Assert.DoesNotContain(ValidRequest.Password, resultJson);
        Assert.DoesNotContain("passwordHash", resultJson, StringComparison.OrdinalIgnoreCase);
    }

    // --- US2: Email 唯一性驗證 ---

    [Fact]
    public void Register_DuplicateEmail_ThrowsInvalidOperationException()
    {
        _repositoryMock.Setup(r => r.ExistsByEmail(ValidRequest.Email)).Returns(true);

        Assert.Throws<InvalidOperationException>(() => _service.Register(ValidRequest));
    }

    [Fact]
    public void Register_DuplicateEmail_DoesNotCallRepositoryAdd()
    {
        _repositoryMock.Setup(r => r.ExistsByEmail(ValidRequest.Email)).Returns(true);

        try { _service.Register(ValidRequest); } catch { }

        _repositoryMock.Verify(r => r.Add(It.IsAny<Member>()), Times.Never);
    }
}
