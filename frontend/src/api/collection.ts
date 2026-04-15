import type { AxiosResponse } from 'axios'
import { api, ApiResponse } from './index'

export interface CollectionItem {
  collectionId: number
  fileId: number
  fileName: string
  isFolder: boolean
  fileSize?: number
  extension?: string
  createdDate?: string
  collectionTime: string
}

export function getCollections(pageIndex: number = 1, pageSize: number = 10): Promise<AxiosResponse<ApiResponse<CollectionItem[]>>> {
  return api.get<CollectionItem[]>('/file/collections', {
    params: { pageIndex, pageSize },
  })
}

export function addToCollection(fileId: number): Promise<ApiResponse<boolean>> {
  return api.post<boolean>('/file/collection', { fileId }).then((res) => res.data)
}

export function removeFromCollection(fileId: number): Promise<ApiResponse<boolean>> {
  return api.delete<boolean>(`/file/collection/${fileId}`).then((res) => res.data)
}

export function checkCollection(fileId: number): Promise<ApiResponse<boolean>> {
  return api.get<boolean>(`/file/collection/check/${fileId}`).then((res) => res.data)
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
