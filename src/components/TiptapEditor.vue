<template>
    <div class="tiptap-editor">
        <div class="tiptap-toolbar" role="toolbar" aria-label="文章编辑工具栏">
            <button type="button" class="tt-btn" title="撤销" aria-label="撤销" :disabled="!editor?.can().undo()"
                @click="editor?.chain().focus().undo().run()">
                <el-icon :size="16">
                    <RefreshLeft />
                </el-icon>
            </button>
            <button type="button" class="tt-btn" title="重做" aria-label="重做" :disabled="!editor?.can().redo()"
                @click="editor?.chain().focus().redo().run()">
                <el-icon :size="16">
                    <RefreshRight />
                </el-icon>
            </button>

            <span class="tt-divider" aria-hidden="true"></span>

            <el-dropdown class="tt-dropdown" trigger="click" @command="setHeading">
                <button type="button" class="tt-btn tt-heading" title="段落格式" aria-label="段落格式">
                    <span>{{ headingLabel }}</span>
                    <el-icon :size="10">
                        <ArrowDown />
                    </el-icon>
                </button>
                <template #dropdown>
                    <el-dropdown-menu>
                        <el-dropdown-item :command="0">正文</el-dropdown-item>
                        <el-dropdown-item :command="1">标题 1</el-dropdown-item>
                        <el-dropdown-item :command="2">标题 2</el-dropdown-item>
                        <el-dropdown-item :command="3">标题 3</el-dropdown-item>
                    </el-dropdown-menu>
                </template>
            </el-dropdown>

            <span class="tt-divider" aria-hidden="true"></span>

            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('bold') }" title="加粗"
                aria-label="加粗" @click="editor?.chain().focus().toggleBold().run()">
                <span class="tt-glyph tt-bold">B</span>
            </button>
            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('italic') }" title="斜体"
                aria-label="斜体" @click="editor?.chain().focus().toggleItalic().run()">
                <span class="tt-glyph tt-italic">I</span>
            </button>
            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('underline') }" title="下划线"
                aria-label="下划线" @click="editor?.chain().focus().toggleUnderline().run()">
                <span class="tt-glyph tt-underline">U</span>
            </button>
            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('strike') }" title="删除线"
                aria-label="删除线" @click="editor?.chain().focus().toggleStrike().run()">
                <span class="tt-glyph tt-strike">S</span>
            </button>
            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('code') }" title="行内代码"
                aria-label="行内代码" @click="editor?.chain().focus().toggleCode().run()">
                <span class="tt-glyph tt-code">&lt;/&gt;</span>
            </button>
            <button type="button" class="tt-btn" title="清除格式" aria-label="清除格式"
                @click="editor?.chain().focus().unsetAllMarks().clearNodes().run()">
                <el-icon :size="16">
                    <Brush />
                </el-icon>
            </button>

            <span class="tt-divider" aria-hidden="true"></span>

            <button v-for="item in alignmentButtons" :key="item.value" type="button" class="tt-btn"
                :class="{ 'is-active': isAlign(item.value) }" :disabled="!editor?.isActive('image')"
                :title="item.label" :aria-label="item.label" @click="setAlign(item.value)">
                <svg class="tt-svg" viewBox="0 0 20 16" aria-hidden="true">
                    <path :d="item.path" />
                </svg>
            </button>

            <span class="tt-divider" aria-hidden="true"></span>

            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('bulletList') }"
                title="无序列表" aria-label="无序列表" @click="editor?.chain().focus().toggleBulletList().run()">
                <svg class="tt-svg" viewBox="0 0 20 16" aria-hidden="true">
                    <circle cx="3" cy="3" r="1.3" />
                    <circle cx="3" cy="8" r="1.3" />
                    <circle cx="3" cy="13" r="1.3" />
                    <path d="M7 3h11M7 8h11M7 13h11" />
                </svg>
            </button>
            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('orderedList') }"
                title="有序列表" aria-label="有序列表" @click="editor?.chain().focus().toggleOrderedList().run()">
                <svg class="tt-svg" viewBox="0 0 20 16" aria-hidden="true">
                    <text x="1" y="5.5" font-size="5" fill="currentColor" stroke="none">1</text>
                    <text x="1" y="10.5" font-size="5" fill="currentColor" stroke="none">2</text>
                    <text x="1" y="15.5" font-size="5" fill="currentColor" stroke="none">3</text>
                    <path d="M7 3h11M7 8h11M7 13h11" />
                </svg>
            </button>
            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('taskList') }" title="任务列表"
                aria-label="任务列表" @click="editor?.chain().focus().toggleTaskList().run()">
                <svg class="tt-svg" viewBox="0 0 20 16" aria-hidden="true">
                    <rect x="1.5" y="1.5" width="4" height="4" rx="1" />
                    <rect x="1.5" y="10.5" width="4" height="4" rx="1" />
                    <path d="M8 3.5h10M8 12.5h10" />
                </svg>
            </button>
            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('blockquote') }" title="块引用"
                aria-label="块引用" @click="editor?.chain().focus().toggleBlockquote().run()">
                <span class="tt-glyph tt-quote">“</span>
            </button>
            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('codeBlock') }" title="代码块"
                aria-label="代码块" @click="editor?.chain().focus().toggleCodeBlock().run()">
                <span class="tt-glyph tt-code-block">{ }</span>
            </button>
            <button type="button" class="tt-btn" title="分割线" aria-label="分割线"
                @click="editor?.chain().focus().setHorizontalRule().run()">
                <span class="tt-glyph">—</span>
            </button>

            <span class="tt-divider" aria-hidden="true"></span>

            <button type="button" class="tt-btn" :class="{ 'is-active': editor?.isActive('link') }" title="插入链接"
                aria-label="插入链接" @click="setLink">
                <el-icon :size="16">
                    <LinkIcon />
                </el-icon>
            </button>
            <button type="button" class="tt-btn" :class="{ 'is-loading': uploading }" :disabled="uploading"
                title="上传图片" aria-label="上传图片" @click="pickImages">
                <el-icon :size="16" :class="{ 'is-loading': uploading }">
                    <Picture />
                </el-icon>
            </button>
            <el-dropdown class="tt-dropdown" trigger="click" :disabled="!editor?.isActive('image')"
                @command="handleImageCommand">
                <button type="button" class="tt-btn" title="图片设置" aria-label="图片设置"
                    :disabled="!editor?.isActive('image')">
                    <el-icon :size="16">
                        <Setting />
                    </el-icon>
                </button>
                <template #dropdown>
                    <el-dropdown-menu>
                        <el-dropdown-item command="align-left">左对齐</el-dropdown-item>
                        <el-dropdown-item command="align-center">居中</el-dropdown-item>
                        <el-dropdown-item command="align-right">右对齐</el-dropdown-item>
                        <el-dropdown-item divided command="remove">
                            <span class="tt-danger">删除图片</span>
                        </el-dropdown-item>
                    </el-dropdown-menu>
                </template>
            </el-dropdown>
            <el-dropdown class="tt-dropdown" trigger="click" @command="handleTableCommand">
                <button type="button" class="tt-btn" title="表格" aria-label="表格" :disabled="!editor">
                    <el-icon :size="16">
                        <Grid />
                    </el-icon>
                </button>
                <template #dropdown>
                    <el-dropdown-menu>
                        <el-dropdown-item command="insert">插入表格</el-dropdown-item>
                        <el-dropdown-item divided command="row-after">在下方插入行</el-dropdown-item>
                        <el-dropdown-item command="col-after">在右侧插入列</el-dropdown-item>
                        <el-dropdown-item divided command="delete-row">删除当前行</el-dropdown-item>
                        <el-dropdown-item command="delete-col">删除当前列</el-dropdown-item>
                        <el-dropdown-item command="delete-table">删除表格</el-dropdown-item>
                    </el-dropdown-menu>
                </template>
            </el-dropdown>
        </div>

        <EditorContent v-if="editor" :editor="editor" />

        <div class="tiptap-status" aria-live="polite">{{ characterCount }} 字</div>

        <input ref="fileInput" class="tiptap-file-input" type="file" accept="image/*" multiple
            @change="handleFileChange" />
    </div>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { EditorContent, useEditor } from '@tiptap/vue-3';
