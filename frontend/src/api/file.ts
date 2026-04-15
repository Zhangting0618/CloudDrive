import type { AxiosResponse } from 'axios'
import request, { api, ApiResponse } from './index'

export interface FileItem {
  id: number
  name: string
  parentFolderId?: number | null
  extension?: string
  isFolder: boolean
  size?: number
  fileType?: string
  createdDate?: string
  updatedDate?: string
  previewUrl?: string
}

export interface UploadResult {
  fileId: number
  fileName: string
  filePath: string
  fileSize: number
  fileType: string
}

export interface FileListParams {
  parentFolderId?: number
  page?: number
  pageSize?: number
}

export function uploadFile(file: File, parentFolderId?: number, filePath?: string): Promise<ApiResponse<UploadResult>> {
  const formData = new FormData()
  formData.append('file', file)

  if (parentFolderId !== undefined) {
    formData.append('parentFolderId', parentFolderId.toString())
  }

  if (filePath) {
    formData.append('filePath', filePath)
  }

  return api.upload<UploadResult>('/file/upload', formData).then((res) => res.data)
}

export function getFileList(params?: FileListParams): Promise<AxiosResponse<ApiResponse<FileItem[]>>> {
  return api.get<FileItem[]>('/file/list', { params })
}

export function createFolder(folderName: string, parentFolderId?: number, path?: string): Promise<ApiResponse<boolean>> {
  return api.post<boolean>('/file/folder', {
    folderName,
    parentFolderId,
    path,
  }).then((res) => res.data)
}

export function deleteFile(fileId: number): Promise<ApiResponse<boolean>> {
  return api.delete<boolean>(`/file/${fileId}`).then((res) => res.data)
}

export function renameFile(fileId: number, newName: string): Promise<ApiResponse<boolean>> {
  return api.put<boolean>(`/file/${fileId}/rename`, { newName }).then((res) => res.data)
}

export function moveFile(fileId: number, newParentFolderId: number | null): Promise<ApiResponse<boolean>> {
  return api.post<boolean>('/file/move', {
    fileId,
    newParentFolderId,
  }).then((res) => res.data)
}

export function copyFile(fileId: number, targetParentFolderId: number | null): Promise<ApiResponse<boolean>> {
  return api.post<boolean>('/file/copy', {
    fileId,
    targetParentFolderId,
  }).then((res) => res.data)
}

export function downloadFile(fileId: number): Promise<Blob> {
  return request.get<Blob>(`/downfile/download/${fileId}`, {
    responseType: 'blob',
  }).then((res) => res.data)
}

export function batchDownload(fileIds: number[]): Promise<Blob> {
  return request.post<Blob>('/downfile/download/batch', { fileIds }, {
    responseType: 'blob',
  }).then((res) => res.data)
}

export function getFilePreviewUrl(fileId: number): string {
  return `/cloudapi/file/${fileId}/preview`
}

export function getFileIcon(extension?: string): string {
  const ext = extension?.toLowerCase() || ''

  const iconMap: Record<string, string> = {
    folder: 'Folder',
    '.jpg': 'Picture',
    '.jpeg': 'Picture',
    '.png': 'Picture',
    '.gif': 'Picture',
    '.bmp': 'Picture',
    '.svg': 'Picture',
    '.pdf': 'Document',
    '.doc': 'Document',
    '.docx': 'Document',
    '.xls': 'Office',
    '.xlsx': 'Office',
    '.ppt': 'Office',
    '.pptx': 'Office',
    '.txt': 'Document',
    '.zip': 'Files',
    '.rar': 'Files',
    '.7z': 'Files',
    '.mp4': 'VideoCamera',
    '.avi': 'VideoCamera',
    '.mov': 'VideoCamera',
    '.mp3': 'Headset',
    '.wav': 'Headset',
  }

  return iconMap[ext] || 'Document'
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
