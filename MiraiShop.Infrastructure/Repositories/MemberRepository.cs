using MiraiShop.Domain.Entities;
using MiraiShop.Domain.Interfaces;

namespace MiraiShop.Infrastructure.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly List<Member> _members = [];

    public Member? GetByEmail(string email) =>
        _members.FirstOrDefault(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public void Add(Member member) => _members.Add(member);

    public bool ExistsByEmail(string email) =>
        _members.Any(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    // 如果沒有填寫戶籍地址，則帶入通訊地址
    public string ChkResidentialAddress(string residentialAddress, string mailingAddress)
    {
        // 如果戶籍地址為空或 Null，就回傳通訊地址，否則回傳原本的戶籍地址
        return string.IsNullOrEmpty(residentialAddress) ? mailingAddress : residentialAddress;
    }
}
