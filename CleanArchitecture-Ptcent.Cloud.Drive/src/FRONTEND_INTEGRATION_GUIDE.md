# 前后端集成指南

## 项目架构

```
┌─────────────────────────────────────────────────────────────┐
│                        前端 (Vue3)                           │
│  http://localhost:3000                                       │
│                                                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │   Login     │  │   Files     │  │  Preview    │         │
│  │   Register  │  │   Upload    │  │  Download   │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ HTTP / Bearer Token
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      后端 (.NET 8)                           │
│  http://localhost:5000                                       │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                  API Controllers                     │    │
│  │  /api/user/*      /api/file/*     /api/downfile/*   │    │
│  └─────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Application Layer (CQRS)                │    │
│  │  Commands / Queries / Handlers / Validators          │    │
│  └─────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │           Domain + Infrastructure Layers             │    │
│  │  Entities / Repositories / Services                  │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                     MySQL + Redis                          │
└─────────────────────────────────────────────────────────────┘
```

## API 接口文档

### 认证接口

#### 1. 用户登录
```
POST /api/user/login
Content-Type: application/json

Request:
{
  "phone": "13800138000",
  "password": "123456"
}

Response:
{
  "code": 200,
  "isSuccess": true,
  "message": "登录成功",
  "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### 2. 用户注册
```
POST /api/user/register
Content-Type: application/json

Request:
{
  "userName": "张三",
  "phone": "13800138000",
  "password": "123456",
  "email": "zhangsan@example.com",
  "sex": 0
}

Response:
{
  "code": 200,
  "isSuccess": true,
  "message": "注册成功",
  "data": true
}
```

#### 3. 用户登出
```
POST /api/user/logout
Authorization: Bearer {token}

Response:
{
  "code": 200,
  "isSuccess": true,
  "message": "登出成功",
  "data": true
}
```

### 文件接口

#### 1. 上传文件
```
POST /api/file/upload
Content-Type: multipart/form-data
Authorization: Bearer {token}

Form Data:
- file: [文件]
- parentFolderId: [可选] 父文件夹 ID
- filePath: [可选] 文件路径

Response:
{
  "code": 200,
  "isSuccess": true,
  "message": "文件上传成功",
  "data": {
    "fileId": 1234567890,
    "fileName": "test.pdf",
    "filePath": "/",
    "fileSize": 102400,
    "fileType": "PDF 文档"
  }
}
```

#### 2. 获取文件列表
```
GET /api/file/list?parentFolderId=123
Authorization: Bearer {token}

Response:
{
  "code": 200,
  "isSuccess": true,
  "message": "获取成功",
  "data": [
    {
      "id": 123,
      "name": "文件夹 1",
      "isFolder": true,
      "createdDate": "2024-01-01T00:00:00Z"
    },
    {
      "id": 456,
      "name": "文件 1.pdf",
      "extension": ".pdf",
      "isFolder": false,
      "size": 102400,
      "createdDate": "2024-01-01T00:00:00Z"
    }
  ]
}
```

#### 3. 创建文件夹
```
POST /api/file/folder
Content-Type: application/json
Authorization: Bearer {token}

Request:
{
  "folderName": "新文件夹",
  "parentFolderId": 123,
  "path": "/"
}

Response:
{
  "code": 200,
  "isSuccess": true,
  "message": "创建成功",
  "data": true
}
```

#### 4. 删除文件
```
DELETE /api/file/{fileId}
Authorization: Bearer {token}

Response:
{
  "code": 200,
  "isSuccess": true,
  "message": "删除成功",
  "data": true
}
```

#### 5. 重命名文件
```
PUT /api/file/{fileId}/rename
Content-Type: application/json
Authorization: Bearer {token}

Request:
{
  "newName": "新文件名.pdf"
}

Response:
{
  "code": 200,
  "isSuccess": true,
  "message": "重命名成功",
  "data": true
}
```

#### 6. 移动文件
```
POST /api/file/move
Content-Type: application/json
Authorization: Bearer {token}

Request:
{
  "fileId": 123,
  "newParentFolderId": 456
}

Response:
{
  "code": 200,
  "isSuccess": true,
  "message": "移动成功",
  "data": true
}
```

### 下载接口

#### 1. 单个文件下载
```
GET /api/downfile/download/{fileId}
Authorization: Bearer {token}

Response:
Content-Type: application/octet-stream
Content-Disposition: attachment; filename*=UTF-8''文件名
[文件二进制数据]
```

#### 2. 批量下载
```
POST /api/downfile/download/batch
Content-Type: application/json
Authorization: Bearer {token}

Request:
{
  "fileIds": [123, 456, 789]
}

Response:
Content-Type: application/zip
Content-Disposition: attachment; filename*=UTF-8''download_20240101.zip
[ZIP 文件二进制数据]
```

#### 3. 文件预览
```
GET /api/file/{fileId}/preview
Authorization: Bearer {token}

