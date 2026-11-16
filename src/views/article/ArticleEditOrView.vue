<template>
    <div class="editor-page" :class="{ 'is-wide': isWide }">
        <header class="editor-header">
            <a class="brand" href="/index" aria-label="返回首页" @click.prevent="goHome">
                <img class="brand-logo" src="/blog.jpg" alt="博客 Logo" />
            </a>

            <el-button class="back-button" text aria-label="返回" @click="goBack">
                <svg class="back-icon" viewBox="0 0 24 24" aria-hidden="true">
                    <path d="M15 5l-7 7 7 7" />
                </svg>
            </el-button>

            <button type="button" class="publish-entry">
                <span>发布文章</span>

            </button>
        </header>

        <!-- 自定义工具栏先注释掉，改用编辑器自带工具栏
        <nav class="editor-toolbar" aria-label="文章编辑工具栏">
            <div class="toolbar-inner">
                <template v-for="(group, groupIndex) in toolGroups" :key="groupIndex">
                    <div class="tool-group">
                        <template v-for="item in group" :key="item.key">
                            <el-dropdown v-if="item.menu" class="tool-dropdown" trigger="click"
                                @command="(command: string) => handleCommand(item.key, command)">
                                <button type="button" class="tool-button" :aria-label="item.label">
                                    <span class="tool-icon-wrap">
                                        <span v-if="item.key === 'list'" class="glyph-list">
                                            <i></i><i></i><i></i>
                                        </span>
                                        <span v-else-if="item.key === 'align'" class="glyph-align">
                                            <i></i><i></i><i></i><i></i>
                                        </span>
                                        <span v-else-if="item.key === 'resource'" class="glyph-resource">
                                            <el-icon :size="20">
                                                <Picture />
                                            </el-icon>
                                            <span class="glyph-plus">+</span>
                                        </span>
                                        <el-icon v-else-if="item.icon" :size="20">
                                            <component :is="item.icon" />
                                        </el-icon>
                                        <span v-else class="tool-glyph" :class="`glyph-${item.key}`">{{ item.glyph
                                        }}</span>
                                        <el-icon class="tool-caret" :size="10">
                                            <CaretBottom />
                                        </el-icon>
                                    </span>
                                    <span class="tool-label">{{ item.label }}</span>
                                </button>
                                <template #dropdown>
                                    <el-dropdown-menu>
                                        <el-dropdown-item v-for="option in item.menu" :key="option.value"
                                            :command="option.value">
                                            {{ option.label }}
                                        </el-dropdown-item>
                                    </el-dropdown-menu>
                                </template>
</el-dropdown>

<button v-else type="button" class="tool-button" :aria-label="item.label" @click="handleTool(item.key)">
    <span class="tool-icon-wrap">
        <span v-if="item.key === 'list'" class="glyph-list">
            <i></i><i></i><i></i>
        </span>
        <span v-else-if="item.key === 'align'" class="glyph-align">
            <i></i><i></i><i></i><i></i>
        </span>
        <span v-else-if="item.key === 'resource'" class="glyph-resource">
            <el-icon :size="20">
                <Picture />
            </el-icon>
            <span class="glyph-plus">+</span>
        </span>
        <el-icon v-else-if="item.icon" :size="20">
            <component :is="item.icon" />
        </el-icon>
        <span v-else class="tool-glyph" :class="`glyph-${item.key}`">{{ item.glyph
            }}</span>
    </span>
    <span class="tool-label">{{ item.label }}</span>
</button>
</template>
</div>

