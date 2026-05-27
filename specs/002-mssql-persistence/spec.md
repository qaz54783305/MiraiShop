# Feature Specification: MSSQL 會員資料持久化

**Feature Branch**: `002-mssql-persistence`

**Created**: 2026-05-27

**Status**: Ready for Planning

**Input**: User description: "基於註冊功能,增加寫入資料庫的方法,資料庫為MSSQL,連線方式為localhost/window驗證,table為Member"

## User Scenarios & Testing *(mandatory)*

### User Story 1 — 會員資料持久化至 MSSQL（Priority: P1）

系統管理員或開發人員啟動應用程式後，會員透過 `POST /api/members/register` 完成註冊，其資料應被永久寫入 MSSQL 資料庫的 `Member` table，而非儲存在記憶體中；重啟服務後資料依然存在。

**Why this priority**: 記憶體實作僅適用於開發測試階段，正式環境需要持久化儲存。此為所有後續功能（如登入、查詢會員）的基礎，必須優先完成。

**Independent Test**: 呼叫 `POST /api/members/register` 送出有效資料，確認：① 回傳 201 Created + MemberDto；② 直接查詢 MSSQL `Member` table，確認新紀錄存在且 PasswordHash 欄位為 SHA-256 雜湊值（非明文）；③ 重啟服務後再次查詢，資料仍存在。

**Acceptance Scenarios**:

1. **Given** MSSQL 已啟動且 `MiraiShop` 資料庫與 `Member` table 已建立，**When** 送出完整有效的會員資料至 `POST /api/members/register`，**Then** 系統回傳 201 Created，且 `Member` table 中出現一筆對應紀錄，PasswordHash 欄位為雜湊值。
2. **Given** 資料庫中已存在相同 Email 的會員，**When** 再次以該 Email 送出註冊，**Then** 系統回傳 409 Conflict，且資料庫不新增重複紀錄。
3. **Given** 應用程式已重新啟動，**When** 查詢 `Member` table，**Then** 先前註冊的會員資料仍完整存在。

---

### User Story 2 — 資料庫初始化與 Migration（Priority: P2）

開發者或 DevOps 人員在全新環境部署時，應能透過 EF Core Migration 自動建立 `Member` table（包含正確的欄位、型態、主鍵設定），而無需手動執行 SQL 腳本。

**Why this priority**: 可重複的自動化資料庫初始化流程是團隊協作與多環境部署的基礎，但不阻擋 US1 的運作（可手動建 table 先跑通）。

**Independent Test**: 在全新的 MSSQL 資料庫上執行 `dotnet ef database update`，確認 `Member` table 被自動建立，且欄位結構（Id、Name、Email、PasswordHash、MailingAddress、ResidentialAddress、CreatedAt）與 Domain entity 一致。

**Acceptance Scenarios**:

1. **Given** 空白的 MSSQL 資料庫（僅存在連線），**When** 執行 EF Core Migration，**Then** `Member` table 依正確的 schema 自動建立。
2. **Given** `Member` table 已存在，**When** 再次執行 Migration，**Then** 不重複建立、不拋出錯誤。

---

### Edge Cases

- 資料庫連線失敗時（如 MSSQL 服務未啟動），API 應回傳明確的錯誤，不暴露連線字串細節。
- Email 唯一性檢查須依賴資料庫層（`ExistsByEmail` 查詢 DB），不得依賴記憶體快取。
- `CreatedAt` 欄位由應用程式設定（非資料庫預設值），確保時區一致性。

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: 系統必須將會員資料（Id、Name、Email、PasswordHash、MailingAddress、ResidentialAddress、CreatedAt）永久寫入 MSSQL `Member` table。
- **FR-002**: 系統必須使用 Windows 驗證（Trusted_Connection=True）連線至本機 MSSQL（localhost）。
- **FR-003**: 系統必須確保 Email 欄位在 `Member` table 中的唯一性（`ExistsByEmail` 需查詢資料庫）。
- **FR-004**: 系統必須透過 EF Core Migration 管理 `Member` table 的 schema，使資料庫結構可版本化。
- **FR-005**: 資料庫操作失敗時，系統必須向上層拋出可識別的例外，由 Controller 統一處理回傳適當 HTTP 狀態碼。
- **FR-006**: 新增的資料庫持久化實作必須完全替換現有的記憶體實作（`MemberRepository`），Application 層與 Domain 層介面不得修改。

### Key Entities

- **Member**：會員資料（對應現有 `MiraiShop.Domain/Entities/Member.cs`），欄位：Id (Guid, PK)、Name (string)、Email (string, unique)、PasswordHash (string)、MailingAddress (string)、ResidentialAddress (string)、CreatedAt (DateTime)。

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 透過 `POST /api/members/register` 成功註冊的會員，重啟應用程式後資料依然存在於資料庫（持久化率 100%）。
- **SC-002**: 重複 Email 的檢查由資料庫層完成，不依賴記憶體狀態，確保多實例部署時行為正確。
- **SC-003**: EF Core Migration 可在全新環境自動建立 `Member` table，無需手動 SQL 介入。
- **SC-004**: 所有現有單元測試（10 個）在切換至資料庫實作後仍然通過（Application 層介面不變）。

## Assumptions

- 開發環境已安裝 MSSQL（SQL Server），且以 Windows 驗證方式可從 localhost 連線。
- 資料庫名稱為 `MiraiShop`（需預先建立資料庫，table 由 Migration 建立）。
- 連線字串格式：`Server=localhost;Database=MiraiShop;Trusted_Connection=True;TrustServerCertificate=True`。
- EF Core 版本與專案現有的 .NET 8 相容（使用 `Microsoft.EntityFrameworkCore.SqlServer` 8.x）。
- Infrastructure 層可新增 NuGet 套件（EF Core、SQL Server Provider），Domain 與 Application 層不新增任何套件。
- 現有的記憶體 `MemberRepository` 將被新的 EF Core 實作完全取代；DI 註冊由 `Program.cs` 替換。
- `CreatedAt` 欄位儲存 UTC 時間（`DateTime.UtcNow`）。
- 不實作 EF Core Interceptor 或 Audit Log（超出此功能範圍）。
