import { api, ApiResponse } from './index'

// 收藏项
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

/**
 * 获取收藏列表
 */
export function getCollections(pageIndex: number = 1, pageSize: number = 10): Promise<ApiResponse<CollectionItem[]>> {
  return api.get('/file/collections', {
    params: { pageIndex, pageSize }
  })
}

/**
 * 添加收藏
 */
export function addToCollection(fileId: number): Promise<ApiResponse<boolean>> {
  return api.post('/file/collection', { fileId })
}

/**
 * 取消收藏
 */
export function removeFromCollection(fileId: number): Promise<ApiResponse<boolean>> {
  return api.delete(`/file/collection/${fileId}`)
}

/**
 * 检查是否已收藏
 */
export function checkCollection(fileId: number): Promise<ApiResponse<boolean>> {
  return api.get(`/file/collection/check/${fileId}`)
}
