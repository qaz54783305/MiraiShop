# Quickstart: MSSQL 本地環境設定

**Feature**: `002-mssql-persistence`
**Date**: 2026-05-27

本文說明如何在本機從零開始設定 MSSQL 持久化環境，並驗證功能正常運作。

---

## 前置需求

| 工具 | 版本 | 備註 |
|------|------|------|
| SQL Server | 2019 / 2022 / LocalDB | Windows 驗證模式 |
| .NET SDK | 8.x | 含 `dotnet ef` CLI |
| EF Core Tools | 8.x | `dotnet tool install --global dotnet-ef` |

### 安裝 EF Core CLI 工具

```bash
dotnet tool install --global dotnet-ef
dotnet ef --version   # 確認安裝成功
```

---

## Step 1：建立 MSSQL 資料庫

使用 SSMS 或 `sqlcmd` 建立資料庫（EF Migration 不會自動建立資料庫）：

```sql
-- 使用 sqlcmd 或 SSMS 執行
CREATE DATABASE MiraiShop;
```

```bash
# 或使用 sqlcmd
sqlcmd -S localhost -E -Q "CREATE DATABASE MiraiShop"
```

---

## Step 2：確認連線字串

確認 `MiraiShop.Server/appsettings.json` 包含正確連線字串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MiraiShop;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

---

## Step 3：執行 EF Core Migration

```bash
# 在 solution root 目錄執行
dotnet ef migrations add InitialCreate \
  --project MiraiShop.Infrastructure \
  --startup-project MiraiShop.Server \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project MiraiShop.Infrastructure \
  --startup-project MiraiShop.Server
```

**成功訊息**：
```
Done. To undo this action, use 'ef migrations remove'
...
Applying migration '..._InitialCreate'.
Done.
```

---

## Step 4：驗證 Table 建立

```sql
-- 查詢 Member table 是否存在
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_CATALOG = 'MiraiShop' AND TABLE_NAME = 'Member';

-- 查詢欄位結構
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Member'
ORDER BY ORDINAL_POSITION;
```

---

## Step 5：啟動 API 並測試

```bash
# 啟動後端
cd MiraiShop.Server
dotnet run
```

### 測試情境 1：成功註冊

```http
POST http://localhost:{port}/api/members/register
Content-Type: application/json

{
  "name": "測試使用者",
  "email": "test@example.com",
  "password": "securepass123",
  "mailingAddress": "台北市信義區松仁路100號",
  "residentialAddress": "台北市大安區忠孝東路四段100號"
}
```

**預期回應（201 Created）**：
```json
{
  "id": "...",
  "name": "測試使用者",
  "email": "test@example.com",
  "mailingAddress": "台北市信義區松仁路100號",
  "residentialAddress": "台北市大安區忠孝東路四段100號",
  "createdAt": "2026-05-27T..."
}
```

### 測試情境 2：驗證資料已寫入 DB

```sql
SELECT * FROM Member;
-- 應看到剛才註冊的會員，PasswordHash 為 SHA-256 hex 字串（64 字元）
```

### 測試情境 3：重啟服務後資料仍存在

```bash
# 停止服務後重新啟動
dotnet run

# 再次查詢 DB，確認資料仍存在
```

### 測試情境 4：重複 Email 回傳 409

```http
POST http://localhost:{port}/api/members/register
Content-Type: application/json

{
  "name": "另一位使用者",
  "email": "test@example.com",
  "password": "anotherpass",
  "mailingAddress": "...",
  "residentialAddress": "..."
}
```

**預期回應（409 Conflict）**：
```json
{
  "error": "Email already exists."
}
```

---

## Step 6：執行單元測試（確認未破壞現有邏輯）

```bash
dotnet test MiraiShop.Tests/MiraiShop.Tests.csproj
```

**預期**：10 個測試全數通過（`MemberServiceTests` 6 個 + `WeatherForecastServiceTests` 4 個）。

---

## 常見問題

| 問題 | 解決方式 |
|------|---------|
| `A network-related or instance-specific error` | 確認 SQL Server 服務已啟動，且允許 TCP/IP 連線 |
| `Login failed for user` | 確認使用 Windows 驗證（不需密碼），且目前 Windows 帳號有 SQL Server 存取權限 |
| `Cannot open database "MiraiShop"` | 需先手動建立 `MiraiShop` 資料庫（Step 1） |
| `SSL connection error` | 連線字串已包含 `TrustServerCertificate=True`，確認無誤 |
| `No migrations were found` | 確認 `--project` 參數指向 `MiraiShop.Infrastructure`（含 DbContext） |
