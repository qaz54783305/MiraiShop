using MiraiShop.Domain.Entities;
using MiraiShop.Domain.Interfaces;
using MiraiShop.Infrastructure.Persistence;

namespace MiraiShop.Infrastructure.Repositories;

public class EfMemberRepository : IMemberRepository
{
    private readonly MiraiShopDbContext _context;

    public EfMemberRepository(MiraiShopDbContext context)
    {
        _context = context;
    }

    public void Add(Member member)
    {
        _context.Members.Add(member);
        _context.SaveChanges();
    }

    public bool ExistsByEmail(string email)
    {
        return _context.Members.Any(m => m.Email.ToLower() == email.ToLower());
    }

    public Member? GetByEmail(string email)
    {
        return _context.Members.FirstOrDefault(m => m.Email.ToLower() == email.ToLower());
    }
}
