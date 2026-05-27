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

}
