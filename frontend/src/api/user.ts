import { api, ApiResponse } from './index'

export interface LoginParams {
  phone: string
  password: string
}

export interface RegisterParams {
  userName: string
  phone: string
  password: string
  email: string
  sex?: number
}

export interface UserInfo {
  id: number
  userName: string
  phone: string
  email?: string
  sex?: number
  imageUrl?: string
  userType?: number
  isAdmin?: boolean
}

export interface UserManagementItem {
  id: number
  userName: string
  phone: string
  email?: string
  sex?: number
  imageUrl?: string
  registerTime?: string
  userType: number
  isAdmin: boolean
  status: number
  isEnabled: boolean
}

export function login(data: LoginParams): Promise<ApiResponse<string>> {
  return api.post<string>('/user/login', data).then((res) => res.data)
}

export function register(data: RegisterParams): Promise<ApiResponse<boolean>> {
  return api.post<boolean>('/user/register', data).then((res) => res.data)
}

export function logout(): Promise<ApiResponse<boolean>> {
  return api.post<boolean>('/user/logout').then((res) => res.data)
}

export function getCurrentUser(): Promise<ApiResponse<UserInfo>> {
  return api.get<UserInfo>('/user/current').then((res) => res.data)
}

export function getCurrentUserInfo(): Promise<ApiResponse<UserInfo>> {
  return getCurrentUser()
}

export function updateUserInfo(data: {
  userName?: string
  email?: string
  sex?: number
  imageUrl?: string
}): Promise<ApiResponse<boolean>> {
  return api.put<boolean>('/user/profile', data).then((res) => res.data)
}

export function changePassword(data: {
  oldPassword: string
  newPassword: string
}): Promise<ApiResponse<boolean>> {
  return api.post<boolean>('/user/change-password', data).then((res) => res.data)
}

export function getUserManagementList(params: {
  keyword?: string
  status?: number
  pageIndex?: number
  pageSize?: number
}): Promise<ApiResponse<UserManagementItem[]>> {
  return api.get<UserManagementItem[]>('/user/manage/list', { params }).then((res) => res.data)
}

export function updateUserStatus(userId: number, isEnabled: boolean): Promise<ApiResponse<boolean>> {
  return api.put<boolean>(`/user/manage/${userId}/status`, { isEnabled }).then((res) => res.data)
}

export function resetUserPassword(userId: number, newPassword?: string): Promise<ApiResponse<boolean>> {
  return api.post<boolean>(`/user/manage/${userId}/reset-password`, { newPassword }).then((res) => res.data)
}
