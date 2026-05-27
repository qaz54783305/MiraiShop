# Tasks: 會員登入

**Input**: Design documents from `specs/004-Login/`

**Prerequisites**: plan.md ✓ | spec.md ✓ | research.md ✓ | data-model.md ✓ | contracts/ ✓

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1/US2/US3)

---

## Phase 1: Setup（安裝與設定）

**Purpose**: 新增 NuGet 套件與 JWT 設定，無需改動邏輯程式碼

- [x] T001 Add `System.IdentityModel.Tokens.Jwt` 8.x to `MiraiShop.Application/MiraiShop.Application.csproj`
- [x] T002 [P] Add `Microsoft.AspNetCore.Authentication.JwtBearer` 8.x to `MiraiShop.Server/MiraiShop.Server.csproj`
- [x] T003 Add JwtSettings JSON section (SecretKey, Issuer, Audience, ExpiryMinutes) to `MiraiShop.Server/appsettings.json`

---

## Phase 2: Foundational（基礎建設 — 阻擋所有 User Story）

**Purpose**: 完成 Member 加鹽遷移與所有新 DTO/Interface，這些是所有 User Story 的共同前置條件

**⚠️ CRITICAL**: 此 Phase 完成前，任何 User Story 均無法開始

- [x] T004 Add nullable `string? PasswordSalt` property to `MiraiShop.Domain/Entities/Member.cs`
- [x] T005 Update `OnModelCreating` in `MiraiShop.Infrastructure/Persistence/MiraiShopDbContext.cs` to map `PasswordSalt` as optional column
- [x] T006 Run `dotnet ef migrations add AddPasswordSalt` to create migration in `MiraiShop.Infrastructure/Persistence/Migrations/`
- [x] T007 Update `MemberService.HashPassword` to accept optional salt and update `Register` to generate `Guid.NewGuid().ToString("N")` as salt in `MiraiShop.Application/Services/MemberService.cs`
- [x] T008 Update `MiraiShop.Tests/MemberServiceTests.cs` to verify salted hash is stored and existing test logic remains correct
- [x] T009 [P] Create `LoginRequest` record (Email, Password) in `MiraiShop.Application/DTOs/LoginRequest.cs`
- [x] T010 [P] Create `LoginResponse` record (Token, Expiry, MemberId) in `MiraiShop.Application/DTOs/LoginResponse.cs`
- [x] T011 [P] Create `JwtSettings` record (SecretKey, Issuer, Audience, ExpiryMinutes) in `MiraiShop.Application/DTOs/JwtSettings.cs`
- [x] T012 Create `IAuthService` interface with `Login(LoginRequest) : LoginResponse` in `MiraiShop.Application/Interfaces/IAuthService.cs`

**Checkpoint**: Foundation complete — 可平行開始所有 User Story

---

## Phase 3: User Story 1 — 訪客完成會員登入（Priority: P1）🎯 MVP

**Goal**: 已註冊會員可填寫電子信箱與密碼登入，後端驗證後回傳 JWT；前端登入頁面可送出表單並顯示結果。

**Independent Test**:
1. 以 Swagger 或 curl 對 `POST /api/members/login` 送出正確憑證，確認收到含 `token` 的 200 回應
2. 開啟 `https://localhost:56501/login`，輸入正確帳密後確認顯示登入成功

### 後端實作

- [x] T013 [P] [US1] Implement `AuthService.Login`: password verification (salt-aware) + JWT generation (sub/email/exp claims) in `MiraiShop.Application/Services/AuthService.cs`
- [x] T014 [US1] Register JWT auth middleware (`AddAuthentication().AddJwtBearer(...)`) + bind `JwtSettings` singleton + register `IAuthService → AuthService` in `MiraiShop.Server/Program.cs`
- [x] T015 [US1] Add `[HttpPost("login")]` action to `MiraiShop.Server/Controllers/MembersController.cs` — calls `IAuthService.Login`, returns 200 with `LoginResponse` or 401 `{ error: "電子信箱或密碼錯誤" }`
- [x] T016 [US1] Create `MiraiShop.Tests/AuthServiceTests.cs` covering: valid credentials return token, wrong password returns null, unknown email returns null, token contains correct claims

