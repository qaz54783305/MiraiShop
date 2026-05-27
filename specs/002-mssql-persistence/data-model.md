# Data Model: MSSQL 會員資料持久化

**Feature**: `002-mssql-persistence`
**Date**: 2026-05-27

---

## Entity: Member

對應至現有 `MiraiShop.Domain/Entities/Member.cs`，**不修改 Domain Entity**。

### C# Entity（現有，不變）

```csharp
namespace MiraiShop.Domain.Entities;

public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string MailingAddress { get; set; } = string.Empty;
    public string ResidentialAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### MSSQL Table Schema（由 EF Core Migration 建立）

**Table Name**: `Member`（Fluent API 指定，不使用 EF Core 複數化預設 `Members`）

| Column | MSSQL Type | Nullable | Constraint | 說明 |
|--------|-----------|----------|-----------|------|
| `Id` | `UNIQUEIDENTIFIER` | NOT NULL | PRIMARY KEY | 由應用程式產生（`Guid.NewGuid()`） |
| `Name` | `NVARCHAR(MAX)` | NOT NULL | — | 會員姓名 |
| `Email` | `NVARCHAR(256)` | NOT NULL | UNIQUE INDEX | 登入帳號，唯一 |
| `PasswordHash` | `NVARCHAR(MAX)` | NOT NULL | — | SHA-256 雜湊值（64 字元 hex） |
| `MailingAddress` | `NVARCHAR(MAX)` | NOT NULL | — | 通訊地址 |
| `ResidentialAddress` | `NVARCHAR(MAX)` | NOT NULL | — | 住址 |
| `CreatedAt` | `DATETIME2` | NOT NULL | — | UTC 時間，由應用程式設定 |

### Indexes

| Index Name | Columns | Type | 說明 |
|-----------|---------|------|------|
| `PK_Member` | `Id` | PRIMARY KEY | EF Core 自動建立 |
| `IX_Member_Email` | `Email` | UNIQUE | Fluent API 指定，防重複 Email |

---

## EF Core Fluent API 設定（`MiraiShopDbContext.OnModelCreating`）

```csharp
modelBuilder.Entity<Member>(entity =>
{
    entity.ToTable("Member");                    // 指定 table 名稱（單數）

    entity.HasKey(m => m.Id);
    entity.Property(m => m.Id)
          .ValueGeneratedNever();               // 由應用程式產生 Guid

    entity.Property(m => m.Email)
          .HasMaxLength(256)
          .IsRequired();

    entity.HasIndex(m => m.Email)
          .IsUnique();                          // Email 唯一索引

    entity.Property(m => m.Name).IsRequired();
    entity.Property(m => m.PasswordHash).IsRequired();
    entity.Property(m => m.MailingAddress).IsRequired();
    entity.Property(m => m.ResidentialAddress).IsRequired();

    entity.Property(m => m.CreatedAt)
          .IsRequired();
});
```

---

## DbContext 設計

```csharp
// MiraiShop.Infrastructure/Persistence/MiraiShopDbContext.cs
namespace MiraiShop.Infrastructure.Persistence;

public class MiraiShopDbContext : DbContext
{
    public MiraiShopDbContext(DbContextOptions<MiraiShopDbContext> options)
        : base(options) { }

    public DbSet<Member> Members => Set<Member>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API 設定（如上）
    }
}
```

---

## Migration 產生結果（預期）

Migration `InitialCreate` 將產生以下 SQL（示意）：

```sql
CREATE TABLE [Member] (
    [Id]                   UNIQUEIDENTIFIER NOT NULL,
    [Name]                 NVARCHAR(MAX)    NOT NULL,
    [Email]                NVARCHAR(256)    NOT NULL,
    [PasswordHash]         NVARCHAR(MAX)    NOT NULL,
    [MailingAddress]       NVARCHAR(MAX)    NOT NULL,
    [ResidentialAddress]   NVARCHAR(MAX)    NOT NULL,
    [CreatedAt]            DATETIME2        NOT NULL,
    CONSTRAINT [PK_Member] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_Member_Email] ON [Member] ([Email]);
```

---

## 資料流向

```
POST /api/members/register
        ↓
MembersController（不變）
        ↓
MemberService.Register（不變）
  ① SHA-256 加密密碼
  ② 建立 Member entity（含 Guid.NewGuid(), DateTime.UtcNow）
  ③ 呼叫 IMemberRepository.ExistsByEmail → EfMemberRepository → DB 查詢
  ④ 呼叫 IMemberRepository.Add → EfMemberRepository → DB INSERT
  ⑤ 回傳 MemberDto（不含密碼）
        ↓
MiraiShopDbContext（EF Core）
        ↓
MSSQL Member Table（localhost, Windows 驗證）
```
