# MiraiShop

MiraiShop 是一個以 **ASP.NET Core 8** 為後端、**Angular 17** 為前端的全端電商應用程式。  
後端採用 **Clean Architecture（整潔架構）** 設計，確保各層職責分明、易於測試與維護。

---

## 已完成功能

### 會員系統
- **會員註冊**：表單驗證、鹽值雜湊密碼儲存、二次密碼確認檢核
- **登入 / 登出**：JWT 驗證、登入後導向歡迎頁面、防止重複導回登入頁
- **地址管理**：主要地址 + 通訊地址，若未填通訊地址則自動帶入主要地址

### 商品管理
- **商品批次上傳**：解析 Excel / CSV 檔案並批次寫入資料庫
- **範例檔案下載**：提供標準格式範例檔供使用者參考
- **上傳狀態管理**：上傳失敗後按鈕狀態即時更新、編碼問題修正

### 資料庫
- **Entity 與 Migration**：Category、Order、Product 資料表建立與關聯設計
- **Category 欄位調整**：資料表欄位名稱修改並同步 Migration

### 測試
- **Auth 服務單元測試**：涵蓋 JwtSettings（含 AdminEmails）的建構子初始化情境

---

## 專案結構

```
MiraiShop.sln
├── MiraiShop.Domain/          ← 領域層（最核心，無任何外部依賴）
├── MiraiShop.Application/     ← 應用層（業務邏輯與流程編排）
├── MiraiShop.Infrastructure/  ← 基礎設施層（資料庫、外部服務）
├── MiraiShop.Server/          ← 展示層（API Controllers、程式進入點）
├── MiraiShop.Tests/           ← 單元測試
└── miraishop.client/          ← 前端（Angular 17）
```

---

## Clean Architecture 架構說明

Clean Architecture 的核心概念是：**依賴只能由外向內**，內層不知道外層的存在。

```
┌──────────────────────────────────────────┐
│              展示層 (Server)              │
│  ┌────────────────────────────────────┐  │
│  │       基礎設施層 (Infrastructure)   │  │
│  │  ┌──────────────────────────────┐  │  │
│  │  │      應用層 (Application)     │  │  │
│  │  │  ┌────────────────────────┐  │  │  │
│  │  │  │    領域層 (Domain)      │  │  │  │
│  │  │  │  Entities / Interfaces  │  │  │  │
│  │  │  └────────────────────────┘  │  │  │
│  │  └──────────────────────────────┘  │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘

依賴方向：Server → Application → Domain
                    ↑
          Infrastructure ────→ Domain
```

---

## 各層職責與放置規則

### 1. Domain 層（`MiraiShop.Domain`）

**這是整個系統的核心，不依賴任何其他層或 NuGet 套件。**

| 該放什麼 | 範例 |
|---|---|
| **Entity（實體）** — 業務核心資料模型 | `WeatherForecast`、`Product`、`Order` |
| **Domain Interface（領域介面）** — 定義倉儲契約 | `IWeatherForecastRepository`、`IProductRepository` |
| **Value Object（值物件）** — 無 ID 的不可變資料 | `Money`、`Address`、`Email` |
| **Domain Exception（領域例外）** | `ProductNotFoundException`、`InsufficientStockException` |
| **Domain Event（領域事件）** | `OrderPlacedEvent`、`StockReducedEvent` |

**不該放什麼：** 任何框架程式碼、資料庫存取、HttpClient、ASP.NET 類別。

---

### 2. Application 層（`MiraiShop.Application`）

**負責編排業務流程，只使用 Domain 提供的介面，不直接碰實作。**

| 該放什麼 | 範例 |
|---|---|
| **Service Interface（服務介面）** | `IWeatherForecastService`、`IOrderService` |
| **Service 實作** | `WeatherForecastService`、`OrderService` |
| **DTO（資料傳輸物件）** — API 輸入/輸出格式 | `WeatherForecastDto`、`CreateOrderRequest` |
| **Use Case / Command / Query** | `PlaceOrderCommand`、`GetProductListQuery` |
| **Validator（輸入驗證）** | 使用 FluentValidation 驗證 DTO |
| **Mapper（物件映射）** | Entity ↔ DTO 的轉換邏輯 |

