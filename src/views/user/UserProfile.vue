<template>
    <main id="user-profile">
        <aside class="profile-sidebar" aria-label="用户资料导航">
            <button v-for="item in sections" :key="item.key" type="button" class="sidebar-item"
                :class="{ 'is-active': activeSection === item.key }" @click="selectSection(item.key)">
                {{ item.label }}
            </button>
        </aside>

        <section class="profile-content">
            <header class="profile-summary">
                <div class="summary-avatar">
                    <img v-if="profile.avatar" :src="getSource(profile.avatar,'Images')" :alt="`${profile.nickname}的头像`" />
                    <span v-else>{{ profile.nickname.slice(0, 1) || '?' }}</span>
                </div>
                <div class="summary-detail">
                    <h1>{{ profile.nickname || '未设置昵称' }}</h1>
                    <span class="summary-id">用户 ID：{{ profile.userId || '-' }}</span>
                </div>
            </header>

            <section v-if="activeSection === 'personal'" class="detail-section">
                <div class="section-heading">
                    <h2>基本信息</h2>
                    <el-button v-if="!isEditing" text type="primary" @click="startEditing">编辑资料</el-button>
                </div>

                <el-form v-if="isEditing" ref="editFormRef" class="profile-form" :model="profileEditForm"
                    :rules="editRules" label-width="100px" @submit.prevent="saveProfile">
                    <el-form-item label="头像">
                        <div class="avatar-editor">
                            <div class="avatar-preview">
                                <el-image v-if="avatarPreview" :src="avatarPreview" 
                                 :preview-src-list="[avatarPreview]"></el-image>
                                <span v-else>{{ profileEditForm.nickname?.slice(0, 1) || '?' }}</span>
                            </div>
                            <div>
                                <el-upload :auto-upload="false" :show-file-list="false" accept="image/*"
                                    :on-change="handleAvatarChange">
                                    <el-button size="small">选择图片</el-button>
                                </el-upload>
                                <p class="avatar-hint">支持图片格式，大小不超过 2MB</p>
                            </div>
                        </div>
                    </el-form-item>
                    <el-form-item label="用户昵称" prop="nickname">
                        <el-input v-model="profileEditForm.nickname" maxlength="20" show-word-limit
                            placeholder="请输入昵称" />
                    </el-form-item>
                    <el-form-item label="性别" prop="gender">
                        <el-radio-group v-model="profileEditForm.gender">
                            <el-radio v-for="option in genderOptions" :key="option.value" :value="option.value">
                                {{ option.label }}
                            </el-radio>
                        </el-radio-group>
                    </el-form-item>
                    <el-form-item>
                        <el-button @click="cancelEditing">取消</el-button>
                        <el-button type="primary" :loading="saving" @click="saveProfile">保存</el-button>
                    </el-form-item>
                </el-form>

                <dl v-else class="info-list">
                    <div class="info-row">
                        <dt>用户昵称</dt>
                        <dd>{{ profile.nickname || '未设置' }}</dd>
                    </div>
                    <div class="info-row">
                        <dt>用户 ID</dt>
                        <dd>{{ profile.userId || '-' }}</dd>
                    </div>
                    <div class="info-row">
                        <dt>性别</dt>
                        <dd>{{ genderLabel(profile.gender) }}</dd>
                    </div>
                    <div class="info-row">
                        <dt>加入时间</dt>
                        <dd>{{ formatDate(profile.joinTime) }}</dd>
                    </div>
                </dl>
            </section>

            <section v-else-if="activeSection === 'account'" class="detail-section">
                <div class="section-heading">
                    <h2>账号设置</h2>
                    <el-button v-if="!isEditing" text type="primary" @click="startEditing">编辑邮箱</el-button>
                </div>
                <el-form v-if="isEditing" ref="editFormRef" class="profile-form" :model="profileEditForm"
                    :rules="editRules" label-width="100px" @submit.prevent="saveProfile">
                    <el-form-item label="用户 ID">
                        <span class="read-only-value">{{ profile.userId || '-' }}</span>
                    </el-form-item>
                    <el-form-item label="邮箱" prop="email">
                        <el-input v-model="profileEditForm.email" maxlength="50" placeholder="请输入邮箱" />
                    </el-form-item>
                    <el-form-item>
                        <el-button @click="cancelEditing">取消</el-button>
                        <el-button type="primary" :loading="saving" @click="saveProfile">保存</el-button>
                    </el-form-item>
                </el-form>
                <dl v-else class="info-list">
                    <div class="info-row">
                        <dt>用户 ID</dt>
                        <dd>{{ profile.userId || '-' }}</dd>
                    </div>
                    <div class="info-row">
                        <dt>绑定邮箱</dt>
                        <dd>{{ profile.email || '未绑定' }}</dd>
                    </div>
                </dl>
            </section>

            <section v-else class="detail-section">
                <div class="section-heading">
                    <h2>浏览记录</h2>
                </div>
                <el-empty description="暂无浏览记录" />
            </section>
        </section>
    </main>
