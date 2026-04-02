<template>
  <div class="share-container">
    <!-- 顶部工具栏 -->
    <header class="share-header">
      <div class="page-title">
        <el-icon :size="20"><Link /></el-icon>
        <span>我的分享</span>
      </div>
    </header>

    <!-- 分享列表 -->
    <div class="file-list">
      <el-table
        :data="shares"
        v-loading="loading"
        row-key="shareId"
      >
        <el-table-column prop="fileName" label="文件名" min-width="250">
          <template #default="{ row }">
            <div class="file-name">
              <el-icon :size="24">
                <component :is="row.isFolder ? Folder : Document" />
              </el-icon>
              <span>{{ row.fileName }}</span>
              <el-tag v-if="row.isFolder" size="small">文件夹</el-tag>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="shareCode" label="分享码" width="120">
          <template #default="{ row }">
            <el-tag>{{ row.shareCode }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="expireTime" label="过期时间" width="160">
          <template #default="{ row }">
            <span v-if="row.expireTime">{{ formatDate(row.expireTime) }}</span>
            <span v-else class="text-muted">永久有效</span>
          </template>
        </el-table-column>
        <el-table-column prop="visitCount" label="访问次数" width="100" align="center">
          <template #default="{ row }">
            {{ row.visitCount }}
          </template>
        </el-table-column>
        <el-table-column prop="hasPassword" label="密码" width="80" align="center">
          <template #default="{ row }">
            <el-icon v-if="row.hasPassword" color="#67c23a"><Lock /></el-icon>
            <span v-else class="text-muted">无</span>
          </template>
        </el-table-column>
        <el-table-column prop="isValid" label="状态" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isValid ? 'success' : 'info'" size="small">
              {{ row.isValid ? '有效' : '无效' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleCopyLink(row)">
              复制链接
            </el-button>
            <el-button link type="danger" @click="handleCancelShare(row)">
              取消分享
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 空状态 -->
      <el-empty v-if="!loading && shares.length === 0" description="暂无分享" />

      <!-- 分页 -->
      <div class="pagination" v-if="!loading && shares.length > 0">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="total"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="loadShares"
          @current-change="loadShares"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Link,
  Folder,
  Document,
  Lock,
} from '@element-plus/icons-vue'
import {
  getMyShares,
  cancelShare,
  type MyShareItem,
  formatDate,
} from '@/api/share'

const loading = ref(false)
const shares = ref<MyShareItem[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

// 加载分享列表
const loadShares = async () => {
  loading.value = true
  try {
    const res = await getMyShares(currentPage.value, pageSize.value)
    shares.value = res.data?.data || []
    total.value = res.data?.totalCount || 0
  } catch (error: any) {
    ElMessage.error(error.message || '加载分享失败')
  } finally {
    loading.value = false
  }
}

// 复制链接
const handleCopyLink = (row: MyShareItem) => {
  const url = `${window.location.origin}${row.shareUrl}`
  navigator.clipboard.writeText(url).then(() => {
    ElMessage.success('链接已复制到剪贴板')
  }).catch(() => {
    ElMessage.error('复制失败')
  })
}

// 取消分享
const handleCancelShare = (row: MyShareItem) => {
  ElMessageBox.confirm('确定要取消该分享吗？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(async () => {
    try {
      const res = await cancelShare(row.shareId)
      if (res.isSuccess) {
        ElMessage.success('取消分享成功')
        loadShares()
      }
    } catch (error: any) {
      ElMessage.error(error.message || '取消分享失败')
    }
  })
}

onMounted(() => {
  loadShares()
})
</script>

<style scoped lang="scss">
.share-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: #fff;
  margin: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.share-header {
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
  }

  .pagination {
    margin-top: 20px;
    display: flex;
    justify-content: flex-end;
  }
}

.text-muted {
  color: #909399;
}
</style>
