namespace MiraiShop.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0;
    public int Stock { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // 匯入時填寫，用來關聯 Category 取得 CategoryId，不入庫
    public string CategoryCode { get; set; } = string.Empty;
}