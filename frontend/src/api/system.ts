import { api, type ApiResponse } from './index'

export interface OperationLogItem {
  id: number
  userId?: number
  userName?: string
  phone?: string
  method: string
  path: string
  actionName?: string
  ipAddress?: string
  requestQuery?: string
  statusCode: number
  isSuccess: boolean
  durationMs: number
  errorMessage?: string
  createdTime: string
}

export interface OperationLogQuery {
  keyword?: string
  startTime?: string
  endTime?: string
  pageIndex?: number
  pageSize?: number
}

export interface StorageStats {
  userUsedBytes: number
  userRecycleBytes: number
  userFileCount: number
  userFolderCount: number
  systemUsedBytes: number
  systemRecycleBytes: number
  systemFileCount: number
  systemFolderCount: number
}

export function getOperationLogs(params: OperationLogQuery): Promise<ApiResponse<OperationLogItem[]>> {
  return api.get<OperationLogItem[]>('/system/operation-logs', { params }).then((res) => res.data)
}

export function getStorageStats(): Promise<ApiResponse<StorageStats>> {
  return api.get<StorageStats>('/system/storage-stats').then((res) => res.data)
}

export function cleanupExpiredRecycleBin(): Promise<ApiResponse<number>> {
  return api.post<number>('/system/recycle/cleanup-expired').then((res) => res.data)
}
