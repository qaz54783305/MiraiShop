# Research: 會員註冊

**Branch**: `001-member-registration` | **Date**: 2026-05-27

---

## RES-001：密碼加密策略

**Decision**: 使用 SHA-256 加密密碼（專案關係人指定）。

**Rationale**: 專案關係人明確要求使用 SHA-256。在 .NET 中以 `System.Security.Cryptography.SHA256` 實作，不需要額外套件。

**Implementation note**: 由 `MemberService` 在儲存前呼叫加密，Repository 只負責儲存已加密的值。加密邏輯封裝在 Application 層，不洩漏到其他層。

**Security note**: ⚠️ SHA-256 未加鹽值（salt）時，相同密碼會產生相同雜湊，易遭彩虹表攻擊。建議後續版本改為 PBKDF2 或 bcrypt 並加入隨機鹽值。本版本依照關係人指定實作，風險已記錄於 spec.md Assumptions。

**Alternatives considered**:
- bcrypt / Argon2：業界最佳實踐，但非本次指定。
- PBKDF2（.NET 內建）：比 SHA-256 更安全，後續版本優先考慮升級。

---

## RES-002：Email 唯一性驗證

**Decision**: 在 Application Service 層呼叫 `IMemberRepository.ExistsByEmail(email)` 驗證唯一性，衝突時拋出例外，Controller 捕捉後回傳 HTTP 409 Conflict。

**Rationale**: 唯一性驗證屬於業務規則，放在 Application 層符合 Clean Architecture 原則，不讓 Controller 直接判斷。

**Alternatives considered**:
- 資料庫層唯一約束（Unique Constraint）：更安全，但初期記憶體實作無此機制，後續加入 EF Core 時補上。

---

## RES-003：地址欄位設計

**Decision**: 通訊地址（`MailingAddress`）與住址（`ResidentialAddress`）各為獨立的 `string` 欄位，不建立 Address Value Object（初期版本）。

**Rationale**: 初期需求僅為純文字輸入，不需要結構化地址解析。保持簡單，後續如需郵遞區號分離或地圖整合，再重構為 Value Object。

**Alternatives considered**:
- Address Value Object：含城市、鄉鎮、路段等子欄位，過度設計於初期需求。

---

## RES-004：DTO 設計

**Decision**: 使用 C# `record` 定義 DTO，符合不可變（immutable）原則。

| DTO | 用途 | 位置 |
|---|---|---|
| `RegisterMemberRequest` | 前端送出的註冊資料 | Application/DTOs |
| `MemberDto` | 後端回傳的會員資料（不含密碼） | Application/DTOs |

**Rationale**: `record` 是 C# 9+ 的不可變資料結構，適合 DTO 場景，與現有 `WeatherForecastDto` 一致。

---

## RES-005：前端表單驗證策略

**Decision**: 使用 Angular Reactive Forms 進行前端驗證，搭配 `Validators.required`、`Validators.email` 等內建驗證器。

**Rationale**: Reactive Forms 提供型別安全的表單控制，適合有多個必填欄位的表單，且容易撰寫單元測試。

**Alternatives considered**:
- Template-driven Forms：較簡單但型別安全性較弱，不適合複雜表單。

---

## 所有 NEEDS CLARIFICATION 狀態

| 項目 | 狀態 |
|---|---|
| 帳號格式 | ✅ 已解決 — 使用 Email |
| 密碼加密方式 | ✅ 已明確 — SHA-256（關係人指定） |
| 地址欄位結構 | ✅ 已決定 — 各自獨立 string 欄位 |
