<template>
  <div class="operation-logs-page">
    <div class="page-header">
      <div>
        <h2>操作日志</h2>
        <p>查看当前账号的关键操作记录和执行结果。</p>
      </div>
    </div>

    <el-card class="filter-card" shadow="never">
      <div class="filters">
        <el-input
          v-model="keyword"
          placeholder="搜索路径、操作名或错误信息"
          clearable
          @keyup.enter="handleSearch"
        />
        <el-date-picker
          v-model="dateRange"
          type="datetimerange"
          range-separator="至"
          start-placeholder="开始时间"
          end-placeholder="结束时间"
          value-format="YYYY-MM-DD HH:mm:ss"
        />
        <el-button type="primary" @click="handleSearch">查询</el-button>
        <el-button @click="handleReset">重置</el-button>
      </div>
    </el-card>

    <el-card shadow="never">
      <el-table :data="logs" v-loading="loading" border>
        <el-table-column prop="createdTime" label="时间" width="180" />
        <el-table-column prop="method" label="方法" width="90">
          <template #default="{ row }">
            <el-tag :type="getMethodType(row.method)" effect="plain">{{ row.method }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="path" label="路径" min-width="220" show-overflow-tooltip />
        <el-table-column prop="actionName" label="操作" min-width="180" show-overflow-tooltip />
        <el-table-column prop="statusCode" label="状态码" width="100" />
        <el-table-column prop="durationMs" label="耗时" width="100">
          <template #default="{ row }">{{ row.durationMs }} ms</template>
        </el-table-column>
        <el-table-column label="结果" width="100">
          <template #default="{ row }">
            <el-tag :type="row.isSuccess ? 'success' : 'danger'">
              {{ row.isSuccess ? '成功' : '失败' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="errorMessage" label="错误信息" min-width="220" show-overflow-tooltip />
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
import { ElMessage } from 'element-plus'
import { getOperationLogs, type OperationLogItem } from '@/api/system'

const loading = ref(false)
const logs = ref<OperationLogItem[]>([])
const total = ref(0)
const keyword = ref('')
const dateRange = ref<[string, string] | []>([])
const pageIndex = ref(1)
const pageSize = ref(20)

const loadLogs = async () => {
  loading.value = true
  try {
    const res = await getOperationLogs({
      keyword: keyword.value || undefined,
      startTime: dateRange.value[0],
      endTime: dateRange.value[1],
      pageIndex: pageIndex.value,
      pageSize: pageSize.value,
    })

    logs.value = res.data || []
    total.value = res.totalCount || 0
  } catch (error: any) {
    ElMessage.error(error.message || '加载操作日志失败')
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  pageIndex.value = 1
  loadLogs()
}

const handleReset = () => {
  keyword.value = ''
  dateRange.value = []
  pageIndex.value = 1
  loadLogs()
}

const handlePageChange = (page: number) => {
  pageIndex.value = page
  loadLogs()
}

const handleSizeChange = (size: number) => {
  pageSize.value = size
  pageIndex.value = 1
  loadLogs()
}

const getMethodType = (method: string) => {
  if (method === 'POST') return 'success'
  if (method === 'PUT') return 'warning'
  if (method === 'DELETE') return 'danger'
  return 'info'
}

onMounted(() => {
  loadLogs()
})
</script>

<style scoped lang="scss">
.operation-logs-page {
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
    grid-template-columns: minmax(220px, 1fr) minmax(320px, 1.4fr) auto auto;
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
