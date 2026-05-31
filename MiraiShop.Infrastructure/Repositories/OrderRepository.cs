using MiraiShop.Domain.Entities;
using MiraiShop.Domain.Exceptions;
using MiraiShop.Domain.Interfaces;
using MiraiShop.Infrastructure.Persistence;

namespace MiraiShop.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly MiraiShopDbContext _context;

    public OrderRepository(MiraiShopDbContext context)
    {
        _context = context;
    }

    public void Add(Order order)
    {
        _context.Orders.Add(order);
        _context.SaveChanges();
    }

    public Order GetById(Guid id)
    {
        return _context.Orders.Find(id)
               ?? throw new OrderNotFoundException(id);
    }

    public IList<Order> GetByMemberId(Guid memberId)
    {
        return _context.Orders
                       .Where(o => o.MemberId == memberId)
                       .ToList();
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
        _context.SaveChanges();
    }
}
