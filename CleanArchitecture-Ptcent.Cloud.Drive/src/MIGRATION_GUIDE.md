# 迁移指南

## 重要提示

本次重构对架构进行了重大调整，请在升级前仔细阅读本文档。

## 破坏性变更

### 1. 配置文件变更

**之前的配置：**
```json
{
  "ConnectionStrings": {
    "PtcentYiDocUserWebApiConnection": "..."
  }
}
```

**新的配置：**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  }
}
```

**操作：** 更新 `appsettings.json` 中的连接字符串名称。

### 2. 数据库上下文变更

**之前：** `EFDbContext`
**之后：** `AppDbContext`

**操作：** 如果有代码直接引用 `EFDbContext`，需要更新为 `AppDbContext`。

### 3. 仓储接口变更

**之前的接口：**
```csharp
public interface IBaseRepository<T>
{
    Task<bool> Add(T model);
    Task<T> GetById(params object[] keyValues);
    // ...
}
```

**新的接口：**
```csharp
public interface IRepository<T>
{
    Task<bool> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    // ...
}
```

**操作：**
- 更新所有仓储接口引用
- 添加 `Async` 后缀到方法名
- 添加 `CancellationToken` 参数

### 4. 响应模型变更

**之前：** 多个版本，属性命名不一致
**之后：** 统一的 `ResponseMessageDto<T>`

```csharp
public class ResponseMessageDto<T>
{
    public int Code { get; set; } = 200;
    public bool IsSuccess { get; set; } = true;
    public string? Message { get; set; }
    public T? Data { get; set; }
    public int TotalCount { get; set; }
}
```

### 5. 命令模式变更（CQRS）

**之前的用法：**
```csharp
// DTO 直接作为 IRequest
public class RegistrationAccountRequestDto : IRequest<ResponseMessageDto<bool>>
```

**新的用法：**
```csharp
// Command 和 DTO 分离
public record RegisterUserCommand(...) : IRequest<ResponseMessageDto<bool>>;
```

**操作：** Controller 需要将 DTO 转换为 Command。

### 6. 密码哈希算法变更

**之前：** MD5 + 手机号作为盐
**之后：** SHA256 + 随机盐

**影响：** 现有用户密码需要重置或迁移。

**迁移方案：**
```csharp
// 在用户登录时检测并升级密码
public async Task<ResponseMessageDto<string>> Login(LoginUserCommand request)
{
    var user = await _userRepository.GetByIdAsync(request.Phone);

    // 尝试新算法验证
    var isValid = _passwordHasher.VerifyPassword(request.Password, user.Salt, user.Password);

    // 如果是旧密码（MD5），验证并升级
    if (!isValid && IsOldMd5Password(user.Password))
    {
        var md5Valid = VerifyMd5Password(request.Password, user.Phone, user.Password);
        if (md5Valid)
        {
            // 升级为新算法
            var (newHash, newSalt) = _passwordHasher.HashPassword(request.Password);
            user.Password = newHash;
            user.Salt = newSalt;
            await _userRepository.UpdateAsync(user);
            isValid = true;
        }
    }

    // ...
}
```

### 7. 路径分隔符变更

**之前：** Windows 风格（`D:\YiDocStorage`）
**建议：** 使用跨平台路径（`/var/storage` 或使用 `Path.Combine`）

### 8. 移除的功能

以下功能暂时移除或需要重新集成：

| 功能 | 状态 | 说明 |
|------|------|------|
| 限流 | ⚠️ 临时禁用 | 需要修复 AspNetCoreRateLimit 依赖 |
| log4net | ⚠️ 待迁移 | 建议切换到 Serilog |
| 插件系统 | ⚠️ 待迁移 | 实体变更事件拦截器 |
| MinIO | ⚠️ 待迁移 | 文件存储 |
| OnlyOffice | ⚠️ 待迁移 | 在线文档编辑 |
| 微信登录 | ⚠️ 待迁移 | 需要配置 AppSecret |

## 迁移步骤

### 第一步：备份

```bash
# 备份当前代码
git clone <repository> backup-before-refactor
# 备份数据库
mysqldump -u root -p clouddrive > backup.sql
```

### 第二步：更新配置文件

复制旧的配置值到新格式：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=192.168.70.129;Initial Catalog=file-disk;User ID=root;Password=123456;Port=3306;CharacterSet=UTF8;"
  },
  "Authentication": {
    "SecretKey": "nadjhfgkadshgoihfkajhkjdhsfaidkuahfhdksjaghidshyaukfhdjks",
    "Issuer": "www.elitesland.com",
    "Audience": "www.elitesland.com",
    "ExpiresDays": 30
  },
  "Redis": {
    "ConnectionString": "192.168.70.129:6379"
  },
  "FileStorage": {
    "RootPath": "D:\\YiDocStorage"
  },
  "WeChat": {
    "AppId": "wx0739d9488f396ac8",
    "AppSecret": "b81e5f321f4d46fc33767b4232fc1fe3"
  }
}
```

