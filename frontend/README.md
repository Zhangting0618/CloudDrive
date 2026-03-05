# Ptcent Cloud Drive - 前端

基于 Vue3 + TypeScript + Element Plus 的云端磁盘前端项目。

## 技术栈

- **框架**: Vue 3.4
- **构建工具**: Vite 5
- **语言**: TypeScript 5
- **UI 组件库**: Element Plus 2.5
- **状态管理**: Pinia 2
- **路由**: Vue Router 4
- **HTTP 客户端**: Axios 1.6
- **图标**: Element Plus Icons
- **样式**: SCSS

## 功能特性

- ✅ 用户登录/注册
- ✅ 文件上传（支持拖拽）
- ✅ 文件下载
- ✅ 批量下载（ZIP 打包）
- ✅ 文件夹管理
- ✅ 文件预览（图片、PDF、视频、文本）
- ✅ 文件重命名
- ✅ 文件删除
- ✅ 面包屑导航
- ✅ JWT 认证

## 项目结构

```
frontend/
├── src/
│   ├── api/              # API 接口
│   │   ├── index.ts      # Axios 配置
│   │   ├── user.ts       # 用户接口
│   │   └── file.ts       # 文件接口
│   ├── components/       # 公共组件
│   ├── views/            # 页面组件
│   │   ├── Login.vue     # 登录页
│   │   ├── Register.vue  # 注册页
│   │   ├── Home.vue      # 主页
│   │   ├── Files.vue     # 文件列表
│   │   ├── Preview.vue   # 文件预览
│   │   └── NotFound.vue  # 404 页面
│   ├── stores/           # Pinia 状态管理
│   │   ├── user.ts       # 用户状态
│   │   └── file.ts       # 文件状态
│   ├── router/           # 路由配置
│   ├── types/            # TypeScript 类型定义
│   ├── utils/            # 工具函数
│   ├── App.vue           # 根组件
│   └── main.ts           # 入口文件
├── package.json
├── tsconfig.json
├── vite.config.ts
└── README.md
```

## 快速开始

### 1. 安装依赖

```bash
npm install
```

### 2. 配置开发环境

确保后端服务运行在 `http://localhost:5000`

### 3. 启动开发服务器

```bash
npm run dev
```

访问 `http://localhost:3000`

### 4. 构建生产版本

```bash
npm run build
```

### 5. 预览生产构建

```bash
npm run preview
```

## API 配置

在 `vite.config.ts` 中配置代理：

```typescript
server: {
  port: 3000,
  proxy: {
    '/cloudapi': {
      target: 'http://localhost:5000',
      changeOrigin: true,
      rewrite: (path) => path.replace(/^\/cloudapi/, '/api')
    }
  }
}
```

## 主要功能说明

### 文件上传

支持拖拽上传和点击选择上传，自动计算文件 Hash 实现秒传。

```typescript
import { uploadFile } from '@/api/file'

const result = await uploadFile(file, parentFolderId, filePath)
```

### 文件下载

支持单个文件下载和批量下载（ZIP 打包）。

```typescript
import { downloadFile, batchDownload } from '@/api/file'

// 单个下载
const blob = await downloadFile(fileId)

// 批量下载
const blob = await batchDownload([fileId1, fileId2])
```

### 文件预览

根据文件类型自动选择预览方式：

- 图片：使用 Element Plus Image 组件
- PDF：使用 iframe 嵌入
- 视频：使用 HTML5 video 标签
- 文本：直接显示内容

### 状态管理

使用 Pinia 管理用户状态和文件状态：

```typescript
import { useUserStore } from '@/stores/user'
import { useFileStore } from '@/stores/file'

const userStore = useUserStore()
const fileStore = useFileStore()
```

## 代码规范

### 命名规范

- 组件：PascalCase (如 `FileList.vue`)
- 函数/变量：camelCase (如 `handleUpload`)
- 常量：UPPER_SNAKE_CASE (如 `API_BASE_URL`)
- 类型：PascalCase (如 `UploadResult`)

### 代码风格

- 使用 Composition API
- 使用 `<script setup>` 语法
- 使用 TypeScript 类型注解
- 使用 SCSS 样式

## 常见问题

### Q: 跨域问题
A: 开发环境使用 Vite 代理，生产环境需要配置 CORS。

### Q: 大文件上传
A: 后端已配置 100MB 上传限制，可通过修改配置调整。

### Q: 文件预览失败
A: 检查后端是否正确返回文件内容和 Content-Type。

## 浏览器支持

- Chrome >= 90
- Firefox >= 88
- Safari >= 14
- Edge >= 90

## 开发计划

- [ ] 文件分享功能
- [ ] 文件版本管理
- [ ] 离线下载
- [ ] 文件搜索
- [ ] 文件收藏
- [ ] 回收站功能
- [ ] 文件评论
- [ ] 协作编辑

## License

MIT
