# EF Core Migration 操作備忘

本專案使用 EF Core（SQL Server），DbContext 位於 `MiraiShop.Infrastructure`，啟動專案為 `MiraiShop.Server`。

## 觀念
- Migration **不會自動產生**，一律由 `dotnet ef migrations add` 觸發。
- EF 會比對「目前模型（DbContext + 實體）」與「`MiraiShopDbContextModelSnapshot.cs`」的差異，自動算出 `Up()`/`Down()`。
- 產生(add) 與 套用(update) 是兩件事，分開執行。
- 不要手寫 snapshot，避免模型與快照不同步。

## 一次性準備
```bash
dotnet --version
dotnet tool install --global dotnet-ef   # 已裝可改 update
```
啟動或目標專案需有 `Microsoft.EntityFrameworkCore.Design` 套件。

## 標準流程
```bash
# 1. 先確認可 build（add 會載入編譯後的 DbContext）
dotnet build

# 2. 產生 migration
dotnet ef migrations add <MigrationName> \
  --project MiraiShop.Infrastructure \
  --startup-project MiraiShop.Server

# 3. 檢查產出的 *_<MigrationName>.cs 內容是否正確

# 4. 套用到資料庫（DefaultConnection）
dotnet ef database update \
  --project MiraiShop.Infrastructure \
  --startup-project MiraiShop.Server
```

## 常用輔助
```bash
# 列出所有 migration
dotnet ef migrations list --project MiraiShop.Infrastructure --startup-project MiraiShop.Server

# 移除「最後一個未套用」的 migration
dotnet ef migrations remove --project MiraiShop.Infrastructure --startup-project MiraiShop.Server

# 確認模型與快照是否一致（理想：No changes）
dotnet ef migrations has-pending-model-changes --project MiraiShop.Infrastructure --startup-project MiraiShop.Server

# 退回到指定 migration（含還原 DB）
dotnet ef database update <TargetMigrationName> --project MiraiShop.Infrastructure --startup-project MiraiShop.Server
```

## 提醒
- 報錯找不到連線字串／DbContext，通常是 `--startup-project` 沒指對。
- 每次改了實體或 `OnModelCreating` 後，重複「產生 → 檢查 → 套用」，名稱用本次變更描述。

## GUI 操作（JetBrains Rider）
Rider 沒有內建 EF migration UI，可安裝外掛：
- Settings → Plugins → Marketplace 搜尋 **「Entity Framework Core UI」**（作者 seclerp）安裝後重啟。
- 於方案總管對 `MiraiShop.Infrastructure` 按右鍵 → **EF Core** → `Add Migration` / `Update Database`，對話框中選好 startup project 即可，等同上述 CLI。

> Visual Studio（Windows）則可用 Package Manager Console：`Add-Migration <Name>`、`Update-Database`。VS for Mac 已停止支援，Mac 上建議用 Rider 外掛或 CLI。