import StarterKit from '@tiptap/starter-kit';
import Placeholder from '@tiptap/extension-placeholder';
import { TaskItem, TaskList } from '@tiptap/extension-list';
import { Table, TableCell, TableHeader, TableRow } from '@tiptap/extension-table';
import { Markdown } from '@tiptap/markdown';
import { ElMessage, ElMessageBox } from 'element-plus';
import { ArrowDown, Brush, Grid, Link as LinkIcon, Picture, RefreshLeft, RefreshRight, Setting } from '@element-plus/icons-vue';
import { normalizeAlign, ResizableImage, type ImageAlign } from './tiptap/ResizableImage';
import './tiptap/tiptap-content.css';

interface TiptapEditorProps {
    modelValue?: string;
    placeholder?: string;
    minHeight?: number;
    /** 上传图片并返回可访问的图片地址 */
    onUpload?: (file: File) => Promise<string>;
}

const props = withDefaults(defineProps<TiptapEditorProps>(), {
    modelValue: '',
    placeholder: '开始创作……',
    minHeight: 520,
});

const emit = defineEmits<{ 'update:modelValue': [value: string] }>();

const fileInput = ref<HTMLInputElement | null>(null);
const uploading = ref(false);

const alignmentButtons: { value: ImageAlign; label: string; path: string }[] = [
    { value: 'left', label: '图片左对齐', path: 'M2 2h16M2 6h10M2 10h16M2 14h10' },
    { value: 'center', label: '图片居中', path: 'M2 2h16M5 6h10M2 10h16M5 14h10' },
    { value: 'right', label: '图片右对齐', path: 'M2 2h16M8 6h10M2 10h16M8 14h10' },
];