### 前端實作

- [x] T017 [P] [US1] Add `LoginRequest` and `LoginResponse` TypeScript interfaces to `miraishop.client/src/app/models/member.model.ts`
- [x] T018 [P] [US1] Add `login(request: LoginRequest): Observable<LoginResponse>` to `miraishop.client/src/app/services/member.service.ts`
- [x] T019 [US1] Create `LoginComponent` with Reactive Form (email/password controls, submit handler that calls `MemberService.login()` and stores result in localStorage key `miraishop_auth`) in `miraishop.client/src/app/login/login.component.ts` + `login.component.html` + `login.component.css`
- [x] T020 [US1] Add `/login` route entry in `miraishop.client/src/app/app-routing.module.ts`
- [x] T021 [US1] Declare `LoginComponent` in `declarations` and add to imports/routing in `miraishop.client/src/app/app.module.ts`

**Checkpoint**: US1 完成 — 登入端對端流程可獨立驗證（後端 API + 前端頁面均可測試）

---

## Phase 4: User Story 2 — 系統拒絕不完整的資料（Priority: P2）

**Goal**: 前端表單在欄位缺失或格式錯誤時，即時顯示錯誤提示並阻止 API 請求發出。

**Independent Test**:
1. 開啟登入頁，不填任何欄位點擊送出 → 兩欄位下方均顯示「此欄位為必填」，DevTools Network 無請求
2. 填入格式錯誤的 Email（如 `abc`) → 顯示「請輸入有效的電子信箱格式」
3. 只填 Email 不填密碼 → 密碼欄顯示「此欄位為必填」

- [x] T022 [US2] Add `Validators.required` + `Validators.email` to email control and `Validators.required` to password control in `miraishop.client/src/app/login/login.component.ts`
- [x] T023 [US2] Add `<div>` error message elements below each input field (顯示「此欄位為必填」或「請輸入有效的電子信箱格式」) in `miraishop.client/src/app/login/login.component.html`
- [x] T024 [US2] Guard `onSubmit()` with `if (this.loginForm.invalid) return` to prevent API call when form is invalid in `miraishop.client/src/app/login/login.component.ts`

**Checkpoint**: US2 完成 — 前端驗證可獨立測試，不依賴後端回應

---

## Phase 5: User Story 3 — 存取憑證過期處理（Priority: P3）

**Goal**: 前端自動在請求中附帶 Bearer Token；Token 過期或收到 401 時，清除 localStorage 並導向 `/login`。

**Independent Test**:
1. 登入後在 DevTools Network 確認後續請求含 `Authorization: Bearer ...` header
2. 手動修改 localStorage 中的 expiry 為過去時間，重新發出請求，確認導向 `/login`
3. 將 `appsettings.json` 中 `ExpiryMinutes` 設為 1，等待過期後操作，確認顯示過期提示

- [x] T025 [P] [US3] Create `AuthInterceptor` implementing `HttpInterceptor` — read `miraishop_auth` from localStorage, attach `Authorization: Bearer {token}` if token present and not expired in `miraishop.client/src/app/interceptors/auth.interceptor.ts`
- [x] T026 [P] [US3] Create `AuthGuard` implementing `CanActivate` — check localStorage token validity, redirect to `/login` if missing/expired in `miraishop.client/src/app/guards/auth.guard.ts`
- [x] T027 [US3] Register `AuthInterceptor` in `providers` array with `HTTP_INTERCEPTORS` multi-provider token in `miraishop.client/src/app/app.module.ts`
- [x] T028 [US3] Add 401 response handler to `AuthInterceptor` — on `HttpErrorResponse.status === 401`, clear `miraishop_auth` from localStorage and call `Router.navigate(['/login'])` in `miraishop.client/src/app/interceptors/auth.interceptor.ts`
- [x] T029 [US3] Add expiry pre-check to `AuthInterceptor` — if `new Date(auth.expiry) <= new Date()`, clear storage and redirect before sending request in `miraishop.client/src/app/interceptors/auth.interceptor.ts`

