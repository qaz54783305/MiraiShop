using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;
using MiraiShop.Domain.Interfaces;
using MiraiShop.Domain.Exceptions;
namespace MiraiShop.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    public OrderService(ILinePayService linePayService, IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(string orderId)
    {
        if (!Guid.TryParse(orderId, out var guidId))
            return null;

        try
        {
            var order = _orderRepository.GetById(guidId);

            return new OrderDto(
                Id: order.Id.ToString(),
                TotalAmount: (int)order.TotalAmount,
                Currency: order.Currency);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
    
}