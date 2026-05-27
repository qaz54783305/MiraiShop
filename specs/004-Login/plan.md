# Implementation Plan: 會員登入

**Branch**: `004-Login` | **Date**: 2026-05-27 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/004-Login/spec.md`

---

## Summary

實作會員以電子信箱和密碼登入的完整流程。後端新增 `AuthService` 負責密碼驗證與 JWT 簽發，前端新增登入頁面並以 HTTP 攔截器自動附帶 Bearer Token；同步更新密碼雜湊策略，新增鹽值（salt）以提升安全性，並保持對現有無鹽帳號的向後相容。

---

## Technical Context

**Language/Version**: C# 12 / .NET 8（後端）、TypeScript 5.4 / Angular 17（前端）

**Primary Dependencies**:
- 後端：ASP.NET Core 8、EF Core 8、`System.IdentityModel.Tokens.Jwt` 8.x（新增）、`Microsoft.AspNetCore.Authentication.JwtBearer` 8.x（新增）
- 前端：Angular 17 Reactive Forms、HttpClient、Angular Router

**Storage**: SQL Server — 現有 `Member` 資料表新增 `PasswordSalt` nullable 欄位（新 EF Migration）

**Testing**: xUnit + Moq（後端 Unit Tests）；Angular component 測試超出本次範圍

**Target Platform**: Web（ASP.NET Core 8 後端 + Angular 17 SPA）

**Performance Goals**: 登入 API 回應 < 2 秒（含 DB 查詢 + JWT 簽發）

**Constraints**: 密碼不得明文儲存（SC-005）；前端驗證即時顯示（SC-003）；密碼遮蔽（Edge Case）

**Scale/Scope**: 單一使用者功能；JWT 無狀態設計，不需 Session Store

---

## Constitution Check

Constitution 文件尚為模板未填寫，依 `CLAUDE.md` 既有原則進行 Gate 檢查：

| 檢查項目 | 結果 | 說明 |
|---------|------|------|
| 依賴方向（Server → Application → Domain） | ✓ Pass | `AuthService` 在 Application，Controller 呼叫 `IAuthService` |
| Infrastructure 只依賴 Domain | ✓ Pass | 不新增 Infrastructure 層的 JWT 實作 |
| Controller 無業務邏輯 | ✓ Pass | Login action 只做接收 → 呼叫 Service → 回傳 |
| DTO 使用 record | ✓ Pass | `LoginRequest`、`LoginResponse`、`JwtSettings` 皆為 record |
| 無意義抽象 | ✓ Pass | 不新增 `ITokenService`，JWT 邏輯集中於 `AuthService` |
| 測試覆蓋 | ✓ Pass | 新增 `AuthServiceTests` 覆蓋所有 Service 路徑 |

---

## Project Structure

### Documentation (this feature)

```text
specs/004-Login/
├── spec.md          ← 功能規格（/speckit-specify 輸出）
├── plan.md          ← 本文件（/speckit-plan 輸出）
├── research.md      ← Phase 0 研究決策
├── data-model.md    ← Phase 1 資料模型
├── quickstart.md    ← Phase 1 手動測試指南
├── contracts/
│   └── login-api.md ← Phase 1 API 合約
├── checklists/
│   └── requirements.md
└── tasks.md         ← Phase 2 輸出（/speckit-tasks 指令建立）
```

### Source Code (repository root)

```text
後端

MiraiShop.Domain/
└── Entities/
    └── Member.cs                         ← 新增 PasswordSalt 屬性

MiraiShop.Application/
├── DTOs/
│   ├── LoginRequest.cs                   ← 新增
│   ├── LoginResponse.cs                  ← 新增
│   └── JwtSettings.cs                    ← 新增
├── Interfaces/
│   └── IAuthService.cs                   ← 新增
└── Services/
    ├── AuthService.cs                    ← 新增
    └── MemberService.cs                  ← 更新 HashPassword 加鹽邏輯

MiraiShop.Infrastructure/
└── Persistence/
    ├── MiraiShopDbContext.cs             ← 更新 PasswordSalt 欄位對應
    └── Migrations/
        └── XXXXXX_AddPasswordSalt.cs     ← 新增 EF Migration

MiraiShop.Server/
├── Controllers/
│   └── MembersController.cs             ← 新增 POST /login action
└── Program.cs                           ← 新增 JWT Auth + Rate Limiting + DI 註冊

MiraiShop.Tests/
├── AuthServiceTests.cs                  ← 新增
└── MemberServiceTests.cs                ← 更新（覆蓋加鹽雜湊邏輯）

前端

miraishop.client/src/app/
├── login/
│   ├── login.component.ts               ← 新增
│   ├── login.component.html             ← 新增
│   └── login.component.css              ← 新增
├── interceptors/
│   └── auth.interceptor.ts              ← 新增（自動附帶 Bearer Token）
├── guards/
│   └── auth.guard.ts                    ← 新增（Token 過期導向 /login）
├── models/
│   └── member.model.ts                  ← 新增 LoginRequest、LoginResponse 介面
├── services/
│   └── member.service.ts               ← 新增 login() 方法
├── app-routing.module.ts               ← 新增 /login 路由
└── app.module.ts                       ← 註冊 LoginComponent、AuthInterceptor
```

**Structure Decision**: Web 應用程式架構（後端 Clean Architecture + 前端 Angular）。使用現有專案結構直接新增/修改檔案，不新增專案層。

---

## Complexity Tracking

無 Constitution 違規，略。

---

## 實作重點備忘

### 後端

1. **密碼加鹽**：`MemberService.HashPassword` 更新為接受 `salt` 參數；`Register` 生成 `Guid.NewGuid().ToString("N")` 作為鹽值並存入 `PasswordSalt`。

2. **登入驗證**：`AuthService.Login` 流程：
   - `GetByEmail` → 若 null 回傳 null（統一 401，不揭露 Email 是否存在）
   - 若 `PasswordSalt` 為 null → 使用舊式雜湊比對
   - 若 `PasswordSalt` 不為 null → 使用加鹽雜湊比對
   - 比對成功 → 呼叫 `GenerateToken` 返回 `LoginResponse`

3. **JWT Payload**：`sub` = memberId, `email` = 電子信箱, `exp` = 到期 UTC

4. **速率限制**：`Program.cs` 對 `/api/members/login` 套用固定視窗 Policy（10 req/60s/IP）

### 前端

1. **`AuthInterceptor`**：讀取 `localStorage["miraishop_auth"]`，若 Token 存在且未過期，在請求 Header 附加 `Authorization: Bearer {token}`；若回應為 401，清除 localStorage 並導向 `/login`。

2. **`AuthGuard`**：保護需登入的路由；若無有效 Token 導向 `/login`。

3. **登入表單**：Reactive Form，提交後呼叫 `MemberService.login()`，成功寫入 localStorage，失敗在表單下方顯示 API 錯誤訊息。
