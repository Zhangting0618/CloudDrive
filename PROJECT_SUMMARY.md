# Ptcent Cloud Drive - 项目总结

## 项目概述

Ptcent Cloud Drive 是一个基于 Clean Architecture 架构的云端磁盘系统，采用前后端分离设计。

**后端**: .NET 8 + Clean Architecture + CQRS + EF Core + MySQL
**前端**: Vue 3 + TypeScript + Element Plus + Pinia

## 完成的优化和重构

### 1. 架构优化 ✅

- ✅ 重构为 Clean Architecture 架构
- ✅ 实现 CQRS 模式（使用 MediatR）
- ✅ 明确的分层依赖关系
- ✅ 统一的响应模型
- ✅ 全局异常处理

### 2. 移除 OnlyOffice 功能 ✅

- ✅ 删除 OnlyOffice 相关代码
- ✅ 删除 OnlyOffice 相关配置
- ✅ 改用前端预览方案

### 3. 文件上传下载优化 ✅

**后端新增功能**:
- ✅ 文件秒传（基于 Hash）
- ✅ 批量下载（ZIP 打包）
- ✅ 文件预览接口
- ✅ 文件类型验证
- ✅ 文件大小限制

**前端新增功能**:
- ✅ 拖拽上传
- ✅ 上传进度显示
- ✅ 批量选择下载
- ✅ 文件预览（图片/PDF/视频/文本）
- ✅ 面包屑导航
- ✅ 文件夹管理

### 4. 安全性提升 ✅

- ✅ JWT 认证
- ✅ 密码 SHA256 加密
- ✅ 输入验证（FluentValidation）
- ✅ 文件类型白名单
- ✅ 文件大小限制

### 5. 高可用特性 ✅

- ✅ 全局异常处理中间件
- ✅ 健康检查端点 `/health`
- ✅ 数据库重试机制
- ✅ 分布式缓存（Redis）
- ✅ 响应压缩

## 项目结构

### 后端结构

```
src/
├── Ptcent.Cloud.Drive.Domain/          # 领域层
│   ├── Entities/
│   │   ├── UserEntity.cs
│   │   └── FileEntity.cs
│   └── Constants/
│       └── CacheKeys.cs
│
├── Ptcent.Cloud.Drive.Application/     # 应用层
│   ├── Contracts/
│   │   ├── Requests/
│   │   └── Responses/
│   ├── Features/
│   │   ├── Users/Commands/
│   │   └── Files/Commands|Queries/
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IUserRepository.cs
│   │   └── IFileRepository.cs
│   └── Services/
│       ├── ICacheService.cs
│       ├── IJwtService.cs
│       ├── IPasswordHasher.cs
│       └── IFileHashService.cs
│
├── Ptcent.Cloud.Drive.Infrastructure/  # 基础设施层
│   ├── Persistence/
│   │   └── AppDbContext.cs
│   ├── Repositories/
│   │   ├── Repository.cs
│   │   ├── UserRepository.cs
│   │   └── FileRepository.cs
│   └── Services/
│       ├── CacheService.cs
│       ├── JwtService.cs
│       ├── PasswordHasher.cs
│       └── FileHashService.cs
│
└── Ptcent.Cloud.Drive.Web/             # 表示层
    ├── Controllers/
    │   ├── UserController.cs
    │   ├── FileController.cs
    │   └── DownFileController.cs
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs
    └── Extensions/ServiceCollection/
```

### 前端结构

```
frontend/
├── src/
│   ├── api/
│   │   ├── index.ts      # Axios 配置
│   │   ├── user.ts       # 用户 API
│   │   └── file.ts       # 文件 API
│   ├── stores/
│   │   ├── user.ts       # 用户状态
│   │   └── file.ts       # 文件状态
│   ├── views/
│   │   ├── Login.vue     # 登录页
│   │   ├── Register.vue  # 注册页
│   │   ├── Home.vue      # 主页
│   │   ├── Files.vue     # 文件列表
│   │   └── Preview.vue   # 预览页
│   ├── router/
│   ├── types/
│   └── utils/
```

## API 接口列表

### 认证接口

| 方法 | 路径 | 说明 | 认证 |
|------|------|------|------|
| POST | /api/user/login | 用户登录 | ❌ |
| POST | /api/user/register | 用户注册 | ❌ |
| POST | /api/user/logout | 用户登出 | ✅ |

### 文件接口

| 方法 | 路径 | 说明 | 认证 |
|------|------|------|------|
| POST | /api/file/upload | 上传文件 | ✅ |
| GET | /api/file/list | 获取文件列表 | ✅ |
| POST | /api/file/folder | 创建文件夹 | ✅ |
| DELETE | /api/file/{id} | 删除文件 | ✅ |
| PUT | /api/file/{id}/rename | 重命名文件 | ✅ |
| POST | /api/file/move | 移动文件 | ✅ |
| GET | /api/file/{id}/preview | 文件预览 | ✅ |

### 下载接口

| 方法 | 路径 | 说明 | 认证 |
|------|------|------|------|
| GET | /api/downfile/download/{id} | 单个下载 | ✅ |
| POST | /api/downfile/download/batch | 批量下载 | ✅ |

## 快速开始

### 1. 配置数据库

