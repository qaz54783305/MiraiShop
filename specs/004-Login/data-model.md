# Data Model: 會員登入

**Branch**: `004-Login` | **Date**: 2026-05-27

---

## 實體異動

### Member（現有，需更新）

位置：`MiraiShop.Domain/Entities/Member.cs`

| 欄位 | 型別 | 變更 | 說明 |
|------|------|------|------|
| `Id` | `Guid` | 不變 | 主鍵 |
| `Name` | `string` | 不變 | 姓名 |
| `Email` | `string` | 不變 | 電子信箱（唯一索引） |
| `PasswordHash` | `string` | 不變 | 雜湊後的密碼 |
| `PasswordSalt` | `string?` | **新增** | 密碼鹽值；null 表示舊式無鹽帳號 |
| `MailingAddress` | `string` | 不變 | 通訊地址 |
| `ResidentialAddress` | `string` | 不變 | 戶籍地址 |
| `CreatedAt` | `DateTime` | 不變 | 建立時間 |

**資料庫遷移**：新增 EF Core Migration，在 `Member` 資料表加入 nullable 欄位 `PasswordSalt nvarchar(max) NULL`。

---

## 新增 DTOs

### LoginRequest（新增）

位置：`MiraiShop.Application/DTOs/LoginRequest.cs`

```
record LoginRequest(
    string Email,      // 電子信箱
    string Password    // 明文密碼（後端雜湊後比對）
)
```

驗證規則：
- `Email`：必填，符合 Email 格式
- `Password`：必填

---

### LoginResponse（新增）

位置：`MiraiShop.Application/DTOs/LoginResponse.cs`

```
record LoginResponse(
    string Token,      // JWT 字串
    DateTime Expiry,   // Token 到期時間（UTC）
    Guid MemberId      // 登入會員的 ID
)
```

---

### JwtSettings（新增）

位置：`MiraiShop.Application/DTOs/JwtSettings.cs`

```
record JwtSettings(
    string SecretKey,     // 簽名金鑰（至少 32 字元）
    string Issuer,        // Token 發行者
    string Audience,      // Token 受眾
    int ExpiryMinutes     // Token 有效分鐘數
)
```

來源：由 `Program.cs` 從 `appsettings.json["JwtSettings"]` 讀取並以 Singleton 注入。

---

## 新增 Service 介面

### IAuthService（新增）

位置：`MiraiShop.Application/Interfaces/IAuthService.cs`

```
interface IAuthService
{
    LoginResponse Login(LoginRequest request);
}
```

---

## 密碼雜湊邏輯

### 新帳號（有鹽）

```
salt   = Guid.NewGuid().ToString("N")
hash   = SHA256( UTF8(salt + password) ) → hex string
儲存   → PasswordSalt = salt, PasswordHash = hash
```

### 舊帳號（無鹽，向後相容）

```
hash   = SHA256( UTF8(password) ) → hex string
比對   → PasswordHash == hash（PasswordSalt 為 null）
```

### 登入比對

```
member = repository.GetByEmail(email)
if member.PasswordSalt is null:
    inputHash = SHA256(password)
else:
    inputHash = SHA256(member.PasswordSalt + password)

return inputHash == member.PasswordHash
```

---

## appsettings.json 新增區段

```json
"JwtSettings": {
  "SecretKey": "REPLACE_WITH_SECURE_32_CHAR_SECRET",
  "Issuer": "MiraiShop",
  "Audience": "MiraiShop",
  "ExpiryMinutes": 60
}
```

---

## 前端模型異動

### member.model.ts（現有，需更新）

位置：`miraishop.client/src/app/models/member.model.ts`

新增：
```typescript
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiry: string;   // ISO 8601 datetime string
  memberId: string;
}
```

### localStorage 結構

鍵名：`miraishop_auth`

```typescript
interface StoredAuth {
  token: string;
  expiry: string;   // ISO 8601
  memberId: string;
}
```
