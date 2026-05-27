using System.Security.Cryptography;
using System.Text;
using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;
using MiraiShop.Domain.Entities;
using MiraiShop.Domain.Interfaces;

namespace MiraiShop.Application.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _repository;

    public MemberService(IMemberRepository repository)
    {
        _repository = repository;
    }

    public MemberDto Register(RegisterMemberRequest request)
    {
        if (_repository.ExistsByEmail(request.Email))
            throw new InvalidOperationException($"此電子信箱已被註冊：{request.Email}");

        var salt = Guid.NewGuid().ToString("N");
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password, salt),
            PasswordSalt = salt,
            MailingAddress = request.MailingAddress,
            ResidentialAddress = ChkResidentialAddress(request.ResidentialAddress, request.MailingAddress),
            CreatedAt = DateTime.Now
        };

        _repository.Add(member);

        return new MemberDto(
            member.Id,
            member.Name,
            member.Email,
            member.MailingAddress,
            member.ResidentialAddress,
            member.CreatedAt);
    }

    public static string HashPassword(string password, string? salt = null)
    {
        var input = salt is null ? password : salt + password;
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public string ChkResidentialAddress(string ResidentialAddress, string mailingAddress)
    {
        return string.IsNullOrEmpty(ResidentialAddress) ? mailingAddress : ResidentialAddress;
    }
}
