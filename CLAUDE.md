# MiraiShop — AI 開發指引

這份文件是給 AI（Claude Code 等工具）閱讀的專案設定檔。  
每次開啟此專案時請遵守以下規則，不需要使用者重複說明。

---

## 技術堆疊

| 層 | 技術 |
|---|---|
| 後端框架 | ASP.NET Core 8、C# 12 |
| 前端框架 | Angular 17、TypeScript 5.4 |
| 測試框架 | xUnit + Moq |
| 程式碼品質 | SonarQube（GitHub Actions CI） |

---

## 專案架構：Clean Architecture

```
MiraiShop.Domain          ← 領域層（最內層）
MiraiShop.Application     ← 應用層
MiraiShop.Infrastructure  ← 基礎設施層
MiraiShop.Server          ← 展示層（API）
MiraiShop.Tests           ← 單元測試
miraishop.client          ← 前端（Angular）
```

**依賴方向（絕對不可逆）：**
```
Server → Application → Domain
Infrastructure ────────→ Domain
Tests  → Application, Infrastructure, Domain
```

---

## 各層放置規則

### Domain（`MiraiShop.Domain/`）
- `Entities/` — 業務實體（純 C# class，無任何 framework 依賴）
- `Interfaces/` — Repository 介面（`IXxxRepository`）
- `Exceptions/` — 領域例外（`XxxNotFoundException` 等）
- **禁止**：DbContext、HttpClient、ASP.NET 任何類別、NuGet 套件參考

### Application（`MiraiShop.Application/`）
- `Interfaces/` — Service 介面（`IXxxService`）
- `Services/` — Service 實作（實作同層的介面）
- `DTOs/` — 輸入/輸出資料結構（`record` 優先）
- **禁止**：DbContext、Repository 實作、Controller、SQL

### Infrastructure（`MiraiShop.Infrastructure/`）
- `Repositories/` — Repository 實作（實作 Domain 的介面）
- `Persistence/` — DbContext、EF Core 設定、Migrations
- `ExternalServices/` — 第三方 API 客戶端、Email、SMS
- **禁止**：業務邏輯判斷、Controller、DTO 定義

### Server（`MiraiShop.Server/`）
- `Controllers/` — Controller（只呼叫 Application Service，不含業務邏輯）
- `Middleware/` — 全域例外處理、Logging
- `Program.cs` — DI 註冊、Middleware 管線
- **禁止**：業務邏輯、直接呼叫 Repository、SQL

### Tests（`MiraiShop.Tests/`）
- 每個 Service 對應一個測試類別（`XxxServiceTests`）
- 使用 Moq mock 所有 Repository 介面
- 測試命名：`方法名稱_情境_預期結果`

---

## 命名規範

| 類型 | 命名規則 | 範例 |
|---|---|---|
| Entity | PascalCase 名詞 | `Product`、`Order` |
| Repository 介面 | `I` + Entity + `Repository` | `IProductRepository` |
| Service 介面 | `I` + 名詞 + `Service` | `IProductService` |
| Service 實作 | 名詞 + `Service` | `ProductService` |
| DTO | 名詞 + `Dto` / `Request` / `Response` | `ProductDto`、`CreateProductRequest` |
| Controller | 名詞複數 + `Controller` | `ProductsController` |
| 測試類別 | 被測類別 + `Tests` | `ProductServiceTests` |

---

## 實作新功能的標準順序

**必須按照以下順序**，不可跳層或跨層直接實作：

```
1. Domain     → Entity + IXxxRepository
2. Application → IXxxService + XxxService + Dto
3. Infrastructure → XxxRepository（實作 Domain 介面）
4. Server     → XxxController（注入 IXxxService）+ 更新 Program.cs DI 註冊
5. Tests      → XxxServiceTests（mock IXxxRepository）
```

---

## 程式碼風格

- DTO 優先使用 `record`（不可變）
- 不加無意義的註解；只在「為什麼這樣做」非顯而易見時加
- 不加空的 catch block 或無條件 try/catch
- Controller action 只做：接收 → 呼叫 Service → 回傳，不超過 10 行
- 不建立未被使用的介面或抽象

---

## 新增功能時請使用 `docs/feature-spec.md` 的範本

開發者會提供一份填好的 feature spec，AI 應依照 spec 內容與上方規則逐層實作。

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan:
specs/004-Login/plan.md
<!-- SPECKIT END -->
