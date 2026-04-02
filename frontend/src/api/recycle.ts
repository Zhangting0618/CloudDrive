import { api, ApiResponse } from './index'

// 回收站文件项
export interface RecycleBinItem {
  id: number
  name: string
  extension?: string
  isFolder: boolean
  size?: number
  fileType?: string
  deletedDate?: string
  fileSizeStr?: string
}

/**
 * 获取回收站列表
 */
export function getRecycleBinList(pageIndex: number = 1, pageSize: number = 10): Promise<ApiResponse<RecycleBinItem[]>> {
  return api.get('/file/recycle', {
    params: { pageIndex, pageSize }
  })
}

/**
 * 还原文件
 */
export function restoreFile(fileId: number): Promise<ApiResponse<boolean>> {
  return api.post(`/file/recycle/${fileId}/restore`)
}

/**
 * 彻底删除文件
 */
export function deleteFilePermanently(fileId: number): Promise<ApiResponse<boolean>> {
  return api.delete(`/file/recycle/${fileId}`)
}

/**
 * 清空回收站
 */
export function clearRecycleBin(): Promise<ApiResponse<boolean>> {
  return api.delete('/file/recycle')
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
