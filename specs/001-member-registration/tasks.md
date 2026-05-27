# Tasks: 會員註冊

**Input**: Design documents from `/specs/001-member-registration/`

**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅ | contracts/api.md ✅

---

## Phase 1: Setup（共用基礎結構）

**Purpose**: 確認專案結構就緒，無需額外初始化（Clean Architecture 已建立）

- [x] T001 確認 MiraiShop.Domain、MiraiShop.Application、MiraiShop.Infrastructure、MiraiShop.Tests 各層目錄存在，與 plan.md 的 Source Code 結構一致

---

## Phase 2: Foundational（所有 User Story 的基礎，必須先完成）

**Purpose**: Domain 層的 Entity 與 Repository 介面是所有上層的依賴，必須在任何 Story 實作前完成

⚠️ **CRITICAL**: 所有 User Story 實作都依賴此 Phase 完成

- [x] T002 [P] 建立 Member entity 於 `MiraiShop.Domain/Entities/Member.cs`（欄位：Id、Name、Email、PasswordHash、MailingAddress、ResidentialAddress、CreatedAt）
- [x] T003 [P] 建立 IMemberRepository 介面於 `MiraiShop.Domain/Interfaces/IMemberRepository.cs`（方法：GetByEmail、Add、ExistsByEmail）

**Checkpoint**: Domain 層完成 → 上層可以開始實作

---

## Phase 3: User Story 1 — 訪客完成會員註冊（Priority: P1）🎯 MVP

**Goal**: 訪客可填寫所有必填欄位（姓名、Email、密碼、通訊地址、住址）並送出，後端建立帳號並回傳 201 Created。

**Independent Test**: 使用 REST Client（如 Swagger UI）對 `POST /api/members/register` 送出完整有效資料，確認回傳 201 且 MemberDto 不含密碼欄位。

### 後端 Application 層

- [x] T004 [P] [US1] 建立 `RegisterMemberRequest` record 於 `MiraiShop.Application/DTOs/RegisterMemberRequest.cs`（欄位：Name、Email、Password、MailingAddress、ResidentialAddress）
- [x] T005 [P] [US1] 建立 `MemberDto` record 於 `MiraiShop.Application/DTOs/MemberDto.cs`（欄位：Id、Name、Email、MailingAddress、ResidentialAddress、CreatedAt，不含密碼）
- [x] T006 [P] [US1] 建立 `IMemberService` 介面於 `MiraiShop.Application/Interfaces/IMemberService.cs`（方法：`MemberDto Register(RegisterMemberRequest request)`）
- [x] T007 [US1] 實作 `MemberService` 於 `MiraiShop.Application/Services/MemberService.cs`（注入 IMemberRepository；Register 方法執行：① SHA-256 加密密碼 ② 建立 Member entity ③ 呼叫 Repository.Add ④ 回傳 MemberDto）

### 後端 Infrastructure 層

- [x] T008 [US1] 實作 `MemberRepository`（記憶體假資料）於 `MiraiShop.Infrastructure/Repositories/MemberRepository.cs`（實作 IMemberRepository 的 GetByEmail、Add、ExistsByEmail）

### 後端 Server 層

- [x] T009 [US1] 建立 `MembersController` 於 `MiraiShop.Server/Controllers/MembersController.cs`（POST /api/members/register；注入 IMemberService；成功回傳 201 Created + MemberDto）
- [x] T010 [US1] 在 `MiraiShop.Server/Program.cs` 新增 DI 註冊：`IIMemberRepository → MemberRepository`、`IMemberService → MemberService`

### 後端單元測試

- [x] T011 [US1] 建立 `MemberServiceTests` 於 `MiraiShop.Tests/MemberServiceTests.cs`，涵蓋測試情境：
  - `Register_ValidRequest_ReturnsMemberDto`（正常流程，確認回傳 DTO 不含密碼）
  - `Register_ValidRequest_PasswordIsHashed`（確認 Repository.Add 收到的是 SHA-256 加密值，非明文）
  - `Register_ValidRequest_CallsRepositoryAdd`（確認 Add 被呼叫一次）

**Checkpoint**: `POST /api/members/register` 可成功建立會員，單元測試全數通過

---

## Phase 4: User Story 2 — 系統拒絕不完整的資料（Priority: P2）

**Goal**: 送出空白或格式錯誤的欄位時，系統回傳 400 Bad Request 與明確欄位錯誤訊息；Email 重複時回傳 409 Conflict。

**Independent Test**: 分別測試以下情境均得到正確 HTTP 狀態碼與錯誤訊息：① 空白 name → 400 ② 無效 email 格式 → 400 ③ 重複 email → 409。

### 後端 Application 層

- [x] T012 [US2] 在 `MiraiShop.Application/DTOs/RegisterMemberRequest.cs` 加入 DataAnnotations 驗證屬性（`[Required]`、`[EmailAddress]`、`[MinLength(5)]` 於地址欄位）
- [x] T013 [US2] 在 `MiraiShop.Application/Services/MemberService.cs` 的 Register 方法加入 Email 唯一性檢查：呼叫 `IMemberRepository.ExistsByEmail`，若已存在則拋出 `InvalidOperationException`

### 後端 Server 層

