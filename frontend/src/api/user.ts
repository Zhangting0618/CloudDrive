import { api, ApiResponse } from './index'

// 登录请求参数
export interface LoginParams {
  phone: string
  password: string
}

// 注册请求参数
export interface RegisterParams {
  userName: string
  phone: string
  password: string
  email: string
  sex?: number
}

// 用户信息
export interface UserInfo {
  id: number
  userName: string
  phone: string
  email?: string
  imageUrl?: string
}

/**
 * 用户登录
 */
export function login(data: LoginParams): Promise<ApiResponse<string>> {
  return api.post('/user/login', data).then(res => res.data)
}

/**
 * 用户注册
 */
export function register(data: RegisterParams): Promise<ApiResponse<boolean>> {
  return api.post('/user/register', data).then(res => res.data)
}

/**
 * 用户登出
 */
export function logout(): Promise<ApiResponse<boolean>> {
  return api.post('/user/logout')
}

/**
 * 获取当前用户信息
 */
export function getCurrentUser(): Promise<ApiResponse<UserInfo>> {
  return api.get('/user/current')
}

/**
 * 获取当前用户信息
 */
export function getCurrentUserInfo(): Promise<ApiResponse<UserInfo>> {
  return api.get('/user/current')
}

/**
 * 更新用户信息
 */
export function updateUserInfo(data: {
  userName?: string
  email?: string
  sex?: number
  imageUrl?: string
}): Promise<ApiResponse<boolean>> {
  return api.put('/user/profile', data)
}

/**
 * 修改密码
 */
export function changePassword(data: {
  oldPassword: string
  newPassword: string
}): Promise<ApiResponse<boolean>> {
  return api.post('/user/change-password', data)
}