const editor = useEditor({
    content: props.modelValue,
    contentType: 'html',
    extensions: [
        StarterKit.configure({
            link: {
                openOnClick: false,
                autolink: true,
                HTMLAttributes: { rel: 'noopener noreferrer', target: '_blank' },
            },
        }),
        Placeholder.configure({ placeholder: props.placeholder }),
        TaskList,
        TaskItem.configure({ nested: true }),
        Table.configure({ resizable: false }),
        TableRow,
        TableHeader,
        TableCell,
        ResizableImage.configure({
            resize: {
                enabled: true,
                directions: ['top-left', 'top-right', 'bottom-left', 'bottom-right'],
                minWidth: 80,
                minHeight: 40,
                alwaysPreserveAspectRatio: true,
            },
        }),
        Markdown.configure({ indentation: { style: 'space', size: 2 } }),
    ],
    editorProps: {
        attributes: {
            class: 'tiptap-content',
            style: `min-height: ${props.minHeight}px`,
        },
        handlePaste: (_view, event) => {
            const files = imageFilesFrom(event.clipboardData);
            if (!files.length) {
                return false;
            }
            event.preventDefault();
            void insertFiles(files);
            return true;
        },
        handleDrop: (_view, event, _slice, moved) => {
            if (moved) {
                return false;
            }
            const files = imageFilesFrom(event.dataTransfer);
            if (!files.length) {
                return false;
            }
            event.preventDefault();
            void insertFiles(files);
            return true;
        },
    },
    onUpdate: ({ editor: instance }) => {
        const html = instance.getHTML();
        if (html !== props.modelValue) {
            emit('update:modelValue', html);
        }
    },
});

