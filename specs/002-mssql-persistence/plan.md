# Implementation Plan: MSSQL 會員資料持久化

**Branch**: `002-mssql-persistence` | **Date**: 2026-05-27 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-mssql-persistence/spec.md`

---

## Summary

將現有的記憶體 `MemberRepository` 替換為 Entity Framework Core + MSSQL 的真實持久化實作。後端以 Clean Architecture 分層：僅修改 Infrastructure 層（新增 `MiraiShopDbContext`、`EfMemberRepository`）與 Server 層（DI 替換 + appsettings.json 連線字串）；Domain 層與 Application 層的介面及邏輯完全不變，確保單元測試仍全數通過。

---

## Technical Context

**Language/Version**: C# 12 / .NET 8

**Primary Dependencies**:
- `Microsoft.EntityFrameworkCore.SqlServer` 8.x（EF Core SQL Server Provider）
- `Microsoft.EntityFrameworkCore.Tools` 8.x（`dotnet ef` CLI 工具，Migration）
- `Microsoft.EntityFrameworkCore.Design` 8.x（設計時工具）

**Storage**: MSSQL（SQL Server），Windows 驗證，localhost，資料庫名 `MiraiShop`

**Connection String**: `Server=localhost;Database=MiraiShop;Trusted_Connection=True;TrustServerCertificate=True`

**Testing**: xUnit + Moq（Application 層單元測試維持不變；不新增 EF Core Integration Test）

**Target Platform**: Windows 開發環境（Windows 驗證）

**Project Type**: Clean Architecture Web Service（Infrastructure 層替換）

**Performance Goals**: 與現有 API 回應時間一致（< 2 秒）

**Constraints**:
- Domain 層與 Application 層禁止修改（介面不變，Clean Architecture 邊界）
- `Member` table 名稱由使用者指定（不使用 EF Core 複數化預設）
- `Id` 欄位型別為 `Guid`（`UNIQUEIDENTIFIER`），由應用程式產生
- `CreatedAt` 儲存 UTC 時間，由應用程式設定（非 DB 預設值）
- 不實作 Audit Log、Soft Delete 或多租戶

**Scale/Scope**: 單一資料庫、單一 Schema，適用初期電商會員規模

---

## Constitution Check

> 以 CLAUDE.md 架構規範作為品質閘門

| 閘門 | 狀態 | 說明 |
|---|---|---|
| Domain 不依賴外層 | ✅ PASS | `Member.cs` 不修改，無 EF 屬性標記（使用 Fluent API） |
| Application 只使用介面 | ✅ PASS | `MemberService` 只注入 `IMemberRepository`，不變 |
| Infrastructure 實作 Domain 介面 | ✅ PASS | `EfMemberRepository` 實作 `IMemberRepository` |
| DbContext 置於 Infrastructure | ✅ PASS | `MiraiShopDbContext` 在 `Infrastructure/Persistence/` |
| Controller 只呼叫 Application Service | ✅ PASS | `MembersController` 不修改 |
| 單元測試不依賴 DbContext | ✅ PASS | `MemberServiceTests` mock `IMemberRepository`，不觸及 EF |

---

## Project Structure

### Documentation (this feature)

```text
specs/002-mssql-persistence/
├── plan.md              ← 此檔案
├── research.md          ← Phase 0 輸出
├── data-model.md        ← Phase 1 輸出
├── contracts/
│   └── migration.md     ← Phase 1 輸出（Migration 操作說明）
├── quickstart.md        ← Phase 1 輸出（本地環境設定步驟）
└── tasks.md             ← /speckit-tasks 輸出
```

### Source Code（僅修改 Infrastructure + Server）

```text
MiraiShop.Infrastructure/
├── MiraiShop.Infrastructure.csproj    ← 新增 EF Core NuGet 套件參考
├── Persistence/
│   └── MiraiShopDbContext.cs          ← 新增
└── Repositories/
    └── EfMemberRepository.cs          ← 新增（替換記憶體 MemberRepository）

MiraiShop.Server/
├── appsettings.json                   ← 新增 ConnectionStrings 區段
└── Program.cs                         ← 更新 DI 註冊

（以下各層不修改）
MiraiShop.Domain/          ← 不變
MiraiShop.Application/     ← 不變
MiraiShop.Tests/           ← 不變（10 個單元測試繼續通過）
miraishop.client/          ← 不變
```

**Structure Decision**: 嚴格遵循 Clean Architecture，僅在 Infrastructure 層新增 EF Core 實作，替換記憶體假資料，不破壞任何現有介面或上層邏輯。

---

## Complexity Tracking

> 無 Constitution 違規，此區塊記錄重要架構決策

| 決策 | 選擇 | 理由 |
|------|------|------|
| Entity 標記方式 | Fluent API（非 Data Annotations） | 保持 Domain Entity 純 C#，不引入 EF 依賴至 Domain 層 |
| `Id` 產生策略 | 應用程式產生 `Guid.NewGuid()`（已在 MemberService） | Domain/Application 層控制，一致性高 |
| Email 唯一索引 | DB 層 Fluent API `HasIndex().IsUnique()` | 雙重保護：Application 邏輯層 + DB 約束 |
| Migration 目錄 | `Infrastructure/Persistence/Migrations/` | Infrastructure 負責持久化細節 |
| 新 Repository 命名 | `EfMemberRepository`（非覆蓋舊 `MemberRepository`） | 明確區分，便於 Code Review；舊檔可移除或留存 |
