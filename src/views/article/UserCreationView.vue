<template>
    <div class="user-creation">
        <header class="profile-card">
            <a class="avatar" :href="profile.homeUrl" aria-label="个人主页" @click.prevent>
                <img v-if="profile.avatar" class="avatar-image" :src="profile.avatar" :alt="`${profile.nickname}的头像`" />
                <span v-else class="avatar-fallback">{{ profile.nickname.slice(0, 1) }}</span>
            </a>

            <div class="profile-detail">
                <div class="name-row">
                    <h1 class="nickname">{{ profile.nickname }}</h1>
                    <span class="code-age">码龄{{ profile.codeAge }}年</span>
                    <span class="level-badge" :title="`等级 Lv.${profile.level}`">
                        <svg viewBox="0 0 24 24" aria-hidden="true">
                            <path
                                d="M12 2.5l7.5 3.2v6.1c0 4.6-3.1 8.3-7.5 10.7-4.4-2.4-7.5-6.1-7.5-10.7V5.7L12 2.5z" />
                        </svg>
                        <span class="level-text">{{ profile.level }}</span>
                    </span>
                </div>

                <ul class="stats">
                    <li class="stat">
                        <span class="stat-value">{{ profile.original }}</span>
                        <span class="stat-label">原创</span>
                    </li>
                    <li class="stat">
                        <span class="stat-value">{{ profile.fans }}</span>
                        <span class="stat-label">粉丝</span>
                    </li>
                    <li class="stat">
                        <span class="stat-value">{{ profile.following }}</span>
                        <span class="stat-label">关注</span>
                    </li>
                </ul>

                <div class="meta">
                    <span class="meta-item">加入{{ profile.platformName }}时间：{{ profile.joinedAt }}</span>
                </div>
            </div>

            <div class="actions">
                <el-button v-for="action in actions" :key="action.key" class="action-button" round
                    :aria-label="action.label" @click="handleAction(action.key)">
                    <el-icon class="action-icon" :size="16">
                        <component :is="action.icon" />
                    </el-icon>
                    <span>{{ action.label }}</span>
                </el-button>
            </div>
        </header>

        <section class="article-panel">
            <nav class="article-tabs" aria-label="文章分类">
                <button v-for="tab in tabs" :key="tab.key" type="button" class="tab-button"
                    :class="{ 'is-active': activeTab === tab.key }" @click="switchTab(tab.key)">
                    {{ tab.label }}
                </button>
            </nav>

            <div class="article-filters">
                <el-select v-model="query.publishedStatus" class="filter-select" placeholder="全部状态" clearable
                    :disabled="activeTab !== 'article'">
                    <el-option v-for="option in statusOptions" :key="option.value" :label="option.label"
                        :value="option.value" />
                </el-select>

                <el-date-picker v-model="dateRange" class="filter-date" type="daterange"
                    range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期"
                    value-format="YYYY-MM-DD" unlink-panels @change="handleDateChange" />

                <div class="filter-search">
                    <el-input v-model="query.search" class="search-input" placeholder="请输入标题或关键字" clearable
                        @keyup.enter="searchArticles">
                        <template #prefix>
                            <el-icon><Search /></el-icon>
                        </template>
                    </el-input>
                    <el-button type="primary" class="filter-button search-button" @click="searchArticles">搜索</el-button>
                    <el-button class="filter-button reset-button" @click="resetFilters">重置</el-button>
                </div>
            </div>

            <div class="article-content">
                <articles-view v-model:query="query" :articles="articles" :loading="loading" use-pagination
                    with-user-creation @change="handleQueryChange" @create="writeArticle" @edit="openArticle"
                    @delete="handleDeleteArticle" @article-click="openArticle" />
            </div>
        </section>
    </div>
</template>

