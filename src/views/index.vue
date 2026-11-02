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
                <el-popover placement="bottom-end" trigger="click" :width="268" :popper-style="{ padding: '0' }">
                    <template #reference>
                        <el-avatar circle :size="40" :src="getSource(user.avatar,'Images')" class="user-avatar" />
                    </template>
                    <div class="user-card">
                        <div class="user-card-profile">
                            <el-avatar circle :size="56" :src="getSource(user.avatar,'Images')"
                                class="user-card-avatar" />
                            <span class="user-card-name">{{ user.nickname || '未登录' }}</span>
                        </div>
                        <el-link underline>{{ user.email || '' }}</el-link>
                        <div class="user-card-stats">
                            <div v-for="stat in stats" :key="stat.label" class="user-card-stat">
                                <span class="stat-value">{{ stat.value }}</span>
                                <span class="stat-label">{{ stat.label }}</span>
                            </div>
                        </div>
                        <ul class="user-card-menu">
                            <li v-for="item in menus" :key="item.key" class="user-card-menu-item"
                                @click="handleMenuSelect(item.key)">
                                <el-icon class="menu-icon">
                                    <component :is="item.icon" />
                                </el-icon>
                                <span>{{ item.label }}</span>
                            </li>
                        </ul>
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
        <main class="home-content">
            <section class="home-articles" aria-label="文章列表">
                <h1 class="section-title">文章列表</h1>
                <articles-view v-model:query="articleQuery" :articles="articles" :loading="articlesLoading"
                    max-height="calc(100vh - 190px)" @change="loadArticles" @create="createArticle" :withUserCreation="false"
                    @article-click="openArticle" />
            </section>
            <aside class="home-sidebar">
                <ArticleRanking />
            </aside>
        </main>
    </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { GetRecentArticles, type ArticleListItem } from '../api/article';
import type { BasePageQuery, Pagination } from '../modules/Request';
import type { User } from '../api/user';
import ArticleRanking from '../components/ArticleRanking.vue';
import stateStorage from '../modules/StateStorage';
import { getSource } from '../modules/Request';

const router = useRouter();

onMounted(() => {
    const storedUser = stateStorage.get<User>('User');
    if (storedUser) {
        user.value = storedUser;
    }
    loadArticles();
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

const articleQuery = ref<BasePageQuery>({
    page: 1,
    pageSize: 10,
});
const articlesLoading = ref(false);
const articles = ref<Pagination<ArticleListItem>>({
    page: 1,
    pageSize: 10,
    total: 0,
    data: [],
});

let contentRequestId = 0;

const stats = [
    { label: '粉丝', value: '--' },
    { label: '关注', value: '--' },
    { label: '获赞', value: '--' }
];

const menus = [
    { key: 'home', label: '我的主页', icon: 'House', path: '/index' },
    { key: 'profile', label: '个人中心', icon: 'User', path: '' },
    { key: 'content', label: '内容管理', icon: 'Opportunity', path: '/article/editor' }
];

function handleMenuSelect(key: string) {
    const menu = menus.find(item => item.key === key);
    if (menu?.path) {
        router.push(menu.path);
    }
}

async function loadArticles(query = articleQuery.value) {
    articleQuery.value = query;
    articlesLoading.value = true;
    try {
        const result = await GetRecentArticles({ ...query });
        if (!result.success) {
            ElMessage.error(result.message);
            return;
        }
        articles.value = result.data ?? {
            page: query.page,
            pageSize: query.pageSize,
            total: 0,
            data: [],
        };
    } catch (error) {
        ElMessage.error(error instanceof Error ? error.message : '文章加载失败');
    } finally {
        articlesLoading.value = false;
    }
}

function openArticle(article: ArticleListItem) {
    router.push(`/article/editor/${article.articleId}`);
}

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

.home-content {
    display: grid;
    grid-template-columns: minmax(0, 1fr) minmax(320px, 30%);
    gap: 24px;
    width: 100%;
    max-width: 1440px;
    margin: 24px auto 0;
}

.home-articles {
    min-width: 0;
    padding: 0 16px;
}

.section-title {
    margin: 0;
    padding: 0 4px 12px;
    color: #222226;
    font-size: 20px;
    font-weight: 600;
}

.home-sidebar {
    min-width: 0;
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

.user-card {
    text-align: left;
}

.user-card-profile {
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 16px 16px 12px;
}

.user-card-avatar {
    flex-shrink: 0;
}

.user-card-name {
    flex: 1;
    min-width: 0;
    font-size: 19px;
    font-weight: 600;
    color: var(--text-h);
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
}

.user-card-stats {
    display: flex;
    padding: 4px 0 14px;
    border-bottom: 1px solid var(--border);
}

.user-card-stat {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
}

.stat-value {
    font-size: 17px;
    font-weight: 600;
    color: var(--text-h);
}

.stat-label {
    font-size: 13px;
    color: #9c9ba1;
}

.user-card-menu {
    list-style: none;
    margin: 0;
    padding: 8px 0;
}

.user-card-menu-item {
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 12px 16px;
    font-size: 16px;
    color: var(--text-h);
    cursor: pointer;
    transition: background-color 0.2s;
}

.user-card-menu-item:hover {
    background: var(--accent-bg);
}

.menu-icon {
    font-size: 20px;
    color: var(--text-h);
}

@media (max-width: 800px) {
    .home-content {
        grid-template-columns: minmax(0, 1fr);
        gap: 20px;
    }

    .home-articles {
        padding: 0;
    }

    .home-sidebar {
        margin-top: 20px;
    }
}
</style>