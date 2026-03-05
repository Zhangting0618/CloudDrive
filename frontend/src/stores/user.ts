import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as loginApi, logout as logoutApi } from '@/api/user'

export interface UserInfo {
  id: number
  userName: string
  phone: string
  email?: string
  imageUrl?: string
}

export const useUserStore = defineStore('user', () => {
  // 状态
  const token = ref<string>(localStorage.getItem('token') || '')
  const userInfo = ref<UserInfo | null>(null)

  // 计算属性
  const isLoggedIn = computed(() => !!token.value)
  const userName = computed(() => userInfo.value?.userName || '')
  const avatar = computed(() => userInfo.value?.imageUrl || '')

  // 方法
  /**
   * 登录
   */
  async function loginAction(phone: string, password: string) {
    try {
      const res = await loginApi({ phone, password })

      if (res.isSuccess && res.data) {
        token.value = res.data
        localStorage.setItem('token', res.data)

        // 解析 JWT 获取用户信息（可选）
        const userInfo = parseJwt(res.data)
        if (userInfo) {
          userInfo.value = {
            id: parseInt(userInfo.UserId),
            userName: userInfo.UserName,
            phone: userInfo.Phone,
            email: userInfo.Email,
            imageUrl: userInfo.ImageUrl,
          }
        }

        return { success: true }
      }

      return { success: false, message: res.message }
    } catch (error: any) {
      return { success: false, message: error.message }
    }
  }

  /**
   * 登出
   */
  async function logoutAction() {
    try {
      await logoutApi()
    } catch (error) {
      console.error('登出失败:', error)
    } finally {
      token.value = ''
      userInfo.value = null
      localStorage.removeItem('token')
    }
  }

  /**
   * 设置用户信息
   */
  function setUserInfo(info: UserInfo) {
    userInfo.value = info
  }

  /**
   * 解析 JWT
   */
  function parseJwt(token: string): any {
    try {
      const base64Url = token.split('.')[1]
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      )
      return JSON.parse(jsonPayload)
    } catch (error) {
      console.error('解析 JWT 失败:', error)
      return null
    }
  }

  return {
    // 状态
    token,
    userInfo,
    // 计算属性
    isLoggedIn,
    userName,
    avatar,
    // 方法
    loginAction,
    logoutAction,
    setUserInfo,
    parseJwt,
  }
})
