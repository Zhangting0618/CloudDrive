import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { FileItem } from '@/api/file'

export const useFileStore = defineStore('file', () => {
  // 状态
  const currentFolderId = ref<number | undefined>(undefined)
  const currentPath = ref<FileItem[]>([])
  const selectedFiles = ref<Set<number>>(new Set())
  const isSelectMode = ref(false)

  // 计算属性
  const selectedFileIds = computed(() => Array.from(selectedFiles.value))
  const hasSelection = computed(() => selectedFiles.value.size > 0)

  // 方法
  /**
   * 设置当前文件夹
   */
  function setCurrentFolder(folderId?: number) {
    currentFolderId.value = folderId
  }

  /**
   * 更新路径导航
   */
  function updatePath(folder: FileItem) {
    // 查找是否已存在该路径
    const index = currentPath.value.findIndex(f => f.id === folder.id)

    if (index >= 0) {
      // 如果已存在，截取到该位置
      currentPath.value = currentPath.value.slice(0, index + 1)
    } else {
      // 否则添加到路径
      currentPath.value.push(folder)
    }
  }

  /**
   * 清除路径
   */
  function clearPath() {
    currentPath.value = []
  }

  /**
   * 切换选择模式
   */
  function toggleSelectMode() {
    isSelectMode.value = !isSelectMode.value
    if (!isSelectMode.value) {
      selectedFiles.value.clear()
    }
  }

  /**
   * 选择文件
   */
  function selectFile(fileId: number) {
    selectedFiles.value.add(fileId)
  }

  /**
   * 取消选择文件
   */
  function deselectFile(fileId: number) {
    selectedFiles.value.delete(fileId)
  }

  /**
   * 切换文件选择状态
   */
  function toggleSelectFile(fileId: number) {
    if (selectedFiles.value.has(fileId)) {
      selectedFiles.value.delete(fileId)
    } else {
      selectedFiles.value.add(fileId)
    }
  }

  /**
   * 清空选择
   */
  function clearSelection() {
    selectedFiles.value.clear()
  }

  /**
   * 全选
   */
  function selectAll(files: FileItem[]) {
    files.forEach(f => selectedFiles.value.add(f.id))
  }

  return {
    // 状态
    currentFolderId,
    currentPath,
    selectedFiles,
    isSelectMode,
    // 计算属性
    selectedFileIds,
    hasSelection,
    // 方法
    setCurrentFolder,
    updatePath,
    clearPath,
    toggleSelectMode,
    selectFile,
    deselectFile,
    toggleSelectFile,
    clearSelection,
    selectAll,
  }
})