<span v-if="groupIndex < toolGroups.length - 1" class="tool-divider" aria-hidden="true"></span>
</template>
</div>
</nav>
-->

        <main class="editor-main">
            <div class="editor-paper">
                <el-input v-model="articleContent.title" class="article-title" maxlength="100"
                    placeholder="请输入文章标题（2～100个字）" />

                <div class="editor-body">
                    <div class="editor-surface">
                        <TiptapEditor v-if="editorMode === 'rich'" ref="tiptapEditorRef"
                            v-model="articleContent.articleTrueContent" :onUpload="uploadSource" placeholder="创作" />
                        <ArticleMarkdownEditorView v-else ref="markdownEditorRef"
                            v-model="articleContent.articleTrueContent" :onUpload="uploadSource" placeholder="创作" />
                    </div>
                </div>

                <div class="editor-footer">
                    <div class="footer-controls">
                        <button type="button" class="mode-button" @click="toggleEditorMode">
                            <el-icon class="mode-icon" :size="15">
                                <Document v-if="editorMode === 'rich'" />
                                <EditPen v-else />
                            </el-icon>
                            {{ editorMode === 'rich' ? 'Markdown 模式' : '富文本模式' }}
                        </button>
                        <button type="button" class="width-button" @click="isWide = !isWide">
                            {{ isWide ? '标准宽度' : '宽屏模式' }}
                        </button>
                        <el-button-group>
                            <el-button type="primary" @click="saveArticle(articleContent, false)">
                                保存
                            </el-button>
                            <el-button type="info" @click="openPreview">
                                预览
                            </el-button>
                            <el-button color="#626aef" @click="publishArticle(articleContent)">
                                发布
                            </el-button>
                        </el-button-group>
                    </div>
                </div>
            </div>
        </main>

        <el-dialog v-model="previewVisible" class="preview-dialog" title="文章预览" width="min(900px, 92vw)" top="6vh"
            append-to-body @opened="renderPreview">
            <div class="preview-scroll">
                <h1 v-if="articleContent.title" class="preview-title">{{ articleContent.title }}</h1>
                <div ref="previewRef" class="preview-body"
                    :class="{ 'tiptap-content': editorMode === 'rich' }"></div>
            </div>
            <template #footer>
                <div class="preview-actions">
                    <el-button @click="previewVisible = false">取消</el-button>
                    <el-button type="primary" @click="saveAndClosePreview">保存</el-button>
                </div>
            </template>
        </el-dialog>
    </div>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { onBeforeRouteLeave, useRoute, useRouter } from 'vue-router';
import TiptapEditor from '../../components/TiptapEditor.vue';
import ArticleMarkdownEditorView from './ArticleMarkdownEditorView.vue';
import '../../components/tiptap/tiptap-content.css';
import { Document, EditPen } from '@element-plus/icons-vue';
import { UploadFile } from '../../api/artcileSource.ts';
import { ElMessage, ElMessageBox, ElNotification } from 'element-plus';
import { type ArticleSaveModel, GetArticleContent, SaveArticle } from '../../api/article.ts';
import stateStroge from '../../modules/StateStorage.ts';
import { type User } from '../../api/user.ts';
import { loading } from '../../modules/Utils.ts';
import { getSource } from '../../modules/Request.ts';
type EditorMode = 'rich' | 'markdown';

interface RichTextEditorInstance {
    getHTML: () => string;
    getMarkdown: () => string;
    isEmpty: () => boolean;
}

interface MarkdownEditorInstance {
    getValue: () => string;
    getHTML: () => string;
    renderPreview: (element: HTMLDivElement) => Promise<void>;
}

const route = useRoute();
const router = useRouter();
const isWide = ref(false);
const editorMode = ref<EditorMode>('rich');
const previewVisible = ref(false);
const previewRef = ref<HTMLDivElement | null>(null);
const tiptapEditorRef = ref<RichTextEditorInstance | null>(null);
const markdownEditorRef = ref<MarkdownEditorInstance | null>(null);
const articleId = ref('');
const articleContent = reactive<ArticleSaveModel>({
    userId: 0,
    isDraft: true,
    articleOriginalContent: ''
});

// 记录最近一次保存/加载时的内容快照，用于判断是否存在未保存的修改
const savedTitle = ref('');
const savedContent = ref('');
const isDirty = computed(() =>
    (articleContent.title ?? '') !== savedTitle.value
    || (articleContent.articleTrueContent ?? '') !== savedContent.value
);

function markSaved() {
    savedTitle.value = articleContent.title ?? '';
    savedContent.value = articleContent.articleTrueContent ?? '';
}

onMounted(() => {
    const user = stateStroge.get<User>("User");
    const articleIdVal = route.params.articleId as string
        || sessionStorage.getItem("CurrentArticleId") as string;
    const timeout = setTimeout(() => {
        isWide.value = true;
        clearTimeout(timeout);
    }, 1000);
    if (articleIdVal)
        articleId.value = articleIdVal;
    if (!articleId.value.length) {
        articleContent.userId = user.userId;
        saveArticle(articleContent, true);
    }
    else
        getArticleContent();
    window.addEventListener('beforeunload', handleBeforeUnload);
})

onBeforeUnmount(() => {
    window.removeEventListener('beforeunload', handleBeforeUnload);
});

// 刷新/关闭浏览器标签页时由浏览器弹出原生确认框
function handleBeforeUnload(event?: BeforeUnloadEvent) {
    if (!isDirty.value) {
        return;
    }
    event?.preventDefault();
}

