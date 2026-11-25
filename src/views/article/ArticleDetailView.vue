<template>
    <div class="article-detail-page">
        <header class="detail-header">
            <a class="brand" href="/index" aria-label="返回首页" @click.prevent="goHome">
                <img class="brand-logo" src="/blog.jpg" alt="博客 Logo" />
            </a>
            <el-button class="back-button" text circle aria-label="返回" @click="goBack">
                <svg class="back-icon" viewBox="0 0 24 24" aria-hidden="true">
                    <path d="M15 5l-7 7 7 7" />
                </svg>
            </el-button>
            <span class="header-title">文章详情</span>
        </header>

        <main class="detail-main">
            <div v-if="detail" class="detail-body">
                <article class="detail-content">
                    <h1 class="detail-title">{{ detail.title || '未命名文章' }}</h1>

                    <div class="detail-meta">
                        <span class="meta-origin">原创</span>
                        <span class="meta-dot">·</span>
                        <span>{{ formatDateTime(detail.publishTime || detail.createTime) }}</span>
                        <span class="meta-dot">·</span>
                        <span class="meta-views" title="阅读数">
                            <svg class="meta-icon" viewBox="0 0 24 24" aria-hidden="true">
                                <path d="M12 5.5c-4.6 0-8.5 4-8.5 6.5s3.9 6.5 8.5 6.5 8.5-4 8.5-6.5-3.9-6.5-8.5-6.5z" />
                                <circle cx="12" cy="12" r="2.8" />
                            </svg>
                            {{ formatCount(detail.viewCount) }} 阅读
                        </span>
                        <span v-if="declarationLabel" class="meta-declaration">{{ declarationLabel }}</span>
                    </div>

                    <section class="detail-author">
                        <el-avatar :size="44" :src="authorAvatarUrl || undefined" class="author-avatar">
                            {{ authorInitial }}
                        </el-avatar>
                        <div class="author-info">
                            <span class="author-name">{{ detail.authorName || '匿名用户' }}</span>
                            <span class="author-sub">发布者</span>
                        </div>
                        <el-button class="follow-button" round @click="followAuthor">+ 关注</el-button>
                    </section>

                    <section v-if="coverUrl || detail.abstract" class="detail-summary">
                        <el-image v-if="coverUrl" class="summary-cover" :src="coverUrl" fit="cover" alt="文章封面" />
                        <div class="summary-text">
                            <span class="summary-label">摘要</span>
                            <p class="summary-body">{{ detail.abstract || '作者暂未填写摘要' }}</p>
                        </div>
                    </section>

                    <div ref="contentRef" class="article-content tiptap-content" v-html="detail.content || ''"></div>
                    <el-empty v-if="!detail.content" description="本文暂无内容" :image-size="90" />
                </article>

                <aside class="detail-rail" aria-label="文章操作">
                    <el-popover placement="left" :width="230" trigger="click" :disabled="!toc.length"
                        popper-class="toc-popper">
                        <template #reference>
                            <button class="rail-button" type="button" :class="{ 'is-muted': !toc.length }"
                                title="目录" aria-label="目录">
                                <svg class="rail-icon" viewBox="0 0 24 24" aria-hidden="true">
                                    <path d="M8 7h12M8 12h12M8 17h12" />
                                    <circle cx="4.2" cy="7" r="0.9" />
                                    <circle cx="4.2" cy="12" r="0.9" />
                                    <circle cx="4.2" cy="17" r="0.9" />
                                </svg>
                                <span class="rail-label">目录</span>
                            </button>
                        </template>
                        <div class="toc-panel">
                            <span class="toc-title">目录</span>
                            <ul class="toc-list">
                                <li v-for="item in toc" :key="item.id" class="toc-item"
                                    :style="{ paddingLeft: `${(item.level - 1) * 12}px` }"
                                    @click="scrollToHeading(item.id)">
                                    {{ item.text }}
                                </li>
                            </ul>
                        </div>
                    </el-popover>

                    <button class="rail-button" type="button" :class="{ 'is-active': detail.liked }"
                        :title="detail.liked ? '已点赞' : '点赞'" aria-label="点赞" @click="likeArticle">
                        <svg class="rail-icon" viewBox="0 0 24 24" aria-hidden="true">
                            <path d="M7 10.5V19H4.6a1.6 1.6 0 0 1-1.6-1.6v-5.3A1.6 1.6 0 0 1 4.6 10.5H7z" />
                            <path
                                d="M7 10.5l4.1-6.2a1.2 1.2 0 0 1 2.1.3l.6 1.9a3 3 0 0 1-.2 2.4l-.9 1.6h6a2 2 0 0 1 2 2.3l-1 5a2 2 0 0 1-2 1.7H7" />
                        </svg>
                        <span class="rail-count">{{ formatCount(detail.likeCount) }}</span>
                    </button>

                    <button class="rail-button rail-dislike" type="button" :class="{ 'is-active': detail.disliked }"
                        :title="detail.disliked ? '已点踩' : '点踩'" aria-label="点踩" @click="dislikeArticle">
                        <svg class="rail-icon" viewBox="0 0 24 24" aria-hidden="true">
                            <path d="M7 10.5V19H4.6a1.6 1.6 0 0 1-1.6-1.6v-5.3A1.6 1.6 0 0 1 4.6 10.5H7z" />
                            <path
                                d="M7 10.5l4.1-6.2a1.2 1.2 0 0 1 2.1.3l.6 1.9a3 3 0 0 1-.2 2.4l-.9 1.6h6a2 2 0 0 1 2 2.3l-1 5a2 2 0 0 1-2 1.7H7" />
                        </svg>
                        <span class="rail-count">{{ formatCount(detail.dislikeCount) }}</span>
                    </button>

                    <div class="rail-button is-static" role="status" title="阅读数" aria-label="阅读数">
                        <svg class="rail-icon" viewBox="0 0 24 24" aria-hidden="true">
                            <path d="M12 5.5c-4.6 0-8.5 4-8.5 6.5s3.9 6.5 8.5 6.5 8.5-4 8.5-6.5-3.9-6.5-8.5-6.5z" />
                            <circle cx="12" cy="12" r="2.8" />
                        </svg>
                        <span class="rail-count">{{ formatCount(detail.viewCount) }}</span>
                    </div>

                    <button class="rail-button" type="button" title="评论" aria-label="评论" @click="openComments">
                        <svg class="rail-icon" viewBox="0 0 24 24" aria-hidden="true">
                            <path
                                d="M20.5 12c0 3.6-3.8 6.5-8.5 6.5-1 0-2-.1-2.9-.4L4 20l1.1-3.2C3.9 15.6 3.5 13.9 3.5 12 3.5 8.4 7.3 5.5 12 5.5s8.5 2.9 8.5 6.5z" />
                            <path d="M8.5 11.8h.01M12 11.8h.01M15.5 11.8h.01" />
                        </svg>
                        <span class="rail-count">{{ formatCount(detail.commentCount) }}</span>
                    </button>

                    <button class="rail-button" type="button" :class="{ 'is-active': detail.collected }"
                        :title="detail.collected ? '取消收藏' : '收藏'" aria-label="收藏" @click="toggleCollect">
                        <svg class="rail-icon" viewBox="0 0 24 24" aria-hidden="true">
                            <path d="M6.8 3.8h10.4a1 1 0 0 1 1 1v15.4l-6.2-3.9-6.2 3.9V4.8a1 1 0 0 1 1-1z" />
                        </svg>
                        <span class="rail-count">{{ formatCount(detail.collectCount) }}</span>
                    </button>

                    <button class="rail-button" type="button" title="分享" aria-label="分享" @click="shareArticle">
                        <svg class="rail-icon" viewBox="0 0 24 24" aria-hidden="true">
                            <path d="M14.5 5.5 21 12l-6.5 6.5" />
                            <path d="M21 12h-9.5A7.5 7.5 0 0 0 4 19.5v-1A7.5 7.5 0 0 1 11.5 11H21" />
                        </svg>
                        <span class="rail-label">分享</span>
                    </button>
                </aside>
            </div>

            <div v-else-if="loadFailed" class="detail-error">
                <el-empty description="文章不存在或已被删除">
                    <el-button class="home-button" round @click="goHome">返回首页</el-button>
                </el-empty>
            </div>
        </main>
    </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage, ElNotification } from 'element-plus';
