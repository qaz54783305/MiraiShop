# Quickstart: 會員登入功能手動測試指南

**Branch**: `004-Login` | **Date**: 2026-05-27

---

## 前置條件

1. SQL Server 已啟動，`DefaultConnection` 連線字串已設定於 `appsettings.json`
2. 已執行 `dotnet ef database update` 套用最新 Migration（含 `PasswordSalt` 欄位）
3. `appsettings.json` 已填入 `JwtSettings.SecretKey`（至少 32 字元）
4. 已有一筆測試用已註冊會員（可先用 `/api/members/register` 建立）

---

## 啟動後端

```bash
cd MiraiShop.Server
dotnet run
```

Swagger UI：`https://localhost:{PORT}/swagger`

---

## 啟動前端

```bash
cd miraishop.client
npm install
npm start
```

前端預設位址：`https://localhost:56501`

---

## 測試流程

### 1. 正常登入

**透過 Swagger / curl**：

```bash
curl -X POST https://localhost:{PORT}/api/members/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"MyP@ssword123"}'
```

預期回應（200）：
```json
{
  "token": "eyJ...",
  "expiry": "2026-05-27T15:00:00Z",
  "memberId": "..."
}
```

**透過前端**：
1. 開啟 `https://localhost:56501/login`
2. 填入正確的電子信箱和密碼
3. 點擊「登入」
4. 確認頁面顯示登入成功訊息

---

### 2. 錯誤密碼

```bash
curl -X POST https://localhost:{PORT}/api/members/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"WrongPassword"}'
```

預期回應（401）：
```json
{ "error": "電子信箱或密碼錯誤" }
```

---

### 3. 必填欄位缺失（前端驗證）

在登入頁面：
1. 不填任何欄位，點擊「登入」
2. 確認兩個欄位下方各顯示「此欄位為必填」
3. 確認未發出 API 請求（開啟 DevTools Network 確認）

---

### 4. Token 過期測試

1. 暫時將 `appsettings.json` 中 `JwtSettings.ExpiryMinutes` 設為 `1`
2. 登入成功後等待 1 分鐘
3. 嘗試執行任何需驗證的操作（若有）
4. 確認頁面顯示「登入逾時，請重新登入」並跳轉至 `/login`
5. 測試完畢後恢復 `ExpiryMinutes` 原始值

---

### 5. 速率限制測試

在 60 秒內送出超過 10 次登入請求，確認第 11 次回傳 HTTP 429。

```bash
for i in {1..11}; do
  curl -s -o /dev/null -w "%{http_code}\n" \
    -X POST https://localhost:{PORT}/api/members/login \
    -H "Content-Type: application/json" \
    -d '{"email":"test@example.com","password":"wrong"}'
done
```

---

## 確認 Token 正確性

可至 [jwt.io](https://jwt.io) 貼上收到的 Token，確認 Payload 包含：
- `sub`：memberId
- `email`：會員電子信箱
- `exp`：到期 Unix timestamp
