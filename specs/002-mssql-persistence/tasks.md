# Tasks: MSSQL 會員資料持久化

**Input**: Design documents from `/specs/002-mssql-persistence/`

**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅ | contracts/migration.md ✅ | quickstart.md ✅

---

## Phase 1: Setup（NuGet 套件與工具）

**Purpose**: 在 Infrastructure 專案加入 EF Core 相關 NuGet 套件，確認 CLI 工具就緒

- [x] T001 在 `MiraiShop.Infrastructure/MiraiShop.Infrastructure.csproj` 加入以下 NuGet 套件參考（執行 `dotnet add MiraiShop.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 8.*`、`dotnet add MiraiShop.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 8.*`、`dotnet add MiraiShop.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 8.*`）
- [x] T002 確認 `dotnet-ef` CLI 工具已安裝（執行 `dotnet ef --version`；若未安裝執行 `dotnet tool install --global dotnet-ef`）

---

## Phase 2: Foundational（共用基礎結構，所有 User Story 依賴）

**Purpose**: 建立 DbContext 與連線字串設定，是 US1 與 US2 的共同前置需求

⚠️ **CRITICAL**: US1 實作依賴此 Phase 完成

- [x] T003 [P] 建立 `MiraiShop.Infrastructure/Persistence/MiraiShopDbContext.cs`：繼承 `DbContext`，建構式注入 `DbContextOptions<MiraiShopDbContext>`；定義 `DbSet<Member> Members => Set<Member>()`；在 `OnModelCreating` 以 Fluent API 設定 Member 映射（`ToTable("Member")`、`HasKey(m => m.Id)`、`ValueGeneratedNever()`、`Email HasMaxLength(256) + HasIndex().IsUnique()`、所有欄位 `IsRequired()`）；using `MiraiShop.Domain.Entities`
- [x] T004 [P] 在 `MiraiShop.Server/appsettings.json` 的頂層加入 `"ConnectionStrings": { "DefaultConnection": "Server=localhost;Database=MiraiShop;Trusted_Connection=True;TrustServerCertificate=True" }` 區段

**Checkpoint**: DbContext 與連線字串就緒 → 可開始 US1 實作

---

## Phase 3: User Story 1 — 會員資料持久化至 MSSQL（Priority: P1）🎯 MVP

**Goal**: 完成 EF Core Repository 實作與 DI 替換，使會員資料寫入 MSSQL，重啟後資料仍存在。

**Independent Test**: 啟動 API，呼叫 `POST /api/members/register` 取得 201；直接查詢 MSSQL `Member` table 確認資料存在且 PasswordHash 為 SHA-256 hex；重啟服務後再次查詢，資料仍在。

### Infrastructure 層

- [x] T005 [US1] 建立 `MiraiShop.Infrastructure/Repositories/EfMemberRepository.cs`：實作 `IMemberRepository`，注入 `MiraiShopDbContext`；`Add(Member member)` → `_context.Members.Add(member); _context.SaveChanges()`；`ExistsByEmail(string email)` → `_context.Members.Any(m => m.Email.ToLower() == email.ToLower())`；`GetByEmail(string email)` → `_context.Members.FirstOrDefault(m => m.Email.ToLower() == email.ToLower())`；命名空間 `MiraiShop.Infrastructure.Repositories`

### Server 層（DI 替換）

- [x] T006 [US1] 更新 `MiraiShop.Server/Program.cs`：在現有 DI 區段加入 `builder.Services.AddDbContext<MiraiShopDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));` 並將 `AddScoped<IMemberRepository, MemberRepository>()` 替換為 `AddScoped<IMemberRepository, EfMemberRepository>()`；加入所需 using（`MiraiShop.Infrastructure.Persistence`、`MiraiShop.Infrastructure.Repositories`）

### 驗證編譯與單元測試

- [x] T007 [US1] 執行 `dotnet build MiraiShop.sln` 確認所有專案編譯成功（無錯誤）
- [x] T008 [US1] 執行 `dotnet test MiraiShop.Tests/MiraiShop.Tests.csproj` 確認 10 個單元測試全數通過（MemberServiceTests 6 個 + WeatherForecastServiceTests 4 個；Application 層測試 mock IMemberRepository，不依賴 EF Core）

### 資料庫建立與端對端驗證

- [x] T009 [US1] 在 MSSQL 手動建立資料庫：執行 `sqlcmd -S localhost -E -Q "CREATE DATABASE MiraiShop"` 或使用 SSMS 執行 `CREATE DATABASE MiraiShop`
- [x] T010 [US1] 執行 `dotnet ef migrations add InitialCreate --project MiraiShop.Infrastructure --startup-project MiraiShop.Server --output-dir Persistence/Migrations` 產生 Migration 檔案至 `MiraiShop.Infrastructure/Persistence/Migrations/`
- [x] T011 [US1] 執行 `dotnet ef database update --project MiraiShop.Infrastructure --startup-project MiraiShop.Server` 套用 Migration，建立 `Member` table 與 `IX_Member_Email` 唯一索引
- [ ] T012 [US1] 啟動 API（`dotnet run --project MiraiShop.Server`），對 `POST /api/members/register` 送出有效請求確認回傳 201 Created + MemberDto；以 SSMS 或 sqlcmd 查詢 `SELECT * FROM Member` 確認資料已寫入且 PasswordHash 為 64 字元 hex；重啟服務後再次查詢確認資料持久

