import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/stores/user'

// 响应接口
export interface ApiResponse<T = any> {
  code: number
  isSuccess: boolean
  message: string
  data: T
  totalCount?: number
}

// 创建 axios 实例
const request: AxiosInstance = axios.create({
  baseURL: '/cloudapi',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// 请求拦截器
request.interceptors.request.use(
  (config) => {
    const userStore = useUserStore()
    const token = userStore.token

    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    return config
  },
  (error) => {
    console.error('请求错误:', error)
    return Promise.reject(error)
  }
)

// 响应拦截器
request.interceptors.response.use(
  (response: AxiosResponse<ApiResponse>) => {
    const res = response.data

    // 如果响应不成功
    if (!res.isSuccess) {
      ElMessage.error(res.message || '请求失败')

      // 401: 未授权，跳转到登录页
      if (res.code === 401) {
        const userStore = useUserStore()
        userStore.logout()
        window.location.href = '/login'
      }

      return Promise.reject(new Error(res.message || '请求失败'))
    }

    return response
  },
  (error) => {
    console.error('响应错误:', error)

    if (error.response) {
      const { status, data } = error.response

      switch (status) {
        case 400:
          ElMessage.error(data?.message || '请求参数错误')
          break
        case 401:
          ElMessage.error('未授权，请登录')
          const userStore = useUserStore()
          userStore.logout()
          window.location.href = '/login'
          break
        case 403:
          ElMessage.error('拒绝访问')
          break
        case 404:
          ElMessage.error('请求资源不存在')
          break
        case 500:
          ElMessage.error('服务器内部错误')
          break
        default:
          ElMessage.error(data?.message || '请求失败')
      }
    } else if (error.request) {
      ElMessage.error('网络错误，请检查网络连接')
    } else {
      ElMessage.error(error.message || '请求失败')
    }

    return Promise.reject(error)
  }
)

// 导出请求方法
export const api = {
  get<T = any>(url: string, params?: any): Promise<AxiosResponse<ApiResponse<T>>> {
    return request.get(url, { params })
  },

  post<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<ApiResponse<T>>> {
    return request.post(url, data, config)
  },

  put<T = any>(url: string, data?: any): Promise<AxiosResponse<ApiResponse<T>>> {
    return request.put(url, data)
  },

  delete<T = any>(url: string): Promise<AxiosResponse<ApiResponse<T>>> {
    return request.delete(url)
  },

  upload<T = any>(url: string, formData: FormData): Promise<AxiosResponse<ApiResponse<T>>> {
    return request.post(url, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
  },

  download(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<Blob>> {
    return request.get(url, {
      ...config,
      responseType: 'blob',
    })
  },
}

export default request