const headingLabel = computed(() => {
    const instance = editor.value;
    if (!instance) {
        return '正文';
    }
    for (const level of [1, 2, 3, 4, 5, 6] as const) {
        if (instance.isActive('heading', { level })) {
            return `标题 ${level}`;
        }
    }
    return '正文';
});

const characterCount = computed(() => {
    const text = editor.value?.getText() ?? '';
    return text.replace(/\s+/g, '').length;
});

watch(
    () => props.modelValue,
    value => {
        const instance = editor.value;
        if (!instance) {
            return;
        }
        const next = value ?? '';
        if (next === instance.getHTML()) {
            return;
        }
        instance.commands.setContent(next, { contentType: 'html', emitUpdate: false });
    },
);

function setHeading(level: number) {
    const instance = editor.value;
    if (!instance) {
        return;
    }
    if (level === 0) {
        instance.chain().focus().setParagraph().run();
        return;
    }
    instance.chain().focus().toggleHeading({ level: level as 1 | 2 | 3 }).run();
}

function isAlign(align: ImageAlign) {
    const instance = editor.value;
    if (!instance || !instance.isActive('image')) {
        return false;
    }
    return normalizeAlign(instance.getAttributes('image').align) === align;
}

function setAlign(align: ImageAlign) {
    const instance = editor.value;
    if (!instance || !instance.isActive('image')) {
        return;
    }
    instance.chain().focus().updateAttributes('image', { align }).run();
}

async function setLink() {
    const instance = editor.value;
    if (!instance) {
        return;
    }
    const previous = (instance.getAttributes('link').href as string | undefined) ?? '';
    try {
        const { value } = await ElMessageBox.prompt('请输入链接地址，留空可移除链接', '插入链接', {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            inputValue: previous,
            inputPlaceholder: 'https://example.com',
            inputValidator: () => true,
        });
        const href = (value ?? '').trim();
        if (!href) {
            instance.chain().focus().extendMarkRange('link').unsetLink().run();
            return;
        }
        instance.chain().focus().extendMarkRange('link').setLink({ href }).run();
    } catch {
        // 用户取消
    }
}

function pickImages() {
    fileInput.value?.click();
}

async function handleFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';
    if (files.length) {
        await insertFiles(files);
    }
}

function imageFilesFrom(data: DataTransfer | null): File[] {
    if (!data) {
        return [];
    }
    return Array.from(data.files ?? []).filter(file => file.type.startsWith('image/'));
}

async function insertFiles(files: File[]) {
    const images = files.filter(file => file.type.startsWith('image/'));
    if (!images.length) {
        return;
    }
    const upload = props.onUpload;
    if (!upload) {
        await insertImageByUrl();
        return;
    }
    uploading.value = true;
    try {
        for (const file of images) {
            const url = await upload(file);
            if (url) {
                insertImage(url, file.name);
            }
        }
    } catch (error) {
        ElMessage.error(error instanceof Error ? error.message : '图片上传失败');
    } finally {
        uploading.value = false;
    }
}

async function insertImageByUrl() {
    try {
        const { value } = await ElMessageBox.prompt('请输入图片地址', '插入图片', {
            confirmButtonText: '插入',
            cancelButtonText: '取消',
            inputPlaceholder: 'https://example.com/image.png',
            inputValidator: input => (input?.trim() ? true : '请输入图片地址'),
        });
        if (value?.trim()) {
            insertImage(value.trim());
        }
    } catch {
        // 用户取消
    }
}

function insertImage(src: string, alt?: string) {
    editor.value?.chain().focus().insertContent({
        type: 'image',
        attrs: { src, alt: alt ?? null, align: 'center' },
    }).run();
}

function handleImageCommand(command: string) {
    const instance = editor.value;
    if (!instance || !instance.isActive('image')) {
        return;
    }
    switch (command) {
        case 'align-left':
            setAlign('left');
            break;
        case 'align-center':
            setAlign('center');
            break;
        case 'align-right':
            setAlign('right');
            break;
        case 'remove':
            instance.chain().focus().deleteSelection().run();
            break;
    }
}