**Checkpoint**: `POST /api/members/register` 寫入 MSSQL，重啟後資料仍存在，單元測試全數通過

---

## Phase 4: User Story 2 — EF Core Migration 可重複性（Priority: P2）

**Goal**: 確認 Migration 在全新環境可自動建立正確 schema，且可重複執行不報錯。

**Independent Test**: 對空白 `MiraiShop` 資料庫（drop 後重建）執行 `dotnet ef database update`，確認 `Member` table 依正確 schema 建立；對已存在的 table 再次執行，確認無錯誤。

- [x] T013 [US2] 驗證 `Member` table schema 符合 data-model.md 規格：執行以下 SQL 查詢並確認結果 — `SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Member' ORDER BY ORDINAL_POSITION`；確認 Id(UNIQUEIDENTIFIER)、Email(NVARCHAR 256)、其餘欄位(NVARCHAR MAX)、CreatedAt(DATETIME2)
- [x] T014 [US2] 驗證 Email 唯一索引存在：執行 `SELECT name, is_unique FROM sys.indexes WHERE object_id = OBJECT_ID('Member') AND name = 'IX_Member_Email'`；確認 `is_unique = 1`
- [x] T015 [US2] 驗證 Migration 幂等性：對已套用 Migration 的資料庫再次執行 `dotnet ef database update --project MiraiShop.Infrastructure --startup-project MiraiShop.Server`；確認輸出為「No pending migrations」或類似訊息，且資料庫無異常

**Checkpoint**: Migration 可重複執行，schema 正確，Email 唯一索引建立 ✅

---

## Phase 5: Polish & Cross-Cutting Concerns

- [x] T016 執行 `dotnet test MiraiShop.Tests/MiraiShop.Tests.csproj` 最終確認全數通過（10/10 ✅）
- [ ] T017 以重複 Email 呼叫 `POST /api/members/register` 確認回傳 409 Conflict（驗證 Application 層邏輯 + DB 唯一索引雙重保護均正常）
- [ ] T018 確認 Swagger UI（`/swagger`）顯示 `POST /api/members/register` 端點，schema 與 `RegisterMemberRequest` 欄位一致

---

## Dependencies & Execution Order

### Phase 依賴順序

```
Phase 1 (Setup: NuGet + CLI 工具)
    ↓
Phase 2 (Foundational: DbContext + Connection String)
    ↓
Phase 3 (US1: EfMemberRepository + DI + DB + 端對端驗證)
    ↓
Phase 4 (US2: Migration 可重複性驗證)
    ↓
Phase 5 (Polish: 最終驗證)
```

### User Story 依賴

- **US1 (P1)**: 依賴 Phase 2 完成；是端對端持久化的核心
- **US2 (P2)**: 依賴 US1 完成（Migration 已在 US1 產生並套用）；US2 為驗證性任務

### Phase 2 內部並行機會

T003、T004 可同時執行（不同檔案，無相互依賴）

### Phase 3 內部順序說明

T005（EfMemberRepository 建立）可在 T003、T004 完成後立即執行；
T006（Program.cs DI 替換）需 T003、T005 完成；
T007、T008（編譯+測試）需 T006 完成；
T009（建立 DB）可在 T007 完成確認後執行；
T010（migrations add）需 T009 完成；
T011（database update）需 T010 完成；
T012（端對端驗證）需 T011 完成

---

## Parallel Example: Phase 2（Foundational）

```text
# 可同時執行（不同檔案）:
T003: 建立 MiraiShopDbContext.cs
T004: 新增 appsettings.json ConnectionStrings

# T003、T004 完成後依序執行:
T005: 建立 EfMemberRepository.cs
T006: 更新 Program.cs DI
T007: dotnet build
T008: dotnet test
```

---

## Implementation Strategy

### MVP First（僅 User Story 1）

1. ✅ Phase 1: NuGet 套件安裝
2. ✅ Phase 2: DbContext + 連線字串
3. ✅ Phase 3: EfMemberRepository + DI + DB + 端對端驗證（T012 手動驗證）
4. **STOP & VALIDATE**：`POST /api/members/register` 資料寫入 DB、重啟後仍存在
5. ✅ Phase 4: Migration 可重複性驗證通過
6. ✅ Phase 5: 最終單元測試全數通過

### Incremental Delivery

1. Phase 1 + 2 → EF Core 基礎就緒
2. Phase 3 → 持久化可用（MVP）→ 可實際使用
3. Phase 4 → Migration 品質確認
4. Phase 5 → 整體品質驗證

---

## Notes

- `[P]` 標記的任務可並行執行（不同檔案，無依賴）
- `[US1]`、`[US2]` 對應 spec.md 的 User Story 編號
- 記憶體實作 `MemberRepository.cs` 在 DI 替換後保留（供參考）
- `EfMemberRepository` 的 email 比較使用 `.ToLower()` 以維持與記憶體實作的大小寫不敏感行為一致性
- `ChkResidentialAddress` 已新增至 `IMemberRepository` 介面（由使用者在開發過程中加入）；`MemberServiceTests` 已同步更新 mock 設定
