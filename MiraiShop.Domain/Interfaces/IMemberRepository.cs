using MiraiShop.Domain.Entities;
namespace MiraiShop.Domain.Interfaces;

public interface IMemberRepository
{
    Member? GetByEmail(string email);
    void Add(Member member);
    bool ExistsByEmail(string email);

}
