import { api, ApiResponse } from './index'

export interface CreateShareParams {
  fileId: number
  accessPassword?: string
  expireDays?: number
  maxVisitCount?: number
}

export interface ShareResult {
  shareId: number
  shareCode: string
  shareUrl: string
  expireTime?: string
  hasPassword: boolean
}

export interface ShareInfo {
  fileId: number
  fileName: string
  isFolder: boolean
  fileSize: number
  userName: string
  expireTime?: string
  hasPassword: boolean
  isExpired: boolean
  isValid: boolean
}

export interface ShareAccessToken {
  accessToken: string
}

export interface MyShareItem {
  shareId: number
  fileId: number
  fileName: string
  isFolder: boolean
  shareCode: string
  shareUrl: string
  expireTime?: string
  hasPassword: boolean
  visitCount: number
  isValid: boolean
  createTime: string
}

export function createShare(data: CreateShareParams): Promise<ApiResponse<ShareResult>> {
  return api.post<ShareResult>('/file/share', data).then((res) => res.data)
}

export function getShareInfo(shareCode: string): Promise<ApiResponse<ShareInfo>> {
  return api.get<ShareInfo>(`/file/share/${shareCode}`).then((res) => res.data)
}

export function verifyShareAccess(shareCode: string, password: string): Promise<ApiResponse<ShareAccessToken>> {
  return api.post<ShareAccessToken>(`/file/share/${shareCode}/verify`, { password }).then((res) => res.data)
}

export function cancelShare(shareId: number): Promise<ApiResponse<boolean>> {
  return api.delete<boolean>(`/file/share/${shareId}`).then((res) => res.data)
}

export function getMyShares(pageIndex: number = 1, pageSize: number = 10): Promise<ApiResponse<MyShareItem[]>> {
  return api.get<MyShareItem[]>('/file/shares', {
    params: { pageIndex, pageSize },
  }).then((res) => res.data)
}

export function buildShareDownloadUrl(shareCode: string, accessToken?: string): string {
  const params = new URLSearchParams()
  if (accessToken) {
    params.set('accessToken', accessToken)
  }

  const query = params.toString()
  return `/cloudapi/file/share/${shareCode}/download${query ? `?${query}` : ''}`
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