**Checkpoint**: US3 完成 — Token 過期機制可獨立驗證

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: 速率限制與端對端驗證

- [x] T030 Add Fixed Window `RateLimiter` policy (`10 req/60s/IP`) in `MiraiShop.Server/Program.cs` and apply `[EnableRateLimiting("login")]` attribute to the `/login` action in `MiraiShop.Server/Controllers/MembersController.cs`
- [x] T031 [P] Verify `type="password"` is set on password input in `miraishop.client/src/app/login/login.component.html` (密碼遮蔽 Edge Case)
- [x] T032 Run quickstart.md 5 個測試情境，確認端對端行為正確

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: 無依賴，立即開始
- **Phase 2 (Foundational)**: 依賴 Phase 1 完成 — **阻擋所有 User Story**
- **Phase 3 (US1)**: 依賴 Phase 2 完成 — 無其他 Story 依賴
- **Phase 4 (US2)**: 依賴 Phase 3 完成（需要 LoginComponent 存在）
- **Phase 5 (US3)**: 依賴 Phase 3 完成（需要 MemberService.login() 與 localStorage 結構）
- **Phase 6 (Polish)**: 依賴所有 Story Phase 完成

### User Story Dependencies

- **US1 (P1)**: Foundational 完成後即可開始 — 無 Story 間依賴
- **US2 (P2)**: 依賴 US1（在同一 LoginComponent 上擴充驗證邏輯）
- **US3 (P3)**: 依賴 US1（需要 localStorage auth 結構已定義）

### 各 Phase 內部順序

```
Phase 2: T004 → T005 → T006（Entity → DbContext → Migration）
         T007 → T008（Service 更新 → 測試更新）
         T009, T010, T011, T012 可平行

Phase 3: T013 與 T017, T018 可平行（後端 Service vs 前端 Model/Service）
         T013 → T016（AuthService → 其測試）
         T014 → T015（DI 完成後才能 compile Controller）
         T017, T018 → T019（Model/Service 完成後再建 Component）
         T019 → T020 → T021

Phase 5: T025, T026 可平行（Interceptor vs Guard 獨立檔案）
         T025 → T028 → T029（同一 Interceptor 分步擴充）
         T025 完成後 → T027
```

---

## Parallel Opportunities

```bash
# Phase 1 — 兩個 .csproj 可同時修改
T001: Edit MiraiShop.Application.csproj
T002: Edit MiraiShop.Server.csproj

# Phase 2 — DTOs 可全部平行
T009: Create LoginRequest.cs
T010: Create LoginResponse.cs
T011: Create JwtSettings.cs

# Phase 3 — 後端 Service 與前端 Model 可平行
T013: AuthService.cs (後端)
T017: member.model.ts (前端)
T018: member.service.ts (前端)

# Phase 5 — 兩個新檔案可平行
T025: auth.interceptor.ts
T026: auth.guard.ts
```

---

## Implementation Strategy

### MVP（僅 US1）

1. 完成 Phase 1: Setup
2. 完成 Phase 2: Foundational（CRITICAL）
3. 完成 Phase 3: US1（後端 + 前端）
4. **STOP and VALIDATE**：用正確帳密登入，確認後端回傳 Token，前端顯示成功
5. 可 Demo

### Incremental Delivery

1. Setup + Foundational → 基礎完成
2. US1 → 登入功能可用（MVP）
3. US2 → 前端驗證 UX 完整
4. US3 → Token 過期處理完整
5. Polish → 速率限制 + 最終驗證

---

## Notes

- `[P]` 任務操作不同檔案，無依賴，可並行執行
- T006 需在 T004、T005 完成後才能跑（EF Migration 讀取當前 Model 狀態）
- T008（更新測試）需確認新的加鹽邏輯不破壞舊有測試案例
- T030（Rate Limiter）使用 ASP.NET Core 8 內建，無需額外 NuGet
- `miraishop_auth` 是 localStorage 的統一鍵名，Interceptor 與 Guard 共用此常數