</template>

<script lang="ts" setup>
import { ChangeAvatar, GetUserprofile, type User, type UserProfile, type UserProfileDTO } from '../../api/user.ts';
import { getSource } from '../../modules/Request.ts';
import stateStorage from '../../modules/StateStorage.ts';
import { ElMessage, ElNotification, type FormInstance, type FormRules, type UploadFile } from 'element-plus';
import { onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

type ProfileSection = 'personal' | 'account' | 'history';

const router = useRouter();
const sections: { key: ProfileSection; label: string }[] = [
    { key: 'personal', label: '个人资料' },
    { key: 'account', label: '账号设置' },
    { key: 'history', label: '浏览记录' },
];
const activeSection = ref<ProfileSection>('personal');
const isEditing = ref(false);
const editFormRef = ref<FormInstance>();
const saving = ref(false);
const profile = reactive<UserProfile>({
    userId: 0,
    nickname: '',
    email: '',
    avatar: '',
    gender: 0,
    joinTime: new Date(),
    creationCount: 0,
    followingCount: 0,
    fansCount: 0,
});
const profileEditForm = ref<UserProfileDTO>({
    userId: 0,
    nickname: '',
    email: '',
    avatar: '',
    gender: 0,
});
const avatarPreview = ref('');
let avatarObjectUrl = '';
let user: User | undefined;

const editRules: FormRules = {
    nickname: [
        { required: true, message: '请输入昵称', trigger: 'blur' },
        { min: 1, max: 100, message: '昵称长度需在 1 到 100 个字符之间', trigger: 'blur' },
    ],
    email: [
        { required: true, message: '请输入邮箱', trigger: 'blur' },
        { type: 'email', message: '邮箱格式不正确', trigger: ['blur', 'change'] },
    ],
};

const genderOptions = [
    { value: 0, label: '男' },
    { value: 1, label: '女' },
];

onMounted(() => {
    user = stateStorage.get<User>('User');
    if (!user?.userId) {
        ElMessage.error('请先登录');
        router.replace('/login');
        return;
    }

    Object.assign(profile, {
        userId: user.userId,
        nickname: user.nickname,
        email: user.email,
        avatar: user.avatar,
        gender: user.gender,
    });
    syncFormFromProfile();

    GetUserprofile(user.userId)
        .then((res) => {
            if (!res.success) {
                ElMessage.error(res.message);
                return;
            }
            Object.assign(profile, res.data);
            avatarPreview.value = getSource(profile.avatar,"Images");
            syncFormFromProfile();
        })
        .catch((error) => ElNotification.error(error));
});

onBeforeUnmount(() => {
    clearAvatarPreview();
});

function selectSection(section: ProfileSection) {
    if (activeSection.value === section) return;
    cancelEditing();
    activeSection.value = section;
}

function startEditing() {
    syncFormFromProfile();
    isEditing.value = true;
}

function cancelEditing() {
    isEditing.value = false;
    syncFormFromProfile();
}

function syncFormFromProfile() {
    Object.assign(profileEditForm.value, {
        userId: profile.userId,
        nickname: profile.nickname,
        email: profile.email,
        avatar: profile.avatar,
        gender: profile.gender,
        avatarFile: undefined,
    });
    clearAvatarPreview();
    avatarPreview.value = getSource(profile.avatar,"Images");
}

function clearAvatarPreview() {
    if (avatarObjectUrl) {
        URL.revokeObjectURL(avatarObjectUrl);
        avatarObjectUrl = '';
    }
}

function handleAvatarChange(file: UploadFile) {
    const raw = file.raw;
    if (!raw) return;
    if (!raw.type.startsWith('image/')) {
        ElMessage.warning('请选择图片文件');
        return;
    }
    if (raw.size > 2 * 1024 * 1024) {
        ElMessage.warning('图片大小不能超过 2MB');
        return;
    }

    clearAvatarPreview();
    avatarObjectUrl = URL.createObjectURL(raw);
    avatarPreview.value = avatarObjectUrl;
    profileEditForm.value.avatarFile = raw;
}

function saveProfile() {
    editFormRef.value?.validate((valid) => {
        if (!valid) return;
        const currentUser = user;
        const form = profileEditForm.value;
        if (!form.userId || !currentUser?.userId) {
            ElMessage.error('未获取到用户信息，无法保存');
            return;
        }

        const changes: UserProfileDTO = activeSection.value === 'personal'
            ? {
                userId: form.userId,
                nickname: form.nickname?.trim(),
                gender: form.gender,
                avatar: form.avatar,
                avatarFile: form.avatarFile,
            }
            : {
                userId: form.userId,
                email: form.email?.trim(),
            };

        saving.value = true;
        ChangeAvatar(changes)
            .then((res) => {
                if (!res.success) {
                    ElMessage.error(res.message);
                    return;
                }

                const updatedUser: User = { ...currentUser };
                if (changes.nickname !== undefined) updatedUser.nickname = changes.nickname;
                if (changes.email !== undefined) updatedUser.email = changes.email;
                if (changes.gender !== undefined) updatedUser.gender = changes.gender;
                if (changes.avatar !== undefined) updatedUser.avatar = changes.avatar;
                if (activeSection.value === 'personal' && typeof res.data === 'string' && res.data) {
                    updatedUser.avatar = res.data;
                }
                stateStorage.set('User', updatedUser);
                user = updatedUser;
                Object.assign(profile, {
                    nickname: updatedUser.nickname,
                    email: updatedUser.email,
                    gender: updatedUser.gender,
                    avatar: updatedUser.avatar,
                });
                isEditing.value = false;
                syncFormFromProfile();
                ElMessage.success('资料已更新');
            })
            .catch((error) => ElNotification.error(error))
            .finally(() => {
                saving.value = false;
            });
    });
}

function genderLabel(gender: number) {
    return genderOptions.find((option) => option.value === gender)?.label ?? '未选择';
}

function formatDate(value: Date | string) {
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? '未设置' : date.toLocaleDateString();
}
</script>

<style scoped>
#user-profile {
    display: grid;
    grid-template-columns: 190px minmax(0, 1fr);
    gap: 12px;
    min-height: calc(100vh - 80px);
    max-width: 1480px;
    margin: 0 auto;
    padding: 24px 32px;
    width: 70vw;
    color: #303133;
    box-sizing: border-box;
}

