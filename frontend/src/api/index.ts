import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/stores/user'

export interface ApiResponse<T = any> {
  code: number
  isSuccess: boolean
  message: string
  data: T
  totalCount?: number
}

const request: AxiosInstance = axios.create({
  baseURL: '/cloudapi',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

request.interceptors.request.use(
  (config) => {
    const userStore = useUserStore()
    const token = userStore.token

    config.headers = config.headers ?? {}
    config.headers.Source = '1'

    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    return config
  },
  (error) => Promise.reject(error)
)

request.interceptors.response.use(
  (response: AxiosResponse<ApiResponse>) => {
    const res = response.data

    if (!res.isSuccess) {
      if (res.code === 401) {
        const userStore = useUserStore()
        userStore.clearAuth()
        window.location.href = '/login'
      } else {
        ElMessage.error(res.message || '请求失败')
      }

      return Promise.reject(new Error(res.message || '请求失败'))
    }

    return response
  },
  (error) => {
    if (error.response?.status === 401) {
      const userStore = useUserStore()
      userStore.clearAuth()
      ElMessage.error('登录已失效，请重新登录')
      window.location.href = '/login'
    } else if (error.response?.data?.message) {
      ElMessage.error(error.response.data.message)
    } else if (error.request) {
      ElMessage.error('网络错误，请检查连接')
    } else {
      ElMessage.error(error.message || '请求失败')
    }

    return Promise.reject(error)
  }
)

export const api = {
  get<T = any>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<ApiResponse<T>>> {
    return request.get(url, config)
  },

  post<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<ApiResponse<T>>> {
    return request.post(url, data, config)
  },

  put<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<ApiResponse<T>>> {
    return request.put(url, data, config)
  },

  delete<T = any>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<ApiResponse<T>>> {
    return request.delete(url, config)
  },

  upload<T = any>(url: string, formData: FormData, config?: AxiosRequestConfig): Promise<AxiosResponse<ApiResponse<T>>> {
    return request.post(url, formData, {
      ...config,
      headers: {
        ...(config?.headers ?? {}),
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
