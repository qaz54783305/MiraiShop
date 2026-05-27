# Research: MSSQL 會員資料持久化

**Feature**: `002-mssql-persistence`
**Date**: 2026-05-27

---

## Decision 1: ORM 選擇

**Decision**: 使用 Entity Framework Core 8（`Microsoft.EntityFrameworkCore.SqlServer`）

**Rationale**:
- 與 .NET 8 / ASP.NET Core 8 深度整合，無需額外配置
- Code-First Migration 可自動管理 schema 版本，無需手動 SQL 腳本
- Fluent API 允許在 Infrastructure 層定義映射規則，保持 Domain Entity 純 C#
- 官方支援 MSSQL，Windows 驗證支援完整

**Alternatives considered**:
- **Dapper**：需手動撰寫 SQL，Migration 管理需額外工具（FluentMigrator），適合複雜查詢場景；此專案初期 CRUD 需求使 EF Core 更合適
- **ADO.NET**：最低層，完全手動，開發效率低，不符合此專案規模需求

---

## Decision 2: Entity 設定方式（Fluent API vs Data Annotations）

**Decision**: 使用 Fluent API，在 `MiraiShopDbContext.OnModelCreating` 中設定

**Rationale**:
- CLAUDE.md 架構規範：Domain 層禁止引入任何 framework 依賴
- Data Annotations（如 `[Key]`、`[MaxLength]`）需在 Domain Entity 加入 EF Core attribute，違反 Clean Architecture
- Fluent API 集中於 Infrastructure 層，符合「持久化細節屬於 Infrastructure」原則

**Alternatives considered**:
- Data Annotations：快速但污染 Domain Entity，違反架構規範，不採用

---

## Decision 3: 連線字串管理

**Decision**: 連線字串儲存於 `MiraiShop.Server/appsettings.json` 的 `ConnectionStrings` 區段

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MiraiShop;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Rationale**:
- .NET 標準慣例，與 `builder.Configuration.GetConnectionString("DefaultConnection")` 無縫整合
- 開發環境使用 Windows 驗證（`Trusted_Connection=True`），不需管理密碼
- `TrustServerCertificate=True` 避免本地開發時的 SSL 憑證驗證錯誤
- 正式環境可透過環境變數覆蓋（`ConnectionStrings__DefaultConnection`），不需修改程式碼

**Alternatives considered**:
- 環境變數（.env）：適合正式環境，但本地開發設定繁瑣；可後續搭配 User Secrets 使用
- `appsettings.Development.json`：可行，但開發/共用配置分散；保持統一於 `appsettings.json` 較清晰

**Security Note**: `appsettings.json` 中的連線字串使用 Windows 驗證（無明文密碼），風險可接受；正式環境應使用 SQL 驗證搭配 Secret Manager 或 Azure Key Vault

---

## Decision 4: Email 唯一性保證策略

**Decision**: 雙重保護 — Application 層邏輯檢查（現有）+ 資料庫唯一索引

**Rationale**:
- 現有 `MemberService.Register` 已呼叫 `IMemberRepository.ExistsByEmail` 做應用層檢查（回傳 409 + 友好錯誤訊息）
- 資料庫唯一索引作為最後防線（防止 Race Condition、多實例部署時的並發問題）
- 兩層保護各有職責，互補而非重複

**Fluent API 設定**:
```csharp
modelBuilder.Entity<Member>()
    .HasIndex(m => m.Email)
    .IsUnique();
```

**Alternatives considered**:
- 僅應用層檢查：Race Condition 下可能寫入重複 Email（高並發場景）
- 僅資料庫唯一索引：EF Core 拋出 `DbUpdateException`，需在 Infrastructure/Controller 捕捉並轉換為友好訊息，邏輯分散

---

## Decision 5: Guid 主鍵產生策略

**Decision**: 由應用程式（`MemberService`）產生 `Guid.NewGuid()`，EF Core 設定 `ValueGeneratedNever()`

**Rationale**:
- `MemberService.Register` 已設定 `Id = Guid.NewGuid()`（現有程式碼）
- 由應用程式控制 Id 生成，方便單元測試驗證（可預測或 mock）
- 避免 MSSQL 的 NEWID() 產生非循序 GUID 導致索引碎片化（若使用 `ValueGeneratedOnAdd`，EF Core 仍會呼叫 MSSQL NEWID()）

**EF Core 設定**:
```csharp
modelBuilder.Entity<Member>()
    .Property(m => m.Id)
    .ValueGeneratedNever();
```

**Alternatives considered**:
- `ValueGeneratedOnAdd()`（MSSQL NEWID()）：DB 自動產生，但測試不易驗證且有索引碎片問題
- `NEWSEQUENTIALID()`：循序 GUID，減少碎片，但需 MSSQL 特定設定

---

## Decision 6: Migration 位置與執行方式

**Decision**: Migration 檔案儲存於 `MiraiShop.Infrastructure/Persistence/Migrations/`，使用 `dotnet ef` CLI 產生

**Rationale**:
- Migration 屬於持久化細節，應在 Infrastructure 層管理
- `dotnet ef migrations add` 需指定 `--project MiraiShop.Infrastructure --startup-project MiraiShop.Server`
- 初始 Migration 命名：`InitialCreate`

**Commands**:
```bash
# 產生 Migration
dotnet ef migrations add InitialCreate \
  --project MiraiShop.Infrastructure \
  --startup-project MiraiShop.Server \
  --output-dir Persistence/Migrations

# 套用至資料庫
dotnet ef database update \
  --project MiraiShop.Infrastructure \
  --startup-project MiraiShop.Server
```

**Alternatives considered**:
- SQL Script（手動）：不可重複執行、無版本管理，不採用
- `EnsureCreated()`：開發快速但不支援 Migration，正式環境不適用

---

## Decision 7: DbContext 生命週期與 DI 註冊

**Decision**: `AddDbContext<MiraiShopDbContext>` 預設 Scoped 生命週期

**Rationale**:
- `AddDbContext` 預設 Scoped（每個 HTTP 請求一個 DbContext 實例），符合 Web API 使用模式
- `EfMemberRepository` 注入 `MiraiShopDbContext`，需相同生命週期（Scoped）
- 與現有 `AddScoped<IMemberRepository, EfMemberRepository>` 一致

**Program.cs DI 設定**:
```csharp
builder.Services.AddDbContext<MiraiShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IMemberRepository, EfMemberRepository>();
```
