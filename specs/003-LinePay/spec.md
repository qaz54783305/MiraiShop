# Feature Specification: 串接LinePay API

**Feature Branch**: `003-LinePay`

**Created**: 2026-05-27

**Status**: Ready for Planning

**Input**: User description: "串接LinePay的付款功能API"

## User Scenarios & Testing *(mandatory)*

### User Story 1 — 前端網頁選擇商品後,傳入指定參數給後端API，後端API串接LinePay API完成付款流程（Priority: P1）

### 注意：網址中的 {transactionId} 要替換成 Line Pay 傳給你的那一串交易序號。
1. API 網址 (Sandbox環境)POST https://line.me{transactionId}/confirm
2. 必要帶入的 HeaderContent-Type: application/jsonX-LINE-ChannelId: 你的 Channel ID 
-X-LINE-Authorization-Nonce: 隨機字串（或當前時間戳記，每次請求都不能重複） 
-X-LINE-Authorization: 簽章（必須使用 Channel Secret + API 路徑 + 隨機字串 + Body 內容 透過 HMAC-SHA256 加密後轉為 Base64） 
範例:
json{
  "amount": 100,
  "currency": "TWD",
  "orderId": "your_side_project_order_001",
  "packages": [
    {
      "id": "package_id_01",
      "amount": 100,
      "products": [
        {
          "name": "Side Project 測試商品",
          "quantity": 1,
          "price": 100
        }
      ]
    }
  ],
  "redirectUrls": {
    "confirmUrl": "https://xn--6qq176gc3ckrd.com",
    "cancelUrl": "https://xn--6qq176gc3ckrd.com"
  }
}

### Edge Cases



## Requirements *(mandatory)*

### Functional Requirements



### Key Entities


## Success Criteria *(mandatory)*

### Measurable Outcomes



## Assumptions


