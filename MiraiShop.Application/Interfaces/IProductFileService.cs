using MiraiShop.Application.DTOs;

namespace MiraiShop.Application.Interfaces;

public interface IProductFileService
{
    byte[] GenerateProductTemplate();
    Task<UploadProductResponse> ImportProductsAsync(Stream fileStream);
}
