namespace MiraiShop.Domain.Exceptions;

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid id)
        : base($"找不到訂單，Id: {id}") { }
}