// 路由离开（返回、首页等）前确认是否保存未提交的内容
onBeforeRouteLeave(async (to, from) => {
    if (to.fullPath != from.fullPath)
        sessionStorage.removeItem("CurrentArticleId")
    handleBeforeUnload(undefined);
    if (!isDirty.value) {
        return true;
    }
    try {
        await ElMessageBox.confirm('当前文章有未保存的修改，是否保存后离开？', '未保存的修改', {
            confirmButtonText: '保存',
            cancelButtonText: '不保存',
            distinguishCancelAndClose: true,
            closeOnClickModal: false,
            closeOnPressEscape: false,
            type: 'warning'
        });
    } catch (action) {
        // 只有明确选择“不保存”才放行，关闭弹窗视为取消离开
        return action === 'cancel';
    }
    return saveArticle(articleContent, false);
});

function syncEditorContent() {
    if (editorMode.value === 'rich' && tiptapEditorRef.value) {
        articleContent.articleTrueContent = tiptapEditorRef.value.getMarkdown();
        return;
    }
    if (editorMode.value === 'markdown' && markdownEditorRef.value) {
        articleContent.articleTrueContent = markdownEditorRef.value.getValue();
    }
}

function toggleEditorMode() {
    editorMode.value = editorMode.value === 'rich' ? 'markdown' : 'rich';
}

function saveArticle(model: ArticleSaveModel, init: boolean): Promise<boolean> {
    if (!init) {
        syncEditorContent();
        if (!model.articleTrueContent?.trim().length) {
            ElMessage.warning("请输入内容");
            return Promise.resolve(false);
        }
        if(!model.title?.trim().length)
        {
            ElMessage.warning("请输入标题");
            return Promise.resolve(false);
        }
    }
    // 非首次保存时携带上次保存的原文，服务端据此比对并删改内嵌资源
    model.articleOriginalContent = init ? '' : savedContent.value;
    return SaveArticle(model)
        .then(res => {
            if (res.success) {
                const successMsg = init ? '已保存为\"未命名\"' : "已保存";
                ElMessage.success(successMsg);
                if (!articleId.value.length) {
                    articleId.value = res.data.articleId;
                    articleContent.articleId = res.data.articleId;
                    sessionStorage.setItem("CurrentArticleId", articleId.value);
                }
                if (!articleContent?.content)
                    articleContent.content = res.data.content;
                markSaved();
                // 保存成功后更新原文基线，下一次删改才能正确比对
                articleContent.articleOriginalContent = model.articleTrueContent ?? '';
                return true;
            }
            if (res.code == 411) {
                const timeout = setTimeout(() => {
                    ElMessage.warning("文章草稿已满五份，请先删除不必要的草稿");
                    clearTimeout(timeout);
                }, 2000);
            }
            ElMessage.error(res.message);
            return false;
        })
        .catch(error => {
            ElNotification.error(error);
            return false;
        });
}

function openPreview() {
    syncEditorContent();
    if (!articleContent.articleTrueContent?.trim().length) {
        ElMessage.warning("请输入内容");
        return;
    }
    previewVisible.value = true;
}

async function renderPreview() {
    if (!previewRef.value) {
        return;
    }
    previewRef.value.innerHTML = '';
    if (editorMode.value === 'rich') {
        previewRef.value.innerHTML = tiptapEditorRef.value?.getHTML() ?? '';
        return;
    }
    await markdownEditorRef.value?.renderPreview(previewRef.value);
}

function saveAndClosePreview() {
    saveArticle(articleContent, false);
    previewVisible.value = false;
}

function getArticleContent() {
    const loadingInstance = loading({ message: "内容加载中...", duration: 0 });
    GetArticleContent(articleId.value)
        .then(res => {
            if (res.success) {
                Object.assign(articleContent, res.data);
                articleContent.articleTrueContent = res.data.articleTrueContent || '';
                articleContent.articleOriginalContent = res.data.articleOriginalContent ?? articleContent.articleTrueContent;
                markSaved();
            }
            else
                ElMessage.error(res.message);
        })
        .catch(error => ElNotification.error(error))
        .finally(() => loadingInstance.close());
}

