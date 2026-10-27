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

            <el-dropdown trigger="click" @command="handlePublishCommand">
                <button type="button" class="publish-entry">
                    <span>发布文章</span>
                    <el-icon class="publish-caret" :size="12">
                        <CaretBottom />
                    </el-icon>
                </button>
                <template #dropdown>
                    <el-dropdown-menu>
                        <el-dropdown-item command="publish">发布文章</el-dropdown-item>
                        <el-dropdown-item command="draft">保存草稿</el-dropdown-item>
                        <el-dropdown-item command="preview">预览</el-dropdown-item>
                    </el-dropdown-menu>
                </template>
            </el-dropdown>
        </header>

        <!-- 自定义工具栏先注释掉，改用 Vditor 自带工具栏
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

                            <button v-else type="button" class="tool-button" :aria-label="item.label"
                                @click="handleTool(item.key)">
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
                <el-input v-model="title" class="article-title" maxlength="100" placeholder="请输入文章标题（2～100个字）" />

                <div class="editor-body">
                    <VditorEditor v-model="markdown" :mode="viewMode" :onUpload="uploadSource"
                        placeholder="使用 Markdown 写作，支持标题、列表、表格、代码块、图片、链接等" />
                </div>

                <div class="editor-footer">
                    <div class="footer-controls">
                        <div class="view-switch">
                            <button v-for="item in viewModes" :key="item.value" type="button" class="switch-button"
                                :class="{ 'is-active': viewMode === item.value }" @click="viewMode = item.value">
                                {{ item.label }}
                            </button>
                        </div>
                        <button type="button" class="width-button" @click="isWide = !isWide">
                            {{ isWide ? '标准宽度' : '宽屏模式' }}
                        </button>
                    </div>
                    <span class="editor-hint">Markdown 将转换为 HTML 展示，支持标题、列表、表格、代码块、公式、图表与链接</span>
                </div>
            </div>
        </main>
    </div>
</template>

<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import VditorEditor from '../../components/VditorEditor.vue';

/* 自定义工具栏类型先注释掉
interface ToolMenuItem {
    label: string;
    value: string;
}

interface ToolItem {
    key: string;
    label: string;
    icon?: string;
    glyph?: string;
    menu?: ToolMenuItem[];
}
*/

type ViewMode = 'ir' | 'wysiwyg' | 'sv';

const router = useRouter();
const title = ref('');
const markdown = ref('');
const isWide = ref(false);
const viewMode = ref<ViewMode>('wysiwyg');

const viewModes: { label: string; value: ViewMode }[] = [
    { label: '即时渲染', value: 'ir' },
    { label: '所见即所得', value: 'wysiwyg' },
    { label: '分屏预览', value: 'sv' }
];
onMounted(()=>{
    const timeout = setTimeout(()=>{
        isWide.value = true;
        clearTimeout(timeout);
    },1000);
})

