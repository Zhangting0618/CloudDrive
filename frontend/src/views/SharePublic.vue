<template>
  <div class="share-public-container">
    <div class="share-card">
      <!-- 加载状态 -->
      <div v-if="loading" class="loading-state">
        <el-skeleton :rows="4" animated />
      </div>

      <!-- 分享信息 -->
      <div v-else-if="shareInfo" class="share-content">
        <div class="share-header">
          <el-icon :size="48" color="#667eea">
            <FolderOpened />
          </el-icon>
          <h2 class="file-name">{{ shareInfo.fileName }}</h2>
          <p class="sharer">
            <el-icon><User /></el-icon>
            {{ shareInfo.userName }} 分享
          </p>
        </div>

        <!-- 已过期或无效 -->
        <div v-if="!shareInfo.isValid || shareInfo.isExpired" class="expired-tip">
          <el-result icon="error" title="分享已失效" sub-title="该分享链接已过期或被取消">
            <template #extra>
              <el-button type="primary" @click="goHome">返回首页</el-button>
            </template>
          </el-result>
        </div>

        <!-- 需要密码 -->
        <div v-else-if="shareInfo.hasPassword && !verified" class="password-form">
          <el-card>
            <p class="card-title">请输入访问密码</p>
            <el-input
              v-model="password"
              type="password"
              placeholder="请输入访问密码"
              show-password
              @keyup.enter="verifyPassword"
            />
            <el-button type="primary" class="mt-3" @click="verifyPassword" :loading="verifying">
              确定
            </el-button>
          </el-card>
        </div>

        <!-- 文件列表 -->
        <div v-else class="file-list">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>文件内容</span>
                <el-button type="primary" size="small" @click="handleDownload">
                  <el-icon><Download /></el-icon>
                  下载
                </el-button>
              </div>
            </template>
            <div class="file-item">
              <el-icon :size="24" color="#667eea">
                <component :is="shareInfo.isFolder ? Folder : Document" />
              </el-icon>
              <span class="file-name-text">{{ shareInfo.fileName }}</span>
              <span class="file-size">{{ formatFileSize(shareInfo.fileSize) }}</span>
            </div>
          </el-card>

          <div class="share-actions">
            <el-button @click="goHome">返回首页</el-button>
          </div>
        </div>
      </div>

      <!-- 分享不存在 -->
      <div v-else class="not-found">
        <el-result icon="error" title="分享不存在" sub-title="该分享链接无效或已被删除">
          <template #extra>
            <el-button type="primary" @click="goHome">返回首页</el-button>
          </template>
        </el-result>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import {
  FolderOpened,
  Folder,
  Document,
  User,
  Download,
} from '@element-plus/icons-vue'
import { getShareInfo, type ShareInfo } from '@/api/share'

const route = useRoute()
const router = useRouter()

const loading = ref(true)
const shareInfo = ref<ShareInfo | null>(null)
const password = ref('')
const verifying = ref(false)
const verified = ref(false)

// 加载分享信息
const loadShareInfo = async () => {
  loading.value = true
  try {
    const shareCode = route.params.shareCode as string
    const res = await getShareInfo(shareCode)
    if (res.isSuccess && res.data) {
      shareInfo.value = res.data
    }
  } catch (error: any) {
    ElMessage.error(error.message || '加载分享信息失败')
  } finally {
    loading.value = false
  }
}

// 验证密码
const verifyPassword = async () => {
  if (!password.value.trim()) {
    ElMessage.warning('请输入密码')
    return
  }

  verifying.value = true
  try {
    // TODO: 实现密码验证 API
    ElMessage.success('验证通过')
    verified.value = true
  } catch (error: any) {
    ElMessage.error(error.message || '密码错误')
  } finally {
    verifying.value = false
  }
}

// 下载文件
const handleDownload = () => {
  if (shareInfo.value) {
    // TODO: 实现分享文件下载
    ElMessage.info('下载功能开发中')
  }
}

// 返回首页
const goHome = () => {
  router.push('/')
}

// 格式化文件大小
const formatFileSize = (bytes: number): string => {
  if (!bytes || bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

onMounted(() => {
  loadShareInfo()
})
</script>

<style scoped lang="scss">
.share-public-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
}

.share-card {
  width: 100%;
  max-width: 500px;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  padding: 30px;
}

.share-header {
  text-align: center;
  padding-bottom: 20px;
  border-bottom: 1px solid #e4e7ed;
  margin-bottom: 20px;

  .file-name {
    font-size: 20px;
    font-weight: bold;
    color: #303133;
    margin: 15px 0 10px;
  }

  .sharer {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 5px;
    color: #909399;
    font-size: 14px;
  }
}

.expired-tip {
  padding: 20px 0;
}

.password-form {
  .card-title {
    font-size: 16px;
    font-weight: bold;
    color: #303133;
    margin-bottom: 15px;
    text-align: center;
  }

  .mt-3 {
    width: 100%;
    margin-top: 15px;
  }
}

.file-list {
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .file-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 10px 0;

    .file-name-text {
      flex: 1;
      font-size: 14px;
      color: #303133;
    }

    .file-size {
      font-size: 12px;
      color: #909399;
    }
  }

  .share-actions {
    margin-top: 20px;
    text-align: center;
  }
}

.not-found {
  padding: 20px 0;
}
</style>
