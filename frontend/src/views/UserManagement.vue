<template>
  <div class="user-management-page">
    <div class="page-header">
      <div>
        <h2>用户管理</h2>
        <p>管理员可查看用户列表、禁用账号并重置密码。</p>
      </div>
    </div>

    <el-card shadow="never" class="filter-card">
      <div class="filters">
        <el-input
          v-model="keyword"
          placeholder="搜索用户名、手机号、邮箱"
          clearable
          @keyup.enter="handleSearch"
        />
        <el-select v-model="status" clearable placeholder="状态">
          <el-option label="正常" :value="1" />
          <el-option label="已禁用" :value="2" />
        </el-select>
        <el-button type="primary" @click="handleSearch">查询</el-button>
        <el-button @click="handleReset">重置</el-button>
      </div>
    </el-card>

    <el-card shadow="never">
      <el-table :data="users" v-loading="loading" border>
        <el-table-column prop="userName" label="用户名" min-width="140" />
        <el-table-column prop="phone" label="手机号" min-width="140" />
        <el-table-column prop="email" label="邮箱" min-width="180" show-overflow-tooltip />
        <el-table-column prop="registerTime" label="注册时间" min-width="170" />
        <el-table-column label="角色" width="100">
          <template #default="{ row }">
            <el-tag :type="row.isAdmin ? 'danger' : 'info'">{{ row.isAdmin ? '管理员' : '用户' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'warning'">{{ row.isEnabled ? '正常' : '已禁用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleResetPassword(row)">重置密码</el-button>
            <el-button
              link
              :type="row.isEnabled ? 'danger' : 'success'"
              @click="handleToggleStatus(row)"
            >
              {{ row.isEnabled ? '禁用' : '启用' }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="table-footer">
        <el-pagination
          background
          layout="total, prev, pager, next, sizes"
          :total="total"
          :page-size="pageSize"
          :current-page="pageIndex"
          :page-sizes="[10, 20, 50]"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getUserManagementList,
  resetUserPassword,
  updateUserStatus,
  type UserManagementItem,
} from '@/api/user'

const loading = ref(false)
const users = ref<UserManagementItem[]>([])
const total = ref(0)
const keyword = ref('')
const status = ref<number | undefined>(undefined)
const pageIndex = ref(1)
const pageSize = ref(20)

const loadUsers = async () => {
  loading.value = true
  try {
    const res = await getUserManagementList({
      keyword: keyword.value || undefined,
      status: status.value,
      pageIndex: pageIndex.value,
      pageSize: pageSize.value,
    })
    users.value = res.data || []
    total.value = res.totalCount || 0
  } catch (error: any) {
    ElMessage.error(error.message || '加载用户列表失败')
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  pageIndex.value = 1
  loadUsers()
}

const handleReset = () => {
  keyword.value = ''
  status.value = undefined
  pageIndex.value = 1
  loadUsers()
}

const handlePageChange = (page: number) => {
  pageIndex.value = page
  loadUsers()
}

const handleSizeChange = (size: number) => {
  pageSize.value = size
  pageIndex.value = 1
  loadUsers()
}

const handleToggleStatus = async (row: UserManagementItem) => {
  const nextEnabled = !row.isEnabled
  const actionText = nextEnabled ? '启用' : '禁用'

  try {
    await ElMessageBox.confirm(`确定要${actionText}用户“${row.userName}”吗？`, '提示', {
      type: 'warning',
    })

    const res = await updateUserStatus(row.id, nextEnabled)
    ElMessage.success(res.message || `${actionText}成功`)
    loadUsers()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error(error.message || `${actionText}失败`)
    }
  }
}

const handleResetPassword = async (row: UserManagementItem) => {
  try {
    const { value } = await ElMessageBox.prompt(
      `为用户“${row.userName}”设置新密码；留空则重置为 123456`,
      '重置密码',
      {
        inputPlaceholder: '请输入新密码',
        confirmButtonText: '确定',
        cancelButtonText: '取消',
      }
    )

    const res = await resetUserPassword(row.id, value || undefined)
    ElMessage.success(res.message || '密码重置成功')
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error(error.message || '密码重置失败')
    }
  }
}

onMounted(() => {
  loadUsers()
})
</script>

<style scoped lang="scss">
.user-management-page {
  padding: 20px;

  .page-header {
    margin-bottom: 16px;

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

  .filter-card {
    margin-bottom: 16px;
  }

  .filters {
    display: grid;
    grid-template-columns: minmax(220px, 1fr) 140px auto auto;
    gap: 12px;
    align-items: center;
  }

  .table-footer {
    display: flex;
    justify-content: flex-end;
    margin-top: 16px;
  }
}
</style>