- [x] T014 [US2] 在 `MiraiShop.Server/Controllers/MembersController.cs` 加入例外處理：捕捉 `InvalidOperationException`（Email 重複）並回傳 `409 Conflict` + 錯誤訊息

### 後端單元測試

- [x] T015 [P] [US2] 在 `MiraiShop.Tests/MemberServiceTests.cs` 補充驗證測試情境：
  - `Register_DuplicateEmail_ThrowsInvalidOperationException`（Email 已存在時拋出例外）
  - `Register_DuplicateEmail_DoesNotCallRepositoryAdd`（確認重複時 Add 不被呼叫）

**Checkpoint**: 所有驗證情境均通過，`dotnet test` 全數綠燈

---

## Phase 5: 前端實作

**Goal**: Angular 前端提供完整的註冊頁面，含表單驗證與 API 呼叫。

**Independent Test**: 瀏覽器開啟 `/register`，確認：① 空白送出顯示欄位錯誤提示 ② 填妥後送出顯示「註冊成功」③ 重複 email 顯示衝突錯誤訊息。

- [x] T016 [P] 建立 `MemberService` 於 `miraishop.client/src/app/services/member.service.ts`（`register(request: RegisterMemberRequest): Observable<MemberDto>` 呼叫 `POST /api/members/register`）
- [x] T017 [P] 建立 `RegisterComponent` TypeScript 於 `miraishop.client/src/app/register/register.component.ts`（Reactive Form 含 5 個必填控制項：name、email、password、mailingAddress、residentialAddress）
- [x] T018 [US1] 建立 `register.component.html` 於 `miraishop.client/src/app/register/`（表單 UI，各欄位顯示驗證錯誤訊息；送出成功顯示成功提示；409 顯示 Email 已被使用）
- [x] T019 建立 `register.component.css` 於 `miraishop.client/src/app/register/`（基本表單樣式）
- [x] T020 在 `miraishop.client/src/app/app-routing.module.ts` 新增 `/register` 路由指向 `RegisterComponent`
- [x] T021 在 `miraishop.client/src/app/app.module.ts` 宣告 `RegisterComponent` 並匯入 `ReactiveFormsModule`

**Checkpoint**: 前端 `/register` 頁面完整可用，含驗證與 API 整合

---

## Phase 6: Polish & Cross-Cutting Concerns

- [x] T022 [P] 執行 `dotnet test MiraiShop.Tests/MiraiShop.Tests.csproj` 確認所有單元測試通過
- [ ] T023 [P] 確認 Swagger UI（`/swagger`）可正確顯示 `POST /api/members/register` 端點與 schema
- [ ] T024 在 `MiraiShop.Server/Program.cs` 確認已加入全域 model validation 回應格式（確保 400 回應包含欄位錯誤清單）

---

## Dependencies & Execution Order

### Phase 依賴順序

```
Phase 1 (Setup)
    ↓
Phase 2 (Foundational: Domain Entity + Interface)
    ↓
Phase 3 (US1: Application + Infrastructure + Server + Tests)
    ↓
Phase 4 (US2: Validation)  ←→  Phase 5 (Frontend) ← 可並行
    ↓
Phase 6 (Polish)
```

### User Story 依賴

- **US1 (P1)**: 依賴 Phase 2 完成；無其他依賴
- **US2 (P2)**: 依賴 US1 完成（需要在現有 Service 和 Controller 上擴充）
- **Frontend (Phase 5)**: 依賴 US1 Controller 完成（需要 API 合約），可與 Phase 4 並行

### Phase 3 內部並行機會

T004、T005、T006 可同時執行（不同檔案，無相互依賴）

---

## Parallel Example: Phase 3（US1 後端）

```text
# 可同時執行（不同檔案）:
T004: 建立 RegisterMemberRequest DTO
T005: 建立 MemberDto
T006: 建立 IMemberService 介面

# T004、T005、T006 完成後才能執行:
T007: 實作 MemberService

# T007 完成後，T008、T009 可並行:
T008: 實作 MemberRepository
T009: 建立 MembersController

# 全部完成後:
T010: 更新 Program.cs DI 註冊
T011: 撰寫 MemberServiceTests
```

---

## Implementation Strategy

### MVP First（僅 User Story 1）

1. ✅ Phase 1: Setup（確認結構）
2. ✅ Phase 2: Foundational（Domain）
3. ✅ Phase 3: US1（完整後端 API + 單元測試）
4. **STOP & VALIDATE**：用 Swagger 確認 `POST /api/members/register` 正常運作
5. 完成後進入 Phase 4 加入驗證

### Incremental Delivery

1. Phase 1 + 2 → Domain 就緒
2. Phase 3 → 後端 API 可用（MVP）→ 可用 Swagger 測試
3. Phase 4 → 驗證強化
4. Phase 5 → 前端頁面
5. Phase 6 → 品質確認

---

## Notes

- `[P]` 標記的任務可並行執行（不同檔案，無依賴）
- `[US1]`、`[US2]` 對應 spec.md 的 User Story 編號
- 每完成一個 Phase 執行 `dotnet test` 確認沒有破壞現有測試
- MemberRepository 初期為記憶體實作，後續接 EF Core 時只需替換 Infrastructure 層