async function uploadSource(file:File) {
    
}
/* 自定义工具栏配置先注释掉
const toolGroups: ToolItem[][] = [
    [
        { key: 'undo', label: '撤销', icon: 'RefreshLeft' },
        { key: 'redo', label: '重做', icon: 'RefreshRight' },
        { key: 'history', label: '历史', icon: 'Clock' }
    ],
    [
        {
            key: 'format', label: '格式', glyph: 'H',
            menu: [
                { label: '标题 1', value: 'h1' },
                { label: '标题 2', value: 'h2' },
                { label: '标题 3', value: 'h3' },
                { label: '标题 4', value: 'h4' },
                { label: '正文', value: 'paragraph' }
            ]
        },
        { key: 'bold', label: '加粗', glyph: 'B' },
        { key: 'color', label: '颜色', glyph: 'A' },
        { key: 'background', label: '背景', icon: 'Brush' },
        {
            key: 'more', label: '其他', icon: 'MoreFilled',
            menu: [
                { label: '插入日期', value: 'date' },
                { label: '插入表情', value: 'emoji' },
                { label: '清除格式', value: 'clear' }
            ]
        }
    ],
    [
        {
            key: 'list', label: '列表',
            menu: [
                { label: '无序列表', value: 'unordered' },
                { label: '有序列表', value: 'ordered' },
                { label: '任务列表', value: 'task' }
            ]
        },
        {
            key: 'align', label: '对齐',
            menu: [
                { label: '左对齐', value: 'left' },
                { label: '居中', value: 'center' },
                { label: '右对齐', value: 'right' },
                { label: '两端对齐', value: 'justify' }
            ]
        },
        { key: 'hline', label: '水平线', icon: 'Minus' }
    ],
    [
        { key: 'quote', label: '块引用', glyph: '“' }
    ],
    [
        {
            key: 'code', label: '代码', glyph: '</>',
            menu: [
                { label: '行内代码', value: 'inline' },
                { label: '代码块', value: 'block' }
            ]
        },
        { key: 'resource', label: '资源绑定' },
        { key: 'table', label: '表格', icon: 'Grid' }
    ],
    [
        { key: 'image', label: '图像', icon: 'Picture' },
        { key: 'video', label: '视频', icon: 'VideoCamera' },
        { key: 'formula', label: '公式', glyph: 'Σ' },
        { key: 'link', label: '链接', icon: 'Link' }
    ],
    [
        { key: 'template', label: '模版', icon: 'CopyDocument' },
        { key: 'toc', label: '目录', icon: 'Memo' },
        { key: 'fullscreen', label: '宽屏', icon: 'FullScreen' }
    ]
];
*/

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

function handlePublishCommand(command: string) {
    if (command === 'preview') {
        viewMode.value = 'sv';
        return;
    }
    console.log(`[article-editor] publish command: ${command}`);
}