import '../../components/tiptap/tiptap-content.css';
import {
    CollectArticle,
    GetArticleDetail,
    LikeOrDislike,
    LikeOrDislikeArticle,
    ViewArticle,
    type ArticleDetail
} from '../../api/article.ts';
import { getSource } from '../../modules/Request.ts';
import { CreationDecalrations, DefaultArticleCover, GetLabelFromValue } from '../../modules/Common.ts';
import { loading } from '../../modules/Utils.ts';

interface TocItem {
    id: string;
    text: string;
    level: number;
}

const route = useRoute();
const router = useRouter();
// 详情页路径形如 /article/detail/:id，进入页面先解析出文章编号
const articleId = computed(() => String(route.params.id ?? '').trim());

const detail = ref<ArticleDetail | null>(null);
const loadFailed = ref(false);
const contentRef = ref<HTMLDivElement | null>(null);
const toc = ref<TocItem[]>([]);
const likePending = ref(false);
const collectPending = ref(false);
let originalTitle = '';

const coverUrl = computed(() => resolveCoverUrl());
const authorAvatarUrl = computed(() => detail.value?.authorAvatar
    ? getSource(detail.value.authorAvatar, 'Images')
    : '');
const authorInitial = computed(() => (detail.value?.authorName ?? '匿').slice(0, 1));
const declarationLabel = computed(() => {
    const value = Number(detail.value?.creationDeclaration ?? 0);
    if (!value) {
        return '';
    }
    return GetLabelFromValue(value, CreationDecalrations) ?? '';
});

