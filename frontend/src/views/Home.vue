<template>
  <div class="home-container">
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
        <router-link to="/recycle" class="nav-item" active-class="active">
          <el-icon><Delete /></el-icon>
          <span>回收站</span>
        </router-link>
        <router-link to="/collection" class="nav-item" active-class="active">
          <el-icon><Star /></el-icon>
          <span>我的收藏</span>
        </router-link>
        <router-link to="/share" class="nav-item" active-class="active">
          <el-icon><Link /></el-icon>
          <span>我的分享</span>
        </router-link>
        <router-link to="/stats" class="nav-item" active-class="active">
          <el-icon><PieChart /></el-icon>
          <span>存储统计</span>
        </router-link>
        <router-link to="/logs" class="nav-item" active-class="active">
          <el-icon><DocumentCopy /></el-icon>
          <span>操作日志</span>
        </router-link>
        <router-link v-if="userStore.userInfo?.isAdmin" to="/users" class="nav-item" active-class="active">
          <el-icon><User /></el-icon>
          <span>用户管理</span>
        </router-link>
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
              <el-dropdown-item command="settings">个人设置</el-dropdown-item>
              <el-dropdown-item command="logout" divided>退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </aside>

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
  Link,
  PieChart,
  DocumentCopy,
  User,
} from '@element-plus/icons-vue'
import { onMounted } from 'vue'

const router = useRouter()
const userStore = useUserStore()

const handleCommand = async (command: string) => {
  if (command === 'settings') {
    router.push('/settings')
    return
  }

  if (command === 'logout') {
    await userStore.logoutAction()
    ElMessage.success('已退出登录')
    router.push('/login')
  }
}

onMounted(() => {
  userStore.refreshCurrentUser()
})
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
  overflow: auto;
  background: #f5f7fa;
}
</style>
