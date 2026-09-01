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

    public async Task<Order> GetByIdAsync(Guid id)
    {
        return await _context.Orders.FindAsync(id)
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
