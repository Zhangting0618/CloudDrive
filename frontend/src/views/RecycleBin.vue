<template>
  <div class="recycle-bin-container">
    <!-- 顶部工具栏 -->
    <header class="recycle-bin-header">
      <div class="page-title">
        <el-icon :size="20"><Delete /></el-icon>
        <span>回收站</span>
      </div>

      <div class="toolbar">
        <el-button
          v-if="hasSelection"
          type="primary"
          @click="handleBatchRestore"
        >
          <el-icon><Refresh /></el-icon>
          还原 ({{ selectedFileIds.length }})
        </el-button>
        <el-button
          v-if="hasSelection"
          type="danger"
          @click="handleBatchDeletePermanently"
        >
          <el-icon><Delete /></el-icon>
          彻底删除
        </el-button>
        <el-button
          type="danger"
          plain
          @click="handleClearRecycleBin"
        >
          <el-icon><Delete /></el-icon>
          清空回收站
        </el-button>
      </div>
    </header>

    <!-- 回收站列表 -->
    <div class="file-list">
      <el-table
        :data="files"
        v-loading="loading"
        @selection-change="handleSelectionChange"
        row-key="id"
      >
        <el-table-column type="selection" width="50" />
        <el-table-column prop="name" label="文件名" min-width="300">
          <template #default="{ row }">
            <div class="file-name">
              <el-icon :size="24" :color="getFileIconColor(row)">
                <component :is="getFileIcon(row)" />
              </el-icon>
              <span>{{ row.name }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="size" label="大小" width="120" align="right">
          <template #default="{ row }">
            <span v-if="!row.isFolder">{{ row.fileSizeStr || formatFileSize(row.size) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="deletedDate" label="删除时间" width="180">
          <template #default="{ row }">
            {{ row.deletedDate ? formatDate(row.deletedDate) : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleRestore(row)">
              还原
            </el-button>
            <el-button link type="danger" @click="handleDeletePermanently(row)">
              彻底删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 空状态 -->
      <el-empty v-if="!loading && files.length === 0" description="回收站为空" />

      <!-- 分页 -->
      <div class="pagination" v-if="!loading && files.length > 0">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="total"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="loadFiles"
          @current-change="loadFiles"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Delete,
  Refresh,
  Folder,
  Picture,
  Document,
  VideoCamera,
  Files,
} from '@element-plus/icons-vue'
import {
  getRecycleBinList,
  restoreFile,
  deleteFilePermanently,
  clearRecycleBin,
  type RecycleBinItem,
  formatFileSize,
  formatDate,
} from '@/api/recycle'

const loading = ref(false)
const files = ref<RecycleBinItem[]>([])
const selectedFileIds = ref<number[]>([])
const hasSelection = ref(false)
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

// 加载回收站列表
const loadFiles = async () => {
  loading.value = true
  try {
    const res = await getRecycleBinList(currentPage.value, pageSize.value)
    files.value = res.data?.data || []
    total.value = res.data?.totalCount || 0
  } catch (error: any) {
    ElMessage.error(error.message || '加载回收站失败')
  } finally {
    loading.value = false
  }
}

// 处理选择变化
const handleSelectionChange = (selection: any[]) => {
  selectedFileIds.value = selection.map(item => item.id)
  hasSelection.value = selection.length > 0
}

// 还原单个文件
const handleRestore = (row: RecycleBinItem) => {
  ElMessageBox.confirm('确定要还原该文件吗？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(async () => {
    try {
      const res = await restoreFile(row.id)
      if (res.isSuccess) {
        ElMessage.success('还原成功')
        loadFiles()
      }
    } catch (error: any) {
      ElMessage.error(error.message || '还原失败')
    }
  })
}

// 彻底删除单个文件
const handleDeletePermanently = (row: RecycleBinItem) => {
  ElMessageBox.confirm('确定要彻底删除该文件吗？此操作不可恢复！', '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'error',
  }).then(async () => {
    try {
      const res = await deleteFilePermanently(row.id)
      if (res.isSuccess) {
        ElMessage.success('彻底删除成功')
        loadFiles()
      }
    } catch (error: any) {
      ElMessage.error(error.message || '删除失败')
    }
  })
}

// 批量还原
const handleBatchRestore = async () => {
  ElMessageBox.confirm(`确定要还原选中的 ${selectedFileIds.value.length} 个文件吗？`, '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(async () => {
    try {
      const promises = selectedFileIds.value.map(id => restoreFile(id))
      await Promise.all(promises)
      ElMessage.success('还原成功')
      selectedFileIds.value = []
      hasSelection.value = false
      loadFiles()
    } catch (error: any) {
      ElMessage.error(error.message || '还原失败')
    }
  })
}

// 批量彻底删除
const handleBatchDeletePermanently = async () => {
  ElMessageBox.confirm(`确定要彻底删除选中的 ${selectedFileIds.value.length} 个文件吗？此操作不可恢复！`, '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'error',
  }).then(async () => {
    try {
      const promises = selectedFileIds.value.map(id => deleteFilePermanently(id))
      await Promise.all(promises)
      ElMessage.success('彻底删除成功')
      selectedFileIds.value = []
      hasSelection.value = false
      loadFiles()
    } catch (error: any) {
      ElMessage.error(error.message || '删除失败')
    }
  })
}

// 清空回收站
const handleClearRecycleBin = () => {
  ElMessageBox.confirm('确定要清空回收站吗？所有文件将永久删除，此操作不可恢复！', '严重警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'error',
  }).then(async () => {
    try {
      const res = await clearRecycleBin()
      if (res.isSuccess) {
        ElMessage.success('清空回收站成功')
        loadFiles()
      }
    } catch (error: any) {
      ElMessage.error(error.message || '清空失败')
    }
  })
}

// 获取文件图标
const getFileIcon = (row: RecycleBinItem) => {
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
const getFileIconColor = (row: RecycleBinItem) => {
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
  loadFiles()
})
</script>

<style scoped lang="scss">
.recycle-bin-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: #fff;
  margin: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.recycle-bin-header {
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

  .toolbar {
    display: flex;
    gap: 10px;
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
  }

  .pagination {
    margin-top: 20px;
    display: flex;
    justify-content: flex-end;
  }
}
</style>
