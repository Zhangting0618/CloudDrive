import type { AxiosResponse } from 'axios'
import { api, ApiResponse } from './index'

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

export function getRecycleBinList(pageIndex: number = 1, pageSize: number = 10): Promise<AxiosResponse<ApiResponse<RecycleBinItem[]>>> {
  return api.get<RecycleBinItem[]>('/file/recycle', {
    params: { pageIndex, pageSize },
  })
}

export function restoreFile(fileId: number): Promise<ApiResponse<boolean>> {
  return api.post<boolean>(`/file/recycle/${fileId}/restore`).then((res) => res.data)
}

export function deleteFilePermanently(fileId: number): Promise<ApiResponse<boolean>> {
  return api.delete<boolean>(`/file/recycle/${fileId}`).then((res) => res.data)
}

export function clearRecycleBin(): Promise<ApiResponse<boolean>> {
  return api.delete<boolean>('/file/recycle').then((res) => res.data)
}

export function formatFileSize(bytes?: number): string {
  if (!bytes || bytes === 0) return '0 B'

  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))

  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`
}

export function formatDate(date?: string): string {
  if (!date) return '-'

  const d = new Date(date)
  const now = new Date()
  const diff = now.getTime() - d.getTime()

  if (diff < 60_000) return '刚刚'
  if (diff < 3_600_000) return `${Math.floor(diff / 60_000)}分钟前`
  if (diff < 86_400_000) return `${Math.floor(diff / 3_600_000)}小时前`
  if (diff < 604_800_000) return `${Math.floor(diff / 86_400_000)}天前`

  return d.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  })
}
