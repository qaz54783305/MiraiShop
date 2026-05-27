# Data Model: 會員註冊

**Branch**: `001-member-registration` | **Date**: 2026-05-27

---

## Entity：Member（Domain 層）

```csharp
// MiraiShop.Domain/Entities/Member.cs
public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; }          // 姓名，必填
    public string Email { get; set; }         // 電子信箱，唯一，作為帳號
    public string PasswordHash { get; set; }  // SHA-256 加密後的密碼
    public string MailingAddress { get; set; }     // 通訊地址，必填
    public string ResidentialAddress { get; set; } // 住址，必填
    public DateTime CreatedAt { get; set; }   // 註冊時間
}
```

**Validation rules**：
- `Name`：不可為 null 或空白
- `Email`：不可為 null、須符合 email 格式、全系統唯一
- `PasswordHash`：不可為 null（由 Service 加密後填入）
- `MailingAddress`：不可為 null 或空白，最短 5 字元
- `ResidentialAddress`：不可為 null 或空白，最短 5 字元
- `CreatedAt`：由系統自動填入，不由前端傳入

---

## DTOs（Application 層）

### RegisterMemberRequest（前端 → 後端）

```csharp
// MiraiShop.Application/DTOs/RegisterMemberRequest.cs
public record RegisterMemberRequest(
    string Name,
    string Email,
    string Password,           // 明文，後端加密後丟棄
    string MailingAddress,
    string ResidentialAddress
);
```

### MemberDto（後端 → 前端）

```csharp
// MiraiShop.Application/DTOs/MemberDto.cs
public record MemberDto(
    Guid Id,
    string Name,
    string Email,
    string MailingAddress,
    string ResidentialAddress,
    DateTime CreatedAt
);
// ⚠️ 不包含 PasswordHash — 密碼永不回傳給前端
```

---

## Repository 介面（Domain 層）

```csharp
// MiraiShop.Domain/Interfaces/IMemberRepository.cs
public interface IMemberRepository
{
    Member? GetByEmail(string email);
    void Add(Member member);
    bool ExistsByEmail(string email);
}
```

---

## Service 介面（Application 層）

```csharp
// MiraiShop.Application/Interfaces/IMemberService.cs
public interface IMemberService
{
    MemberDto Register(RegisterMemberRequest request);
}
```

---

## 前端 TypeScript 型別

```typescript
// miraishop.client/src/app/models/member.model.ts
export interface RegisterMemberRequest {
  name: string;
  email: string;
  password: string;
  mailingAddress: string;
  residentialAddress: string;
}

export interface MemberDto {
  id: string;
  name: string;
  email: string;
  mailingAddress: string;
  residentialAddress: string;
  createdAt: string;
}
```

---

## 狀態流程

```
訪客填寫表單
    ↓
前端驗證（必填、email 格式）
    ↓
POST /api/members/register（RegisterMemberRequest）
    ↓
後端驗證（必填、email 格式、email 唯一性）
    ├── 驗證失敗 → 400 Bad Request（欄位錯誤說明）
    ├── Email 重複 → 409 Conflict
    └── 驗證通過
            ↓
        密碼 SHA-256 加密
            ↓
        建立 Member entity
            ↓
        Repository.Add()
            ↓
        201 Created（MemberDto）
```
