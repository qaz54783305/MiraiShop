# Contract: EF Core Migration 操作

**Feature**: `002-mssql-persistence`
**Date**: 2026-05-27

---

## 說明

此功能的 API 合約（`POST /api/members/register`）**完全不變**，請參閱 `specs/001-member-registration/contracts/api.md`。

本文件定義 EF Core Migration 的操作合約（開發者操作規範），確保資料庫 schema 可重複、可版本化地建立。

---

## 前置條件

| 條件 | 說明 |
|------|------|
| MSSQL 已啟動 | SQL Server 服務在 localhost 運行 |
| 資料庫已建立 | 需手動建立 `MiraiShop` 資料庫（EF Migration 不建立資料庫本身） |
| .NET EF Tools 已安裝 | `dotnet tool install --global dotnet-ef` |

```sql
-- 建立資料庫（手動執行一次）
CREATE DATABASE MiraiShop;
```

---

## Migration 命令合約

### 產生 Migration

```bash
dotnet ef migrations add InitialCreate \
  --project MiraiShop.Infrastructure \
  --startup-project MiraiShop.Server \
  --output-dir Persistence/Migrations
```

| 參數 | 值 | 說明 |
|------|-----|------|
| `--project` | `MiraiShop.Infrastructure` | DbContext 所在專案 |
| `--startup-project` | `MiraiShop.Server` | 含 appsettings.json 的啟動專案 |
| `--output-dir` | `Persistence/Migrations` | Migration 檔案存放路徑（相對於 --project） |
| Migration 名稱 | `InitialCreate` | 第一個 Migration |

**預期產出**:
```text
MiraiShop.Infrastructure/Persistence/Migrations/
├── YYYYMMDDHHMMSS_InitialCreate.cs
└── MiraiShopDbContextModelSnapshot.cs
```

### 套用 Migration 至資料庫

```bash
dotnet ef database update \
  --project MiraiShop.Infrastructure \
  --startup-project MiraiShop.Server
```

**預期結果**: `Member` table 在 `MiraiShop` 資料庫中建立，包含 `IX_Member_Email` 唯一索引。

### 驗證 Migration 結果

```sql
-- 確認 table 存在
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Member';

-- 確認欄位結構
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Member';

-- 確認唯一索引
SELECT name, is_unique FROM sys.indexes WHERE object_id = OBJECT_ID('Member');
```

---

## 幂等性保證

| 場景 | 行為 |
|------|------|
| 全新資料庫 → `database update` | 建立 `Member` table 與索引 |
| 已存在 table → `database update` | 偵測到已是最新版本，無操作 |
| 已存在 table → 新增 Migration → `database update` | 套用差異變更（Incremental） |

---

## 回滾操作

```bash
# 回滾至上一個 Migration（開發環境）
dotnet ef database update 0 \
  --project MiraiShop.Infrastructure \
  --startup-project MiraiShop.Server
```

> ⚠️ 回滾會刪除 `Member` table 及所有資料，僅適用於開發環境。

---

## IMemberRepository 介面合約（不變）

此功能不修改 Application 層介面，`EfMemberRepository` 必須完整實作以下三個方法：

```csharp
public interface IMemberRepository
{
    Member? GetByEmail(string email);
    void Add(Member member);
    bool ExistsByEmail(string email);
}
```

### `Add(Member member)` 實作規格

| 規格 | 說明 |
|------|------|
| 寫入方式 | `context.Members.Add(member)` + `context.SaveChanges()` |
| Id | 由 `MemberService` 設定（`Guid.NewGuid()`），不由 DB 產生 |
| CreatedAt | 由 `MemberService` 設定（`DateTime.UtcNow`） |
| 唯一性衝突 | 若 DB 唯一索引違反，拋出 `DbUpdateException`（由 Controller 捕捉） |

### `ExistsByEmail(string email)` 實作規格

| 規格 | 說明 |
|------|------|
| 查詢方式 | `context.Members.Any(m => m.Email.ToLower() == email.ToLower())` |
| 大小寫 | 不區分大小寫（與記憶體實作行為一致） |
| 回傳 | `bool` |

### `GetByEmail(string email)` 實作規格

| 規格 | 說明 |
|------|------|
| 查詢方式 | `context.Members.FirstOrDefault(m => m.Email.ToLower() == email.ToLower())` |
| 找不到 | 回傳 `null` |