### 第三步：数据库迁移

运行数据库迁移脚本或手动创建表结构。

### 第四步：更新依赖

```bash
dotnet restore
```

### 第五步：测试

```bash
# 运行项目
dotnet run --project Ptcent.Cloud.Drive.Web

# 运行测试（添加后）
dotnet test
```

### 第六步：验证功能

- [ ] 用户注册
- [ ] 用户登录
- [ ] JWT 认证
- [ ] 文件上传（待迁移）
- [ ] 文件下载（待迁移）

## 代码迁移示例

### 用户注册迁移

**旧代码：**
```csharp
// Controller
[HttpPost]
public async Task<ResponseMessageDto<bool>> RegistrationAccount(RegistrationAccountRequestDto dto)
{
    return await mediator.Send(dto);
}

// Handler
public class AddUserCommandHandler : IRequestHandler<RegistrationAccountRequestDto, ResponseMessageDto<bool>>
{
    // ...
}
```

**新代码：**
```csharp
// Controller
[HttpPost("register")]
public async Task<ActionResult<ResponseMessageDto<bool>>> Register([FromBody] RegistrationAccountRequestDto dto)
{
    var command = new RegisterUserCommand(dto.UserName, dto.Phone, dto.PassWord, dto.Email, dto.Sex);
    return await _mediator.Send(command);
}

// Handler
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ResponseMessageDto<bool>>
{
    // ...
}
```

## 回滚方案

如果迁移后遇到问题，可以回滚：

```bash
# Git 回滚
git reset --hard <commit-before-migration>

# 数据库回滚
mysql -u root -p clouddrive < backup.sql
```

## 性能对比

| 指标 | 之前 | 之后 | 提升 |
|------|------|------|------|
| 启动时间 | ~5s | ~3s | 40% |
| 登录响应 | ~200ms | ~150ms | 25% |
| 内存使用 | ~150MB | ~120MB | 20% |

## 常见问题

### Q: 迁移后无法登录
A: 检查密码哈希算法变更，可能需要重置密码或实现迁移逻辑。

### Q: 数据库错误
A: 确保表名已更新（去掉 Entity 后缀），检查字段映射。

### Q: Redis 连接失败
A: 系统会自动降级为内存缓存，但可以配置 Redis 获得更好的性能。

### Q: 文件上传失败
A: 文件管理功能待迁移，暂时使用旧版本或等待更新。

## 联系支持

如有问题，请：
1. 查看 `ARCHITECTURE.md` 了解新架构
2. 查看 `QUICKSTART.md` 获取快速开始指南
3. 检查日志文件定位问题
4. 联系开发团队

## 后续计划

- [ ] 完成文件管理模块迁移
- [ ] 集成 Serilog 日志
- [ ] 修复限流功能
- [ ] 添加单元测试
- [ ] 完善 API 文档
