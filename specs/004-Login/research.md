# Research: 會員登入

**Branch**: `004-Login` | **Date**: 2026-05-27

---

## Decision 1: JWT 函式庫選擇

**Decision**: 後端使用 `Microsoft.AspNetCore.Authentication.JwtBearer`（驗證中介層）搭配 `System.IdentityModel.Tokens.Jwt`（Token 生成）。

**Rationale**: 兩者皆是 ASP.NET Core 8 官方套件，與現有技術堆疊完全相容，有長期維護保證，且與 `AddAuthentication().AddJwtBearer()` 管線整合最直接。

**Alternatives considered**:
- `jose-jwt`：功能更豐富但對此專案過度設計。
- 手動 HMAC + Base64：安全性差，維護成本高。

---

## Decision 2: JWT 生成層級

**Decision**: 在 `Application/Services/AuthService` 內直接生成 JWT，並透過 DI 注入強型別 `JwtSettings` record（定義於 `Application/DTOs/`）。

**Rationale**:
- 現有架構中 Application Services 已包含 SHA-256 密碼雜湊等安全邏輯（見 `MemberService.cs`），JWT 生成屬同類型的應用層關注點。
- 避免新增 `ITokenService` 介面與 Infrastructure 實作的跨層複雜度，符合 CLAUDE.md「不建立未被使用的介面或抽象」原則。
- `JwtSettings` 由 `Program.cs` 從 `appsettings.json` 讀取後以 Singleton 注入，不依賴 `IOptions<T>`，保持 Application 層無 ASP.NET Core 依賴。

**Alternatives considered**:
- 定義 `ITokenService` 在 Application，在 Infrastructure 實作：違反 CLAUDE.md 中 `Infrastructure → Domain only` 的依賴規則。
- 在 Server/Controller 生成 JWT：違反 CLAUDE.md「Controller 禁止業務邏輯」原則。

---

## Decision 3: 前端 Token 儲存

**Decision**: 使用 `localStorage`，以結構化物件 `{ token, expiry, memberId }` 儲存。

**Rationale**: 對此學習專案而言 `localStorage` 實作最簡單。HttpOnly Cookie 雖然對 XSS 更安全，但需要後端 Cookie 設定，增加跨域與 CSRF 複雜度，超出本次功能範圍。spec 的 Assumptions 亦明確說明「具體存放機制依安全政策決定」。

**Alternatives considered**:
- HttpOnly Cookie：更安全但實作複雜，跨域設定需額外處理。
- sessionStorage：不跨 tab，使用者體驗較差。
- In-memory only：頁面重新整理即失效，不符合需求。

---

## Decision 4: 密碼加鹽遷移策略

**Decision**: 在 `Member` 實體新增 nullable `string? PasswordSalt` 欄位，採**漸進式遷移**策略。

**Rationale**:
- 現有已註冊會員的 `PasswordHash` 使用無鹽 SHA-256，強制遷移會使其無法登入。
- 新增 nullable 欄位並建立新 EF Core Migration，不破壞現有資料。
- 登入邏輯：`PasswordSalt` 為 null → 使用無鹽 SHA-256 驗證（既有帳號）；不為 null → 使用加鹽 SHA-256 驗證（新帳號）。
- `MemberService.Register` 同步更新，所有新註冊會員改用加鹽版本。

**Alternatives considered**:
- 強制全員重設密碼：影響範圍大，超出本次功能範圍。
- 不加鹽直接用舊格式：不符合 spec 要求（SC-005、FR-004）。

---

## Decision 5: 速率限制實作

**Decision**: 使用 ASP.NET Core 8 內建 `RateLimiter` middleware，對 `POST /api/members/login` 套用固定視窗（Fixed Window）策略。

**Rationale**: .NET 8 已內建，不需額外 NuGet 套件。策略設定：每個 IP 每 60 秒最多 10 次請求。

**Alternatives considered**:
- AspNetCoreRateLimit NuGet：功能更多，但對此場景過度設計。
- 自行計數快取：維護成本高。

---

## Decision 6: AuthService 與 MemberService 分離

**Decision**: 建立獨立的 `IAuthService` / `AuthService`，不在 `MemberService` 新增登入邏輯。

**Rationale**: 登入（身份驗證）與會員管理（資料 CRUD）是不同的業務關注點。分離可保持每個 Service 的職責單一，且符合 CLAUDE.md 命名規範（`IAuthService`、`AuthService`）。

**Alternatives considered**:
- 在 `MemberService` 新增 `Login` 方法：職責混淆，單一 Service 越來越龐大。

---

## NuGet 套件異動

| 專案 | 套件 | 版本 | 用途 |
|------|------|------|------|
| `MiraiShop.Application` | `System.IdentityModel.Tokens.Jwt` | 8.x | JWT Token 生成 |
| `MiraiShop.Server` | `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.x | JWT 驗證中介層 |
