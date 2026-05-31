using MiraiShop.Domain.Entities;

namespace MiraiShop.Domain.Interfaces;

public interface IOrderRepository
{
    //create
    void Add(Order order);
    //read
    Order GetById(Guid id);
    IList<Order> GetByMemberId(Guid memberId);
    //update
    void Update(Order order);
}