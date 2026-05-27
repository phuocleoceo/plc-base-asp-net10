# JiPLC - Tài Liệu Kỹ Thuật (Technical Documentation)

> Tài liệu này mô tả chi tiết kiến trúc kỹ thuật, các công nghệ, thư viện, design patterns và quy ước code được sử dụng trong dự án **JiPLC**. Dùng để onboard developer mới, làm tài liệu tham chiếu khi maintain và mở rộng hệ thống.

---

## Mục Lục

1. [Tech Stack Tổng Quan](#1-tech-stack-tổng-quan)
2. [Danh Sách Thư Viện (NuGet Packages)](#2-danh-sách-thư-viện-nuget-packages)
3. [Cấu Trúc Thư Mục Dự Án](#3-cấu-trúc-thư-mục-dự-án)
4. [Kiến Trúc Tổng Thể & Layered Design](#4-kiến-trúc-tổng-thể--layered-design)
5. [Pipeline Khởi Động Ứng Dụng (Bootstrap Flow)](#5-pipeline-khởi-động-ứng-dụng-bootstrap-flow)
6. [Design Patterns Được Áp Dụng](#6-design-patterns-được-áp-dụng)
7. [Database Layer - Entity Framework Core + Dapper](#7-database-layer---entity-framework-core--dapper)
8. [Authentication & Authorization](#8-authentication--authorization)
9. [Caching - Redis](#9-caching---redis)
10. [Message Queue - DotNetCore CAP](#10-message-queue---dotnetcore-cap)
11. [File Storage - AWS S3](#11-file-storage---aws-s3)
12. [Payment Gateway - VNPay](#12-payment-gateway---vnpay)
13. [Logging & Observability](#13-logging--observability)
14. [Error Handling & Response Convention](#14-error-handling--response-convention)
15. [Validation](#15-validation)
16. [Pagination, Filtering & Searching](#16-pagination-filtering--searching)
17. [Configuration Management](#17-configuration-management)
18. [Deployment & Infrastructure](#18-deployment--infrastructure)
19. [Dev Tooling - CSharpier, Husky, EF Migrations](#19-dev-tooling---csharpier-husky-ef-migrations)
20. [Testing](#20-testing)
21. [Quy Ước Code (Coding Conventions)](#21-quy-ước-code-coding-conventions)
22. [Hướng Dẫn Mở Rộng (Adding a New Feature)](#22-hướng-dẫn-mở-rộng-adding-a-new-feature)

---

## 1. Tech Stack Tổng Quan

| Lớp | Công nghệ | Phiên bản |
|---|---|---|
| **Runtime** | .NET | 10.0 |
| **Framework** | ASP.NET Core (Web API) | 10.0 |
| **Ngôn ngữ** | C# (`ImplicitUsings = enable`, nullable không bật ở project chính) | 12+ |
| **CSDL chính** | MySQL | 8.x (ServerVersion.AutoDetect) |
| **ORM** | Entity Framework Core (Pomelo.MySql) | 9.0.0 |
| **Micro-ORM** | Dapper | 2.1.72 |
| **Caching** | Redis (StackExchange.Redis) | latest |
| **Message Queue** | DotNetCore.CAP (In-Memory) | 10.0.1 |
| **Email** | MailKit + MimeKit (SMTP) | 4.15.1 |
| **Object Mapping** | AutoMapper | 16.1.1 |
| **Logging** | Serilog (Console sink) | 10.0.0 |
| **Password Hashing** | BCrypt.Net-Next | 4.1.0 |
| **JWT** | Microsoft.AspNetCore.Authentication.JwtBearer (RS256, X509 cert) | 10.0.5 |
| **API Docs** | Swashbuckle (Swagger UI) | 6.5.0 |
| **Health Check** | AspNetCore.HealthChecks.UI.Client | 9.0.0 |
| **Cloud Storage** | AWS S3 (AWSSDK.S3) | 4.0.19 |
| **Payment** | VNPay (sandbox) — custom implementation | — |
| **Containerization** | Docker, Docker Compose | — |
| **Reverse Proxy** | Nginx (HTTPS + Let's Encrypt) | — |
| **Code Format** | CSharpier | 1.2.6 |
| **Git Hooks** | Husky.Net | 0.9.0 |
| **Unit Test** | xUnit + coverlet.collector | 2.9.3 |

---

## 2. Danh Sách Thư Viện (NuGet Packages)

Các package chính được khai báo trong `src/plcbase.csproj`:

### 2.1. Web & API

| Package | Mục đích sử dụng |
|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Xác thực JWT cho ASP.NET |
| `Swashbuckle.AspNetCore` | Sinh tài liệu OpenAPI/Swagger |
| `AspNetCore.HealthChecks.UI.Client` | Định dạng JSON cho endpoint `/health` |

### 2.2. Data & ORM

| Package | Mục đích sử dụng |
|---|---|
| `Microsoft.EntityFrameworkCore` | EF Core core library |
| `Microsoft.EntityFrameworkCore.Design` | Migration tooling (design-time) |
| `Pomelo.EntityFrameworkCore.MySql` | Provider MySQL cho EF Core |
| `Dapper` | Micro-ORM dùng cho truy vấn raw SQL (nằm trong `DapperContainer`) |

### 2.3. Caching & Message Queue

| Package | Mục đích sử dụng |
|---|---|
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Distributed cache trên Redis |
| `DotNetCore.CAP` | Distributed transactional message bus (event-driven) |
| `DotNetCore.CAP.InMemoryStorage` | Storage in-memory cho CAP (không cần SQL/RabbitMQ) |
| `Savorboard.CAP.InMemoryMessageQueue` | Transport in-memory cho CAP |

### 2.4. AWS

| Package | Mục đích sử dụng |
|---|---|
| `AWSSDK.Extensions.NETCore.Setup` | Tích hợp AWS SDK với DI |
| `AWSSDK.S3` | Upload file lên Amazon S3 |

### 2.5. Common Utilities

| Package | Mục đích sử dụng |
|---|---|
| `AutoMapper` | Mapping giữa Entity ↔ DTO |
| `BCrypt.Net-Next` | Hash password an toàn (BCrypt) |
| `MailKit` | Gửi email qua SMTP |

### 2.6. Logging

| Package | Mục đích sử dụng |
|---|---|
| `Serilog.AspNetCore` | Tích hợp Serilog với ASP.NET pipeline |
| `Serilog.Enrichers.Environment` | Enrich logs với info môi trường |
| `Serilog.Exceptions` | Serialize chi tiết exception |
| `Serilog.Settings.Configuration` | Đọc cấu hình Serilog từ `appsettings.json` |

### 2.7. Tooling (dotnet tools - `.config/dotnet-tools.json`)

| Tool | Mục đích |
|---|---|
| `csharpier` v1.2.6 | Code formatter cho C# |
| `husky` v0.9.0 | Git hooks (chạy CSharpier khi pre-commit) |
| `dotnet-ef` v10.0.5 | EF Core migrations CLI |

---

## 3. Cấu Trúc Thư Mục Dự Án

```
plc-base-asp-net10/
├── .config/                          # dotnet-tools.json
├── .husky/                           # Git hooks (pre-commit chạy CSharpier)
├── .nginx/                           # Nginx config (reverse proxy + SSL)
├── src/
│   ├── Base/                         # Cốt lõi: BaseEntity, BaseRepository, BaseController, ...
│   │   ├── Authorize/                # Attribute auth tuỳ biến
│   │   ├── Controller/               # BaseController + ContextResponse extensions
│   │   ├── DomainModel/              # QueryModel, ReqUser, ReqParam (pagination base)
│   │   ├── DTO/                      # BaseResponse, SuccessResponse, ErrorResponse, PagedList
│   │   ├── Entity/                   # BaseEntity, BaseSoftDeletableEntity, ISoftDeletable
│   │   ├── Error/                    # BaseException
│   │   ├── Filter/                   # ValidateModelFilter (validation tự động)
│   │   └── Repository/               # BaseRepository<T>, IBaseRepository<T>
│   │
│   ├── Certificate/                  # pri.key (RSA private), pub.crt (X.509 cert) cho JWT
│   │
│   ├── Common/                       # Hạ tầng chung
│   │   ├── Data/Context/             # DataContext (DbContext) + EF Configurations (Fluent API)
│   │   ├── Data/Migrations/          # EF Migrations + SQL seed (address, role, user)
│   │   ├── Filters/                  # FilterDIExtension
│   │   ├── Repositories/             # UnitOfWork, DapperContainer, RepositoryDIExtension
│   │   └── Services/                 # ServiceDIExtension
│   │
│   ├── Extensions/                   # Tách logic Program.cs ra thành extension methods
│   │   ├── Builders/                 # WebApplicationBuilder extensions (Serilog)
│   │   ├── Pipelines/                # WebApplication.Use* (CORS, Swagger, ErrorHandler, ...)
│   │   └── ServiceCollections/       # IServiceCollection.Add* (DI registration)
│   │
│   ├── Features/                     # Vertical slice — mỗi feature là một thư mục độc lập
│   │   └── <FeatureName>/
│   │       ├── Controllers/          # API endpoints
│   │       ├── DTOs/                 # Request/Response DTOs + AutoMapper Profile
│   │       ├── Entities/             # EF entity classes
│   │       ├── Repositories/         # IRepository + Repository (kế thừa BaseRepository)
│   │       └── Services/             # IService + Service (chứa business logic)
│   │
│   ├── Middlewares/                  # Custom middleware (JWT, error handler, success logger, ...)
│   ├── Properties/                   # launchSettings.json
│   ├── Shared/                       # Helpers tái sử dụng + Enums + Constants
│   │   ├── Configs/                  # PaginationConfig
│   │   ├── Constants/                # HttpCode, ErrorMessage
│   │   ├── Enums/                    # AppRole, IssueType, TableName, PermissionPolicy, ...
│   │   ├── Helpers/                  # JWT, Redis, S3, SendMail, Logging, Permission, DateTime
│   │   ├── Templates/                # MailConfirm.html, MailRecoverPassword.html
│   │   └── Utilities/                # TimeUtility, JsonUtility, PasswordUtility, ...
│   │
│   ├── appsettings.json              # Cấu hình production
│   ├── appsettings.Development.json  # Override cho môi trường Development
│   ├── Makefile                      # Shortcut commands (`make watch`, `make add-mig`)
│   ├── Program.cs                    # Entry point (3 dòng cực gọn — gọi 3 extension methods)
│   └── plcbase.csproj                # File project
│
├── test/
│   ├── Shared/Utilities/             # Unit test cho utility helpers
│   ├── plctest.csproj                # xUnit test project
│   └── Usings.cs                     # Global usings cho test project
│
├── .csharpierignore
├── .csharpierrc                      # Cấu hình CSharpier (printWidth=100, tabWidth=4)
├── .dockerignore
├── .gitignore
├── docker-compose.yml                # MySQL + Redis + App
├── Dockerfile                        # Multi-stage build (.NET 10 SDK → ASP runtime)
├── plc-base-asp.sln                  # Visual Studio solution
└── README.md                         # README gốc (chỉ chứa command setup)
```

### 3.1. Tổ Chức Theo "Vertical Slice"

Mỗi feature trong `src/Features/` là một module độc lập, gồm đủ Controllers, Services, Repositories, Entities, DTOs cho riêng feature đó. Mục tiêu: gom các thứ cùng thay đổi với nhau vào cùng một chỗ — dễ navigate, dễ refactor, ít va chạm khi nhiều người làm song song.

Trái với "horizontal layered" (gom tất cả controllers vào `Controllers/`, services vào `Services/`...), pattern này phù hợp với codebase có nhiều domain riêng biệt như JiPLC.

---

## 4. Kiến Trúc Tổng Thể & Layered Design

### 4.1. Luồng Một Request

```
┌────────────────────────────────────────────────────────────────────┐
│                        HTTP Request                                  │
└────────────────────────────────┬───────────────────────────────────┘
                                 │
                                 ▼
┌────────────────────────────────────────────────────────────────────┐
│ Pipeline (Middleware Chain) — đã đăng ký ở PipelineExtension.cs    │
│                                                                     │
│  HealthCheck → Migration → CORS → Swagger → SuccessHandler         │
│       → ErrorHandler → CustomAuthResponse → HttpsRedirection       │
│       → Authentication (JwtBearer) → Authorization → JwtMiddleware │
│       → MapControllers                                              │
└────────────────────────────────┬───────────────────────────────────┘
                                 │
                                 ▼
┌────────────────────────────────────────────────────────────────────┐
│ Controller Layer (Features/*/Controllers/*Controller.cs)            │
│ - Validate input qua ValidateModelFilter (auto)                     │
│ - Lấy ReqUser từ HttpContext.Items["reqUser"]                       │
│ - Gọi Service                                                       │
│ - Trả về SuccessResponse<T> qua HttpContext.Success(data)           │
└────────────────────────────────┬───────────────────────────────────┘
                                 │
                                 ▼
┌────────────────────────────────────────────────────────────────────┐
│ Service Layer (Features/*/Services/*Service.cs)                     │
│ - Business logic chính                                              │
│ - Quản lý transaction qua IUnitOfWork.CreateTransaction()           │
│ - Validate business rule + throw BaseException khi cần              │
│ - Publish event vào CAP (ví dụ gửi mail)                            │
│ - Sử dụng AutoMapper để map Entity ↔ DTO                            │
└────────────────────────────────┬───────────────────────────────────┘
                                 │
                                 ▼
┌────────────────────────────────────────────────────────────────────┐
│ Repository Layer (Features/*/Repositories/*Repository.cs)           │
│ - Kế thừa BaseRepository<TEntity>                                   │
│ - Truy vấn EF Core qua QueryModel<T>                                │
│ - Hoặc raw SQL qua DapperContainer                                  │
└────────────────────────────────┬───────────────────────────────────┘
                                 │
                                 ▼
┌────────────────────────────────────────────────────────────────────┐
│ Data Source                                                         │
│  - MySQL (qua DataContext)                                          │
│  - Redis (qua IRedisHelper)                                         │
│  - AWS S3 (qua IS3Helper)                                           │
│  - SMTP (qua ISendMailHelper, async qua CAP)                        │
│  - VNPay (qua HTTP outbound, dùng IHttpClientFactory)               │
└────────────────────────────────────────────────────────────────────┘
```

### 4.2. Phân Tách Trách Nhiệm Giữa Các Layer

| Layer | Trách nhiệm | Không được làm |
|---|---|---|
| **Controller** | Nhận HTTP request, validate input, gọi service, trả về response | Không chứa business logic, không gọi DB trực tiếp |
| **Service** | Business logic, validate business rule, quản lý transaction, orchestration | Không expose HTTP-specific (e.g. `HttpContext`), không nhúng SQL trực tiếp |
| **Repository** | Truy cập DB (EF / Dapper), khai báo query reusable | Không chứa business logic |
| **Helper** | Wrapper cho thư viện thứ ba (JWT, Redis, S3, SMTP, ...) | Không phụ thuộc vào tầng cao hơn |
| **Utility** | Hàm static thuần (no DI), tính toán đơn giản | Không phụ thuộc vào DI container |

---

## 5. Pipeline Khởi Động Ứng Dụng (Bootstrap Flow)

### 5.1. `Program.cs` Tối Giản

`Program.cs` chỉ có 3 lệnh — toàn bộ logic được tách ra extension methods:

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Service collection — đăng ký DI
builder.Services.ConfigureService(builder.Configuration);

// Builder — cấu hình Serilog
builder.ConfigureBuilder(builder.Configuration);

WebApplication app = builder.Build();

// HTTP pipeline — đăng ký middleware
app.ConfigurePipeline();

app.Run();
```

### 5.2. `ConfigureService` (Service Collection Phase)

File: `src/Extensions/ServiceCollections/ServiceExtension.cs`

Thứ tự đăng ký (quan trọng cho dependency resolution):

1. **CORS** — `AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()` (default policy)
2. **Health Check** — `services.AddHealthChecks()`
3. **CAP Queue** — In-memory storage + in-memory transport
4. **Database** — `AddDbContext<DataContext>` với MySQL connection (auto-detect server version)
5. **Cache** — Redis distributed cache + `IConnectionMultiplexer` (singleton)
6. **Data Factory** — AutoMapper, AppSettings (`IOptions<T>`), Helpers, Filters, Repositories, Services
7. **Authentication** — JWT Bearer với RSA public key
8. **HTTP Client** — `services.AddHttpClient()` (cho các call ngoài)
9. **Controllers** — `services.AddControllers()` + `RouteOptions.LowercaseUrls = true`
10. **Swagger** — Với security definition Bearer
11. **AWS** — S3 client với `BasicAWSCredentials`

### 5.3. `ConfigurePipeline` (Middleware Phase)

File: `src/Extensions/Pipelines/PipelineExtension.cs`

Thứ tự middleware (rất quan trọng — sai thứ tự sẽ vỡ behavior):

```
1.  app.UseHealthCheck()              → endpoint /health
2.  app.UseMigration()                → tự động chạy EF migration khi start
3.  app.UseCorsPipeline()             → cho phép mọi origin (DEV/STAGE; prod nên siết lại)
4.  app.UseSwaggerPipeline()          → /swagger UI luôn bật (cả prod)
5.  app.UseResponseHandlerPipeline()  → SuccessHandler + ErrorHandler + CustomAuthResponse
6.  app.UseHttpsRedirection()
7.  app.UseAuthentication()           → built-in JwtBearer
8.  app.UseAuthorization()            → check [Authorize] / Role
9.  app.UseMiddleware<JwtMiddleware>()→ extract reqUser → HttpContext.Items["reqUser"]
10. app.MapControllers()
```

### 5.4. `UseMigration` — Auto-Migrate Khi Start

Mỗi lần app khởi động, code sẽ tự gọi `context.Database.Migrate()` để đảm bảo schema DB đồng bộ với code. Tiện cho dev/staging, nhưng **production nên xem xét** việc chạy migration thủ công (CI/CD) để kiểm soát rủi ro.

---

## 6. Design Patterns Được Áp Dụng

### 6.1. Repository + Unit of Work

**Mục đích**: Trừu tượng hoá tầng truy cập DB, gom nhiều repository thành một "đơn vị làm việc" để dùng chung một `DbContext` và quản lý transaction tập trung.

**Triển khai**:
- `IBaseRepository<T>` / `BaseRepository<T>`: CRUD chuẩn cho mọi entity (Add, Update, Remove, FindById, GetMany, GetPaged, GetOne, Any, Count, SoftDelete).
- Mỗi feature có repository riêng kế thừa `BaseRepository<TEntity>` và thêm method chuyên biệt (vd: `IssueRepository.MoveIssueToBacklog`).
- `IUnitOfWork`: tổng hợp tất cả repository, expose `Save()`, `CreateTransaction()`, `CommitTransaction()`, `AbortTransaction()`.

**Ví dụ sử dụng transaction**:

```csharp
try
{
    await uow.CreateTransaction();

    uow.Project.Add(project);
    await uow.Save();

    uow.ProjectMember.Add(new ProjectMemberEntity { ... });
    await uow.Save();

    await uow.CommitTransaction();
}
catch (BaseException)
{
    await uow.AbortTransaction();
    throw;
}
```

### 6.2. Specification-like QueryModel

Thay vì để repository expose hàng chục method (`GetByName`, `GetByEmail`, ...), pattern này gom mọi criteria vào một object `QueryModel<T>`:

```csharp
public class QueryModel<T>
{
    public List<Expression<Func<T, bool>>> Filters { get; set; } = [];
    public List<Expression<Func<T, object>>> Includes { get; set; } = [];
    public Func<IQueryable<T>, IOrderedQueryable<T>> OrderBy { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool Tracking { get; set; } = false;
}
```

Service compose query một cách declarative:

```csharp
var query = new QueryModel<IssueEntity>
{
    OrderBy = c => c.OrderBy(i => i.ProjectStatusIndex),
    Filters = { i => i.ProjectId == projectId && i.DeletedAt == null },
    Includes = { i => i.Assignee.UserProfile },
};
var issues = await uow.Issue.GetManyAsync<IssueBoardDTO>(query);
```

`GetManyAsync<TDto>` tự động `ProjectTo<TDto>()` qua AutoMapper (nếu `TDto != TEntity`) → giảm round-trip data.

### 6.3. Dependency Injection (Built-In)

Toàn bộ DI dùng `Microsoft.Extensions.DependencyInjection` (built-in). Convention:

| Loại | Lifetime | Vì sao |
|---|---|---|
| `DbContext` | Scoped (mặc định EF) | Một context per request |
| `UnitOfWork` | Scoped | Wrap DbContext per request |
| Services (`I*Service`) | Scoped | Cùng vòng đời với UnitOfWork |
| Helpers (JWT, Redis, S3, ...) | **Singleton** | Không state, hoặc state thread-safe |
| `IAmazonS3`, `IConnectionMultiplexer` | Singleton | Pool connection, không tạo mỗi request |
| `ValidateModelFilter` | Scoped | Cần ApplyToActionExecutingContext |

Đăng ký tập trung qua 4 extension methods:
- `ConfigureRepositoryDI()` — chỉ register UnitOfWork (repositories được khởi tạo bên trong UoW constructor)
- `ConfigureServiceDI()` — register tất cả service interfaces
- `ConfigureHelperDI()` — register tất cả helper singletons
- `ConfigureFilterDI()` — register `ValidateModelFilter`

### 6.4. Extension Method Pattern

Toàn bộ setup được tách thành extension methods trên `IServiceCollection` / `WebApplicationBuilder` / `WebApplication`. Lợi ích:

- `Program.cs` ngắn gọn, dễ đọc
- Mỗi concern (CORS, JWT, Swagger, ...) một file riêng → tìm code nhanh
- Dễ disable/enable tính năng (chỉ cần comment một dòng gọi extension)

### 6.5. Outbox-like Event via CAP

Khi cần gửi email, code không gửi đồng bộ. Thay vào đó publish event vào CAP:

```csharp
await capPublisher.PublishAsync(WorkerType.SEND_MAIL, mailContent);
```

Worker (`WorkerController.SendMail`) là một subscriber:

```csharp
[NonAction]
[CapSubscribe(WorkerType.SEND_MAIL)]
public async Task SendMail(MailContent mailContent)
{
    await sendMailHelper.SendEmailAsync(mailContent);
}
```

→ API response không bị block bởi I/O SMTP. Vì CAP dùng in-memory storage, message **không persist** giữa các lần restart — phù hợp với scale 1 instance hiện tại.

### 6.6. Custom Response Convention (HttpContext.Success/Failure)

Thay vì `return Ok(...)` / `return BadRequest(...)`, mọi controller dùng pattern:

```csharp
return HttpContext.Success(await service.DoSomething(...));
// hoặc
return HttpContext.Failure();  // → throw BaseException → ErrorHandler bắt
```

`HttpContext.Success<T>(data)` set `Response.StatusCode = 200` và wrap data vào `SuccessResponse<T>`. `HttpContext.Failure()` ném `BaseException` để middleware `ErrorHandler` format JSON.

### 6.7. AutoMapper Profiles

Mỗi feature có một file `*Mapping.cs` định nghĩa các mapping rules cho feature đó:

```csharp
public class AuthMapping : Profile
{
    public AuthMapping()
    {
        CreateMap<UserRegisterDTO, UserAccountEntity>();
        CreateMap<UserRegisterDTO, UserProfileEntity>();
    }
}
```

Tất cả profile được auto-scan khi gọi `services.AddAutoMapper(typeof(Program).Assembly)`.

---

## 7. Database Layer - Entity Framework Core + Dapper

### 7.1. Code-First với EF Core 9 + Pomelo

Schema được generate từ C# classes thông qua EF Core migrations:

```bash
dotnet ef migrations add <MigrationName> -o Common/Data/Migrations
dotnet ef database update
# hoặc qua Makefile:
make add-mig MGR_NAME=<MigrationName>
make db-update
```

### 7.2. Entity Configuration (Fluent API)

Cấu hình nâng cao (index, relationships phức tạp) đặt ở `Common/Data/Context/Configuration/*Configuration.cs`. Ví dụ `IssueConfiguration`:

```csharp
public class IssueConfiguration : IEntityTypeConfiguration<IssueEntity>
{
    public void Configure(EntityTypeBuilder<IssueEntity> builder)
    {
        builder.HasIndex(c => new { c.ProjectId, c.AssigneeId, c.SprintId });
    }
}
```

Tất cả configuration được apply trong `ContextConfigurationExtension.ApplyEntityConfigurations()` rồi gọi từ `DataContext.OnModelCreating`.

### 7.3. Conventions Đặc Biệt Của Project

| Convention | Mô tả |
|---|---|
| **Table name snake_case** | Tên bảng định nghĩa qua `[Table(TableName.XXX)]` (vd: `user_account`, `project_member`) |
| **Column name snake_case** | Mọi property entity có `[Column("...")]` để map tên cột |
| **PK = `id` (int auto-increment)** | Inherit từ `BaseEntity.Id` |
| **Audit columns** | `created_at`, `updated_at` tự động set trong `DataContext.SaveChangesAsync` qua `ChangeTracker` |
| **Soft delete** | Entity kế thừa `BaseSoftDeletableEntity` implement `ISoftDeletable` — repository tự set `DeletedAt = TimeUtility.Now()` qua method `SoftDelete()` |
| **No tracking by default** | `QueryModel.Tracking = false` mặc định — tránh memory leak, giảm overhead |
| **Auto-include navigation properties** | Qua `Includes` của QueryModel — Service tự khai báo, không cấu hình global |

### 7.4. Dapper - Khi Cần Raw SQL

`DapperContainer` (`Common/Repositories/DapperContainer.cs`) wrap `IDbConnection` từ `DataContext.Database.GetDbConnection()`, expose:

- `Execute(dapperQuery)` — non-query
- `List<T>(dapperQuery)` — multiple rows
- `List<T1, T2>(dapperQuery)` — multi-result set
- `OneRecord<T>()` — single row
- `Single<T>()` — scalar

Dapper hiện tại chưa được dùng rộng trong codebase — chỉ tồn tại như một "escape hatch" cho query phức tạp khi EF không tối ưu.

### 7.5. Migration & Seed Data

- Migrations: `src/Common/Data/Migrations/*.cs` (sinh tự động bằng `dotnet ef`)
- Seed data thủ công bằng SQL: `src/Common/Data/Migrations/SQL/`
  - `1.address.sql` — danh sách 63 tỉnh thành Việt Nam
  - `2.role.sql` — seed admin role
  - `3.user.sql` — seed tài khoản admin đầu tiên

> Lưu ý: Các file SQL **không được chạy tự động** bởi EF — phải chạy thủ công bằng tay sau khi chạy `db-update` lần đầu.

### 7.6. Connection String — Multi-Environment

`appsettings.json` định nghĩa nhiều connection string, chọn cái nào bằng `SelectedDatabase`:

```json
{
  "SelectedDatabase": "Remote",
  "ConnectionStrings": {
    "Local": "server=localhost; port=3306; ...",
    "Remote": "server=plchost.ddns.net; ..."
  }
}
```

Trong `docker-compose.yml` thì override qua biến môi trường: `SelectedDatabase=Local`, `ConnectionStrings:Local=...`.

---

## 8. Authentication & Authorization

### 8.1. JWT với RSA (RS256)

JiPLC dùng JWT ký bằng **RSA SHA-256** — không phải HMAC SHA-256 (HS256). Lợi ích:

- Private key (`pri.key`) chỉ server backend giữ
- Public key (`pub.crt`) có thể chia sẻ cho client / verifier khác mà không lo lộ secret

**Sinh cặp khóa**:

```bash
openssl genpkey -algorithm RSA -out pri.key
openssl req -new -key pri.key -out pub.csr
openssl x509 -req -in pub.csr -signkey pri.key -out pub.crt
```

File `pri.key` và `pub.crt` được copy vào output build (`<CopyToOutputDirectory>Always</CopyToOutputDirectory>` trong .csproj).

### 8.2. Cấu Trúc Token

| Loại | Lifetime | Lưu trữ ở đâu |
|---|---|---|
| **Access Token** | 900s (15 phút) | Frontend (local/session storage) |
| **Refresh Token** | 2.592.000s (30 ngày) | DB: `user_account.refresh_token` + `refresh_token_expired_at` |

**Claims trong token** (xem `CustomClaimTypes.cs`):

- `UserId` — id user
- `Email` — email user
- `Role` — tên role hệ thống (admin/user)
- `Type` — `access_token` hoặc `refresh_token`

### 8.3. JwtMiddleware - Extract Request User

File: `src/Middlewares/JwtMiddleware.cs`

Middleware này chạy SAU `UseAuthentication` và `UseAuthorization`. Nó parse JWT từ header `Authorization: Bearer <token>`, decode claims và lưu vào `HttpContext.Items["reqUser"]` dưới dạng `ReqUser`:

```csharp
context.Items["reqUser"] = new ReqUser
{
    Id = Convert.ToInt32(claims.First(x => x.Type == CustomClaimTypes.UserId).Value),
    Email = claims.First(x => x.Type == CustomClaimTypes.Email).Value,
};
```

Controllers lấy ra qua extension:

```csharp
ReqUser reqUser = HttpContext.GetRequestUser();
```

### 8.4. Authorization Levels

JiPLC có 2 cấp authorization:

#### a. Built-in ASP.NET Role-Based
```csharp
[Authorize]                              // chỉ cần đăng nhập
[Authorize(Roles = AppRole.ADMIN)]       // chỉ admin hệ thống
```

#### b. Project-Level Permission (Tự xây)
- Định nghĩa key theo format `Module.Action` (vd: `Issue.Create`, `Sprint.Delete`) — xem `PermissionPolicy.cs`
- Permission của một Project Role được cache trong Redis (xem `ProjectPermissionService.GetPermissionKeysOfRole`)
- Leader của project **bypass** mọi check permission

Hiện tại check permission **không phải attribute** mà gọi explicit trong service (qua `IProjectPermissionService`). Đây là điểm có thể nâng cấp thành `[ProjectPermissionAttribute]` filter.

### 8.5. Refresh Token Rotation

Mỗi lần gọi `POST /Refresh-Token`, server:
1. Decode access token cũ (kể cả expired) để lấy claims
2. Compare refresh token gửi lên với refresh token trong DB
3. Sinh **cặp token mới** (access + refresh) và update vào DB → mỗi refresh token chỉ dùng được 1 lần

→ Đây là **refresh token rotation pattern**, giảm rủi ro nếu refresh token bị leak.

### 8.6. Password Security - BCrypt

`PasswordUtility` wrap `BCrypt.Net-Next`:

```csharp
public static string GetPasswordHash(string password)
    => BC.HashPassword(password);  // workFactor mặc định = 11

public static bool IsValidPassword(string password, string hash)
    => BC.Verify(password, hash);
```

BCrypt tự sinh salt và lưu trong hash → không cần lưu salt riêng.

---

## 9. Caching - Redis

### 9.1. Cấu Trúc Key

Tất cả key Redis tuân theo pattern (xem `RedisUtility.cs`):

```
plc-base:<entity-class-name-lowercase>:<postfix>
```

Ví dụ:
- `plc-base:configsettingdto:list` — toàn bộ list ConfigSetting
- `plc-base:configsettingdto:free_project` — single ConfigSetting theo key
- `plc-base:projectroledto:permissions` — Hash chứa permission của các project role

Pattern này giúp `ClearByPattern("plc-base:*configsettingdto*")` xoá toàn bộ cache của một entity khi có thay đổi.

### 9.2. Cache Strategies

`RedisHelper` cung cấp các strategy:

| Method | Mô tả |
|---|---|
| `Get<T>(key)` / `Set<T>(key, obj)` | Read/Write thô (string key-value) |
| `SetWithTtl<T>(key, obj)` | Set với TTL (absolute + sliding expiration = TTL/2) |
| `GetCachedOr<T>(key, supplier)` | **Cache-aside pattern**: nếu hit trả luôn; miss → gọi `supplier()` → cache lại |
| `GetListCache<T>` / `SetListCache<T>` | Redis List (LPUSH, RPUSH) |
| `GetMapCache<T>` / `SetMapCache<T>` | Redis Hash — dùng cho cache map nhiều entry (vd: permission của nhiều role một lúc) |
| `ClearByPattern(pattern)` | Scan keys theo pattern và xoá hàng loạt |

### 9.3. Cache Invalidation Strategy

Convention: **mọi write operation phải invalidate cache liên quan**. Ví dụ trong `ConfigSettingService.UpdateForKey`:

```csharp
mapper.Map(configSettingUpdateDTO, configSettingDb);
uow.ConfigSetting.Update(configSettingDb);

await redisHelper.ClearByPattern(RedisUtility.GetClearKey<ConfigSettingDTO>());
return await uow.Save();
```

Trong `ProjectPermissionService` (cache theo Map Hash):

```csharp
await redisHelper.RemoveMapCache(
    GetPermissionKeysOfRoleRedisKey(),
    projectRoleId.ToString()
);
```

### 9.4. Cache Settings

`appsettings.json`:

```json
"CacheSettings": {
  "Enable": true,                            // global switch
  "Expires": 86400,                          // 24 giờ
  "ConnectionString": "plchost.ddns.net:6379"
}
```

Khi `Enable = false` → `GetCachedOr` skip cache hoàn toàn → tiện debug khi nghi ngờ cache có vấn đề.

### 9.5. StackExchange.Redis vs IDistributedCache

`RedisHelper` dùng **cả hai**:

- `IDistributedCache` — cho operation đơn giản (string key-value) → tận dụng tích hợp `Microsoft.Extensions.Caching.StackExchangeRedis`
- `IConnectionMultiplexer.GetDatabase()` — cho List & Hash operations (`ListRange`, `HashSet`, `HashGet`)

---

## 10. Message Queue - DotNetCore CAP

### 10.1. Tại Sao CAP?

CAP (Consistency and Availability for distributed systems) là một thư viện .NET cho event bus + outbox pattern. JiPLC dùng nó để **chạy task bất đồng bộ** (gửi mail) mà không block API response.

### 10.2. Configuration (In-Memory)

```csharp
services.AddCap(capOptions =>
{
    capOptions.UseInMemoryStorage();
    capOptions.UseInMemoryMessageQueue();
});
```

- **Storage**: tin nhắn lưu in-process — restart sẽ mất queue chưa xử lý
- **Transport**: trong cùng process, không cần broker ngoài

→ Đủ dùng cho 1 instance hiện tại. Khi scale ra nhiều instance, cần đổi sang `UseSqlServer/UseMySql + UseRabbitMQ/UseKafka`.

### 10.3. Publish & Subscribe

**Publisher** (`AuthMailService.SendMailBackground`):

```csharp
await capPublisher.PublishAsync(WorkerType.SEND_MAIL, mailContent);
```

**Subscriber** (`WorkerController.SendMail`):

```csharp
[NonAction]
[CapSubscribe(WorkerType.SEND_MAIL)]
public async Task SendMail(MailContent mailContent)
{
    await sendMailHelper.SendEmailAsync(mailContent);
}
```

Worker chạy trong cùng process — CAP route message theo subscription key (`WorkerType.SEND_MAIL = "send.mail"`).

---

## 11. File Storage - AWS S3

### 11.1. Hai Phương Thức Upload

#### a. Direct Upload (`POST /api/upload-file`)
- Frontend gửi file trực tiếp lên backend dưới dạng `multipart/form-data`
- Backend upload lên S3 qua `PutObjectAsync`
- Trả về S3 object key (URL)

```csharp
public async Task<string> UploadFile(S3FileUpload file)
{
    PutObjectRequest request = new()
    {
        BucketName = _awsSettings.S3.Bucket,
        Key = file.FilePath,
        InputStream = file.FileStream,
    };
    await _s3Client.PutObjectAsync(request);
    return AWSUtility.GetObjectKey(file.FilePath, _awsSettings);
}
```

Nhược điểm: file đi qua backend → tốn băng thông + RAM của server.

#### b. Presigned URL Upload (`POST /api/presigned-upload-url`)
- Backend trả về URL có chữ ký (expire sau `PresignedUrlExpires = 600s`)
- Frontend dùng URL này gửi `PUT` trực tiếp lên S3 (không qua backend)
- Backend chỉ lưu metadata sau khi frontend gọi báo upload xong

→ Phương thức ưu việt hơn cho file lớn.

### 11.2. Cấu Hình AWS

```json
"AWSSettings": {
  "AccessKey": "",
  "SecretKey": "",
  "Region": "ap-southeast-1",
  "S3": {
    "Bucket": "plc-base",
    "PresignedUrlExpires": 600
  },
  "CloudFront": {
    "Enable": true,
    "Domain": ""
  }
}
```

`CloudFront.Enable = true` → các URL trả về sẽ qua CloudFront CDN thay vì link S3 trực tiếp (giảm latency, ẩn bucket name).

### 11.3. Polymorphic Media Linking

`MediaEntity` không có foreign key đến entity cụ thể. Thay vào đó dùng cặp `(EntityType, EntityId)`:

```csharp
public class MediaEntity : BaseEntity
{
    public string ContentType { get; set; }
    public string Url { get; set; }
    public string EntityType { get; set; }  // "issue" hoặc "comment"
    public int EntityId { get; set; }
}
```

→ Một media table phục vụ cho nhiều loại entity. Đánh đổi: mất referential integrity ở DB level, nhưng linh hoạt.

---

## 12. Payment Gateway - VNPay

### 12.1. Flow Tổng Quan

1. **Tạo payment** (`POST /api/payment`):
   - Generate `vnp_TxnRef` từ `DateTime.Now.Ticks` (unique)
   - Lưu `PaymentEntity` với status pending
   - Build URL VNPay với hash secure (HMAC-SHA512)
   - Trả URL cho frontend redirect user sang VNPay

2. **User thanh toán trên VNPay** → VNPay redirect về `ReturnUrl` của frontend với query params:
   - `vnp_TxnRef`, `vnp_Amount`, `vnp_ResponseCode`, `vnp_TransactionStatus`, `vnp_SecureHash`, ...

3. **Submit payment** (`PUT /api/payment`):
   - Frontend parse query params từ VNPay → gửi backend
   - Backend verify `vnp_ResponseCode == "00"` và `vnp_TransactionStatus == "00"`
   - Cập nhật `PaymentEntity` (chống double-submit qua check `vnp_TransactionStatus`)
   - Cộng credit vào `UserProfile.CurrentCredit`

### 12.2. Custom VNPay Library

File: `src/Shared/Helpers/VNPAY/VNPSettings.cs` + helper functions (`VNPLibrary`, `VNPHistory`).

```csharp
public class VNPSettings
{
    public string BaseUrl { get; set; }      // https://sandbox.vnpayment.vn/...
    public string TmnCode { get; set; }      // mã website (do VNPay cấp)
    public string HashSecret { get; set; }   // secret để hash
    public string Command { get; set; }      // "pay"
    public string CurrCode { get; set; }     // "VND"
    public string Version { get; set; }      // "2.1.0"
    public string Locale { get; set; }       // "vn"
    public string ReturnUrl { get; set; }    // URL frontend nhận callback
}
```

### 12.3. Anti Double-Submit

Trước khi cộng credit, code kiểm tra:

```csharp
if (paymentEntity.vnp_TransactionStatus == PaymentStatus.VNP_TRANSACTION_STATUS_SUCCESS)
    throw new BaseException(HttpCode.BAD_REQUEST, "payment_already_handled");
```

> **Lưu ý bảo mật**: Hiện tại code đang **comment out** đoạn verify `vnp_SecureHash` — đây là rủi ro, ai có `vnp_TxnRef` cũng có thể giả lập callback. Production cần bật check này lại.

---

## 13. Logging & Observability

### 13.1. Serilog

`appsettings.json` định nghĩa cấu hình Serilog:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Verbose",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware": "Fatal",
      "Microsoft.EntityFrameworkCore": "Fatal",
      "PlcBase": "Information",
      "Savorboard": "Error",
      "DotNetCore.CAP": "Error"
    }
  },
  "Enrich": ["FromLogContext", "WithExceptionDetails"],
  "WriteTo": [{ "Name": "Console" }]
}
```

→ Hiện chỉ ghi log ra Console (đọc qua `docker logs` / `journalctl`). Có thể thêm File sink, Seq, ELK sau.

### 13.2. Request Logging Middleware

`SuccessHandler` middleware log mỗi request thành công với format:

```
LogContent { Path, Method, Params, Message, StatusCode }
```

`ErrorHandler` log mỗi request lỗi (level `LogError`).

### 13.3. Health Check

Endpoint: `GET /health`

Response format: AspNetCore.HealthChecks.UI compatible JSON (qua `UIResponseWriter.WriteHealthCheckUIResponse`).

Hiện chỉ check liveness (server có chạy không). Có thể mở rộng `services.AddHealthChecks()` để check thêm:
- `.AddMySql(connectionString)` — kết nối DB
- `.AddRedis(connectionString)` — kết nối Redis
- `.AddS3(...)` — kết nối S3

---

## 14. Error Handling & Response Convention

### 14.1. Response Format Chuẩn

**Thành công**:
```json
{
  "success": true,
  "statusCode": 200,
  "data": { ... }
}
```

**Lỗi**:
```json
{
  "success": false,
  "statusCode": 400,
  "message": "account_not_found",
  "errors": null
}
```

**Lỗi validation** (status 422):
```json
{
  "success": false,
  "statusCode": 422,
  "message": "validation_error",
  "errors": {
    "Email": ["The Email field is required."],
    "Password": ["The Password field is required."]
  }
}
```

### 14.2. BaseException - Custom Exception

```csharp
public class BaseException(
    int statusCode = HttpCode.INTERNAL_SERVER_ERROR,
    string message = "",
    Dictionary<string, string[]> errors = null
) : Exception(message);
```

Mọi business error đều `throw new BaseException(HttpCode.NOT_FOUND, "user_not_found")`. Middleware `ErrorHandler` bắt:

```csharp
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        var exception = exceptionHandlerFeature.Error;
        var statusCode = exception is BaseException be ? be.StatusCode : 500;
        var message = exception.Message;
        var errors = exception is BaseException be2 ? be2.Errors : null;

        logger.LogErrorResponse(context, message, statusCode);
        await context.WriteErrorResponse(message, statusCode, errors);
    });
});
```

### 14.3. CustomAuthResponse — Convert 401/403 thành BaseException

Khi ASP.NET trả 401/403 (do `[Authorize]` fail), middleware `CustomAuthResponse` convert thành `BaseException` để response format đồng nhất:

```csharp
case HttpCode.UNAUTHORIZED:
    throw new BaseException(HttpCode.UNAUTHORIZED, ErrorMessage.UNAUTHORIZED_USER);

case HttpCode.FORBIDDEN:
    throw new BaseException(HttpCode.FORBIDDEN, ErrorMessage.FORBIDDEN_RESOURCE);
```

### 14.4. HTTP Status Code Convention

Constants tại `Shared/Constants/HttpCode.cs`:

| Mã | Khi nào dùng |
|---|---|
| 200 OK | Thành công |
| 400 BAD_REQUEST | Business rule fail (vd: `email_existed`, `not_enough_credit`) |
| 401 UNAUTHORIZED | Token sai/thiếu |
| 403 FORBIDDEN | Token đúng nhưng không có quyền |
| 404 NOT_FOUND | Resource không tồn tại |
| 422 UNPROCESSABLE_ENTITY | Validation lỗi (sai format input) |
| 500 INTERNAL_SERVER_ERROR | Exception không xác định (catch-all) |

---

## 15. Validation

### 15.1. ValidateModelFilter — Auto-Validate Input

File: `src/Base/Filter/ValidateModelFilter.cs`

Filter này chạy trước action method. Nếu `ModelState.IsValid == false`, throw `BaseException` với status 422:

```csharp
public void OnActionExecuting(ActionExecutingContext context)
{
    if (context.ModelState.IsValid) return;

    var errors = context.ModelState
        .Where(x => x.Value.Errors.Count > 0)
        .ToDictionary(k => k.Key, k => k.Value.Errors.Select(e => e.ErrorMessage).ToArray());

    throw new BaseException(HttpCode.UNPROCESSABLE_ENTITY, ErrorMessage.VALIDATION_ERROR, errors);
}
```

Filter này được attach lên `BaseController` qua `[ServiceFilter(typeof(ValidateModelFilter))]` → áp dụng cho mọi controller.

### 15.2. Disable Auto-Validation Built-In

Built-in của ASP.NET sẽ trả 400 BadRequest mặc định khi ModelState invalid → conflict với custom logic. Disable nó:

```csharp
services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
```

### 15.3. Validation Attributes

Hiện DTOs trong codebase **chưa khai báo nhiều validation attributes** (như `[Required]`, `[EmailAddress]`, `[MinLength]`). Khi mở rộng nên thêm cho rõ ràng:

```csharp
public class UserRegisterDTO
{
    [Required, EmailAddress] public string Email { get; set; }
    [Required, MinLength(8)] public string Password { get; set; }
    // ...
}
```

---

## 16. Pagination, Filtering & Searching

### 16.1. ReqParam Base Class

Mọi query param có pagination kế thừa `ReqParam`:

```csharp
public abstract class ReqParam
{
    private int _pageNumber = 1;
    private int _pageSize = 10;
    private string _searchValue = "";

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = Math.Max(value, PaginationConfig.MinPageNumber);   // ≥ 1
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Min(value, PaginationConfig.MaxPageSize);       // ≤ 50
    }

    public string SearchValue { get; set; }
}
```

→ Tự động clamp giá trị, tránh user request `pageSize=10000`.

### 16.2. PagedList<T> Response

```csharp
public class PagedList<T>
{
    public List<T> Records { get; set; }
    public int TotalRecords { get; set; }
}
```

→ Frontend tự tính số trang: `Math.Ceiling(TotalRecords / PageSize)`.

### 16.3. Search Pattern

Search hiện làm bằng `Contains` (LIKE %x%) — không dùng full-text search:

```csharp
if (!string.IsNullOrWhiteSpace(projectParams.SearchValue))
{
    string searchValue = projectParams.SearchValue.ToLower();
    projectQuery.Filters.Add(p =>
        p.Name.ToLower().Contains(searchValue) || p.Key.ToLower().Contains(searchValue)
    );
}
```

→ Đủ dùng cho dataset nhỏ. Khi data lớn nên dùng MySQL FULLTEXT INDEX hoặc Elasticsearch.

---

## 17. Configuration Management

### 17.1. AppSettings Sections

Mỗi nhóm config có một class POCO trong `Shared/Helpers/*/*Settings.cs`, được bind qua `IOptions<T>`:

```csharp
services.Configure<DateTimeSettings>(configuration.GetSection("DateTimeSettings"));
services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
services.Configure<VNPSettings>(configuration.GetSection("VNPSettings"));
services.Configure<AWSSettings>(configuration.GetSection("AWSSettings"));
services.Configure<ClientAppSettings>(configuration.GetSection("ClientAppSettings"));
services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
```

Inject bằng `IOptions<T>`:

```csharp
public class PaymentService(IOptions<VNPSettings> vnpSettings, ...)
{
    private readonly VNPSettings _vnpSettings = vnpSettings.Value;
}
```

### 17.2. Environment Variables Override

ASP.NET Core tự động đọc env var theo convention `Section__SubKey`:

```yaml
# docker-compose.yml
environment:
  - "SelectedDatabase=Local"
  - "ConnectionStrings:Local=server=plc-mysql; port=3306; ..."
  - "CacheSettings:ConnectionString=plc-redis:6379"
```

→ Cùng key đè lên giá trị trong `appsettings.json`.

### 17.3. Secrets Trong Code (Cảnh Báo)

Hiện tại `appsettings.json` đang lộ:
- Password DB hardcode (`@dmin1234`)
- Mail password trống nhưng nếu fill sẽ commit nhầm
- VNPay HashSecret, AWS AccessKey/SecretKey

→ Production nên:
- Move secrets sang Azure Key Vault / AWS Secrets Manager / Hashicorp Vault
- Hoặc dùng `dotnet user-secrets` cho dev
- Hoặc dùng `.env` file + `dotnet-env`

---

## 18. Deployment & Infrastructure

### 18.1. Dockerfile (Multi-stage Build)

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /app
COPY src/*.csproj .
RUN dotnet restore
COPY .config/ ./.config
RUN dotnet tool restore
COPY src/ .
RUN dotnet publish -c Release -o out

# Run stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/out .
ENTRYPOINT ["dotnet", "plcbase.dll"]
```

- **Builder image**: ~700 MB (SDK + tools)
- **Runtime image**: ~200 MB (chỉ ASP.NET runtime, không có SDK)
- Build linux/amd64: `docker build --no-cache --platform=linux/amd64 -t phuocleoceo/plc-base .`

### 18.2. docker-compose.yml — Local Stack

3 services trong cùng compose:

| Service | Image | Mục đích |
|---|---|---|
| `redis` | `redis` | Cache (port 6379) |
| `mysql` | `mysql` | DB (port 3306, password `@dmin1234`) |
| `plc-service` | build từ Dockerfile | App backend (port 7133 → 80) |

```bash
docker compose up --build
```

### 18.3. Nginx Reverse Proxy + Let's Encrypt

File `.nginx/default` chứa config nginx cho production:

- Listen port 80 → redirect 301 sang HTTPS
- Listen port 443 với SSL từ Let's Encrypt (`/etc/letsencrypt/live/plchost.ddns.net/`)
- Proxy đến upstream `localhost:7133` (container app)
- Forward các header: `Host`, `X-Real-IP`, `X-Forwarded-For`, `X-Forwarded-Proto`
- `client_max_body_size 100M` → cho phép upload file đến 100MB
- Support `.well-known/acme-challenge/` để Let's Encrypt renew cert

Có thể scale ra nhiều instance bằng cách uncomment các dòng `server localhost:7134; localhost:7135;` trong upstream.

### 18.4. Domain & Hosting

- Domain hiện tại: `plchost.ddns.net` (Dynamic DNS — phù hợp self-hosted)
- Frontend: `https://plc-base-react.vercel.app` (Vercel)

---

## 19. Dev Tooling - CSharpier, Husky, EF Migrations

### 19.1. CSharpier - Code Formatter

CSharpier là Prettier cho C# — format code tự động không tranh cãi.

`.csharpierrc`:
```json
{
  "printWidth": 100,
  "useTabs": false,
  "tabWidth": 4,
  "preprocessorSymbolSets": ["", "DEBUG", "DEBUG,CODE_STYLE"]
}
```

Cài và chạy:
```bash
dotnet tool install csharpier
dotnet csharpier format .
```

### 19.2. Husky.Net - Git Hooks

`.husky/pre-commit` chạy `dotnet husky run` → trigger tasks trong `task-runner.json`:

```json
{
  "tasks": [
    {
      "name": "Run CSharpier Formatter",
      "command": "dotnet",
      "args": ["csharpier", "format", "${staged}"],
      "include": ["**/*.cs"]
    }
  ]
}
```

→ Khi commit, CSharpier tự format mọi file `.cs` đã `git add`.

Setup:
```bash
dotnet tool install Husky
dotnet husky install
dotnet husky run
```

### 19.3. EF Core Migrations

Shortcut qua `src/Makefile`:

```makefile
add-mig:
	@dotnet ef migrations add $(MGR_NAME) -o Common/Data/Migrations

db-update:
	@dotnet ef database update
```

Usage:
```bash
cd src
make add-mig MGR_NAME=AddNewField
make db-update
```

Hoặc đầy đủ:
```bash
dotnet tool install dotnet-ef
dotnet ef migrations add MyMigration -o Common/Data/Migrations
dotnet ef database update
```

### 19.4. Hot Reload với `dotnet watch`

```bash
cd src
dotnet watch
# hoặc:
make watch
```

→ Auto restart khi save file `.cs` (không phải hot-reload UI thật sự, nhưng đủ nhanh cho API dev).

---

## 20. Testing

### 20.1. Setup

Test project `test/plctest.csproj`:

| Package | Mục đích |
|---|---|
| `xUnit` 2.9.3 | Test framework |
| `xunit.runner.visualstudio` 3.1.5 | Test discoverer cho VS |
| `Microsoft.NET.Test.Sdk` 18.3.0 | MSTest infrastructure |
| `coverlet.collector` 8.0.0 | Code coverage |

Reference đến main project: `<ProjectReference Include="..\src\plcbase.csproj" />`

### 20.2. Test Categories Hiện Tại

Hiện chỉ có **2 file test** cho utility:

- `JsonUtilityTest.cs` — test serialize/deserialize
- `PasswordUtilityTest.cs` — test BCrypt hash/verify

Pattern test theo **Arrange-Act-Assert** + `[Theory]` với `ClassData`:

```csharp
[Theory]
[ClassData(typeof(TestData))]
public void IsValidPassword_CorrectPassword_ReturnsTrue(string password)
{
    // Arrange
    string hash = PasswordUtility.GetPasswordHash(password);

    // Act
    bool isValid = PasswordUtility.IsValidPassword(password, hash);

    // Assert
    Assert.True(isValid);
}
```

### 20.3. Chạy Tests

```bash
dotnet test
# hoặc với coverage:
dotnet test --collect:"XPlat Code Coverage"
```

### 20.4. Gợi Ý Mở Rộng Test Suite

Hiện codebase chưa có:
- Unit test cho **Services** (mock `IUnitOfWork` qua Moq/NSubstitute)
- Integration test cho **Controllers** (`WebApplicationFactory<Program>` + Testcontainers cho MySQL/Redis)
- Test cho **Repositories** (in-memory EF hoặc real DB với Testcontainers)

---

## 21. Quy Ước Code (Coding Conventions)

### 21.1. Cú Pháp C# Mới

Code sử dụng triệt để các tính năng C# 12+:

| Tính năng | Ví dụ trong codebase |
|---|---|
| **Primary Constructors** | `public class IssueService(IUnitOfWork uow, IMapper mapper) : IIssueService` |
| **Collection expressions** | `List<int> ids = [];` thay vì `new List<int>()` |
| **Target-typed `new`** | `QueryModel<T> q = new() { ... }` |
| **File-scoped namespace** | `namespace PlcBase.Features.Issue;` (không nested {}) |
| **Implicit usings** | `<ImplicitUsings>enable</ImplicitUsings>` |
| **Pattern matching** | `if (entity is not ISoftDeletable softDeletable) return;` |
| **`record` / init-only** | (Chưa dùng nhiều, hầu hết DTO vẫn là class với setter) |

### 21.2. Naming Conventions

| Loại | Convention | Ví dụ |
|---|---|---|
| Class | PascalCase | `ProjectMemberService` |
| Interface | `I` + PascalCase | `IProjectMemberService` |
| Method (public) | PascalCase | `GetProjectMembers` |
| Method (private) | PascalCase | `GetUserClaims` |
| Field (private) | `_` + camelCase | `_jwtSettings`, `_db` |
| Property | PascalCase | `RefreshToken` |
| Parameter / Local var | camelCase | `userAccount`, `projectId` |
| Constant | UPPER_SNAKE_CASE | `MAX_EPOCH_MILLISECOND`, `SEND_MAIL` |
| Enum value | PascalCase hoặc UPPER (tuỳ class) | `IssueType.USER_STORY`, `AppRole.ADMIN` |

### 21.3. File Naming

- 1 file = 1 class chính
- Tên file = tên class (`UserService.cs` chứa `class UserService`)
- Interface ở file riêng (`IUserService.cs` chứa `interface IUserService`)
- DTO mỗi loại 1 file (`UserLoginDTO.cs`, `UserLoginResponseDTO.cs`)

### 21.4. Comments

Code chủ yếu **không có XML doc** trên public method — tên method và parameter đã rõ. Comments chỉ xuất hiện khi:
- Đánh dấu vùng (`#region Board`, `#region Backlog`, `#region Detail`)
- Note tiếng Việt giải thích business (vd: `// Mặc định creator luôn luôn là người có tham dự`)
- Workaround / TODO

### 21.5. Async/Await Convention

- Mọi method I/O trả về `Task<T>` hoặc `Task`
- Đặt tên có hậu tố `Async` cho method async của thư viện ngoài (vd: `FindByIdAsync`); nhưng **service methods của project hiện không dùng hậu tố `Async`** (vd: `GetProjectsForUser`, `CreateInvitation`) → quy ước nội bộ

### 21.6. URL & Routing

- `services.Configure<RouteOptions>(o => o.LowercaseUrls = true)` → URL tự động lowercase
- Mọi controller route prefix `/api/[controller]` (default) hoặc explicit như `[Route("/api/project/{projectId}/board/...")]`

---

## 22. Hướng Dẫn Mở Rộng (Adding a New Feature)

Khi thêm một feature mới (ví dụ "Notification"), follow đúng pattern của codebase:

### Bước 1: Tạo cấu trúc thư mục

```
src/Features/Notification/
├── Controllers/
│   └── NotificationController.cs
├── DTOs/
│   ├── NotificationDTO.cs
│   ├── CreateNotificationDTO.cs
│   └── NotificationMapping.cs
├── Entities/
│   └── NotificationEntity.cs
├── Repositories/
│   ├── INotificationRepository.cs
│   └── NotificationRepository.cs
└── Services/
    ├── INotificationService.cs
    └── NotificationService.cs
```

### Bước 2: Thêm Entity & Configuration

```csharp
// Shared/Enums/TableName.cs → thêm constant
public const string NOTIFICATION = "notification";

// Features/Notification/Entities/NotificationEntity.cs
[Table(TableName.NOTIFICATION)]
public class NotificationEntity : BaseEntity { ... }

// Common/Data/Context/DataContext.cs → thêm DbSet
public DbSet<NotificationEntity> Notifications { get; set; }

// Common/Data/Context/Configuration/NotificationConfiguration.cs (nếu cần index)
public class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity> { ... }

// Common/Data/Context/Configuration/ContextConfigurationExtension.cs → apply
modelBuilder.ApplyConfiguration(new NotificationConfiguration());
```

### Bước 3: Tạo Migration

```bash
cd src
make add-mig MGR_NAME=AddNotification
make db-update
```

### Bước 4: Tạo Repository

```csharp
public class NotificationRepository(DataContext db, IMapper mapper)
    : BaseRepository<NotificationEntity>(db, mapper),
      INotificationRepository
{
    // Custom methods nếu cần
}
```

Đăng ký trong `UnitOfWork.cs`:
```csharp
// Field
public INotificationRepository Notification { get; private set; }

// Constructor
Notification = new NotificationRepository(_db, _mapper);

// Interface IUnitOfWork.cs → thêm:
INotificationRepository Notification { get; }
```

### Bước 5: Tạo Service

```csharp
public class NotificationService(IUnitOfWork uow, IMapper mapper) : INotificationService { ... }
```

Đăng ký trong `Common/Services/ServiceDIExtension.cs`:
```csharp
services.AddScoped<INotificationService, NotificationService>();
```

### Bước 6: Tạo Controller

```csharp
public class NotificationController(INotificationService service) : BaseController
{
    [HttpGet]
    [Authorize]
    public async Task<SuccessResponse<List<NotificationDTO>>> GetNotifications()
    {
        ReqUser reqUser = HttpContext.GetRequestUser();
        return HttpContext.Success(await service.GetForUser(reqUser));
    }
}
```

### Bước 7: Tạo AutoMapper Profile

```csharp
public class NotificationMapping : Profile
{
    public NotificationMapping()
    {
        CreateMap<NotificationEntity, NotificationDTO>();
        CreateMap<CreateNotificationDTO, NotificationEntity>();
    }
}
```

→ Auto-scan, không cần register thủ công.

### Bước 8: (Nếu cần) Thêm Permission

```csharp
// Shared/Enums/PermissionPolicy.cs
public static class NotificationPermission
{
    public const string GetAll = "Notification.GetAll";
    public const string Create = "Notification.Create";
    public const string Delete = "Notification.Delete";
}

// Shared/Helpers/Permission/PermissionHelper.cs → thêm vào GetAllPermissions
new(NotificationPermission.GetAll, "Can view notifications"),
new(NotificationPermission.Create, "Can create notification"),
new(NotificationPermission.Delete, "Can delete notification"),
```

### Bước 9: (Nếu cần) Thêm Cache Invalidation

```csharp
// Sau update/delete:
await redisHelper.ClearByPattern(RedisUtility.GetClearKey<NotificationDTO>());
```

### Bước 10: Format & Commit

```bash
dotnet csharpier format .
git add .
git commit -m "feat: add notification feature"   # Husky pre-commit chạy CSharpier lần nữa
```

---

## Tài Liệu Liên Quan

- `README.md` — Setup environment, build, deploy commands
- `README-Business.md` — Mô tả chi tiết nghiệp vụ (business)
- `README-API.md` (chưa có) — Liệt kê chi tiết API endpoints

> **Phiên bản tài liệu**: 1.0 — Dựa trên source code phiên bản hiện tại. Khi tech stack/library upgrade hoặc kiến trúc thay đổi, vui lòng cập nhật lại.