function handleTableCommand(command: string) {
    const instance = editor.value;
    if (!instance) {
        return;
    }
    const chain = instance.chain().focus();
    switch (command) {
        case 'insert':
            chain.insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run();
            break;
        case 'row-after':
            chain.addRowAfter().run();
            break;
        case 'col-after':
            chain.addColumnAfter().run();
            break;
        case 'delete-row':
            chain.deleteRow().run();
            break;
        case 'delete-col':
            chain.deleteColumn().run();
            break;
        case 'delete-table':
            chain.deleteTable().run();
            break;
    }
}

defineExpose({
    getHTML: () => editor.value?.getHTML() ?? props.modelValue ?? '',
    getMarkdown: () => editor.value?.getMarkdown() ?? props.modelValue ?? '',
    isEmpty: () => editor.value?.isEmpty ?? true,
    focus: () => editor.value?.commands.focus(),
});
</script>

<style scoped>
.tiptap-editor {
    min-width: 0;
    padding: 14px 16px 16px;
    border: 1px solid #e5e6eb;
    border-radius: 10px;
    background: #fff;
    text-align: left;
    transition: border-color 0.2s, box-shadow 0.2s;
}

.tiptap-editor:focus-within {
    border-color: #c9cdd4;
    box-shadow: 0 0 0 3px rgba(98, 106, 239, 0.08);
}

.tiptap-toolbar {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 2px;
    margin-bottom: 12px;
    padding: 0 0 8px;
    border-bottom: 1px solid #f2f3f5;
}

.tt-btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 2px;
    height: 30px;
    min-width: 30px;
    padding: 0 6px;
    border: 0;
    border-radius: 6px;
    background: transparent;
    color: #8f959e;
    font: inherit;
    font-size: 13px;
    cursor: pointer;
    transition: background-color 0.15s, color 0.15s;
}

.tt-btn:hover:not(:disabled) {
    background: #f2f3f5;
    color: #1f2329;
}

.tt-btn:active:not(:disabled) {
    background: #e8eaed;
}

.tt-btn.is-active {
    background: #eaf3ff;
    color: #1f6feb;
}

.tt-btn:disabled {
    opacity: 0.4;
    cursor: not-allowed;
}

.tt-btn.is-loading {
    cursor: progress;
}

.tt-heading {
    min-width: 76px;
    justify-content: space-between;
    color: #4e5969;
}

.tt-dropdown {
    display: inline-flex;
}

.tt-divider {
    flex: none;
    width: 1px;
    height: 18px;
    margin: 0 6px;
    background: #e5e6eb;
}

.tt-glyph {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 18px;
    height: 18px;
    font-size: 15px;
    font-weight: 600;
    line-height: 1;
}

.tt-bold {
    font-family: Georgia, 'Times New Roman', serif;
}

.tt-italic {
    font-family: Georgia, 'Times New Roman', serif;
    font-style: italic;
}

.tt-underline {
    font-family: Georgia, 'Times New Roman', serif;
    text-decoration: underline;
    text-underline-offset: 2px;
}

.tt-strike {
    font-family: Georgia, 'Times New Roman', serif;
    text-decoration: line-through;
}

.tt-code,
.tt-code-block {
    font-family: Consolas, 'Courier New', monospace;
    font-size: 11px;
    letter-spacing: -0.5px;
}

.tt-quote {
    font-family: Georgia, 'Times New Roman', serif;
    font-size: 24px;
    font-weight: 400;
    transform: translateY(5px);
}

.tt-svg {
    width: 18px;
    height: 16px;
    fill: none;
    stroke: currentColor;
    stroke-width: 1.5;
    stroke-linecap: round;
    stroke-linejoin: round;
}

.tt-danger {
    color: #f56c6c;
}

.tiptap-status {
    margin-top: 6px;
    color: #b8bec8;
    font-size: 12px;
    line-height: 1;
    text-align: right;
}

.tiptap-file-input {
    display: none;
}
</style>
