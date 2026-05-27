# Implementation Plan: 會員註冊

**Branch**: `001-member-registration` | **Date**: 2026-05-27 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-member-registration/spec.md`

---

## Summary

允許訪客透過填寫姓名、電子信箱、通訊地址、住址與密碼完成會員註冊。後端以 Clean Architecture 分層實作：Domain 定義 Member 實體與 Repository 介面，Application 定義 Service 介面與 DTO，Infrastructure 實作 Repository（初期記憶體假資料），Server 提供 REST API（`POST /api/members/register`）。密碼以 SHA-256 加密後儲存，電子信箱作為唯一帳號識別碼。

---

## Technical Context

**Language/Version**: C# 12（後端）、TypeScript 5.4（前端）

**Primary Dependencies**: ASP.NET Core 8、Angular 17、xUnit、Moq

**Storage**: 記憶體假資料（開發初期），預留 IMemberRepository 介面供後續接入 Entity Framework Core

**Testing**: xUnit + Moq（後端單元測試）、Jasmine/Karma（前端）

**Target Platform**: Web（Windows 開發 / Linux 部署）

**Project Type**: Web Service（REST API）+ SPA（Angular）

**Performance Goals**: 有效註冊請求於 2 秒內回應（對應 SC-002）

**Constraints**: 密碼僅以 SHA-256 存儲（專案關係人指定）；Email 全系統唯一；所有欄位後端再次驗證

**Scale/Scope**: 初期單一 Server 部署，符合標準電商流量

---

## Constitution Check

> 以 CLAUDE.md 架構規範作為品質閘門（專案尚未設置 Constitution）

| 閘門 | 狀態 | 說明 |
|---|---|---|
| Domain 不依賴外層 | ✅ PASS | Member entity 純 C#，無 framework 依賴 |
| Application 只使用介面 | ✅ PASS | MemberService 只注入 IMemberRepository |
| Infrastructure 實作 Domain 介面 | ✅ PASS | MemberRepository implements IMemberRepository |
| Controller 只呼叫 Application Service | ✅ PASS | MembersController 只注入 IMemberService |
| 每個 Service 有對應單元測試 | ✅ PASS | MemberServiceTests mock IMemberRepository |

---

## Project Structure

### Documentation (this feature)

```text
specs/001-member-registration/
├── plan.md              ← 此檔案
├── research.md          ← Phase 0 輸出
├── data-model.md        ← Phase 1 輸出
├── contracts/
│   └── api.md           ← Phase 1 輸出
└── tasks.md             ← /speckit-tasks 輸出（尚未建立）
```

### Source Code（Clean Architecture 各層）

```text
MiraiShop.Domain/
├── Entities/
│   └── Member.cs                      ← 新增
└── Interfaces/
    └── IMemberRepository.cs           ← 新增

MiraiShop.Application/
├── DTOs/
│   ├── RegisterMemberRequest.cs       ← 新增
│   └── MemberDto.cs                   ← 新增
├── Interfaces/
│   └── IMemberService.cs              ← 新增
└── Services/
    └── MemberService.cs               ← 新增

MiraiShop.Infrastructure/
└── Repositories/
    └── MemberRepository.cs            ← 新增

MiraiShop.Server/
└── Controllers/
    └── MembersController.cs           ← 新增
    （更新 Program.cs DI 註冊）

MiraiShop.Tests/
└── MemberServiceTests.cs              ← 新增

miraishop.client/src/app/
├── register/
│   ├── register.component.ts          ← 新增
│   ├── register.component.html        ← 新增
│   └── register.component.css         ← 新增
└── services/
    └── member.service.ts              ← 新增
```

**Structure Decision**: 遵循現有 Clean Architecture 分層，後端新增完整的 Member 功能切片（Domain → Application → Infrastructure → Server → Tests），前端新增 register 頁面元件與 API 呼叫 service。
