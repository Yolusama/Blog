<template>
    <div class="article-views" :style="{ maxHeight: maxHeightValue }">
        <div ref="scrollRef" class="article-scroll" @scroll.passive="handleScroll">
            <ul v-if="articles.data.length" class="article-list">
                <li v-for="article in articles.data" :key="article.articleId" class="article-item" tabindex="0"
                    @click="handleArticleClick(article)" @keydown.enter="handleItemKeydown($event, article)">
                    <div class="article-author">
                        <el-image :src="getSource(article.authorAvatar, 'Images')" class="author-avatar"></el-image>
                        <span class="author-name">{{ article.authorName || '匿名用户' }}</span>
                    </div>

                    <h2 class="article-title">{{ article.title || '未命名文章' }}</h2>
                    <p v-if="article.abstract" class="article-abstract">{{ article.abstract }}</p>

                    <div class="article-meta" v-if="!article.isDraft">
                        <span class="meta-item">
                            <svg class="meta-icon" viewBox="0 0 24 24" aria-hidden="true">
                                <path d="M12 5.5c-4.6 0-8.5 4-8.5 6.5s3.9 6.5 8.5 6.5 8.5-4 8.5-6.5-3.9-6.5-8.5-6.5z" />
                                <circle cx="12" cy="12" r="2.8" />
                            </svg>
                            阅读 {{ formatCount(article.viewCount) }}
                        </span>
                        <span class="meta-item">
                            <svg class="meta-icon" viewBox="0 0 24 24" aria-hidden="true">
                                <path d="M7 10.5V19H4.6a1.6 1.6 0 0 1-1.6-1.6v-5.3A1.6 1.6 0 0 1 4.6 10.5H7z" />
                                <path
                                    d="M7 10.5l4.1-6.2a1.2 1.2 0 0 1 2.1.3l.6 1.9a3 3 0 0 1-.2 2.4l-.9 1.6h6a2 2 0 0 1 2 2.3l-1 5a2 2 0 0 1-2 1.7H7" />
                            </svg>
                            赞 {{ formatCount(article.likeCount) }}
                        </span>
                        <span class="meta-item">
                            <svg class="meta-icon" viewBox="0 0 24 24" aria-hidden="true">
                                <path d="M6.8 3.8h10.4a1 1 0 0 1 1 1v15.4l-6.2-3.9-6.2 3.9V4.8a1 1 0 0 1 1-1z" />
                            </svg>
                            收藏 {{ formatCount(article.collectCount) }}
                        </span>
                    </div>
                    <el-button v-if="withUserCreation" class="delete-button" link type="danger" circle
                        aria-label="删除文章" title="删除文章" @click.stop="handleDelete(article)">
                        <el-icon>
                            <Close />
                        </el-icon>
                    </el-button>
                </li>
            </ul>

            <div v-else class="article-empty">
                <slot name="empty">
                    <el-empty description="当前内容为空">
                        <el-button v-if="withUserCreation" type="primary" class="create-button"
                            @click="handleCreate">创建文章</el-button>
                    </el-empty>
                </slot>
            </div>
        </div>

        <div v-if="!usePagination" class="article-footer">
            <span v-if="loading" class="footer-loading">
                <el-icon class="is-loading">
                    <Loading />
                </el-icon>
                加载中...
            </span>
            <transition name="footer-fade">
                <span v-if="reachEndVisible" class="footer-end">已经到底了</span>
            </transition>
        </div>

        <div v-if="usePagination && articles.total > 0" class="article-pager">
            <el-pagination :total="articles.total" :page-size="query.pageSize" :current-page="query.page"
                @current-change="handlePageChange" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { Close, Loading } from '@element-plus/icons-vue';
import { computed, nextTick, onBeforeUnmount, ref, watch } from 'vue';
import type { ArticleListItem, UserArticleQuery } from '../api/article.ts';
import { getSource, type Pagination } from '../modules/Request.ts';

/**
 * 文章列表：articles 为外部数据源（Pagination<T>），query 为查询条件；
 * usePagination 为 true 时使用分页器，否则滚动到底部时通过 change 事件请求下一页并追加数据；
 * withUserCreation 为 true 时展示编辑/删除操作，maxHeight 控制列表容器最大高度。
 */
const props = withDefaults(defineProps<{
    articles: Pagination<ArticleListItem>;
    query: UserArticleQuery;
    usePagination?: boolean;
    loading?: boolean;
    withUserCreation?: boolean;
    maxHeight?: string | number;
}>(), {
    usePagination: true,
    loading: false,
    withUserCreation: false,
    maxHeight: '520px',
});

const emit = defineEmits<{
    (e: 'update:query', query: UserArticleQuery): void;
    (e: 'change', query: UserArticleQuery): void;
    (e: 'article-click', article: ArticleListItem): void;
    (e: 'create'): void;
    (e: 'edit', article: ArticleListItem): void;
    (e: 'delete', article: ArticleListItem): void;
}>();

const scrollRef = ref<HTMLDivElement>();
const reachEndVisible = ref(false);
let reachEndTimer: ReturnType<typeof setTimeout> | undefined;
let lastRequestedPage = 0;

const maxHeightValue = computed(() =>
    typeof props.maxHeight === 'number' ? `${props.maxHeight}px` : props.maxHeight,
);

const hasMore = computed(() => props.articles.data.length < props.articles.total);