.profile-sidebar {
    display: flex;
    flex-direction: column;
    align-self: start;
    padding: 8px 0;
    border-right: 1px solid #ebeef5;
}

.sidebar-item {
    min-height: 48px;
    padding: 0 18px;
    border: 0;
    background: transparent;
    color: #606266;
    text-align: left;
    font: inherit;
    cursor: pointer;
}

.sidebar-item:hover {
    color: #303133;
    background: #f5f6f8;
}

.sidebar-item.is-active {
    background: #f0f1f5;
    color: #303133;
    font-weight: 600;
}

.profile-content {
    width: 80%;
    min-width: 0;
    min-height: 520px;
    background: #fff;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
}

.profile-summary {
    display: flex;
    align-items: center;
    gap: 20px;
    min-height: 164px;
    padding: 24px 28px;
    border-bottom: 1px solid #ebeef5;
    box-sizing: border-box;
}

.summary-avatar,
.avatar-preview {
    display: flex;
    flex: none;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    border-radius: 50%;
    background: #f0f1f5;
    color: #606266;
}

.summary-avatar {
    width: 92px;
    height: 92px;
    font-size: 32px;
}

.summary-avatar img,
.avatar-preview img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.summary-detail h1 {
    margin: 0 0 8px;
    font-size: 22px;
    font-weight: 600;
}

.summary-id {
    color: #909399;
    font-size: 14px;
}

.detail-section {
    padding: 20px 28px 32px;
}

.section-heading {
    display: flex;
    align-items: center;
    justify-content: space-between;
    min-height: 42px;
    margin-bottom: 8px;
    border-bottom: 1px solid #ebeef5;
}

.section-heading h2 {
    margin: 0;
    font-size: 17px;
    font-weight: 600;
}

.info-list {
    margin: 0;
}

.info-row {
    display: grid;
    grid-template-columns: 110px minmax(0, 1fr);
    align-items: center;
    min-height: 56px;
    padding: 0 16px;
}

.info-row dt {
    color: #7d8087;
}

.info-row dd {
    margin: 0;
    overflow-wrap: anywhere;
}

.profile-form {
    max-width: 620px;
    padding-top: 20px;
}

.avatar-editor {
    display: flex;
    align-items: center;
    gap: 16px;
}

.avatar-preview {
    width: 64px;
    height: 64px;
    font-size: 24px;
}

.avatar-hint {
    margin: 6px 0 0;
    color: #909399;
    font-size: 12px;
}

.read-only-value {
    color: #606266;
}

@media (max-width: 700px) {
    .user-profile-page {
        grid-template-columns: 1fr;
        gap: 12px;
        padding: 16px 12px;
    }

    .profile-sidebar {
        flex-direction: row;
        padding: 0;
        border-right: 0;
        border-bottom: 1px solid #ebeef5;
    }

    .sidebar-item {
        flex: 1;
        padding: 0 8px;
        text-align: center;
    }

    .profile-content {
        min-height: 420px;
    }

    .profile-summary {
        min-height: 132px;
        padding: 20px;
    }

    .summary-avatar {
        width: 72px;
        height: 72px;
    }

    .detail-section {
        padding: 16px 20px 24px;
    }
}
</style>