```sql
CREATE DATABASE clouddrive CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- 创建用户表
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
    UpdateBy BIGINT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 创建文件表
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
    ItemFileMapUrl VARCHAR(500)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 2. 配置后端

编辑 `src/Ptcent.Cloud.Drive.Web/Configs/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=clouddrive;user=root;password=yourpassword;",
    "Redis": "localhost:6379"
  },
  "Authentication": {
    "SecretKey": "your-secret-key-at-least-32-characters",
    "Issuer": "PtcentCloudDrive",
    "Audience": "PtcentCloudDriveUsers",
    "ExpiresDays": 30
  },
  "FileStorage": {
    "RootPath": "D:\\FileStorage",
    "MaxFileSizeBytes": 104857600
  }
}
```

### 3. 启动后端

```bash
cd src/Ptcent.Cloud.Drive.Web
dotnet run
```

访问 http://localhost:5000/swagger 查看 API 文档

### 4. 启动前端

```bash
cd frontend
npm install
npm run dev
```

访问 http://localhost:3000

## 核心功能说明

### 1. 用户认证

- 手机号 + 密码登录
- JWT Token 认证
- Token 有效期 30 天
- 自动刷新机制

### 2. 文件上传

- 支持拖拽上传
- 文件 Hash 计算（秒传）
- 文件类型验证
- 文件大小限制（100MB）
- 上传进度显示

### 3. 文件下载

- 单个文件下载
- 批量下载（ZIP 打包）
- 断点续传支持
- 中文文件名处理

### 4. 文件预览

- 图片：JPG, PNG, GIF, BMP
- PDF 文档
- 视频：MP4, WebM
- 文本：TXT, MD, JSON

### 5. 文件管理

- 文件夹创建
- 文件重命名
- 文件删除
- 文件移动
- 面包屑导航

## 技术亮点

### 1. Clean Architecture

- 清晰的关注点分离
- 依赖倒置原则
- 便于单元测试
- 易于维护和扩展

### 2. CQRS 模式

- 命令查询职责分离
- 使用 MediatR 实现
- 管道行为处理横切关注点

### 3. 文件秒传

```csharp
// 计算文件 Hash
var fileHash = await _fileHashService.ComputeHashAsync(stream);

// 检查是否存在相同 Hash 的文件
var existingFile = await _fileRepository.FirstOrDefaultAsync(
    f => f.ItemHash == fileHash && f.IsDel == 0
);

if (existingFile != null) {
    // 秒传成功
    return existingFile;
}
```

### 4. 全局异常处理

```csharp
// 中间件统一处理异常
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (Exception ex)
    {
        await HandleExceptionAsync(context, ex);
    }
}
```

### 5. 前端状态管理

```typescript
// 使用 Pinia 管理用户状态
export const useUserStore = defineStore('user', () => {
  const token = ref(localStorage.getItem('token') || '')
  const isLoggedIn = computed(() => !!token.value)

  const loginAction = async (phone: string, password: string) => {
    const res = await loginApi({ phone, password })
    token.value = res.data
    localStorage.setItem('token', res.data)
  }

  return { token, isLoggedIn, loginAction }
})
```

## 文档索引

### 后端文档

- `src/README.md` - 后端项目说明
- `src/ARCHITECTURE.md` - 架构设计文档
- `src/QUICKSTART.md` - 快速开始指南
- `src/MIGRATION_GUIDE.md` - 迁移指南
- `src/FRONTEND_INTEGRATION_GUIDE.md` - 前后端集成指南

### 前端文档

- `frontend/README.md` - 前端项目说明

## 开发计划

### 已完成 ✅

- [x] Clean Architecture 架构重构
- [x] 用户认证模块
- [x] 文件上传下载
- [x] 文件预览（前端）
- [x] 文件夹管理
- [x] 批量下载
- [x] 文件秒传
- [x] Vue3 前端

### 待开发 ⏳

- [ ] 文件分享功能
- [ ] 文件版本管理
- [ ] 文件搜索
- [ ] 文件收藏
- [ ] 回收站
- [ ] 文件评论
- [ ] 离线下载
- [ ] 移动端适配

## 性能优化建议

### 后端

1. 使用 Redis 缓存文件元数据
2. 数据库查询使用 `AsNoTracking()`
3. 添加数据库索引
4. 实现分页查询
5. 使用响应压缩

### 前端

1. 路由懒加载
2. 图片懒加载
3. 虚拟滚动（大文件列表）
4. 防抖节流
5. 组件缓存（Keep-Alive）

## 安全建议

1. 使用 HTTPS
2. 定期更换 JWT SecretKey
3. 实施文件类型验证
4. 限制文件大小和数量
5. 防止 XSS 和 CSRF 攻击
6. 实施访问控制（文件权限）

## 常见问题

### Q: 如何修改上传文件大小限制？

编辑 `appsettings.json`:
```json
{
  "FileStorage": {
    "MaxFileSizeBytes": 209715200  // 200MB
  }
}
```

### Q: 如何配置 Redis？

编辑 `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Redis": "your-redis-host:6379"
  }
}
```

### Q: 如何添加新的文件类型支持？

编辑 `appsettings.json`:
```json
{
  "FileStorage": {
    "AllowedExtensions": [".jpg", ".pdf", ".newtype"]
  }
}
```

### Q: 前端如何修改 API 地址？

编辑 `vite.config.ts`:
```typescript
proxy: {
  '/cloudapi': {
    target: 'http://your-backend-host:5000',
    changeOrigin: true
  }
}
```

## 技术栈版本

### 后端

- .NET 8.0
- Entity Framework Core 8.0
- MySQL 8.0
- Redis
- MediatR 12.2
- FluentValidation 11.8
- Element Plus 2.5

### 前端

- Vue 3.4
- TypeScript 5
- Vite 5
- Element Plus 2.5
- Pinia 2
- Vue Router 4
- Axios 1.6

## License

MIT

## 联系方式

- 邮箱：support@example.com
- 网站：https://www.elitesland.com

---

**最后更新**: 2026-03-05
**版本**: v2.0.0
