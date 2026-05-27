# API Contract: 會員註冊

**Branch**: `001-member-registration` | **Date**: 2026-05-27

---

## POST /api/members/register

**說明**: 建立新會員帳號

### Request

```
POST /api/members/register
Content-Type: application/json
```

**Body**：

```json
{
  "name": "王小明",
  "email": "user@example.com",
  "password": "MyP@ssword123",
  "mailingAddress": "台北市信義區信義路五段7號",
  "residentialAddress": "新北市板橋區文化路一段1號"
}
```

| 欄位 | 型別 | 必填 | 驗證規則 |
|---|---|---|---|
| `name` | string | ✅ | 不可空白 |
| `email` | string | ✅ | 符合 email 格式、全系統唯一 |
| `password` | string | ✅ | 不可空白（後端加密，不回傳） |
| `mailingAddress` | string | ✅ | 不可空白，最少 5 字元 |
| `residentialAddress` | string | ✅ | 不可空白，最少 5 字元 |

---

### Responses

#### 201 Created — 註冊成功

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "王小明",
  "email": "user@example.com",
  "mailingAddress": "台北市信義區信義路五段7號",
  "residentialAddress": "新北市板橋區文化路一段1號",
  "createdAt": "2026-05-27T07:00:00Z"
}
```

> ⚠️ 回應中**不包含** `passwordHash`，密碼永不回傳給前端。

---

#### 400 Bad Request — 欄位驗證失敗

```json
{
  "errors": {
    "email": ["請填寫有效的電子信箱格式"],
    "mailingAddress": ["通訊地址為必填欄位"]
  }
}
```

---

#### 409 Conflict — Email 已被使用

```json
{
  "error": "此電子信箱已被註冊，請使用其他信箱或直接登入。"
}
```

---

## 前端 Service 呼叫範例（Angular）

```typescript
// member.service.ts
register(request: RegisterMemberRequest): Observable<MemberDto> {
  return this.http.post<MemberDto>('/api/members/register', request);
}
```