<script lang="ts" setup>
import { Document, EditPen, Search, Setting } from '@element-plus/icons-vue';
import { ElMessage, ElNotification } from 'element-plus';
import {  onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import {
    ArticleStatus,
    ArticleStatusLabels,
    GetUserArticles,
    type ArticleListItem,
    type UserArticleQuery,
} from '../../api/article.ts';
import type { Pagination } from '../../modules/Request.ts';
import stateStorage from '../../modules/StateStorage.ts';
import type { User } from '../../api/user.ts';

type ArticleTab = 'article' | 'draft' | 'recycle';

const router = useRouter();
const dateRange = ref<string[] | null>(null);

const profile = reactive({
    nickname: '夜736',
    avatar: '',
    homeUrl: '/index',
    codeAge: 5,
    level: 5,
    original: '暂无',
    fans: 0,
    following: 6,
    ipLocation: '广东省',
    platformName: 'CSDN',
    joinedAt: '2021-12-04',
});

const actions = [
    { key: 'edit', label: '编辑资料', icon: EditPen },
    { key: 'navigation', label: '导航设置', icon: Setting },
    { key: 'manage', label: '管理博文', icon: Document },
] as const;

const tabs: { key: ArticleTab; label: string }[] = [
    { key: 'article', label: '文章' },
    { key: 'draft', label: '草稿箱' },
    { key: 'recycle', label: '回收站' },
];

const statusOptions = [
    { value: ArticleStatus.Published, label: ArticleStatusLabels[ArticleStatus.Published] },
    { value: ArticleStatus.Auditing, label: ArticleStatusLabels[ArticleStatus.Auditing] },
    { value: ArticleStatus.Audited, label: ArticleStatusLabels[ArticleStatus.Audited] },
    { value: ArticleStatus.NotPassed, label: ArticleStatusLabels[ArticleStatus.NotPassed] },
];

const activeTab = ref<ArticleTab>('article');
const loading = ref(false);
const query = ref<UserArticleQuery>({
    userId: 0,
    isDraft: false,
    isDeleted: false,
    page: 1,
    pageSize: 10,
});
const articles = ref<Pagination<ArticleListItem>>({
    page: 1,
    pageSize: 10,
    total: 0,
    data: [],
});



onMounted(() => {
    const user = stateStorage.get<User>('User');
    if (user) query.value.userId = user.userId;
    loadArticles();
});

function handleAction(key: (typeof actions)[number]['key']) {
    ElMessage.info(`点击了「${actions.find((item) => item.key === key)?.label}」`);
}

function switchTab(key: ArticleTab) {
    if (activeTab.value === key) return;
    activeTab.value = key;
    query.value.isDraft = key === 'draft';
    query.value.isDeleted = key === 'recycle';
    query.value.publishedStatus = undefined;
    query.value.page = 1;
    loadArticles();
}

function searchArticles() {
    query.value.page = 1;
    loadArticles();
}

function resetFilters() {
    query.value.publishedStatus = undefined;
    query.value.startDate = undefined;
    query.value.endDate = undefined;
    query.value.search = undefined;
    query.value.page = 1;
    loadArticles();
}

function handleQueryChange(next: UserArticleQuery) {
    query.value = next;
    loadArticles();
}

function loadArticles() {
    loading.value = true;
    GetUserArticles({ ...query.value })
        .then((res) => {
            if (!res.success) {
                ElMessage.error(res.message);
                return;
            }
            articles.value = (res.data as Pagination<ArticleListItem>) ?? {
                page: query.value.page,
                pageSize: query.value.pageSize,
                total: 0,
                data: [],
            };
        })
        .catch((error) => ElNotification.error(error))
        .finally(() => {
            loading.value = false;
        });
}

function openArticle(article: ArticleListItem) {
    router.push(`/article/editor/${article.articleId}`);
}

function handleDeleteArticle(article: ArticleListItem) {
    ElMessage.warning(`「${article.title || '未命名文章'}」的删除接口尚未接入`);
}

function writeArticle() {
    router.push('/article/editor');
}

function handleDateChange(dates: string[] | null) {
    if (dates && dates.length === 2) {
        query.value.startDate = dates[0];
        query.value.endDate = dates[1];
    } else {
        query.value.startDate = undefined;
        query.value.endDate = undefined;
    }
    query.value.page = 1;
    loadArticles();
}
</script>

<style scoped>
.user-creation {
    min-height: 100%;
    background: #fff;
    color: #222226;
    text-align: left;
}

.profile-card {
    display: flex;
    align-items: flex-start;
    gap: 24px;
    padding: 28px 40px 24px;
    border-bottom: 1px solid #f2f3f5;
    box-sizing: border-box;
}

.avatar {
    flex: none;
    display: block;
    width: 92px;
    height: 92px;
    border-radius: 50%;
    overflow: hidden;
    box-shadow: 0 0 0 4px #fff, 0 2px 10px rgba(0, 0, 0, 0.12);
    text-decoration: none;
}

.avatar-image {
    display: block;
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.avatar-fallback {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100%;
    height: 100%;
    background: linear-gradient(135deg, #4a3f55, #2b2434);
    color: #fff;
    font-size: 34px;
    font-weight: 600;
}

.profile-detail {
    flex: 1 1 auto;
    min-width: 0;
    padding-top: 2px;
}

.name-row {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
}

.nickname {
    margin: 0;
    font-size: 26px;
    line-height: 1.2;
    font-weight: 600;
    letter-spacing: normal;
    color: #222226;
}

.code-age {
    padding: 2px 8px;
    border-radius: 4px;
    background: #eaf3ff;
    color: #1f6feb;
    font-size: 13px;
    line-height: 18px;
    white-space: nowrap;
}

.level-badge {
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 22px;
    height: 22px;
}

.level-badge svg {
    width: 100%;
    height: 100%;
    fill: #ff6b00;
}

.level-text {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -52%);
    color: #fff;
    font-size: 10px;
    font-weight: 700;
    line-height: 1;
}

.stats {
    display: flex;
    align-items: center;
    margin: 14px 0 0;
    padding: 0;
    list-style: none;
}

.stat {
    display: flex;
    align-items: baseline;
    gap: 6px;
    padding: 0 20px;
    border-left: 1px solid #e8e9eb;
}

.stat:first-child {
    padding-left: 0;
    border-left: 0;
}

.stat-value {
    font-size: 21px;
    line-height: 1.2;
    font-weight: 600;
    color: #222226;
}

.stat-label {
    font-size: 15px;
    color: #7d8087;
}

.meta {
    display: flex;
    flex-wrap: wrap;
    gap: 8px 32px;
    margin-top: 12px;
    font-size: 15px;
    color: #7d8087;
}

.actions {
    flex: none;
    display: flex;
    align-items: center;
    gap: 12px;
    padding-top: 6px;
}

.action-button {
    height: 36px;
    padding: 0 18px;
    border-color: #e4e6eb;
    border-radius: 18px;
    background: #fff;
    color: #4e5969;
    font-size: 15px;
}

.action-button:hover,
.action-button:focus {
    border-color: #d3d6dc;
    background: #f7f8fa;
    color: #222226;
}

.action-icon {
    margin-right: 6px;
    color: currentColor;
}

.article-panel {
    padding: 4px 40px 40px;
    box-sizing: border-box;
}

.article-tabs {
    display: flex;
    align-items: center;
    gap: 36px;
    height: 52px;
    border-bottom: 1px solid #f2f3f5;
}

.tab-button {
    position: relative;
    height: 100%;
    padding: 0;
    border: 0;
    background: transparent;
    color: #4e5969;
    font: inherit;
    font-size: 17px;
    cursor: pointer;
    transition: color 0.2s;
}

.tab-button:hover {
    color: #222226;
}

.tab-button.is-active {
    color: #222226;
    font-weight: 600;
}

.tab-button.is-active::after {
    content: '';
    position: absolute;
    right: 0;
    bottom: -1px;
    left: 0;
    height: 3px;
    border-radius: 2px;
    background: #fc5531;
}

.article-filters {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 12px;
    padding: 20px 0 16px;
}

.filter-select {
    width: 150px;
}

.filter-date {
    width: 280px;
}

.filter-search {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-left: auto;
}

.search-input {
    width: 260px;
}

.filter-search .filter-button {
    margin-left: 0;
    height: 32px;
    padding: 0 16px;
}

.search-button {
    --el-button-bg-color: #fc5531;
    --el-button-border-color: #fc5531;
    --el-button-hover-bg-color: #ff7045;
    --el-button-hover-border-color: #ff7045;
    --el-button-active-bg-color: #e64a28;
    --el-button-active-border-color: #e64a28;
}

.article-content {
    border-radius: 8px;
    background: #fff;
}

@media (max-width: 900px) {
    .profile-card {
        flex-wrap: wrap;
        padding: 24px 20px 20px;
    }

    .actions {
        width: 100%;
        padding-top: 0;
        flex-wrap: wrap;
    }

    .stat {
        padding: 0 14px;
    }

    .article-panel {
        padding: 4px 20px 24px;
    }

    .filter-search {
        width: 100%;
        margin-left: 0;
    }

    .search-input {
        flex: 1 1 auto;
        width: auto;
    }
}
</style>