<template>
  <div class="collection-container">
    <!-- 顶部工具栏 -->
    <header class="collection-header">
      <div class="page-title">
        <el-icon :size="20"><Star /></el-icon>
        <span>我的收藏</span>
      </div>
    </header>

    <!-- 收藏列表 -->
    <div class="file-list">
      <el-table
        :data="collections"
        v-loading="loading"
        row-key="collectionId"
      >
        <el-table-column prop="fileName" label="文件名" min-width="300">
          <template #default="{ row }">
            <div class="file-name" @click="handleGoToFile(row)">
              <el-icon :size="24" :color="getFileIconColor(row)">
                <component :is="getFileIcon(row)" />
              </el-icon>
              <span>{{ row.fileName }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="fileSize" label="大小" width="120" align="right">
          <template #default="{ row }">
            <span v-if="!row.isFolder">{{ formatFileSize(row.fileSize) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="collectionTime" label="收藏时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.collectionTime) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handlePreview(row)">
              预览
            </el-button>
            <el-button link type="danger" @click="handleRemove(row)">
              取消收藏
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 空状态 -->
      <el-empty v-if="!loading && collections.length === 0" description="暂无收藏" />

      <!-- 分页 -->
      <div class="pagination" v-if="!loading && collections.length > 0">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="total"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="loadCollections"
          @current-change="loadCollections"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Star,
  Folder,
  Picture,
  Document,
  VideoCamera,
  Files,
} from '@element-plus/icons-vue'
import {
  getCollections,
  removeFromCollection,
  type CollectionItem,
  formatFileSize,
  formatDate,
} from '@/api/collection'

const router = useRouter()

const loading = ref(false)
const collections = ref<CollectionItem[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

// 加载收藏列表
const loadCollections = async () => {
  loading.value = true
  try {
    const res = await getCollections(currentPage.value, pageSize.value)
    collections.value = res.data?.data || []
    total.value = res.data?.totalCount || 0
  } catch (error: any) {
    ElMessage.error(error.message || '加载收藏失败')
  } finally {
    loading.value = false
  }
}

// 跳转到文件
const handleGoToFile = (row: CollectionItem) => {
  if (row.isFolder) {
    router.push(`/files?folderId=${row.fileId}`)
  } else {
    router.push(`/preview/${row.fileId}`)
  }
}

// 预览
const handlePreview = (row: CollectionItem) => {
  if (row.isFolder) {
    router.push(`/files?folderId=${row.fileId}`)
  } else {
    router.push(`/preview/${row.fileId}`)
  }
}

// 取消收藏
const handleRemove = (row: CollectionItem) => {
  ElMessageBox.confirm('确定要取消收藏该文件吗？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(async () => {
    try {
      const res = await removeFromCollection(row.fileId)
      if (res.isSuccess) {
        ElMessage.success('取消收藏成功')
        loadCollections()
      }
    } catch (error: any) {
      ElMessage.error(error.message || '取消收藏失败')
    }
  })
}

// 获取文件图标
const getFileIcon = (row: CollectionItem) => {
  if (row.isFolder) return Folder

  const ext = row.extension?.toLowerCase()
  if (['.jpg', '.jpeg', '.png', '.gif', '.bmp'].includes(ext)) return Picture
  if (['.pdf', '.doc', '.docx', '.txt'].includes(ext)) return Document
  if (['.xls', '.xlsx'].includes(ext)) return Document
  if (['.ppt', '.pptx'].includes(ext)) return Document
  if (['.mp4', '.avi', '.mov'].includes(ext)) return VideoCamera
  if (['.zip', '.rar', '.7z'].includes(ext)) return Files

  return Document
}

// 获取文件图标颜色
const getFileIconColor = (row: CollectionItem) => {
  if (row.isFolder) return '#f5a623'

  const ext = row.extension?.toLowerCase()
  if (['.jpg', '.jpeg', '.png', '.gif'].includes(ext)) return '#20c997'
  if (['.pdf'].includes(ext)) return '#fa5252'
  if (['.doc', '.docx'].includes(ext)) return '#4c6ef5'
  if (['.xls', '.xlsx'].includes(ext)) return '#2f9e44'
  if (['.ppt', '.pptx'].includes(ext)) return '#f08c00'
  if (['.mp4', '.avi', '.mov'].includes(ext)) return '#845ef7'

  return '#868e96'
}

onMounted(() => {
  loadCollections()
})
</script>

<style scoped lang="scss">
.collection-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: #fff;
  margin: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.collection-header {
  padding: 20px;
  border-bottom: 1px solid #e4e7ed;
  display: flex;
  justify-content: space-between;
  align-items: center;

  .page-title {
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 18px;
    font-weight: bold;
    color: #303133;
  }
}

.file-list {
  flex: 1;
  overflow: auto;
  padding: 20px;

  .file-name {
    display: flex;
    align-items: center;
    gap: 10px;
    cursor: pointer;

    &:hover {
      color: #667eea;
    }
  }

  .pagination {
    margin-top: 20px;
    display: flex;
    justify-content: flex-end;
  }
}
</style>
