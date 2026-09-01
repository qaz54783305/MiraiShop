
using MiraiShop.Application.DTOs;

namespace MiraiShop.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(string id);
}