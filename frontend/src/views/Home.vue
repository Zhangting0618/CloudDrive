<template>
  <div class="home-container">
    <!-- 侧边栏 -->
    <aside class="sidebar">
      <div class="logo">
        <el-icon :size="24"><FolderOpened /></el-icon>
        <span>Ptcent Cloud Drive</span>
      </div>

      <nav class="nav-menu">
        <router-link to="/files" class="nav-item" active-class="active">
          <el-icon><Folder /></el-icon>
          <span>全部文件</span>
        </router-link>
        <div class="nav-item">
          <el-icon><Star /></el-icon>
          <span>我的收藏</span>
        </div>
        <div class="nav-item">
          <el-icon><Delete /></el-icon>
          <span>回收站</span>
        </div>
      </nav>

      <div class="user-info">
        <el-dropdown @command="handleCommand">
          <div class="user-avatar">
            <el-avatar :size="36" :icon="UserFilled" />
            <span class="user-name">{{ userStore.userName || '用户' }}</span>
            <el-icon><ArrowDown /></el-icon>
          </div>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="logout">退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </aside>

    <!-- 主内容区 -->
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/stores/user'
import {
  FolderOpened,
  Folder,
  Star,
  Delete,
  UserFilled,
  ArrowDown,
} from '@element-plus/icons-vue'

const router = useRouter()
const userStore = useUserStore()

const handleCommand = (command: string) => {
  if (command === 'logout') {
    userStore.logoutAction()
    ElMessage.success('已退出登录')
    router.push('/login')
  }
}
</script>

<style scoped lang="scss">
.home-container {
  display: flex;
  height: 100vh;
}

.sidebar {
  width: 240px;
  background: #fff;
  border-right: 1px solid #e4e7ed;
  display: flex;
  flex-direction: column;

  .logo {
    height: 60px;
    display: flex;
    align-items: center;
    padding: 0 20px;
    gap: 10px;
    font-size: 18px;
    font-weight: 600;
    color: #667eea;
    border-bottom: 1px solid #e4e7ed;
  }

  .nav-menu {
    flex: 1;
    padding: 20px 0;

    .nav-item {
      display: flex;
      align-items: center;
      padding: 12px 20px;
      color: #606266;
      text-decoration: none;
      gap: 10px;
      cursor: pointer;
      transition: all 0.3s;

      &:hover {
        background: #f5f7fa;
        color: #667eea;
      }

      &.active {
        background: #f0f2ff;
        color: #667eea;
        border-right: 3px solid #667eea;
      }
    }
  }

  .user-info {
    padding: 20px;
    border-top: 1px solid #e4e7ed;

    .user-avatar {
      display: flex;
      align-items: center;
      gap: 10px;
      cursor: pointer;

      .user-name {
        flex: 1;
        font-size: 14px;
      }
    }
  }
}

.main-content {
  flex: 1;
  overflow: hidden;
  background: #f5f7fa;
}
</style>
