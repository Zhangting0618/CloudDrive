import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { getCurrentUser, login as loginApi, logout as logoutApi } from '@/api/user'

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

interface JwtPayload {
  UserId?: string
  UserName?: string
  Phone?: string
  Email?: string
  ImageUrl?: string
  UserType?: string
  nameid?: string
  unique_name?: string
  [key: string]: unknown
}

export const useUserStore = defineStore('user', () => {
  const token = ref<string>(localStorage.getItem('token') || '')
  const userInfo = ref<UserInfo | null>(null)

  const isLoggedIn = computed(() => !!token.value)
  const userName = computed(() => userInfo.value?.userName || '')
  const avatar = computed(() => userInfo.value?.imageUrl || '')

  function loadUserFromToken(rawToken: string) {
    const payload = parseJwt(rawToken)
    if (!payload) {
      return
    }

    userInfo.value = {
      id: Number(payload.UserId || payload.nameid || 0),
      userName: String(payload.UserName || payload.unique_name || ''),
      phone: String(payload.Phone || ''),
      email: payload.Email ? String(payload.Email) : undefined,
      imageUrl: payload.ImageUrl ? String(payload.ImageUrl) : undefined,
      userType: payload.UserType ? Number(payload.UserType) : undefined,
      isAdmin: payload.UserType ? Number(payload.UserType) === 0 : undefined,
    }
  }

  async function loginAction(phone: string, password: string) {
    try {
      const res = await loginApi({ phone, password })

      if (!res.isSuccess || !res.data) {
        return { success: false, message: res.message }
      }

      token.value = res.data
      localStorage.setItem('token', res.data)
      loadUserFromToken(res.data)
      await refreshCurrentUser()

      return { success: true }
    } catch (error: any) {
      return { success: false, message: error.message }
    }
  }

  async function logoutAction() {
    try {
      if (token.value) {
        await logoutApi()
      }
    } catch (error) {
      console.error('Logout failed:', error)
    } finally {
      clearAuth()
    }
  }

  function clearAuth() {
    token.value = ''
    userInfo.value = null
    localStorage.removeItem('token')
  }

  function setUserInfo(info: UserInfo) {
    userInfo.value = info
  }

  async function refreshCurrentUser() {
    if (!token.value) return

    try {
      const res = await getCurrentUser()
      if (res.isSuccess && res.data) {
        userInfo.value = res.data
      }
    } catch (error) {
      console.error('Load current user failed:', error)
    }
  }

  function parseJwt(rawToken: string): JwtPayload | null {
    try {
      const base64Url = rawToken.split('.')[1]
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      )

      return JSON.parse(jsonPayload) as JwtPayload
    } catch (error) {
      console.error('Parse JWT failed:', error)
      return null
    }
  }

  if (token.value && !userInfo.value) {
    loadUserFromToken(token.value)
  }

  return {
    token,
    userInfo,
    isLoggedIn,
    userName,
    avatar,
    loginAction,
    logoutAction,
    clearAuth,
    setUserInfo,
    refreshCurrentUser,
    parseJwt,
  }
})
