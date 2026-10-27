<template>
    <div id="main">
        <el-row class="header" :gutter="16" align="middle">
            <el-col :span="4" class="header-logo">
                <el-image src="/blog.jpg" fit="cover" class="blog-image" />
            </el-col>
            <el-col :span="14" class="header-search">
                <div class="search-bar">
                    <el-select v-model="keyWord" class="search-select" placeholder="搜索" filterable clearable remote
                        :remote-method="getRemoteContent">
                        <el-option v-for="item in searchContent" :key="item" :label="item" :value="item" />
                    </el-select>
                    <el-button type="primary" @click="search">
                        <el-icon>
                            <Search />
                        </el-icon>
                    </el-button>
                </div>
            </el-col>
            <el-col :span="6" class="header-actions">
                <el-popover placement="bottom-end" trigger="click" :width="200">
                    <template #reference>
                        <el-avatar circle :size="32" :src="getSource(user.avatar,'Images')" class="user-avatar" />
                    </template>
                    <div class="user-info">
                        <p class="user-name">{{ user.nickname || '未登录' }}</p>
                        <p class="user-email">{{ user.email || '—' }}</p>
                    </div>
                </el-popover>
                <el-button type="warning" class="create-button" @click="createArticle">
                    <el-icon>
                        <Plus />
                    </el-icon>
                    创作
                </el-button>
            </el-col>
        </el-row>
    </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { type User } from '../api/user';
import stateStorage from '../modules/StateStorage';
import { getSource } from '../modules/Request';

const router = useRouter();

onMounted(() => {
    const storedUser = stateStorage.get<User>('User');
    if (storedUser) {
        user.value = storedUser;
    }
});

function search() {
    console.log('Searching for:', keyWord.value);
}

function createArticle() {
    router.push('/article/editor');
}

const keyWord = ref('');
const searchContent = ref<string[]>([]);
const user = ref<User>({
    userId: 0,
    nickname: '',
    email: '',
    token: '',
    gender: 0,
    avatar: ''
});

let contentRequestId = 0;

async function getRemoteContent(query: string) {
    const keyword = query.trim();
    const requestId = ++contentRequestId;
    if (!keyword) {
        searchContent.value = [];
        return;
    }
    // Simulate an API call
    const results = await new Promise<string[]>(resolve => {
        setTimeout(() => {
            resolve([`${keyword} result 1`, `${keyword} result 2`, `${keyword} result 3`]);
        }, 500);
    });
    if (requestId !== contentRequestId) return;
    searchContent.value = results;
}


</script>
<style scoped>
#main {
    padding: 10px 20px;
    border-bottom: 1px solid var(--border);
}

.header {
    flex-wrap: nowrap;
    text-align: left;
}

.header-logo,
.header-search,
.header-actions {
    display: flex;
    align-items: center;
    min-width: 0;
    flex-wrap: nowrap;
}

.search-bar {
    display: flex;
    align-items: center;
    gap: 8px;
    width: 100%;
    max-width: 420px;
}

.search-select {
    flex: 1;
    min-width: 0;
}

.header-actions {
    justify-content: flex-end;
    gap: 12px;
}

.create-button {
    flex-shrink: 0;
}

.blog-image {
    width: 40px;
    height: 40px;
    border-radius: 8px;
    flex-shrink: 0;
}

.user-avatar {
    flex-shrink: 0;
    cursor: pointer;
}

.user-info {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.user-info p {
    margin: 0;
    line-height: 1.4;
}

.user-name {
    color: var(--text-h);
    font-weight: 500;
}

.user-email {
    font-size: 13px;
}
</style>