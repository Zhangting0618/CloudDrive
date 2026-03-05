<template>
  <div class="files-container">
    <!-- 顶部工具栏 -->
    <header class="files-header">
      <div class="breadcrumb">
        <el-icon @click="navigateToFolder(undefined)" style="cursor: pointer"><HomeFilled /></el-icon>
        <template v-for="(folder, index) in fileStore.currentPath" :key="folder.id">
          <el-icon><ArrowRight /></el-icon>
          <span @click="navigateToFolder(folder.id)" style="cursor: pointer">{{ folder.name }}</span>
        </template>
      </div>

      <div class="toolbar">
        <el-button type="primary" @click="showUploadDialog = true">
          <el-icon><Upload /></el-icon>
          上传
        </el-button>
        <el-button @click="handleCreateFolder">
          <el-icon><FolderAdd /></el-icon>
          新建文件夹
        </el-button>
        <el-button
          v-if="fileStore.hasSelection"
          type="danger"
          @click="handleBatchDelete"
        >
          <el-icon><Delete /></el-icon>
          删除 ({{ fileStore.selectedFileIds.length }})
        </el-button>
        <el-button
          v-if="fileStore.hasSelection"
          @click="handleBatchDownload"
        >
          <el-icon><Download /></el-icon>
          下载
        </el-button>
      </div>
    </header>

    <!-- 文件列表 -->
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
            <div class="file-name" @dblclick="handleFileClick(row)">
              <el-icon :size="24" :color="getFileIconColor(row)">
                <component :is="getFileIcon(row)" />
              </el-icon>
              <span>{{ row.name }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="size" label="大小" width="120" align="right">
          <template #default="{ row }">
            <span v-if="!row.isFolder">{{ formatFileSize(row.size) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="createdDate" label="修改时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.createdDate) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleDownload(row)">
              下载
            </el-button>
            <el-button link type="primary" @click="handleRename(row)">
              重命名
            </el-button>
            <el-button link type="danger" @click="handleDelete(row)">
              删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 空状态 -->
      <el-empty v-if="!loading && files.length === 0" description="暂无文件" />
    </div>

    <!-- 上传对话框 -->
    <el-dialog
      v-model="showUploadDialog"
      title="上传文件"
      width="500px"
    >
      <el-upload
        ref="uploadRef"
        drag
        :auto-upload="true"
        :http-request="customUpload"
        multiple
      >
        <el-icon class="el-icon--upload"><upload-filled /></el-icon>
        <div class="el-upload__text">
          拖拽文件到此处或<em>点击上传</em>
        </div>
      </el-upload>
    </el-dialog>

    <!-- 新建文件夹对话框 -->
    <el-dialog
      v-model="showFolderDialog"
      title="新建文件夹"
      width="400px"
    >
      <el-input
        v-model="newFolderName"
        placeholder="请输入文件夹名称"
        @keyup.enter="confirmCreateFolder"
      />
      <template #footer>
        <el-button @click="showFolderDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmCreateFolder">确定</el-button>
      </template>
    </el-dialog>

    <!-- 重命名对话框 -->
    <el-dialog
      v-model="showRenameDialog"
      title="重命名"
      width="400px"
    >
      <el-input
        v-model="renameValue"
        placeholder="请输入新名称"
        @keyup.enter="confirmRename"
      />
      <template #footer>
        <el-button @click="showRenameDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmRename">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  HomeFilled,
  ArrowRight,
  Upload,
  FolderAdd,
  Delete,
  Download,
  Folder,
  Picture,
  Document,
  Office,
  VideoCamera,
  Files,
  UploadFilled,
} from '@element-plus/icons-vue'
import { useFileStore } from '@/stores/file'
import {
  getFileList,
  uploadFile,
  createFolder,
  deleteFile,
  renameFile,
  downloadFile,
  batchDownload,
  type FileItem,
  formatFileSize,
  formatDate,
} from '@/api/file'
import { saveAs } from 'file-saver'

const fileStore = useFileStore()

const loading = ref(false)
const files = ref<FileItem[]>([])
const showUploadDialog = ref(false)
const showFolderDialog = ref(false)
const showRenameDialog = ref(false)
const newFolderName = ref('')
const renameValue = ref('')
const currentRenameFile = ref<FileItem | null>(null)

// 加载文件列表
const loadFiles = async () => {
  loading.value = true
  try {
    const res = await getFileList({
      parentFolderId: fileStore.currentFolderId,
    })
    files.value = res.data || []
  } catch (error: any) {
    ElMessage.error(error.message || '加载文件列表失败')
  } finally {
    loading.value = false
  }
}

// 导航到文件夹
const navigateToFolder = (folderId?: number) => {
  fileStore.setCurrentFolder(folderId)
  if (folderId === undefined) {
    fileStore.clearPath()
  }
  loadFiles()
}

// 处理文件点击
const handleFileClick = (row: FileItem) => {
  if (row.isFolder) {
    fileStore.updatePath(row)
    fileStore.setCurrentFolder(row.id)
    loadFiles()
  }
}

// 处理选择变化
const handleSelectionChange = (selection: any[]) => {
  fileStore.clearSelection()
  selection.forEach(item => fileStore.selectFile(item.id))
}

// 上传文件
const customUpload = async (options: any) => {
  try {
    const res = await uploadFile(
      options.file,
      fileStore.currentFolderId,
      '/'
    )

    if (res.isSuccess) {
      ElMessage.success('上传成功')
      loadFiles()
    }
  } catch (error: any) {
    ElMessage.error(error.message || '上传失败')
  }
}

// 新建文件夹
const handleCreateFolder = () => {
  newFolderName.value = ''
  showFolderDialog.value = true
}

const confirmCreateFolder = async () => {
  if (!newFolderName.value.trim()) {
    ElMessage.warning('请输入文件夹名称')
    return
  }

  try {
    const res = await createFolder(
      newFolderName.value,
      fileStore.currentFolderId,
      '/'
    )

    if (res.isSuccess) {
      ElMessage.success('创建成功')
      showFolderDialog.value = false
      loadFiles()
    }
  } catch (error: any) {
    ElMessage.error(error.message || '创建失败')
  }
}

// 重命名
const handleRename = (row: FileItem) => {
  currentRenameFile.value = row
  renameValue.value = row.name
  showRenameDialog.value = true
}

const confirmRename = async () => {
  if (!renameValue.value.trim()) {
    ElMessage.warning('请输入文件名称')
    return
  }

  if (!currentRenameFile.value) return

  try {
    const res = await renameFile(currentRenameFile.value.id, renameValue.value)

    if (res.isSuccess) {
      ElMessage.success('重命名成功')
      showRenameDialog.value = false
      loadFiles()
    }
  } catch (error: any) {
    ElMessage.error(error.message || '重命名失败')
  }
}

// 删除
const handleDelete = (row: FileItem) => {
  ElMessageBox.confirm('确定要删除该文件吗？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(async () => {
    try {
      const res = await deleteFile(row.id)

      if (res.isSuccess) {
        ElMessage.success('删除成功')
        loadFiles()
      }
    } catch (error: any) {
      ElMessage.error(error.message || '删除失败')
    }
  })
}

// 批量删除
const handleBatchDelete = () => {
  ElMessageBox.confirm(`确定要删除选中的 ${fileStore.selectedFileIds.length} 个文件吗？`, '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(async () => {
    // TODO: 实现批量删除
    ElMessage.success('删除成功')
    loadFiles()
  })
}

