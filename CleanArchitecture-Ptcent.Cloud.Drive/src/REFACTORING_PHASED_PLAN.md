# 渐进式重构计划

## 当前状态分析

由于现有项目中有大量旧代码（约 124 个 C# 文件），完全重构需要分阶段进行。

### 当前编译错误原因

1. **Shared 项目**：删除了 Newtonsoft.Json、log4net、protobuf-net 等依赖，但旧代码仍在使用
2. **Application 项目**：删除了对 Shared 的依赖，但旧 Handler 在使用 Shared 中的工具类
3. **Infrastructure 项目**：删除了旧依赖，但旧 Repository 仍在使用

## 建议的重构策略

### 方案 A：完全重构（推荐用于新项目）

**优点**：
- 干净的架构
- 无历史包袱
- 最佳实践

**缺点**：
- 需要重写大量代码
- 迁移成本高

**步骤**：
1. 备份现有代码
2. 使用新架构创建空项目
3. 逐步迁移业务逻辑

### 方案 B：渐进式重构（推荐）

**优点**：
- 风险低
- 可以逐步验证
- 不影响现有功能

**缺点**：
- 过渡期会有临时方案
- 需要维护两套代码

**步骤**：见下方详细计划

## 渐进式重构详细计划

### 第一阶段：修复编译错误（1-2 天）

#### 1.1 恢复 Shared 项目依赖

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
    <PackageReference Include="Yitter.IdGenerator" Version="1.0.14" />
    <PackageReference Include="QRCoder" Version="1.4.1" />
    <PackageReference Include="SixLabors.ImageSharp" Version="3.1.3" />
    <!-- 恢复旧依赖 -->
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="log4net" Version="2.0.14" />
    <PackageReference Include="protobuf-net" Version="3.0.101" />
    <PackageReference Include="SharpCompress" Version="0.33.0" />
    <PackageReference Include="SharpZipLib" Version="1.4.2" />
    <PackageReference Include="Aspose.Words" Version="21.7.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Core" Version="2.2.5" />
  </ItemGroup>
</Project>
```

#### 1.2 恢复 Application 项目依赖

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.0" />
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="AutoMapper" Version="12.0.0" />
    <PackageReference Include="FluentValidation" Version="11.8.1" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.8.1" />
    <PackageReference Include="Microsoft.Extensions.Caching.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
    <PackageReference Include="Yitter.IdGenerator" Version="1.0.14" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Ptcent.Cloud.Drive.Domain\Ptcent.Cloud.Drive.Domain.csproj" />
    <ProjectReference Include="..\Ptcent.Cloud.Drive.Shared\Ptcent.Cloud.Drive.Shared.csproj" />
  </ItemGroup>
</Project>
```

#### 1.3 恢复 Infrastructure 项目依赖

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.1" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Proxies" Version="8.0.1" />
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.0-beta.2" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="StackExchange.Redis" Version="2.7.4" />
    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Ptcent.Cloud.Drive.Application\Ptcent.Cloud.Drive.Application.csproj" />
  </ItemGroup>
</Project>
```

### 第二阶段：清理和整理（2-3 天）

#### 2.1 重命名错误的目录

```bash
# 重命名目录
mv Application/Dto/ReponseModels Application/Dto/ResponseModels
mv Application/Dto Application/Contracts
mv Infrastructure/Respository Infrastructure/Repositories
mv Application/PipeLineBehavior Application/PipelineBehaviors
```

#### 2.2 整理 Shared 项目

将 Shared 中的工具类分类：
- 业务相关 → Application
- 基础设施相关 → Infrastructure
- 真正通用 → 保留在 Shared

#### 2.3 统一响应模型

删除旧的 ResponseMessageDto，统一使用新的：
```
Application/Contracts/Responses/ResponseMessageDto.cs
```

### 第三阶段：CQRS 迁移（3-5 天）

#### 3.1 迁移用户模块

1. 创建新的 Command/Query 结构
2. 迁移 User 相关 Handler
3. 更新 Controller

#### 3.2 迁移文件模块

1. 创建 File 相关 Command/Query
2. 迁移文件上传/下载逻辑
3. 更新 Controller

### 第四阶段：高可用特性（2-3 天）

#### 4.1 全局异常处理

已创建：`Web/Middleware/ExceptionHandlingMiddleware.cs`

#### 4.2 健康检查

已创建：`Program.cs` 中的 `/health` 端点

#### 4.3 数据库重试

已配置：`DatabaseExtensions.cs` 中的重试机制

### 第五阶段：测试和优化（3-5 天）

#### 5.1 单元测试

- Application 层 Command 测试
- Validator 测试
- Repository 测试

#### 5.2 集成测试

- API 端点测试
- 数据库集成测试

#### 5.3 性能测试

- 压力测试
- 并发测试

## 当前可立即使用的功能

以下新架构代码已经完成并可以使用：

### Domain 层
- ✅ `Entities/UserEntity.cs`
- ✅ `Entities/FileEntity.cs`
- ✅ `Constants/CacheKeys.cs`

### Application 层
- ✅ `Contracts/Requests/*`
- ✅ `Contracts/Responses/ResponseMessageDto.cs`
- ✅ `Features/Users/Commands/*` (注册/登录)
- ✅ `Interfaces/IRepository.cs`
- ✅ `Interfaces/IUserRepository.cs`
- ✅ `Services/*` (接口定义)
- ✅ `PipelineBehaviors/ValidationBehavior.cs`

### Infrastructure 层
- ✅ `Persistence/AppDbContext.cs`
- ✅ `Repositories/Repository.cs`
- ✅ `Repositories/UserRepository.cs`
- ✅ `Services/*` (服务实现)

### Web 层
- ✅ `Middleware/ExceptionHandlingMiddleware.cs`
- ✅ `Controllers/UserController.cs` (新版本)
- ✅ `Program.cs` (优化版)
- ✅ `Configs/appsettings.json` (优化版)
- ✅ `Extensions/ServiceCollection/*`

## 下一步行动

### 立即执行

1. **恢复依赖**：更新三个项目文件，恢复必要的 NuGet 包
2. **验证编译**：确保项目可以成功编译
3. **测试运行**：启动项目验证基本功能

### 短期目标（1 周）

1. 完成目录重命名
2. 迁移用户模块
3. 测试注册/登录功能

### 中期目标（2 周）

1. 迁移文件模块
2. 集成 OnlyOffice
3. 完善缓存策略

### 长期目标（1 月）

1. 完成所有模块迁移
2. 添加单元测试
3. 性能优化

## 决策建议

如果您希望：

### 快速上线 → 选择方案 B（渐进式）
1. 先恢复依赖，确保编译通过
2. 逐步迁移核心功能
3. 保持现有功能可用

### 长期维护 → 选择方案 A（完全重构）
1. 创建新分支
2. 使用新架构重写
3. 并行运行，逐步切换

## 联系支持

如有问题，请参考：
- `ARCHITECTURE.md` - 架构设计
- `QUICKSTART.md` - 快速开始
- `MIGRATION_GUIDE.md` - 迁移指南