watch(articleId, () => {
    loadDetail();
}, { immediate: true });

onMounted(() => {
    originalTitle = document.title;
    window.addEventListener('keydown', handleKeydown);
});

onBeforeUnmount(() => {
    window.removeEventListener('keydown', handleKeydown);
    document.title = originalTitle;
});

function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
        goBack();
    }
}

function showRequestError(error: unknown) {
    ElNotification.error(error instanceof Error ? error.message : String(error));
}

async function loadDetail() {
    if (!articleId.value) {
        loadFailed.value = true;
        return;
    }
    const loadingInstance = loading({ message: '文章加载中...', duration: 0 });
    loadFailed.value = false;
    try {
        // 先上报阅读（服务端按用户去重），再取详情，保证阅读数包含本次访问
        await ViewArticle(articleId.value).catch(() => undefined);
        const res = await GetArticleDetail(articleId.value);
        if (!res.success) {
            loadFailed.value = true;
            ElMessage.error(res.message);
            return;
        }
        detail.value = res.data as ArticleDetail;
        document.title = `${detail.value.title || '文章详情'} - Blog`;
        await nextTick();
        buildToc();
    } catch (error) {
        loadFailed.value = true;
        showRequestError(error);
    } finally {
        loadingInstance.close();
    }
}

function buildToc() {
    toc.value = [];
    const root = contentRef.value;
    if (!root) {
        return;
    }
    const headings = Array.from(root.querySelectorAll<HTMLElement>('h1, h2, h3'));
    headings.forEach((heading, index) => {
        const text = (heading.textContent ?? '').trim();
        if (!text) {
            return;
        }
        const id = `article-heading-${index}`;
        heading.id = id;
        toc.value.push({ id, text, level: Number(heading.tagName.slice(1)) });
    });
}