Response:
- 图片/PDF/文本：返回文件内容
- 其他类型：返回错误信息
```

## 认证流程

### 1. JWT Token 获取

```typescript
// 登录
const login = async (phone: string, password: string) => {
  const response = await api.post('/user/login', { phone, password })
  const token = response.data.data

  // 存储 token
  localStorage.setItem('token', token)

  // 设置后续请求的 Authorization header
  api.defaults.headers.common['Authorization'] = `Bearer ${token}`
}
```

### 2. JWT Token 使用

```typescript
// Axios 拦截器自动添加 token
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})
```

### 3. JWT Token 刷新

```typescript
// 响应拦截器处理 401
api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      // token 过期，跳转到登录页
      localStorage.removeItem('token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)
```

## 文件上传流程

### 1. 选择文件

```typescript
const handleFileSelect = (event: ChangeEvent<HTMLInputElement>) => {
  const files = event.target.files
  if (files && files.length > 0) {
    uploadFile(files[0])
  }
}
```

### 2. 上传文件

```typescript
const uploadFile = async (file: File) => {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('parentFolderId', currentFolderId.value?.toString() || '')
  formData.append('filePath', '/')

  try {
    const response = await api.upload('/file/upload', formData)

    if (response.data.isSuccess) {
      ElMessage.success('上传成功')
      // 刷新文件列表
      loadFiles()
    }
  } catch (error) {
    ElMessage.error('上传失败')
  }
}
```

### 3. 上传进度

```typescript
const uploadWithProgress = async (file: File) => {
  const formData = new FormData()
  formData.append('file', file)

  await api.post('/file/upload', formData, {
    onUploadProgress: (progressEvent) => {
      const percentCompleted = Math.round(
        (progressEvent.loaded * 100) / (progressEvent.total || 1)
      )
      console.log(`上传进度：${percentCompleted}%`)
    }
  })
}
```

## 文件下载流程

### 1. 单个文件下载

```typescript
const download = async (fileId: number, fileName: string) => {
  try {
    const blob = await api.download(`/downfile/download/${fileId}`)
    saveAs(blob.data, fileName)
    ElMessage.success('下载成功')
  } catch (error) {
    ElMessage.error('下载失败')
  }
}
```

### 2. 批量下载

```typescript
const batchDownload = async (fileIds: number[]) => {
  try {
    const blob = await api.post('/downfile/download/batch', { fileIds }, {
      responseType: 'blob'
    })
    saveAs(blob.data, `download_${Date.now()}.zip`)
    ElMessage.success('下载成功')
  } catch (error) {
    ElMessage.error('下载失败')
  }
}
```

## 文件预览流程

### 1. 图片预览

```vue
<template>
  <el-image
    :src="previewUrl"
    :preview-src-list="[previewUrl]"
    fit="contain"
  />
</template>

<script setup>
const previewUrl = `/api/file/${fileId}/preview`
</script>
```

### 2. PDF 预览

```vue
<template>
  <iframe :src="previewUrl" width="100%" height="100%" />
</template>

<script setup>
const previewUrl = `/api/file/${fileId}/preview`
</script>
```

### 3. 视频预览

```vue
<template>
  <video :src="previewUrl" controls autoplay />
</template>

<script setup>
const previewUrl = `/api/file/${fileId}/preview`
</script>
```

## 错误处理

### 前端错误处理

```typescript
// API 拦截器统一处理错误
api.interceptors.response.use(
  response => {
    const res = response.data
    if (!res.isSuccess) {
      ElMessage.error(res.message)
      return Promise.reject(new Error(res.message))
    }
    return response
  },
  error => {
    // 网络错误、服务器错误等
    ElMessage.error(error.message || '请求失败')
    return Promise.reject(error)
  }
)
```

### 后端错误处理

```csharp
// 全局异常处理中间件
app.UseExceptionHandling();

// 返回统一格式的错误响应
{
  "code": 500,
  "isSuccess": false,
  "message": "系统异常，请稍后重试"
}
```

## 开发环境配置

### 1. 后端配置

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=clouddrive;user=root;password=yourpassword;",
    "Redis": "localhost:6379"
  },
  "Authentication": {
    "SecretKey": "your-secret-key-at-least-32-characters-long",
    "Issuer": "PtcentCloudDrive",
    "Audience": "PtcentCloudDriveUsers",
    "ExpiresDays": 30
  },
  "FileStorage": {
    "RootPath": "D:\\FileStorage",
    "MaxFileSizeBytes": 104857600,
    "AllowedExtensions": [".jpg", ".pdf", ".doc", ".docx"]
  }
}
```

### 2. 前端配置

```typescript
// vite.config.ts
export default defineConfig({
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
})
```

## 启动项目

### 启动后端

```bash
cd src/Ptcent.Cloud.Drive.Web
dotnet run
```

### 启动前端

```bash
cd frontend
npm install
npm run dev
```

### 访问应用

- 前端：http://localhost:3000
- 后端 API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## 常见问题

### Q: 跨域错误
A: 确保前端代理配置正确，或后端配置 CORS。

### Q: 401 未授权
A: 检查 token 是否有效，是否添加到 Authorization header。

### Q: 文件上传失败
A: 检查文件大小是否超过限制，文件类型是否允许。

### Q: 文件下载乱码
A: 确保后端使用 `filename*=UTF-8''` 格式设置中文文件名。

## 部署建议

### 前端部署

```bash
# 构建
npm run build

# 将 dist 目录部署到 Nginx 或其他静态服务器
```

### Nginx 配置

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        root /path/to/dist;
        try_files $uri $uri/ /index.html;
    }

    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### 后端部署

```bash
# 发布
dotnet publish -c Release -o ./publish

# 使用 Kestrel 运行
dotnet Ptcent.Cloud.Drive.Web.dll
```

## 安全建议

1. 使用 HTTPS
2. 定期更换 JWT SecretKey
3. 实施文件类型验证
4. 限制文件大小
5. 防止 XSS 攻击
6. 实施 CSRF 保护
