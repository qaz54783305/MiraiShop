using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;

namespace MiraiShop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class FileController : ControllerBase
{
    private readonly IProductFileService _productFileService;

    public FileController(IProductFileService productFileService)
    {
        _productFileService = productFileService;
    }

    [HttpGet("products/template")]
    public IActionResult DownloadProductTemplate()
    {
        var bytes = _productFileService.GenerateProductTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "product_template.xlsx");
    }

    [HttpPost("products/upload")]
    public async Task<ActionResult<UploadProductResponse>> UploadProducts(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "請上傳有效的 Excel 檔案" });

        using var stream = file.OpenReadStream();
        var result = await _productFileService.ImportProductsAsync(stream);
        return Ok(result);
    }
}
