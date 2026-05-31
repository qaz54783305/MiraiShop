namespace MiraiShop.Domain.Exceptions;

public class CategoryNotFoundException : Exception
{
    public CategoryNotFoundException(string categoryCode)
        : base($"找不到分類，CategoryCode: {categoryCode}") { }
}
