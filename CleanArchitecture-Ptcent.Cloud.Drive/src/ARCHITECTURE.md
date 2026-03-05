# Clean Architecture 架构说明

## 项目结构

```
src/
├── Ptcent.Cloud.Drive.Domain/          # 领域层 - 核心业务实体和规则
│   ├── Entities/                       # 实体类
│   │   ├── UserEntity.cs
│   │   └── FileEntity.cs
│   ├── Constants/                      # 常量定义
│   │   ├── CacheKeys.cs
│   │   └── CacheExpiration.cs
│   └── Ptcent.Cloud.Drive.Domain.csproj
│
├── Ptcent.Cloud.Drive.Application/     # 应用层 - 业务逻辑和工作流
│   ├── Contracts/                      # 契约定义
│   │   ├── Requests/                   # 请求 DTOs
│   │   │   ├── RegistrationAccountRequestDto.cs
│   │   │   └── LoginUserRequestDto.cs
│   │   └── Responses/                  # 响应 DTOs
│   │       └── ResponseMessageDto.cs
│   ├── Features/                       # 功能模块（按业务领域组织）
│   │   ├── Users/
│   │   │   └── Commands/               # 用户命令
│   │   │       ├── RegisterUserCommand.cs
│   │   │       ├── RegisterUserCommandHandler.cs
│   │   │       ├── RegisterUserCommandValidator.cs
│   │   │       ├── LoginUserCommand.cs
│   │   │       ├── LoginUserCommandHandler.cs
│   │   │       └── LoginUserCommandValidator.cs
│   │   └── Files/
│   │       ├── Commands/               # 文件命令
│   │       │   ├── UploadFileCommand.cs
│   │       │   └── UploadFileCommandHandler.cs
│   │       └── Queries/                # 文件查询
│   │           ├── DownloadFileQuery.cs
│   │           ├── DownloadFileQueryHandler.cs
│   │           ├── BatchDownloadQuery.cs
│   │           └── BatchDownloadQueryHandler.cs
│   ├── Interfaces/                     # 接口定义（仓储、服务等）
│   │   ├── IRepository.cs
│   │   ├── IUserRepository.cs
│   │   └── IFileRepository.cs
│   ├── Services/                       # 应用服务接口
│   │   ├── ICacheService.cs
│   │   ├── IJwtService.cs
│   │   ├── IPasswordHasher.cs
│   │   ├── IIdGeneratorService.cs
│   │   └── IFileHashService.cs
│   ├── PipelineBehaviors/              # MediatR 管道行为
│   │   └── ValidationBehavior.cs
│   └── Ptcent.Cloud.Drive.Application.csproj
│
├── Ptcent.Cloud.Drive.Infrastructure/  # 基础设施层 - 外部依赖实现
│   ├── Persistence/                    # 数据持久化
│   │   └── AppDbContext.cs
│   ├── Repositories/                   # 仓储实现
│   │   ├── Repository.cs
│   │   ├── UserRepository.cs
│   │   └── FileRepository.cs
│   ├── Services/                       # 服务实现
│   │   ├── CacheService.cs
│   │   ├── JwtService.cs
│   │   ├── PasswordHasher.cs
│   │   ├── IdGeneratorService.cs
│   │   └── FileHashService.cs
│   └── Ptcent.Cloud.Drive.Infrastructure.csproj
│
├── Ptcent.Cloud.Drive.Shared/          # 共享内核 - 通用工具
│   ├── Extensions/                     # 扩展方法
│   │   ├── CommonExtensions.cs
│   │   └── NumericExtensions.cs
│   └── Ptcent.Cloud.Drive.Shared.csproj
│
└── Ptcent.Cloud.Drive.Web/             # 表示层 - API 接口
    ├── Controllers/                    # API 控制器
    │   ├── UserController.cs
    │   ├── FileController.cs
    │   └── DownFileController.cs
    ├── Middleware/                     # 中间件
    │   └── ExceptionHandlingMiddleware.cs
    ├── Extensions/
    │   └── ServiceCollection/          # 服务注册扩展
    │       ├── ApplicationExtensions.cs
    │       ├── CorsExtensions.cs
    │       ├── DatabaseExtensions.cs
    │       ├── InfrastructureExtensions.cs
    │       ├── JwtAuthExtensions.cs
    │       ├── SwaggerExtensions.cs
    │       └── WebApiExtensions.cs
    ├── Configs/                        # 配置文件
    │   ├── appsettings.json
    │   └── appsettings.Development.json
    ├── Program.cs
    └── Ptcent.Cloud.Drive.Web.csproj
```

## 架构原则