function scrollToHeading(id: string) {
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

async function likeArticle() {
    const current = detail.value;
    if (!current || likePending.value) {
        return;
    }
    likePending.value = true;
    try {
        const res = await LikeOrDislikeArticle(current.articleId, LikeOrDislike.Like);
        if (!res.success) {
            ElMessage.error(res.message);
            return;
        }
        if (current.disliked) {
            current.disliked = false;
            current.dislikeCount = Math.max(0, current.dislikeCount - 1);
        }
        current.liked = true;
        current.likeCount += 1;
    } catch (error) {
        showRequestError(error);
    } finally {
        likePending.value = false;
    }
}

async function dislikeArticle() {
    const current = detail.value;
    if (!current || likePending.value) {
        return;
    }
    likePending.value = true;
    try {
        const res = await LikeOrDislikeArticle(current.articleId, LikeOrDislike.Dislike);
        if (!res.success) {
            ElMessage.error(res.message);
            return;
        }
        if (current.liked) {
            current.liked = false;
            current.likeCount = Math.max(0, current.likeCount - 1);
        }
        current.disliked = true;
        current.dislikeCount += 1;
    } catch (error) {
        showRequestError(error);
    } finally {
        likePending.value = false;
    }
}

async function toggleCollect() {
    const current = detail.value;
    if (!current || collectPending.value) {
        return;
    }
    const toCollect = !current.collected;
    collectPending.value = true;
    try {
        const res = await CollectArticle(current.articleId, toCollect);
        if (!res.success) {
            ElMessage.error(res.message);
            return;
        }
        current.collected = toCollect;
        current.collectCount = Math.max(0, current.collectCount + (toCollect ? 1 : -1));
        ElMessage.info(toCollect ? '已收藏' : '已取消收藏');
    } catch (error) {
        showRequestError(error);
    } finally {
        collectPending.value = false;
    }
}

function openComments() {
    ElMessage.info('评论功能开发中');
}

function followAuthor() {
    ElMessage.info('关注功能开发中');
}

async function shareArticle() {
    try {
        await navigator.clipboard.writeText(window.location.href);
        ElMessage.success('文章链接已复制');
    } catch {
        ElMessage.warning('复制失败，请手动复制地址栏链接');
    }
}

function resolveCoverUrl() {
    const current = detail.value;
    const value = (current?.cover ?? '').trim();
    if (!value) {
        return '';
    }
    if (value === DefaultArticleCover) {
        return getSource(value, 'Images');
    }
    if (/^(https?:|data:|blob:)/i.test(value) || value.startsWith('/')) {
        return value;
    }
    return getSource(value, `Creations/${current?.authorId ?? 0}`);
}

function formatCount(value?: number) {
    const count = value ?? 0;
    if (count >= 10000) return `${(count / 10000).toFixed(1)}w`;
    if (count >= 1000) return `${(count / 1000).toFixed(1)}k`;
    return `${count}`;
}

function formatDateTime(value?: string | Date) {
    if (!value) return '';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '';
    const pad = (num: number) => String(num).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function goHome() {
    router.push('/index');
}

function goBack() {
    if (window.history.length > 1) {
        router.back();
        return;
    }
    router.push('/index');
}
</script>

<style scoped>
.article-detail-page {
    min-height: 100vh;
    background:
        radial-gradient(1100px 380px at 50% -10%, rgba(255, 255, 255, 0.92), rgba(255, 255, 255, 0) 70%),
        linear-gradient(180deg, #faf6ee 0%, #f1e9d9 100%);
    color: #2b2f36;
    text-align: left;
}

.detail-header {
    display: flex;
    align-items: center;
    gap: 6px;
    max-width: 1120px;
    margin: 0 auto;
    padding: 16px 32px 0;
}

.brand-logo {
    display: block;
    width: 38px;
    height: 38px;
    border-radius: 9px;
}

.back-button {
    margin-left: 4px;
    color: #6b675c;
}

.back-icon {
    width: 20px;
    height: 20px;
    fill: none;
    stroke: currentColor;
    stroke-width: 1.8;
    stroke-linecap: round;
    stroke-linejoin: round;
}

.header-title {
    font-size: 14px;
    color: #8d8778;
    letter-spacing: 0.4px;
}

.detail-main {
    max-width: 1120px;
    margin: 0 auto;
    padding: 12px 32px 96px;
}

.detail-body {
    display: grid;
    grid-template-columns: minmax(0, 1fr) 56px;
    column-gap: 36px;
    align-items: start;
}

.detail-content {
    min-width: 0;
    max-width: 860px;
}

.detail-title {
    margin: 26px 0 14px;
    color: #1d2025;
    font-size: 34px;
    font-weight: 700;
    line-height: 1.35;
    letter-spacing: 0.4px;
}

.detail-meta {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 8px;
    font-size: 14px;
    color: #948d7d;
}

.meta-origin {
    padding: 1px 8px;
    border: 1px solid #e3d8c2;
    border-radius: 999px;
    color: #a3833f;
    font-size: 13px;
}

.meta-dot {
    color: #c3baa6;
}

.meta-views {
    display: inline-flex;
    align-items: center;
    gap: 5px;
}

.meta-icon {
    width: 16px;
    height: 16px;
    fill: none;
    stroke: currentColor;
    stroke-width: 1.5;
    stroke-linecap: round;
    stroke-linejoin: round;
}

.meta-declaration {
    padding: 2px 10px;
    border-radius: 999px;
    border: 1px solid #eadfc9;
    background: rgba(255, 255, 255, 0.75);
    color: #a3833f;
    font-size: 13px;
}

.detail-author {
    display: flex;
    align-items: center;
    gap: 12px;
    margin: 20px 0 22px;
    padding-bottom: 18px;
    border-bottom: 1px dashed #e5dbc7;
}

.author-avatar {
    flex: none;
    background: linear-gradient(135deg, #f2a65a, #d97541);
    color: #fff;
    font-size: 16px;
    font-weight: 600;
}

.author-info {
    display: flex;
    flex-direction: column;
    gap: 2px;
    min-width: 0;
}

.author-name {
    color: #2b2f36;
    font-size: 15px;
    font-weight: 600;
}

.author-sub {
    color: #9c9484;
    font-size: 13px;
}

.follow-button {
    margin-left: auto;
    --el-button-bg-color: rgba(255, 255, 255, 0.7);
    --el-button-border-color: #ddcfb2;
    --el-button-text-color: #8a6d3b;
    --el-button-hover-bg-color: #f6ecd8;
    --el-button-hover-border-color: #cbb389;
    --el-button-hover-text-color: #7a5c2c;
    --el-button-active-bg-color: #f0e3c9;
    --el-button-active-border-color: #c2a878;
    --el-button-active-text-color: #7a5c2c;
    padding: 8px 18px;
    font-weight: 500;
}

.detail-summary {
    display: flex;
    align-items: stretch;
    gap: 16px;
    margin-bottom: 30px;
    padding: 14px 18px;
    border: 1px solid #ece1cb;
    border-radius: 12px;
    background: rgba(255, 253, 247, 0.9);
    box-shadow: 0 6px 18px rgba(122, 101, 62, 0.06);
}

.summary-cover {
    flex: none;
    width: 104px;
    height: 78px;
    border-radius: 8px;
    overflow: hidden;
    background: #f2ece0;
}

.summary-text {
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 6px;
    min-width: 0;
}

.summary-label {
    color: #a3833f;
    font-size: 13px;
    font-weight: 600;
    letter-spacing: 1px;
}

.summary-body {
    margin: 0;
    color: #5f5a4e;
    font-size: 15px;
    line-height: 1.7;
    word-break: break-word;
}

.article-content {
    padding-bottom: 28px;
    font-size: 16px;
    line-height: 1.95;
    color: #33373f;
}

.article-content :deep(h1),
.article-content :deep(h2),
.article-content :deep(h3),
.article-content :deep(h4) {
    scroll-margin-top: 24px;
}

.article-content :deep(a) {
    color: #a4671f;
}

.article-content :deep(blockquote) {
    padding: 8px 16px;
    border-left-color: #ddd0b6;
    border-radius: 0 8px 8px 0;
    background: rgba(255, 255, 255, 0.55);
    color: #6b6559;
}

.article-content :deep(pre) {
    background: #2a2a2c;
}

.article-content :deep(img) {
    border-radius: 8px;
}

.article-content :deep(table th) {
    background: #f7f1e4;
}

.detail-rail {
    position: sticky;
    top: 24px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 12px;
    padding-top: 32px;
}

.rail-button {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 3px;
    width: 52px;
    min-height: 52px;
    padding: 9px 0 7px;
    border: 1px solid #e7dcc7;
    border-radius: 999px;
    background: rgba(255, 255, 255, 0.78);
    color: #6f6a5e;
    cursor: pointer;
    transition: background-color 0.2s, color 0.2s, border-color 0.2s, transform 0.2s;
}

.rail-button:hover {
    border-color: #d9c8a4;
    background: #fff;
    color: #3b382f;
    transform: translateY(-1px);
}

.rail-button:focus-visible {
    outline: 2px solid #e5484d;
    outline-offset: 2px;
}

.rail-button.is-static {
    cursor: default;
}

.rail-button.is-static:hover {
    border-color: #e7dcc7;
    background: rgba(255, 255, 255, 0.78);
    color: #6f6a5e;
    transform: none;
}

.rail-button.is-muted {
    color: #b5ae9f;
    cursor: not-allowed;
}

.rail-button.is-active {
    border-color: rgba(229, 72, 77, 0.4);
    background: #fff1f1;
    color: #e5484d;
}

.rail-icon {
    width: 21px;
    height: 21px;
    fill: none;
    stroke: currentColor;
    stroke-width: 1.6;
    stroke-linecap: round;
    stroke-linejoin: round;
}

.rail-dislike .rail-icon {
    transform: rotate(180deg);
}

.rail-count,
.rail-label {
    font-size: 12px;
    line-height: 1.1;
}

.toc-panel {
    display: flex;
    flex-direction: column;
    gap: 8px;
    max-height: 320px;
    overflow-y: auto;
}

.toc-title {
    color: #a3833f;
    font-size: 13px;
    font-weight: 600;
    letter-spacing: 1px;
}

.toc-list {
    margin: 0;
    padding: 0;
    list-style: none;
}

.toc-item {
    padding: 6px 8px;
    overflow: hidden;
    border-radius: 6px;
    color: #55524a;
    font-size: 13px;
    line-height: 1.5;
    text-overflow: ellipsis;
    white-space: nowrap;
    cursor: pointer;
}

.toc-item:hover {
    background: #f7f1e4;
    color: #a4671f;
}

.detail-error {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 60vh;
}

.home-button {
    --el-button-text-color: #8a6d3b;
    --el-button-border-color: #ddcfb2;
    --el-button-hover-bg-color: #f6ecd8;
    --el-button-hover-border-color: #cbb389;
    --el-button-hover-text-color: #7a5c2c;
}

@media (max-width: 1080px) {
    .detail-body {
        grid-template-columns: minmax(0, 1fr);
    }

    .detail-content {
        max-width: 100%;
    }

    .detail-rail {
        position: sticky;
        top: auto;
        bottom: 0;
        flex-direction: row;
        justify-content: space-around;
        margin: 24px -32px -96px;
        padding: 12px 16px calc(12px + env(safe-area-inset-bottom, 0px));
        border-top: 1px solid #e7dcc7;
        background: rgba(250, 246, 238, 0.96);
        backdrop-filter: blur(8px);
    }

    .rail-button {
        width: 46px;
        min-height: 46px;
        border: 0;
        background: transparent;
    }

    .rail-button.is-active {
        background: #fff1f1;
    }
}

@media (max-width: 640px) {
    .detail-header,
    .detail-main {
        padding-right: 18px;
        padding-left: 18px;
    }

    .detail-title {
        font-size: 26px;
    }

    .detail-summary {
        flex-direction: column;
    }

    .summary-cover {
        width: 100%;
        height: 160px;
    }

    .detail-rail {
        margin-right: -18px;
        margin-left: -18px;
    }
}
</style>
