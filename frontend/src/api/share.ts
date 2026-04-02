import { api, ApiResponse } from './index'

// 分享创建请求
export interface CreateShareParams {
  fileId: number
  accessPassword?: string
  expireDays?: number
  maxVisitCount?: number
}

// 分享结果
export interface ShareResult {
  shareId: number
  shareCode: string
  shareUrl: string
  expireTime?: string
  hasPassword: boolean
}

// 分享信息
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

// 我的分享项
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

/**
 * 创建分享
 */
export function createShare(data: CreateShareParams): Promise<ApiResponse<ShareResult>> {
  return api.post('/file/share', data)
}

/**
 * 获取分享信息
 */
export function getShareInfo(shareCode: string): Promise<ApiResponse<ShareInfo>> {
  return api.get(`/file/share/${shareCode}`)
}

/**
 * 取消分享
 */
export function cancelShare(shareId: number): Promise<ApiResponse<boolean>> {
  return api.delete(`/file/share/${shareId}`)
}

/**
 * 获取我的分享列表
 */
export function getMyShares(pageIndex: number = 1, pageSize: number = 10): Promise<ApiResponse<MyShareItem[]>> {
  return api.get('/file/shares', {
    params: { pageIndex, pageSize }
  })
}
