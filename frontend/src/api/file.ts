import { api, ApiResponse } from './index'

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

// 上传结果
export interface UploadResult {
  fileId: number
  fileName: string
  filePath: string
  fileSize: number
  fileType: string
}

// 文件列表请求参数
export interface FileListParams {
  parentFolderId?: number
  page?: number
  pageSize?: number
}

/**
 * 上传文件
 */
export function uploadFile(file: File, parentFolderId?: number, filePath?: string): Promise<ApiResponse<UploadResult>> {
  const formData = new FormData()
  formData.append('file', file)
  if (parentFolderId !== undefined) {
    formData.append('parentFolderId', parentFolderId.toString())
  }
  if (filePath) {
    formData.append('filePath', filePath)
  }
  return api.upload('/file/upload', formData)
}

/**
 * 获取文件列表
 */
export function getFileList(params?: FileListParams): Promise<ApiResponse<FileItem[]>> {
  return api.get('/file/list', { params })
}

/**
 * 创建文件夹
 */
export function createFolder(folderName: string, parentFolderId?: number, path?: string): Promise<ApiResponse<boolean>> {
  return api.post('/file/folder', {
    folderName,
    parentFolderId,
    path,
  })
}

/**
 * 删除文件
 */
export function deleteFile(fileId: number): Promise<ApiResponse<boolean>> {
  return api.delete(`/file/${fileId}`)
}

/**
 * 重命名文件
 */
export function renameFile(fileId: number, newName: string): Promise<ApiResponse<boolean>> {
  return api.put(`/file/${fileId}/rename`, { newName })
}

/**
 * 移动文件
 */
export function moveFile(fileId: number, newParentFolderId: number): Promise<ApiResponse<boolean>> {
  return api.post('/file/move', {
    fileId,
    newParentFolderId,
  })
}

/**
 * 下载文件
 */
export function downloadFile(fileId: number): Promise<Blob> {
  return api.download(`/downfile/download/${fileId}`, {
    responseType: 'blob',
  }).then(res => res.data)
}

/**
 * 批量下载
 */
export function batchDownload(fileIds: number[]): Promise<Blob> {
  return api.post('/downfile/download/batch', { fileIds }, {
    responseType: 'blob',
  }).then(res => res.data)
}

/**
 * 获取文件预览 URL
 */
export function getFilePreviewUrl(fileId: number): string {
  return `/cloudapi/file/${fileId}/preview`
}

/**
 * 获取文件类型图标
 */
export function getFileIcon(extension?: string): string {
  const ext = extension?.toLowerCase() || ''

  const iconMap: Record<string, string> = {
    // 文件夹
    folder: 'Folder',
    // 图片
    '.jpg': 'Picture',
    '.jpeg': 'Picture',
    '.png': 'Picture',
    '.gif': 'Picture',
    '.bmp': 'Picture',
    '.svg': 'Picture',
    // PDF
    '.pdf': 'Document',
    // Word
    '.doc': 'Document',
    '.docx': 'Document',
    // Excel
    '.xls': 'Office',
    '.xlsx': 'Office',
    // PPT
    '.ppt': 'Office',
    '.pptx': 'Office',
    // 文本
    '.txt': 'Document',
    // 压缩包
    '.zip': 'Files',
    '.rar': 'Files',
    '.7z': 'Files',
    // 视频
    '.mp4': 'VideoCamera',
    '.avi': 'VideoCamera',
    '.mov': 'VideoCamera',
    // 音频
    '.mp3': 'Headset',
    '.wav': 'Headset',
  }

  return iconMap[ext] || 'Document'
}

/**
 * 格式化文件大小
 */
export function formatFileSize(bytes?: number): string {
  if (!bytes || bytes === 0) return '0 B'

  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))

  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

/**
 * 格式化日期
 */
export function formatDate(date?: string): string {
  if (!date) return '-'

  const d = new Date(date)
  const now = new Date()
  const diff = now.getTime() - d.getTime()

  // 1 分钟内
  if (diff < 60000) {
    return '刚刚'
  }

  // 1 小时内
  if (diff < 3600000) {
    return `${Math.floor(diff / 60000)}分钟前`
  }

  // 24 小时内
  if (diff < 86400000) {
    return `${Math.floor(diff / 3600000)}小时前`
  }

  // 7 天内
  if (diff < 604800000) {
    return `${Math.floor(diff / 86400000)}天前`
  }

  // 超过 7 天显示具体日期
  return d.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  })
}
