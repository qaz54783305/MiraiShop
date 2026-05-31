namespace MiraiShop.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid CategoryId { get; set; }
    public string Status{ get; set; } = string.Empty;
    public decimal TotalAmount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } 
    public DateTime RefundedAt { get; set; }
}