function uploadSource(file: File) {
    if (!articleId.value || !articleContent.userId) {
        ElMessage.warning('文章正在初始化，请稍后再上传图片');
        return Promise.resolve('');
    }
    return new Promise<string>((resolve, reject) => {
        UploadFile(file, articleId.value)
            .then(res => {
                if (!res.success) {
                    ElMessage.error(res.message);
                    resolve('');
                }
                else
                    resolve(getSource(res.data, `Source/${articleContent.userId}`));
            }).catch(error => reject(error))
    });
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

function publishArticle(model: ArticleSaveModel) {
    if (!model.articleTrueContent?.trim().length) {
        ElMessage.warning("请输入内容");
        return;
    }
    model.isDraft = false;
}
</script>

<style scoped>
.editor-page {
    height: 100%;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    background: #fff;
    color: #1f2329;
    text-align: left;
}

.editor-header {
    flex: none;
    display: flex;
    align-items: center;
    gap: 16px;
    height: 52px;
    padding: 0 30px;
    border-bottom: 1px solid #f2f3f5;
    box-sizing: border-box;
}

.brand {
    display: flex;
    align-items: center;
}

.brand-logo {
    display: block;
    width: 36px;
    height: 36px;
    border-radius: 8px;
    object-fit: cover;
}

.back-button {
    width: 30px;
    height: 30px;
    padding: 0;
    color: #4e5969;
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

.back-button:hover {
    color: #1f2329;
    background: #f2f3f5;
}

.publish-entry {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 4px 6px;
    border: 0;
    border-radius: 6px;
    background: transparent;
    color: #1f2329;
    font: inherit;
    font-size: 18px;
    cursor: pointer;
    transition: background-color 0.2s;
}

.publish-entry:hover {
    background: #f2f3f5;
}

.publish-caret {
    color: #4e5969;
}

.editor-main {
    flex: 1;
    overflow-y: auto;
    background: #fff;
}

.editor-paper {
    width: min(860px, calc(100% - 48px));
    margin: 28px auto 80px;
}

.is-wide .editor-paper {
    width: min(1200px, calc(100% - 48px));
}

.article-title :deep(.el-input__wrapper) {
    padding: 0;
    box-shadow: none;
}

.article-title :deep(.el-input__wrapper.is-focus) {
    box-shadow: none;
}

.article-title :deep(.el-input__inner) {
    height: 54px;
    font-size: 30px;
    font-weight: 600;
    color: #1f2329;
}

.article-title :deep(.el-input__inner::placeholder) {
    color: #b8bec8;
}

.editor-body {
    margin-top: 12px;
    min-height: calc(100vh - 320px);
}

.editor-surface {
    min-width: 0;
}

.editor-footer {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-top: 0;
    padding: 10px 0 4px;
}

.footer-controls {
    display: inline-flex;
    align-items: center;
    gap: 12px;
}

.mode-button {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 4px 12px;
    border: 1px solid #e5e6eb;
    border-radius: 6px;
    background: #fff;
    color: #4e5969;
    font: inherit;
    font-size: 13px;
    cursor: pointer;
    transition: border-color 0.2s, color 0.2s;
}

.mode-button:hover {
    border-color: #c9cdd4;
    color: #1f2329;
}

.mode-icon {
    color: currentColor;
}

.width-button {
    padding: 4px 12px;
    border: 1px solid #e5e6eb;
    border-radius: 6px;
    background: #fff;
    color: #4e5969;
    font: inherit;
    font-size: 13px;
    cursor: pointer;
    transition: border-color 0.2s, color 0.2s;
}

.width-button:hover {
    border-color: #c9cdd4;
    color: #1f2329;
}

.editor-hint {
    margin: 0;
    font-size: 12px;
    color: #8f959e;
}

/* 预览对话框 */
.preview-scroll {
    max-height: 62vh;
    overflow-y: auto;
    padding: 0 4px;
}

.preview-title {
    margin: 0 0 16px;
    font-size: 26px;
    font-weight: 600;
    line-height: 1.4;
    color: #1f2329;
}

.preview-body {
    font-size: 15px;
    line-height: 1.9;
    color: #333;
}

.preview-body :deep(img) {
    max-width: 100%;
}

.preview-actions {
    display: flex;
    justify-content: flex-end;
    gap: 8px;
}

.preview-actions :deep(.el-button + .el-button) {
    margin-left: 0;
}

@media (max-width: 768px) {
    .editor-header {
        gap: 8px;
        padding: 0 16px;
    }

    .brand-logo {
        width: 32px;
        height: 32px;
    }

    .publish-entry {
        font-size: 16px;
    }

    .editor-paper {
        width: calc(100% - 32px);
        margin-top: 20px;
    }

    .editor-footer {
        align-items: flex-start;
        flex-direction: column;
    }

    .article-title :deep(.el-input__inner) {
        font-size: 24px;
    }
}
</style>
