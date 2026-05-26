# 功能開發規格範本（Feature Spec）

> **使用方式：**  
> 複製本檔案，填寫各欄位後，直接貼給 AI（Claude Code 等工具）並說：  
> 「請依據 CLAUDE.md 的規則，根據以下 spec 實作這個功能。」
>
> AI 會按照 Domain → Application → Infrastructure → Server → Tests 的順序逐層實作。

---

## 範本開始（複製以下內容填寫）

---

# Spec：[功能名稱]

## 1. 功能描述

<!-- 用一兩句話說明這個功能做什麼、解決什麼問題 -->

範例：管理商品（Product），讓後台人員可以新增、查詢、下架商品。

---

## 2. Domain Entity

<!-- 定義這個功能的核心資料模型 -->

**Entity 名稱：** `Xxx`

| 欄位名稱 | 型別 | 說明 |
|---|---|---|
| Id | `Guid` | 唯一識別碼 |
| Name | `string` | 名稱 |
| ... | ... | ... |

**領域規則（如有）：**
- 範例：Price 不可為負數
- 範例：Name 長度不可超過 100 字

---

## 3. Repository 介面（Domain 層）

<!-- 列出需要哪些資料存取方法 -->

**介面名稱：** `IXxxRepository`

| 方法簽章 | 說明 |
|---|---|
| `IEnumerable<Xxx> GetAll()` | 取得全部 |
| `Xxx? GetById(Guid id)` | 依 ID 查詢 |
| `void Add(Xxx entity)` | 新增 |
| `void Remove(Guid id)` | 刪除 |

---

## 4. Application Service

<!-- 定義業務流程，Service 只能呼叫 Repository 介面，不直接碰資料庫 -->

**介面名稱：** `IXxxService`  
**實作名稱：** `XxxService`

| 方法簽章 | 說明 |
|---|---|
| `IEnumerable<XxxDto> GetAll()` | 取得全部並轉為 DTO |
| `XxxDto? GetById(Guid id)` | 依 ID 查詢 |
| `void Create(CreateXxxRequest request)` | 新增 |

**DTO 定義：**

```
XxxDto：
  - Id: Guid
  - Name: string
  - ...

CreateXxxRequest：
  - Name: string
  - ...
```

---

## 5. API 端點（Server 層）

<!-- 定義 Controller 要暴露哪些 HTTP endpoint -->

**Controller 名稱：** `XxxsController`  
**Route：** `/api/xxs`

| HTTP 方法 | 路徑 | 說明 | 回傳 |
|---|---|---|---|
| GET | `/api/xxs` | 取得全部 | `200 XxxDto[]` |
| GET | `/api/xxs/{id}` | 依 ID 查詢 | `200 XxxDto` / `404` |
| POST | `/api/xxs` | 新增 | `201 Created` |
| DELETE | `/api/xxs/{id}` | 刪除 | `204 No Content` |

---

## 6. 單元測試情境

<!-- 列出 XxxServiceTests 中需要覆蓋的測試案例 -->

| 測試方法名稱 | 測試情境 | 預期結果 |
|---|---|---|
| `GetAll_HasItems_ReturnsMappedDtos` | Repository 有資料 | 回傳對應數量的 DTO |
| `GetById_ValidId_ReturnsDto` | ID 存在 | 回傳正確 DTO |
| `GetById_InvalidId_ReturnsNull` | ID 不存在 | 回傳 null |
| `Create_ValidRequest_CallsRepository` | 正常輸入 | Repository.Add 被呼叫一次 |

---

## 7. Infrastructure 實作備註

<!-- 說明 Repository 實作細節，例如用哪個 DB、是否有快取、是否呼叫外部 API -->

- [ ] 使用 Entity Framework Core（`MiraiShopDbContext`）
- [ ] 使用記憶體假資料（開發初期）
- [ ] 呼叫外部 API（請說明 API 端點）
- [ ] 其他：___

---

## 8. 其他備註

<!-- 例外情況、相依功能、效能需求、安全性需求等 -->

- 範例：需要驗證登入才能呼叫 POST / DELETE
- 範例：GetAll 結果需要分頁

---
## 範本結束

---

## 實際填寫範例

以下是一個填好的範例，示範如何使用：

---

# Spec：商品管理（Product）

## 1. 功能描述

讓後台管理員可以查詢、新增、刪除商品，前台使用者只能查詢。

## 2. Domain Entity

**Entity 名稱：** `Product`

| 欄位名稱 | 型別 | 說明 |
|---|---|---|
| Id | `Guid` | 唯一識別碼 |
| Name | `string` | 商品名稱 |
| Price | `decimal` | 售價 |
| Stock | `int` | 庫存數量 |
| IsActive | `bool` | 是否上架 |

**領域規則：**
- Price 不可小於 0
- Name 不可為空白

## 3. Repository 介面

**介面名稱：** `IProductRepository`

| 方法簽章 | 說明 |
|---|---|
| `IEnumerable<Product> GetAll()` | 取得全部商品 |
| `Product? GetById(Guid id)` | 依 ID 查詢 |
| `void Add(Product product)` | 新增商品 |
| `void Remove(Guid id)` | 刪除商品 |

## 4. Application Service

**介面名稱：** `IProductService`  
**實作名稱：** `ProductService`

| 方法 | 說明 |
|---|---|
| `IEnumerable<ProductDto> GetAll()` | 取得全部並轉 DTO |
| `ProductDto? GetById(Guid id)` | 依 ID 查詢 |
| `void Create(CreateProductRequest request)` | 新增商品 |
| `void Delete(Guid id)` | 刪除商品 |

**DTO 定義：**
```
ProductDto：Id, Name, Price, Stock, IsActive
CreateProductRequest：Name, Price, Stock
```

## 5. API 端點

**Controller：** `ProductsController`，Route：`/api/products`

| 方法 | 路徑 | 說明 | 回傳 |
|---|---|---|---|
| GET | `/api/products` | 全部商品 | `200 ProductDto[]` |
| GET | `/api/products/{id}` | 單一商品 | `200` / `404` |
| POST | `/api/products` | 新增 | `201` |
| DELETE | `/api/products/{id}` | 刪除 | `204` |

## 6. 單元測試情境

| 測試方法 | 情境 | 預期 |
|---|---|---|
| `GetAll_HasProducts_ReturnsMappedDtos` | 有商品資料 | 回傳對應 DTO 列表 |
| `GetById_ExistingId_ReturnsDto` | ID 存在 | 回傳正確 DTO |
| `GetById_NonExistingId_ReturnsNull` | ID 不存在 | 回傳 null |
| `Create_ValidRequest_CallsRepositoryAdd` | 正常輸入 | `Add` 被呼叫一次 |
| `Delete_ValidId_CallsRepositoryRemove` | ID 存在 | `Remove` 被呼叫一次 |

## 7. Infrastructure 實作備註

- [x] 使用記憶體假資料（開發初期）

## 8. 其他備註

- 暫不需要驗證，後續再加