// 下载
const handleDownload = async (row: FileItem) => {
  if (row.isFolder) {
    ElMessage.warning('文件夹暂不支持直接下载')
    return
  }

  try {
    const blob = await downloadFile(row.id)
    saveAs(blob, row.name)
    ElMessage.success('下载成功')
  } catch (error: any) {
    ElMessage.error(error.message || '下载失败')
  }
}

// 批量下载
const handleBatchDownload = async () => {
  try {
    const blob = await batchDownload(fileStore.selectedFileIds)
    saveAs(blob, `download_${new Date().getTime()}.zip`)
    ElMessage.success('打包下载成功')
    fileStore.clearSelection()
  } catch (error: any) {
    ElMessage.error(error.message || '下载失败')
  }
}

// 获取文件图标
const getFileIcon = (row: FileItem) => {
  if (row.isFolder) return Folder

  const ext = row.extension?.toLowerCase()
  if (['.jpg', '.jpeg', '.png', '.gif', '.bmp'].includes(ext)) return Picture
  if (['.pdf', '.doc', '.docx', '.txt'].includes(ext)) return Document
  if (['.xls', '.xlsx'].includes(ext)) return Office
  if (['.ppt', '.pptx'].includes(ext)) return Office
  if (['.mp4', '.avi', '.mov'].includes(ext)) return VideoCamera
  if (['.zip', '.rar', '.7z'].includes(ext)) return Files

  return Document
}

// 获取文件图标颜色
const getFileIconColor = (row: FileItem) => {
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
.files-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: #fff;
  margin: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.files-header {
  padding: 20px;
  border-bottom: 1px solid #e4e7ed;
  display: flex;
  justify-content: space-between;
  align-items: center;

  .breadcrumb {
    display: flex;
    align-items: center;
    gap: 5px;
    font-size: 14px;
    color: #606266;

    span:hover {
      color: #667eea;
    }
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
    cursor: pointer;

    &:hover {
      color: #667eea;
    }
  }
}
</style>
