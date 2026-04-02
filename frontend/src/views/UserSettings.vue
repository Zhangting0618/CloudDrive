<template>
  <div class="user-settings-container">
    <div class="settings-card">
      <el-tabs v-model="activeTab">
        <!-- 基本信息 -->
        <el-tab-pane label="基本信息" name="profile">
          <el-form
            ref="profileFormRef"
            :model="profileForm"
            :rules="profileRules"
            label-width="100px"
            class="profile-form"
          >
            <el-form-item label="头像">
              <div class="avatar-upload">
                <el-avatar :size="100" :src="profileForm.imageUrl || defaultAvatar" />
                <div class="avatar-actions">
                  <el-button size="small" @click="handleUpdateAvatar">
                    {{ profileForm.imageUrl ? '更换头像' : '上传头像' }}
                  </el-button>
                  <el-input
                    v-model="profileForm.imageUrl"
                    placeholder="请输入头像 URL"
                    size="small"
                    style="width: 250px; margin-left: 10px;"
                  />
                </div>
              </div>
            </el-form-item>

            <el-form-item label="用户名" prop="userName">
              <el-input v-model="profileForm.userName" placeholder="请输入用户名" />
            </el-form-item>

            <el-form-item label="手机号">
              <el-input :value="userStore.userInfo?.phone" disabled />
            </el-form-item>

            <el-form-item label="邮箱" prop="email">
              <el-input v-model="profileForm.email" placeholder="请输入邮箱" />
            </el-form-item>

            <el-form-item label="性别">
              <el-radio-group v-model="profileForm.sex">
                <el-radio :label="0">男</el-radio>
                <el-radio :label="1">女</el-radio>
              </el-radio-group>
            </el-form-item>

            <el-form-item>
              <el-button type="primary" @click="handleUpdateProfile" :loading="updatingProfile">
                保存修改
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <!-- 修改密码 -->
        <el-tab-pane label="修改密码" name="password">
          <el-form
            ref="passwordFormRef"
            :model="passwordForm"
            :rules="passwordRules"
            label-width="100px"
            class="password-form"
          >
            <el-form-item label="原密码" prop="oldPassword">
              <el-input
                v-model="passwordForm.oldPassword"
                type="password"
                placeholder="请输入原密码"
                show-password
              />
            </el-form-item>

            <el-form-item label="新密码" prop="newPassword">
              <el-input
                v-model="passwordForm.newPassword"
                type="password"
                placeholder="请输入新密码"
                show-password
              />
            </el-form-item>

            <el-form-item label="确认密码" prop="confirmPassword">
              <el-input
                v-model="passwordForm.confirmPassword"
                type="password"
                placeholder="请确认新密码"
                show-password
              />
            </el-form-item>

            <el-form-item>
              <el-button type="primary" @click="handleChangePassword" :loading="updatingPassword">
                修改密码
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>
      </el-tabs>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, FormInstance, FormRules } from 'element-plus'
import { useUserStore } from '@/stores/user'
import { updateUserInfo, changePassword } from '@/api/user'

const userStore = useUserStore()

const activeTab = ref('profile')
const updatingProfile = ref(false)
const updatingPassword = ref(false)

const defaultAvatar = 'https://cube.elemecdn.com/0/88/03b0d39583f48206768a7534e55bcpng.png'

// 基本信息表单
const profileFormRef = ref<FormInstance>()
const profileForm = reactive({
  userName: '',
  email: '',
  sex: 0,
  imageUrl: '',
})

const profileRules: FormRules = {
  userName: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 2, max: 20, message: '用户名长度在 2 到 20 个字符', trigger: 'blur' },
  ],
  email: [
    { type: 'email', message: '请输入正确的邮箱格式', trigger: 'blur' },
  ],
}

// 修改密码表单
const passwordFormRef = ref<FormInstance>()
const passwordForm = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const passwordRules: FormRules = {
  oldPassword: [
    { required: true, message: '请输入原密码', trigger: 'blur' },
  ],
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    { min: 6, max: 20, message: '密码长度在 6 到 20 个字符', trigger: 'blur' },
  ],
  confirmPassword: [
    { required: true, message: '请确认新密码', trigger: 'blur' },
    {
      validator: (rule, value, callback) => {
        if (value !== passwordForm.newPassword) {
          callback(new Error('两次输入的密码不一致'))
        } else {
          callback()
        }
      },
      trigger: 'blur',
    },
  ],
}

// 加载用户信息
const loadUserInfo = () => {
  const info = userStore.userInfo
  if (info) {
    profileForm.userName = info.userName
    profileForm.email = info.email || ''
    profileForm.sex = info.sex ?? 0
    profileForm.imageUrl = info.imageUrl || ''
  }
}

// 更新头像
const handleUpdateAvatar = () => {
  ElMessage.info('请输入头像 URL 或使用默认头像')
}

// 更新基本信息
const handleUpdateProfile = async () => {
  if (!profileFormRef.value) return

  await profileFormRef.value.validate(async (valid) => {
    if (!valid) return

    updatingProfile.value = true
    try {
      const res = await updateUserInfo({
        userName: profileForm.userName,
        email: profileForm.email || undefined,
        sex: profileForm.sex,
        imageUrl: profileForm.imageUrl || undefined,
      })

      if (res.isSuccess) {
        ElMessage.success('更新成功')
        // 更新本地用户信息
        userStore.setUserInfo({
          ...userStore.userInfo,
          userName: profileForm.userName,
          email: profileForm.email,
          sex: profileForm.sex,
          imageUrl: profileForm.imageUrl,
        } as any)
      }
    } catch (error: any) {
      ElMessage.error(error.message || '更新失败')
    } finally {
      updatingProfile.value = false
    }
  })
}

// 修改密码
const handleChangePassword = async () => {
  if (!passwordFormRef.value) return

  await passwordFormRef.value.validate(async (valid) => {
    if (!valid) return

    updatingPassword.value = true
    try {
      const res = await changePassword({
        oldPassword: passwordForm.oldPassword,
        newPassword: passwordForm.newPassword,
      })

      if (res.isSuccess) {
        ElMessage.success('密码修改成功，请重新登录')
        // 清空密码表单
        passwordForm.oldPassword = ''
        passwordForm.newPassword = ''
        passwordForm.confirmPassword = ''
        // 退出登录
        setTimeout(() => {
          userStore.logoutAction()
          window.location.href = '/login'
        }, 1500)
      }
    } catch (error: any) {
      ElMessage.error(error.message || '修改失败')
    } finally {
      updatingPassword.value = false
    }
  })
}

onMounted(() => {
  loadUserInfo()
})
</script>

<style scoped lang="scss">
.user-settings-container {
  height: 100%;
  display: flex;
  justify-content: center;
  padding: 40px 20px;
  background: #f5f7fa;
}

.settings-card {
  width: 100%;
  max-width: 700px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
  padding: 30px;

  .profile-form,
  .password-form {
    max-width: 500px;
    margin-top: 20px;
  }

  .avatar-upload {
    display: flex;
    align-items: center;
    gap: 20px;

    .avatar-actions {
      display: flex;
      align-items: center;
    }
  }
}
</style>