/* 自定义工具栏的编辑命令先注释掉
function ensureSourceVisible() {
    if (viewMode.value === 'preview') {
        viewMode.value = 'split';
    }
}

function focusRange(start: number, end: number) {
    requestAnimationFrame(() => {
        const textarea = sourceRef.value;
        if (!textarea) {
            return;
        }
        textarea.focus();
        textarea.setSelectionRange(start, end);
    });
}

function selectionRange(): { start: number; end: number } {
    const source = markdown.value;
    return {
        start: sourceRef.value?.selectionStart ?? source.length,
        end: sourceRef.value?.selectionEnd ?? source.length
    };
}

function replaceSelection(prefix: string, suffix = prefix, placeholder = '') {
    ensureSourceVisible();
    const source = markdown.value;
    const { start, end } = selectionRange();
    const selected = source.slice(start, end) || placeholder;
    markdown.value = `${source.slice(0, start)}${prefix}${selected}${suffix}${source.slice(end)}`;
    focusRange(start + prefix.length, start + prefix.length + selected.length);
}

function insertBlock(block: string) {
    ensureSourceVisible();
    const source = markdown.value;
    const { start } = selectionRange();
    const before = source.slice(0, start);
    const lead = !before ? '' : before.endsWith('\n\n') ? '' : before.endsWith('\n') ? '\n' : '\n\n';
    const inserted = `${before}${lead}${block}\n`;
    markdown.value = `${inserted}${source.slice(start)}`;
    focusRange(inserted.length, inserted.length);
}

function replaceLines(transform: (line: string, index: number) => string) {
    ensureSourceVisible();
    const source = markdown.value;
    const { start, end } = selectionRange();
    const blockStart = start > 0 ? source.lastIndexOf('\n', start - 1) + 1 : 0;
    const nextBreak = source.indexOf('\n', end);
    const blockEnd = nextBreak === -1 ? source.length : nextBreak;
    const nextBlock = source.slice(blockStart, blockEnd).split('\n').map(transform).join('\n');
    markdown.value = `${source.slice(0, blockStart)}${nextBlock}${source.slice(blockEnd)}`;
    focusRange(blockStart, blockStart + nextBlock.length);
}

function applyHeading(level: number) {
    replaceLines(line => {
        const indent = line.match(/^[ \t]+/)?.[0] ?? '';
        const body = line.slice(indent.length).replace(/^#{1,6}\s+/, '');
        if (!body) {
            return line;
        }
        return level === 0 ? `${indent}${body}` : `${indent}${'#'.repeat(level)} ${body}`;
    });
}

function applyList(kind: 'unordered' | 'ordered' | 'task') {
    replaceLines((line, index) => {
        const indent = line.match(/^[ \t]+/)?.[0] ?? '';
        const body = line.slice(indent.length).replace(/^(?:[-*+]|\d+[.)])\s+(?:\[[ xX]\]\s+)?/, '');
        if (!body) {
            return line;
        }
        if (kind === 'ordered') {
            return `${indent}${index + 1}. ${body}`;
        }
        if (kind === 'task') {
            return `${indent}- [ ] ${body}`;
        }
        return `${indent}- ${body}`;
    });
}

function applyQuote() {
    replaceLines(line => {
        if (!line.trim()) {
            return line;
        }
        return line.trimStart().startsWith('>') ? line.replace(/^(\s*)>\s?/, '$1') : line.replace(/^(\s*)/, '$1> ');
    });
}

function insertCodeBlock() {
    ensureSourceVisible();
    const source = markdown.value;
    const { start, end } = selectionRange();
    const before = source.slice(0, start);
    const lead = !before ? '' : before.endsWith('\n') ? '' : '\n';
    const selected = source.slice(start, end);
    const block = `${lead}\`\`\`\n${selected}\n\`\`\`\n`;
    markdown.value = `${before}${block}${source.slice(end)}`;
    const caretStart = start + lead.length + 4;
    focusRange(caretStart, caretStart + selected.length);
}

function insertText(text: string) {
    replaceSelection('', '', text);
}

function clearFormatting() {
    ensureSourceVisible();
    const source = markdown.value;
    const { start, end } = selectionRange();
    if (start === end) {
        return;
    }
    const cleaned = source
        .slice(start, end)
        .replace(/\*\*|__|~~|`/g, '')
        .replace(/^#{1,6}\s+/gm, '')
        .replace(/^\s*>\s?/gm, '');
    markdown.value = `${source.slice(0, start)}${cleaned}${source.slice(end)}`;
    focusRange(start, start + cleaned.length);
}

function handleCommand(tool: string, command: string) {
    switch (tool) {
        case 'format':
            applyHeading(command === 'paragraph' ? 0 : Number(command.slice(1)));
            return;
        case 'list':
            applyList(command as 'unordered' | 'ordered' | 'task');
            return;
        case 'code':
            if (command === 'inline') {
                replaceSelection('`', '`', '代码');
            } else {
                insertCodeBlock();
            }
            return;
        case 'more':
            if (command === 'date') {
                insertText(new Date().toLocaleDateString('zh-CN'));
            } else if (command === 'emoji') {
                insertText('😀');
            } else {
                clearFormatting();
            }
            return;
        default:
            console.log(`[article-editor] ${tool}: ${command}`);
    }
}

function handleTool(tool: string) {
    switch (tool) {
        case 'fullscreen':
            isWide.value = !isWide.value;
            return;
        case 'bold':
            replaceSelection('**', '**', '加粗文字');
            return;
        case 'quote':
            applyQuote();
            return;
        case 'hline':
            insertBlock('---');
            return;
        case 'table':
            insertBlock('| 列 1 | 列 2 |\n| --- | --- |\n| 内容 | 内容 |');
            return;
        case 'link':
            replaceSelection('[', '](https://example.com)', '链接文字');
            return;
        case 'image':
            replaceSelection('![', '](https://example.com/image.png)', '图片描述');
            return;
        default:
            console.log(`[article-editor] tool: ${tool}`);
    }
}
*/
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

