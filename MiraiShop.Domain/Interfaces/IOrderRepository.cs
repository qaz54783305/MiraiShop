using MiraiShop.Domain.Entities;

namespace MiraiShop.Domain.Interfaces;

public interface IOrderRepository
{
    //create
    void Add(Order order);
    //read
    Task<Order> GetByIdAsync(Guid id);
    IList<Order> GetByMemberId(Guid memberId);
    //update
    void Update(Order order);
}