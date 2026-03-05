# Ptcent Cloud Drive - 云端磁盘系统

基于 Clean Architecture 的高可用云端磁盘系统。

## 快速导航

- [快速开始](QUICKSTART.md) - 快速上手指南
- [架构说明](ARCHITECTURE.md) - 详细的架构设计文档
- [迁移指南](MIGRATION_GUIDE.md) - 从旧版本迁移
- [重构总结](REFACTORING_SUMMARY.md) - 本次重构的详细说明

## 项目简介

Ptcent Cloud Drive 是一个企业级云端磁盘系统，支持文件上传、下载、在线预览、版本管理等功能。

### 核心特性

- 用户注册/登录
- JWT 认证
- 文件管理（上传、下载、删除）
- 文件夹管理
- 文件版本控制
- 在线预览（OnlyOffice 集成）
- 微信登录
- 分布式缓存（Redis）
- 限流保护

## 技术栈

| 组件 | 技术 |
|------|------|
| 框架 | .NET 8.0 |
| 数据库 | MySQL 8.0+ |
| 缓存 | Redis |
| ORM | Entity Framework Core 8.0 |
| 认证 | JWT Bearer |
| 文档 | Swagger/OpenAPI |
| 架构 | Clean Architecture + CQRS |

## 项目结构

```
src/
├── Ptcent.Cloud.Drive.Domain/       # 领域层（核心业务实体）
├── Ptcent.Cloud.Drive.Application/  # 应用层（业务逻辑）
├── Ptcent.Cloud.Drive.Infrastructure/ # 基础设施层（外部依赖实现）
├── Ptcent.Cloud.Drive.Shared/       # 共享内核（通用工具）
└── Ptcent.Cloud.Drive.Web/          # 表示层（API 接口）
```

## 快速开始

### 1. 环境要求

- .NET 8.0 SDK
- MySQL 8.0+
- Redis (可选)

### 2. 安装

```bash
# 克隆项目
git clone <repository-url>

# 还原依赖
cd src
dotnet restore

# 配置数据库（编辑 appsettings.json）
# 编辑 Ptcent.Cloud.Drive.Web/Configs/appsettings.json

# 运行项目
cd Ptcent.Cloud.Drive.Web
dotnet run
```

### 3. 访问

- Swagger UI: `http://localhost:5000/swagger`
- 健康检查：`http://localhost:5000/health`

## API 端点

### 用户接口

| 方法 | 路径 | 说明 | 认证 |
|------|------|------|------|
| POST | /api/user/register | 用户注册 | ❌ |
| POST | /api/user/login | 用户登录 | ❌ |
| POST | /api/user/logout | 用户登出 | ✅ |

## 架构优势

### 1. 可维护性
- 清晰的关注点分离
- 明确的依赖方向
- 便于单元测试

### 2. 可扩展性
- 模块化设计
- 依赖注入
- 易于添加新功能

### 3. 高可用性
- 全局异常处理
- 健康检查
- 数据库重试
- 分布式缓存

### 4. 安全性
- JWT 认证
- 密码加密存储
- 输入验证
- SQL 注入防护

## 开发指南

### 添加新功能

遵循以下步骤：

1. **Domain 层**：添加实体和业务规则
2. **Application 层**：添加 Command/Query 和接口
3. **Infrastructure 层**：实现接口
4. **Web 层**：添加 Controller 端点

### 代码规范

- 使用 C# 最新特性
- 遵循 SOLID 原则
- 添加 XML 文档注释
- 使用异步编程

### 示例代码

```csharp
// Command
public record CreateFileCommand(...) : IRequest<ResponseMessageDto<bool>>;

// Handler
public class CreateFileCommandHandler : IRequestHandler<CreateFileCommand, ResponseMessageDto<bool>>
{
    // 实现逻辑
}

// Validator
public class CreateFileCommandValidator : AbstractValidator<CreateFileCommand>
{
    // 验证规则
}
```

## 配置说明

### 数据库连接

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=clouddrive;user=root;password=xxx;"
  }
}
```

### Redis 配置

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### JWT 配置

```json
{
  "Authentication": {
    "SecretKey": "至少 32 个字符的密钥",
    "Issuer": "发行者",
    "Audience": "受众",
    "ExpiresDays": 30
  }
}
```

## 部署

### Docker

```bash
docker build -t clouddrive .
docker run -p 8080:80 clouddrive
```

### IIS

1. 发布项目：`dotnet publish -c Release`
2. 在 IIS 中创建站点
3. 指向发布目录
4. 配置应用程序池（无托管代码）

## 监控

### 健康检查

```bash
curl http://localhost:5000/health
```

### 日志

日志输出到控制台和文件（配置 log4net）。

## 性能优化

1. 启用响应压缩
2. 使用缓存
3. 数据库索引
4. 分页查询

## 安全建议

1. 使用 HTTPS
2. 定期更换密钥
3. 启用限流
4. 实施输入验证

## 常见问题

### Q: 数据库连接失败
A: 检查连接字符串格式和数据库服务状态。

### Q: JWT Token 无效
A: 确保 SecretKey 至少 32 个字符，检查过期时间。

### Q: Swagger 无法访问
A: 确保项目成功启动，检查端口配置。

## 贡献指南

1. Fork 项目
2. 创建功能分支
3. 提交更改
4. 推送到分支
5. 创建 Pull Request

## 许可证

[查看许可证文件](LICENSE)

## 联系方式

- 邮箱：support@example.com
- 网站：https://www.elitesland.com

## 更新日志

### v2.0.0 (2026-03-05)
- 重构为 Clean Architecture
- 引入 CQRS 模式
- 改进密码加密
- 添加全局异常处理
- 优化项目结构

### v1.0.0
- 初始版本