/* 自定义工具栏样式先注释掉
.editor-toolbar {
    flex: none;
    overflow-x: auto;
    overflow-y: hidden;
    border-bottom: 1px solid #ebedf0;
    background: #fff;
    scrollbar-width: none;
}

.editor-toolbar::-webkit-scrollbar {
    display: none;
}

.toolbar-inner {
    display: flex;
    align-items: center;
    justify-content: center;
    width: max-content;
    min-width: 100%;
    padding: 6px 24px;
    box-sizing: border-box;
}

.tool-group {
    display: flex;
    align-items: center;
    gap: 2px;
}

.tool-dropdown {
    display: flex;
}

.tool-divider {
    flex: none;
    width: 1px;
    height: 26px;
    margin: 0 10px;
    background: #e5e6eb;
}

.tool-button {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 7px;
    width: 66px;
    height: 62px;
    padding: 0;
    border: 0;
    border-radius: 6px;
    background: transparent;
    font: inherit;
    cursor: pointer;
    transition: background-color 0.2s;
}

.tool-button:hover {
    background: #f2f3f5;
}

.tool-button:active {
    background: #e8eaed;
}

.tool-icon-wrap {
    display: flex;
    align-items: center;
    gap: 1px;
    height: 22px;
    color: #8f959e;
    transition: color 0.2s;
}

.tool-button:hover .tool-icon-wrap {
    color: #1f2329;
}

.tool-caret {
    color: #8f959e;
}

.tool-label {
    font-size: 12px;
    line-height: 1;
    color: #4e5969;
    white-space: nowrap;
}

.tool-glyph {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 20px;
    height: 20px;
    font-size: 19px;
    font-weight: 600;
    line-height: 1;
}

.glyph-color {
    position: relative;
    padding-bottom: 3px;
}

.glyph-color::after {
    content: '';
    position: absolute;
    right: 1px;
    bottom: 0;
    left: 1px;
    height: 2px;
    border-radius: 1px;
    background: currentColor;
}

.glyph-quote {
    font-family: Georgia, 'Times New Roman', serif;
    font-size: 30px;
    font-weight: 400;
    transform: translateY(5px);
}

.glyph-code {
    font-family: Consolas, 'Courier New', monospace;
    font-size: 12px;
    letter-spacing: -0.5px;
}

.glyph-formula {
    font-family: Georgia, 'Times New Roman', serif;
    font-size: 21px;
}

.glyph-list {
    display: flex;
    flex-direction: column;
    gap: 3px;
    width: 18px;
}

.glyph-list i {
    position: relative;
    display: block;
    width: 14px;
    height: 2px;
    margin-left: 4px;
    border-radius: 1px;
    background: currentColor;
}

.glyph-list i::before {
    content: '';
    position: absolute;
    top: 0;
    left: -4px;
    width: 2px;
    height: 2px;
    border-radius: 50%;
    background: currentColor;
}

.glyph-align {
    display: flex;
    flex-direction: column;
    gap: 3px;
    width: 18px;
}

.glyph-align i {
    display: block;
    height: 2px;
    border-radius: 1px;
    background: currentColor;
}

.glyph-align i:nth-child(1),
.glyph-align i:nth-child(3) {
    width: 18px;
}

.glyph-align i:nth-child(2),
.glyph-align i:nth-child(4) {
    width: 11px;
}

.glyph-resource {
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 22px;
    height: 22px;
}

.glyph-plus {
    position: absolute;
    right: -1px;
    bottom: -1px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 11px;
    height: 11px;
    border-radius: 50%;
    background: #fff;
    font-size: 9px;
    font-weight: 700;
    line-height: 1;
}
*/

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

.editor-footer {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-top: 20px;
    padding-top: 14px;
    border-top: 1px solid #f2f3f5;
}

.view-switch {
    display: inline-flex;
    padding: 2px;
    border-radius: 8px;
    background: #f2f3f5;
}

.switch-button {
    padding: 4px 14px;
    border: 0;
    border-radius: 6px;
    background: transparent;
    color: #4e5969;
    font: inherit;
    font-size: 13px;
    cursor: pointer;
    transition: background-color 0.2s, color 0.2s;
}

.switch-button.is-active {
    background: #fff;
    color: #1f2329;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.08);
}

.footer-controls {
    display: inline-flex;
    align-items: center;
    gap: 12px;
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