**不該放什麼：** SQL 查詢、`DbContext`、`HttpClient`、Controller、任何框架實作細節。

---

### 3. Infrastructure 層（`MiraiShop.Infrastructure`）

**實作所有對外的技術細節：資料庫、快取、Email、第三方 API 等。**

| 該放什麼 | 範例 |
|---|---|
| **Repository 實作** — 實作 Domain 定義的介面 | `WeatherForecastRepository`、`ProductRepository` |
| **DbContext（Entity Framework）** | `MiraiShopDbContext` |
| **Migration（資料庫遷移）** | EF Core Migrations |
| **快取（Redis、MemoryCache）** | `CachedProductRepository` |
| **Email / SMS 服務** | `SmtpEmailService`、`TwilioSmsService` |
| **外部 API 客戶端** | `PaymentGatewayClient`、`ShippingApiClient` |
| **DI 擴充方法** | `InfrastructureServiceExtensions.AddInfrastructure()` |

**不該放什麼：** 業務邏輯判斷、Controller、DTO 定義。

---

### 4. Server 層（`MiraiShop.Server`）

**系統進入點，負責接收 HTTP 請求並將結果回傳給客戶端。**

| 該放什麼 | 範例 |
|---|---|
| **Controller** — 接收請求，呼叫 Application Service | `WeatherForecastController`、`ProductController` |
| **Program.cs** — DI 容器設定、Middleware 管線 | 服務註冊、Swagger、CORS |
| **Middleware** — 全域請求處理 | `ExceptionHandlingMiddleware`、`RequestLoggingMiddleware` |
| **Filter** | `ValidationFilter`、`AuthorizationFilter` |
| **設定檔** | `appsettings.json`、`appsettings.Development.json` |

**不該放什麼：** 業務邏輯、直接存取資料庫、SQL 查詢。Controller 只做三件事：接收請求 → 呼叫 Service → 回傳結果。

---

### 5. Tests 層（`MiraiShop.Tests`）

**針對 Application 層的 Service 進行單元測試，使用 Moq 隔離外部依賴。**

| 該放什麼 | 範例 |
|---|---|
| **Service 單元測試** | `WeatherForecastServiceTests` |
| **Domain 邏輯測試** | Value Object、Domain Exception 的行為測試 |
| **Mock 設定** | 使用 `Moq` mock Repository 介面 |

測試命名規則：`方法名稱_情境描述_預期結果`  
例如：`GetForecasts_EmptyRepository_ReturnsEmpty`

---

## 新增功能的標準流程

以新增「商品（Product）」功能為例：

```
1. Domain         → 建立 Product entity 與 IProductRepository 介面
2. Application    → 建立 IProductService 介面、ProductService 實作、ProductDto
3. Infrastructure → 建立 ProductRepository 實作（接 DB 或 API）
4. Server         → 建立 ProductController，注入 IProductService
5. Tests          → 為 ProductService 撰寫單元測試，mock IProductRepository
```

> **原則：** 永遠先定義介面，再實作。Controller 只能呼叫 Application 的介面，絕不直接呼叫 Repository 或 DbContext。

---

## 開發環境啟動

```bash
# 還原套件並建置整個 Solution
dotnet build

# 執行單元測試
dotnet test MiraiShop.Tests/MiraiShop.Tests.csproj

# 啟動後端（含 SPA Proxy，會自動啟動前端）
cd MiraiShop.Server
dotnet run
```

後端 API：`https://localhost:7140`  
Swagger UI：`https://localhost:7140/swagger`  
前端（Angular dev server）：`https://localhost:56501`

---

## 技術堆疊

| 類別 | 技術 |
|---|---|
| 後端框架 | ASP.NET Core 8 |
| 前端框架 | Angular 17 |
| 測試框架 | xUnit + Moq |
| API 文件 | Swagger / OpenAPI |
| 語言 | C# 12、TypeScript 5.4 |
| ORM | Entity Framework Core 8 |
| 資料庫 | Microsoft SQL Server |
| 認證 | JWT（JSON Web Token） |
