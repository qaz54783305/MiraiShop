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
 
         var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            MailingAddress = request.MailingAddress,
            //沒有填寫第二個通訊地址則帶入主要通訊地址
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

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
    // 如果戶籍地址為空或 Null，就回傳通訊地址，否則回傳原本的戶籍地址
    public string ChkResidentialAddress(string ResidentialAddress, string mailingAddress)
    {
        return string.IsNullOrEmpty(ResidentialAddress) ? mailingAddress : ResidentialAddress;
    }
}
