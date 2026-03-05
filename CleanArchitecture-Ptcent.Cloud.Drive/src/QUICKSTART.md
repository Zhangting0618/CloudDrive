# 快速开始指南

## 前提条件

- .NET 8.0 SDK
- MySQL 8.0+
- Redis (可选，用于分布式缓存)

## 配置步骤

### 1. 配置数据库

```sql
CREATE DATABASE clouddrive CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### 2. 修改配置文件

编辑 `Ptcent.Cloud.Drive.Web/Configs/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=你的数据库地址;port=3306;database=clouddrive;user=用户名;password=密码;",
    "Redis": "localhost:6379"
  },
  "Authentication": {
    "SecretKey": "你的 32 位以上密钥",
    "Issuer": "你的发行者",
    "Audience": "你的受众"
  }
}
```

### 3. 数据库迁移

使用 EF Core 迁移或手动创建表：

```sql
-- 用户表
CREATE TABLE User (
    Id BIGINT PRIMARY KEY,
    UserName VARCHAR(100) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Password VARCHAR(255),
    Email VARCHAR(100),
    Sex INT DEFAULT 0,
    IsDel INT DEFAULT 0,
    ImageUrl VARCHAR(500),
    RegisterTime DATETIME,
    CreateDate DATETIME,
    UpdateDate DATETIME,
    Salt VARCHAR(100),
    CreateBy BIGINT,
    UpdateBy BIGINT,
    INDEX idx_phone (Phone),
    INDEX idx_email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 文件表
CREATE TABLE File (
    Id BIGINT PRIMARY KEY,
    LeafName VARCHAR(255) NOT NULL,
    Extension VARCHAR(50),
    Path VARCHAR(1000),
    ParentFolderId BIGINT,
    Idpath VARCHAR(1000) NOT NULL,
    IsFolder INT DEFAULT 0,
    FileType INT,
    IsDel INT DEFAULT 0,
    CreatedDate DATETIME,
    UpdatedDate DATETIME,
    CreatedBy BIGINT,
    UpdatedBy BIGINT,
    DeletedDate DATETIME,
    DeletedBy BIGINT,
    VersionId BIGINT,
    ItemHash VARCHAR(100),
    FileSize BIGINT,
    PhysicalDirectory VARCHAR(500),
    ItemFileMapUrl VARCHAR(500),
    INDEX idx_parent (ParentFolderId),
    INDEX idx_idpath (Idpath),
    INDEX idx_createdby (CreatedBy)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 4. 运行项目

```bash
cd Ptcent.Cloud.Drive.Web
dotnet run
```

### 5. 访问 API

- Swagger UI: `http://localhost:5000/swagger`
- 健康检查：`http://localhost:5000/health`

## API 测试

### 用户注册

```bash
curl -X POST http://localhost:5000/api/user/register \
  -H "Content-Type: application/json" \
  -d '{
    "UserName": "testuser",
    "Phone": "13800138000",
    "PassWord": "123456",
    "Email": "test@example.com",
    "Sex": 0
  }'
```

### 用户登录

```bash
curl -X POST http://localhost:5000/api/user/login \
  -H "Content-Type: application/json" \
  -d '{
    "Phone": "13800138000",
    "PassWord": "123456"
  }'
```

响应：
```json
{
  "code": 200,
  "isSuccess": true,
  "message": "登录成功",
  "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "totalCount": 0
}
```

### 使用 Token

```bash
curl -X GET http://localhost:5000/api/user/profile \
  -H "Authorization: Bearer {token}"
```

## 开发指南

### 添加新功能

1. 在 `Domain/Entities` 添加实体
2. 在 `Application/Contracts` 添加请求/响应 DTO
3. 在 `Application/Features/{Module}` 添加 Command/Query
4. 在 `Application/Interfaces` 添加仓储接口（如需要）
5. 在 `Infrastructure/Repositories` 实现仓储
6. 在 `Web/Controllers` 添加 API 端点

### 示例：添加更新用户功能

**Step 1: 创建 Command**
```csharp
// Application/Features/Users/Commands/UpdateUserCommand.cs
public record UpdateUserCommand(
    long UserId,
    string UserName,
    string Email
) : IRequest<ResponseMessageDto<bool>>;
```

**Step 2: 创建 Handler**
```csharp
// Application/Features/Users/Commands/UpdateUserCommandHandler.cs
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ResponseMessageDto<bool>>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResponseMessageDto<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return new ResponseMessageDto<bool>
            {
                IsSuccess = false,
                Message = "用户不存在"
            };
        }

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.UpdateDate = DateTime.UtcNow;

        var result = await _userRepository.UpdateAsync(user, cancellationToken);

        return new ResponseMessageDto<bool>
        {
            IsSuccess = result,
            Message = result ? "更新成功" : "更新失败"
        };
    }
}
```

**Step 3: 创建 Validator**
```csharp
// Application/Features/Users/Commands/UpdateUserCommandValidator.cs
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.UserName).NotEmpty().Length(2, 50);
        RuleFor(x => x.Email).EmailAddress();
    }
}
```

**Step 4: 添加 Controller 端点**
```csharp
// Web/Controllers/UserController.cs
[HttpPut("{id}")]
[Authorize]
public async Task<ActionResult<ResponseMessageDto<bool>>> Update(
    long id,
    [FromBody] UpdateUserRequestDto request)
{
    var command = new UpdateUserCommand(
        id,
        request.UserName,
        request.Email
    );
    return await _mediator.Send(command);
}
```

## 常见问题

### Q: 数据库连接失败
检查连接字符串格式，确保使用 MySQL 8.0+ 的连接格式。

### Q: Redis 连接失败
如果没有 Redis，系统会自动降级为内存缓存。

### Q: JWT Token 无效
检查 `appsettings.json` 中的 `SecretKey` 是否至少 32 个字符。

### Q: Swagger 无法访问
确保 `Program.cs` 中调用了 `UseSwaggerServices()`。

## 部署

### Docker 部署

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Ptcent.Cloud.Drive.Web/Ptcent.Cloud.Drive.Web.csproj", "Ptcent.Cloud.Drive.Web/"]
COPY ["Ptcent.Cloud.Drive.Application/Ptcent.Cloud.Drive.Application.csproj", "Ptcent.Cloud.Drive.Application/"]
COPY ["Ptcent.Cloud.Drive.Domain/Ptcent.Cloud.Drive.Domain.csproj", "Ptcent.Cloud.Drive.Domain/"]
COPY ["Ptcent.Cloud.Drive.Infrastructure/Ptcent.Cloud.Drive.Infrastructure.csproj", "Ptcent.Cloud.Drive.Infrastructure/"]
COPY ["Ptcent.Cloud.Drive.Shared/Ptcent.Cloud.Drive.Shared.csproj", "Ptcent.Cloud.Drive.Shared/"]
COPY . .
RUN dotnet restore "Ptcent.Cloud.Drive.Web/Ptcent.Cloud.Drive.Web.csproj"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ptcent.Cloud.Drive.Web.dll"]
```

### Kestrel 配置

在 `appsettings.json` 中配置：

```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://*:80"
    },
    "Https": {
      "Url": "https://*:443"
    }
  }
}
```

## 监控和日志

### 健康检查端点

- `/health` - 基本健康检查
- 未来可以添加 `/health/live` 和 `/health/ready`

### 日志

当前使用 .NET 内置日志，建议集成 Serilog：

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

## 性能优化建议

1. 启用响应压缩（已配置）
2. 使用 Redis 缓存（已支持）
3. 数据库查询使用 `AsNoTracking()`（已实现）
4. 添加数据库索引（见上方 SQL）
5. 使用分页查询

## 安全建议

1. 使用 HTTPS
2. 定期更换 JWT SecretKey
3. 启用限流保护
4. 实施输入验证（已实现）
5. 记录安全相关事件
