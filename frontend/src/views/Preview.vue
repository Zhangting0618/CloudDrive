<template>
  <div class="preview-container">
    <header class="preview-header">
      <el-button @click="goBack">
        <el-icon><ArrowLeft /></el-icon>
        返回
      </el-button>
      <span class="file-name">{{ fileName }}</span>
      <el-button @click="handleDownload">
        <el-icon><Download /></el-icon>
        下载
      </el-button>
    </header>

    <div class="preview-content">
      <!-- 图片预览 -->
      <div v-if="isImage" class="image-preview">
        <el-image
          :src="previewUrl"
          fit="contain"
          :preview-src-list="[previewUrl]"
        />
      </div>

      <!-- PDF 预览 -->
      <div v-else-if="isPdf" class="pdf-preview">
        <iframe :src="previewUrl" width="100%" height="100%" />
      </div>

      <!-- 视频预览 -->
      <div v-else-if="isVideo" class="video-preview">
        <video :src="previewUrl" controls autoplay />
      </div>

      <!-- 文本预览 -->
      <div v-else-if="isText" class="text-preview">
        <pre>{{ textContent }}</pre>
      </div>

      <!-- 不支持预览 -->
      <div v-else class="not-supported">
        <el-empty description="该文件类型不支持在线预览">
          <el-button type="primary" @click="handleDownload">下载文件</el-button>
        </el-empty>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { ArrowLeft, Download } from '@element-plus/icons-vue'
import { getFilePreviewUrl, downloadFile } from '@/api/file'
import { saveAs } from 'file-saver'

const router = useRouter()
const route = useRoute()

const fileName = ref('')
const previewUrl = ref('')
const textContent = ref('')
const fileType = ref('')

const fileId = computed(() => route.params.fileId as string)

const isImage = computed(() => {
  return /\.(jpg|jpeg|png|gif|bmp|svg)$/i.test(fileName.value)
})

const isPdf = computed(() => {
  return /\.pdf$/i.test(fileName.value)
})

const isVideo = computed(() => {
  return /\.(mp4|webm|ogg|mov)$/i.test(fileName.value)
})

const isText = computed(() => {
  return /\.(txt|md|json|xml|csv)$/i.test(fileName.value)
})

const goBack = () => {
  router.back()
}

const handleDownload = async () => {
  try {
    const blob = await downloadFile(parseInt(fileId.value))
    saveAs(blob, fileName.value)
    ElMessage.success('下载成功')
  } catch (error: any) {
    ElMessage.error(error.message || '下载失败')
  }
}

// 加载文件预览
onMounted(async () => {
  previewUrl.value = getFilePreviewUrl(parseInt(fileId.value))

  // 如果是文本文件，获取内容
  if (isText.value) {
    try {
      const response = await fetch(previewUrl.value)
      textContent.value = await response.text()
    } catch (error) {
      console.error('加载文本内容失败:', error)
    }
  }
})
</script>

<style scoped lang="scss">
.preview-container {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background: #fff;
}

.preview-header {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  border-bottom: 1px solid #e4e7ed;

  .file-name {
    font-size: 16px;
    font-weight: 500;
  }
}

.preview-content {
  flex: 1;
  overflow: hidden;
  background: #f5f7fa;

  .image-preview,
  .pdf-preview,
  .video-preview {
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .image-preview {
    :deep(.el-image) {
      max-width: 100%;
      max-height: 100%;
    }
  }

  .text-preview {
    height: 100%;
    overflow: auto;
    padding: 20px;

    pre {
      background: #fff;
      padding: 20px;
      border-radius: 8px;
      font-family: 'Courier New', Courier, monospace;
      font-size: 14px;
      line-height: 1.6;
      white-space: pre-wrap;
      word-wrap: break-word;
    }
  }

  .not-supported {
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}
</style>
