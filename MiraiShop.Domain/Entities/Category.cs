namespace MiraiShop.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}
