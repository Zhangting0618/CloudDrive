// 通用类型定义

// API 响应基础结构
export interface ApiResponse<T = any> {
  code: number
  isSuccess: boolean
  message: string
  data: T
  totalCount?: number
}

// 分页参数
export interface PageParams {
  page?: number
  pageSize?: number
}

// 分页结果
export interface PageResult<T> {
  items: T[]
  total: number
  pageIndex: number
  pageSize: number
}

// 上传文件结果
export interface UploadResult {
  fileId: number
  fileName: string
  filePath: string
  fileSize: number
  fileType: string
}

// 用户信息
export interface UserInfo {
  id: number
  userName: string
  phone: string
  email?: string
  imageUrl?: string
}

// 文件信息
export interface FileItem {
  id: number
  name: string
  extension?: string
  isFolder: boolean
  size?: number
  fileType?: string
  createdDate?: string
  previewUrl?: string
}
