# API Contract: 會員登入

**Branch**: `004-Login` | **Date**: 2026-05-27

---

## POST /api/members/login

會員以電子信箱和密碼換取 JWT 存取憑證。

### Request

**Headers**
```
Content-Type: application/json
```

**Body**
```json
{
  "email": "test@example.com",
  "password": "MyP@ssword123"
}
```

| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| `email` | string | ✓ | 有效 Email 格式 |
| `password` | string | ✓ | 明文密碼（後端雜湊後比對） |

---

### Responses

#### 200 OK — 登入成功

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiry": "2026-05-27T14:00:00Z",
  "memberId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

| 欄位 | 型別 | 說明 |
|------|------|------|
| `token` | string | JWT Bearer Token |
| `expiry` | string (ISO 8601 UTC) | Token 到期時間 |
| `memberId` | string (UUID) | 登入會員的 ID |

---

#### 400 Bad Request — 格式驗證失敗

```json
{
  "errors": {
    "email": ["The email field is required."],
    "password": ["The password field is required."]
  }
}
```

觸發條件：必填欄位缺失或 Email 格式不正確。

---

#### 401 Unauthorized — 驗證失敗

```json
{
  "error": "電子信箱或密碼錯誤"
}
```

觸發條件：Email 不存在於資料庫，或密碼不符合。**不揭露哪個欄位有誤。**

---

#### 429 Too Many Requests — 超過速率限制

```json
{
  "error": "請求次數過多，請稍後再試"
}
```

觸發條件：同一 IP 在 60 秒內超過 10 次請求。

---

### Rate Limiting

| 策略 | 值 |
|------|----|
| 類型 | Fixed Window |
| 視窗 | 60 秒 |
| 最大請求數 | 10 次 / IP |
| 超過後行為 | 回傳 HTTP 429 |

---

## 現有端點（不異動）

| 方法 | 路徑 | 說明 |
|------|------|------|
| POST | `/api/members/register` | 會員註冊（已實作） |