### 1. 依赖规则
- **Domain 层**：无外部依赖，只包含核心业务实体和规则
- **Application 层**：依赖 Domain 层，定义业务逻辑接口
- **Infrastructure 层**：依赖 Application 层，实现接口
- **Web 层**：依赖 Application 和 Infrastructure 层
- **Shared 层**：无依赖，提供通用工具

### 2. Clean Architecture 核心
```
        ┌─────────────────────────────────┐
        │           Web (API)             │
        └──────────────┬──────────────────┘
                       │
        ┌──────────────▼──────────────────┐
        │      Infrastructure             │
        │  (Repository Implementations)   │
        └──────────────┬──────────────────┘
                       │
        ┌──────────────▼──────────────────┐
        │        Application              │
        │   (Business Logic/Use Cases)    │
        └──────────────┬──────────────────┘
                       │
        ┌──────────────▼──────────────────┐
        │          Domain                 │
        │  (Entities/Business Rules)      │
        └─────────────────────────────────┘
```

### 3. 设计模式

#### CQRS（命令查询职责分离）
- **Commands**：写操作，返回 `ResponseMessageDto<T>`
- **Queries**：读操作，返回数据

#### MediatR
- 使用 MediatR 实现中介者模式
- 通过 Pipeline Behaviors 实现横切关注点（如验证、日志、缓存）

#### Repository 模式
- Application 层定义接口
- Infrastructure 层实现接口
- 依赖倒置，便于单元测试

## 高可用特性

### 1. 全局异常处理
```csharp
// Program.cs
app.UseExceptionHandling();
```

### 2. 健康检查
```
GET /health
Response: { "Status": "Healthy", "Timestamp": "..." }
```

### 3. 数据库重试机制
```csharp
// DatabaseExtensions.cs
mysqlOptions.EnableRetryOnFailure(
    maxRetryCount: 3,
    maxRetryDelay: TimeSpan.FromSeconds(30)
);
```

### 4. 分布式缓存
- Redis 缓存支持
- 可降级为内存缓存

### 5. JWT 认证
- 可配置的 Token 过期时间
- 支持刷新机制

### 6. 文件秒传
- 基于文件 Hash 实现秒传
- 节省存储空间和上传时间

## 核心功能

### 用户模块
- ✅ 用户注册
- ✅ 用户登录
- ✅ JWT 认证
- ✅ 密码加密（SHA256）

### 文件模块
- ✅ 文件上传（支持秒传）
- ✅ 文件下载
- ✅ 批量下载（ZIP 打包）
- ✅ 文件预览（前端实现）
- ⏳ 文件夹管理
- ⏳ 文件移动/复制
- ⏳ 文件重命名
- ⏳ 文件删除

### 已移除功能
- ❌ OnlyOffice 集成（改用前端预览）

## 新增/修改的文件

### 核心文件
| 文件 | 说明 |
|------|------|
| `Domain/Entities/UserEntity.cs` | 用户实体 |
| `Domain/Entities/FileEntity.cs` | 文件实体 |
| `Application/Contracts/Requests/*` | 请求 DTOs |
| `Application/Contracts/Responses/ResponseMessageDto.cs` | 统一响应模型 |
| `Application/Interfaces/IRepository.cs` | 通用仓储接口 |
| `Application/Features/Users/Commands/*` | 用户相关命令 |
| `Application/Features/Files/*` | 文件相关命令和查询 |
| `Infrastructure/Persistence/AppDbContext.cs` | 数据库上下文 |
| `Infrastructure/Repositories/*` | 仓储实现 |
| `Infrastructure/Services/*` | 服务实现 |
| `Web/Middleware/ExceptionHandlingMiddleware.cs` | 异常处理中间件 |

## 命名规范

### 目录命名
- 使用 PascalCase
- 使用复数形式表示集合（如 `Controllers`, `Services`）
- 功能模块按业务领域组织（如 `Features/Users`）

### 类命名
- 实体：`{EntityName}Entity` 或 `{EntityName}`（如 `UserEntity`）
- DTO：`{Action}{Entity}RequestDto` / `{Entity}ResponseDto`
- Command：`{Action}{Entity}Command`
- Handler：`{Action}{Entity}CommandHandler`
- Validator：`{Action}{Entity}CommandValidator`
- Repository：`{Entity}Repository`
- Service：`{ServiceName}Service`

### 接口命名
- 以 `I` 开头
- 仓储接口：`I{Entity}Repository`
- 服务接口：`I{ServiceName}Service`

## 参考资源

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [EF Core Best Practices](https://docs.microsoft.com/en-us/ef/core/)
