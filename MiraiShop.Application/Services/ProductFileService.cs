using System.Drawing;
using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;
using MiraiShop.Domain.Entities;
using MiraiShop.Domain.Exceptions;
using MiraiShop.Domain.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace MiraiShop.Application.Services;

public class ProductFileService : IProductFileService
{
    private readonly IProductRepository _productRepository;

    // 欄位定義：(欄位名稱, 說明, 範例值, 是否必填)
    private static readonly (string Header, string Description, object Example, bool Required)[] Columns =
    [
        ("CategoryCode", "分類代碼（需與系統中既有的 Category 相符）", "ELEC", true),
        ("Name",         "商品名稱",                                   "藍牙耳機",    true),
        ("Price",        "售價（大於等於 0 的數字）",                  299.00m,       true),
        ("Stock",        "庫存數量（大於等於 0 的整數）",              50,            true),
    ];

    public ProductFileService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
        ExcelPackage.License.SetNonCommercialOrganization("MiraiShop");
    }

    public byte[] GenerateProductTemplate()
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("商品匯入");

        // ── 說明列（第 1 列）──────────────────────────────────────
        sheet.Cells[1, 1].Value = "【商品批次匯入範本】請從第 3 列開始填寫資料，勿修改欄位標題列（第 2 列）";
        sheet.Cells[1, 1, 1, Columns.Length].Merge = true;
        StyleInstruction(sheet.Cells[1, 1]);

        // ── 欄位標題列（第 2 列）─────────────────────────────────
        for (int i = 0; i < Columns.Length; i++)
        {
            var cell = sheet.Cells[2, i + 1];
            var (header, _, _, required) = Columns[i];
            cell.Value = required ? $"{header} *" : header;
            StyleHeader(cell);

            // 說明 tooltip（Comment）
            sheet.Cells[2, i + 1].AddComment(Columns[i].Description, "MiraiShop");
        }

        // ── 範例資料列（第 3 列）─────────────────────────────────
        for (int i = 0; i < Columns.Length; i++)
        {
            var cell = sheet.Cells[3, i + 1];
            cell.Value = Columns[i].Example;
            StyleExample(cell);
        }
        // Price 欄位格式
        sheet.Cells[3, 3].Style.Numberformat.Format = "#,##0.00";

        // ── 欄寬設定 ─────────────────────────────────────────────
        sheet.Column(1).Width = 22; // CategoryCode
        sheet.Column(2).Width = 28; // Name
        sheet.Column(3).Width = 18; // Price
        sheet.Column(4).Width = 18; // Stock

        sheet.Row(1).Height = 22;
        sheet.Row(2).Height = 20;

        return package.GetAsByteArray();
    }

    private static void StyleInstruction(ExcelRange cell)
    {
        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0x1F, 0x49, 0x7D)); // 深藍
        cell.Style.Font.Color.SetColor(Color.White);
        cell.Style.Font.Bold = true;
        cell.Style.Font.Size = 11;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    }

    private static void StyleHeader(ExcelRange cell)
    {
        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0x2E, 0x75, 0xB6)); // 藍
        cell.Style.Font.Color.SetColor(Color.White);
        cell.Style.Font.Bold = true;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
        cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(0x1F, 0x49, 0x7D));
    }

    private static void StyleExample(ExcelRange cell)
    {
        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0xDD, 0xEB, 0xF7)); // 淡藍
        cell.Style.Font.Italic = true;
        cell.Style.Font.Color.SetColor(Color.FromArgb(0x40, 0x40, 0x40));
    }

    public async Task<UploadProductResponse> ImportProductsAsync(Stream fileStream)
    {
        using var package = new ExcelPackage(fileStream);
        var sheet = package.Workbook.Worksheets[0];

        var products = new List<Product>();
        var errors = new List<string>();

        if (sheet.Dimension is null)
            return new UploadProductResponse(0, 0, errors);

        // 第 1 列：說明、第 2 列：標題，資料從第 3 列開始
        for (int row = 3; row <= sheet.Dimension.End.Row; row++)
        {
            var categoryCode = sheet.Cells[row, 1].Text.Trim();
            var name = sheet.Cells[row, 2].Text.Trim();

            // 跳過完全空白列或提示文字列（含 ← 的列）
            if (string.IsNullOrEmpty(categoryCode) && string.IsNullOrEmpty(name))
                continue;
            if (categoryCode.StartsWith("←"))
                continue;

            if (string.IsNullOrEmpty(categoryCode) || string.IsNullOrEmpty(name))
            {
                errors.Add($"第 {row} 行：CategoryCode 與 Name 為必填");
                continue;
            }

            // 用 GetValue<T> 直接取數值，避免因 Excel 地區格式造成 Text 解析失敗
            var priceRaw = sheet.Cells[row, 3].GetValue<decimal?>();
            var stockRaw = sheet.Cells[row, 4].GetValue<int?>();

            if (priceRaw is null || priceRaw < 0)
            {
                errors.Add($"第 {row} 行：Price 格式錯誤，請填入大於等於 0 的數字");
                continue;
            }

            if (stockRaw is null || stockRaw < 0)
            {
                errors.Add($"第 {row} 行：Stock 格式錯誤，請填入大於等於 0 的整數");
                continue;
            }

            products.Add(new Product
            {
                Id = Guid.NewGuid(),
                CategoryCode = categoryCode,
                Name = name,
                Price = priceRaw.Value,
                Stock = stockRaw.Value,
                CreatedAt = DateTime.Now
            });
        }

        if (products.Count == 0)
            return new UploadProductResponse(0, errors.Count, errors);

        try
        {
            await _productRepository.AddAsync(products);
        }
        catch (CategoryNotFoundException ex)
        {
            errors.Add(ex.Message);
            return new UploadProductResponse(0, errors.Count, errors);
        }

        return new UploadProductResponse(products.Count, errors.Count, errors);
    }
}
