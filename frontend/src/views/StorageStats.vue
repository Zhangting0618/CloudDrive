<template>
  <div class="storage-stats-page">
    <div class="page-header">
      <div>
        <h2>存储统计</h2>
        <p>查看当前用户与系统整体的空间占用情况。</p>
      </div>
      <el-button type="warning" :loading="cleaning" @click="handleCleanup">
        清理过期回收站
      </el-button>
    </div>

    <div class="stats-grid" v-loading="loading">
      <el-card shadow="hover" class="stats-card accent-blue">
        <template #header>我的空间</template>
        <div class="stat-value">{{ formatFileSize(stats.userUsedBytes) }}</div>
        <div class="stat-meta">文件 {{ stats.userFileCount }} 个，文件夹 {{ stats.userFolderCount }} 个</div>
      </el-card>

      <el-card shadow="hover" class="stats-card accent-amber">
        <template #header>我的回收站</template>
        <div class="stat-value">{{ formatFileSize(stats.userRecycleBytes) }}</div>
        <div class="stat-meta">已删除内容占用空间</div>
      </el-card>

      <el-card shadow="hover" class="stats-card accent-emerald">
        <template #header>系统总空间</template>
        <div class="stat-value">{{ formatFileSize(stats.systemUsedBytes) }}</div>
        <div class="stat-meta">文件 {{ stats.systemFileCount }} 个，文件夹 {{ stats.systemFolderCount }} 个</div>
      </el-card>

      <el-card shadow="hover" class="stats-card accent-rose">
        <template #header>系统回收站</template>
        <div class="stat-value">{{ formatFileSize(stats.systemRecycleBytes) }}</div>
        <div class="stat-meta">全站待清理占用空间</div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { cleanupExpiredRecycleBin, getStorageStats } from '@/api/system'
import { formatFileSize } from '@/api/file'

const loading = ref(false)
const cleaning = ref(false)
const stats = reactive({
  userUsedBytes: 0,
  userRecycleBytes: 0,
  userFileCount: 0,
  userFolderCount: 0,
  systemUsedBytes: 0,
  systemRecycleBytes: 0,
  systemFileCount: 0,
  systemFolderCount: 0,
})

const loadStats = async () => {
  loading.value = true
  try {
    const res = await getStorageStats()
    Object.assign(stats, res.data)
  } catch (error: any) {
    ElMessage.error(error.message || '加载存储统计失败')
  } finally {
    loading.value = false
  }
}

const handleCleanup = async () => {
  cleaning.value = true
  try {
    const res = await cleanupExpiredRecycleBin()
    ElMessage.success(res.message || '清理完成')
    await loadStats()
  } catch (error: any) {
    ElMessage.error(error.message || '清理失败')
  } finally {
    cleaning.value = false
  }
}

onMounted(() => {
  loadStats()
})
</script>

<style scoped lang="scss">
.storage-stats-page {
  padding: 20px;

  .page-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;

    h2 {
      font-size: 22px;
      color: #303133;
      margin-bottom: 6px;
    }

    p {
      color: #909399;
      font-size: 14px;
    }
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(260px, 1fr));
    gap: 16px;
  }

  .stats-card {
    border-radius: 16px;

    :deep(.el-card__header) {
      font-weight: 600;
    }

    .stat-value {
      font-size: 28px;
      font-weight: 700;
      margin-bottom: 10px;
      color: #1f2937;
    }

    .stat-meta {
      color: #6b7280;
      font-size: 14px;
    }
  }

  .accent-blue {
    background: linear-gradient(135deg, #eef4ff 0%, #f8fbff 100%);
  }

  .accent-amber {
    background: linear-gradient(135deg, #fff6e8 0%, #fffaf3 100%);
  }

  .accent-emerald {
    background: linear-gradient(135deg, #ecfdf5 0%, #f7fffb 100%);
  }

  .accent-rose {
    background: linear-gradient(135deg, #fff1f2 0%, #fff8f8 100%);
  }
}
</style>