const queryKey = computed(() => [
    props.query.userId,
    props.query.isDraft,
    props.query.isDeleted,
    props.query.publishedStatus ?? '',
    props.query.startDate ?? '',
    props.query.endDate ?? '',
    props.query.search ?? '',
].join('|'));

watch(queryKey, () => {
    lastRequestedPage = 0;
    reachEndVisible.value = false;
});

watch(() => props.articles.data.length, async () => {
    await nextTick();
    checkContainerFilled();
});

onBeforeUnmount(() => clearTimeout(reachEndTimer));

function emitQuery(query: UserArticleQuery) {
    emit('update:query', query);
    emit('change', query);
}

function handlePageChange(page: number) {
    scrollToTop();
    emitQuery({ ...props.query, page });
}

function handleScroll() {
    if (props.usePagination) return;
    const el = scrollRef.value;
    if (!el) return;
    if (el.scrollHeight - el.scrollTop - el.clientHeight > 16) return;
    loadNextPage();
}

function loadNextPage() {
    if (props.loading) return;
    if (!hasMore.value) {
        showReachEnd();
        return;
    }
    const nextPage = Math.floor(props.articles.data.length / props.query.pageSize) + 1;
    if (nextPage <= lastRequestedPage) return;
    lastRequestedPage = nextPage;
    emitQuery({ ...props.query, page: nextPage });
}

function checkContainerFilled() {
    if (props.usePagination || props.loading) return;
    const el = scrollRef.value;
    if (!el || el.scrollHeight > el.clientHeight + 1) return;
    loadNextPage();
}

function showReachEnd() {
    reachEndVisible.value = true;
    clearTimeout(reachEndTimer);
    reachEndTimer = setTimeout(() => {
        reachEndVisible.value = false;
    }, 2000);
}

function scrollToTop() {
    scrollRef.value?.scrollTo({ top: 0 });
}

function handleArticleClick(article: ArticleListItem) {
    emit('article-click', article);
}

function handleItemKeydown(event: KeyboardEvent, article: ArticleListItem) {
    if (event.target !== event.currentTarget) return;
    emit('article-click', article);
}

function handleCreate() {
    emit('create');
}

function handleDelete(article: ArticleListItem) {
    emit('delete', article);
}

function formatCount(value?: number) {
    const count = value ?? 0;
    if (count >= 10000) return `${(count / 10000).toFixed(1)}w`;
    if (count >= 1000) return `${(count / 1000).toFixed(1)}k`;
    return `${count}`;
}

defineExpose({ scrollToTop });
</script>

<style scoped>
.article-views {
    display: flex;
    flex-direction: column;
    min-height: 0;
}

.article-scroll {
    flex: 1 1 auto;
    min-height: 0;
    overflow-y: auto;
}

.article-list {
    margin: 0;
    padding: 0;
    list-style: none;
}

.article-item {
    position: relative;
    padding: 20px 4px 18px;
    border-bottom: 1px solid #f2f3f5;
    cursor: pointer;
    transition: background-color 0.2s;
}

.delete-button {
    position: absolute;
    top: 14px;
    right: 4px;
    font-size: 18px;
}

.article-item:last-child {
    border-bottom: 0;
}

.article-item:hover {
    background: #fafbfc;
}

.article-item:focus-visible {
    outline: 2px solid #fc5531;
    outline-offset: -2px;
}

.article-author {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 14px;
    color: #55575c;
}

.author-avatar {
    display: block;
    width: 26px;
    height: 26px;
    border-radius: 50%;
    object-fit: cover;
}

.author-avatar-fallback {
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, #f2a65a, #d97541);
    color: #fff;
    font-size: 13px;
    font-weight: 600;
}

.author-name {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.article-title {
    margin: 10px 0 6px;
    font-size: 21px;
    line-height: 1.4;
    font-weight: 600;
    letter-spacing: normal;
    color: #222226;
    transition: color 0.2s;
}

.article-item:hover .article-title {
    color: #fc5531;
}

.article-abstract {
    margin: 0;
    overflow: hidden;
    font-size: 15px;
    line-height: 1.5;
    color: #8a8f99;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.article-meta {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 24px;
    margin-top: 12px;
    font-size: 14px;
    color: #8a8f99;
}

.article-actions {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-right: auto;
}

.article-actions .item-action {
    margin-left: 0;
    padding: 0;
    font-size: 14px;
}

.meta-item {
    display: inline-flex;
    align-items: center;
    gap: 6px;
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

.article-empty {
    display: flex;
    align-items: center;
    justify-content: center;
    box-sizing: border-box;
    min-height: 320px;
    padding: 24px 0;
}

.create-button {
    --el-button-bg-color: #fc5531;
    --el-button-border-color: #fc5531;
    --el-button-hover-bg-color: #ff7045;
    --el-button-hover-border-color: #ff7045;
    --el-button-active-bg-color: #e64a28;
    --el-button-active-border-color: #e64a28;
    padding: 0 22px;
}

.article-footer {
    flex: none;
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 40px;
    font-size: 14px;
    color: #9aa0aa;
}

.footer-loading {
    display: inline-flex;
    align-items: center;
    gap: 6px;
}

.footer-end {
    padding: 2px 12px;
    border-radius: 999px;
    background: #f2f3f5;
    font-size: 13px;
}

.footer-fade-enter-active,
.footer-fade-leave-active {
    transition: opacity 0.2s;
}

.footer-fade-enter-from,
.footer-fade-leave-to {
    opacity: 0;
}

.article-pager {
    flex: none;
    display: flex;
    justify-content: center;
    padding-top: 16px;
}
</style>
