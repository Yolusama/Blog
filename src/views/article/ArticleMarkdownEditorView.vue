<template>
    <div class="article-markdown-editor">
        <div class="markdown-toolbar">
            <div class="view-switch" role="group" aria-label="Markdown 编辑模式">
                <button v-for="item in viewModes" :key="item.value" type="button" class="switch-button"
                    :class="{ 'is-active': viewMode === item.value }" @click="viewMode = item.value">
                    {{ item.label }}
                </button>
            </div>
            <span class="mode-hint">Markdown</span>
        </div>

        <VditorEditor ref="vditorRef" v-model="content" :mode="viewMode" :onUpload="onUpload"
            :placeholder="placeholder" :minHeight="minHeight" />
    </div>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import Vditor from 'vditor';
import 'vditor/dist/index.css';
import VditorEditor from '../../components/VditorEditor.vue';

type MarkdownViewMode = 'ir' | 'wysiwyg' | 'sv';

interface ArticleMarkdownEditorProps {
    modelValue?: string;
    placeholder?: string;
    minHeight?: number;
    /** 上传图片并返回可访问的图片地址 */
    onUpload?: (file: File) => Promise<string>;
}

interface VditorEditorInstance {
    getValue: () => string;
    getHTML: () => string;
}

const props = withDefaults(defineProps<ArticleMarkdownEditorProps>(), {
    modelValue: '',
    placeholder: '开始创作……',
    minHeight: 520,
});

const emit = defineEmits<{ 'update:modelValue': [value: string] }>();

const viewMode = ref<MarkdownViewMode>('wysiwyg');
const vditorRef = ref<VditorEditorInstance | null>(null);

const content = computed({
    get: () => props.modelValue ?? '',
    set: value => emit('update:modelValue', value),
});

const viewModes: { label: string; value: MarkdownViewMode }[] = [
    { label: '即时渲染', value: 'ir' },
    { label: '所见即所得', value: 'wysiwyg' },
    { label: '分屏预览', value: 'sv' },
];

const cdn = `${import.meta.env.BASE_URL.replace(/\/$/, '')}/vditor`;

function getValue() {
    return vditorRef.value?.getValue() ?? props.modelValue ?? '';
}

function getHTML() {
    return vditorRef.value?.getHTML() ?? '';
}

async function renderPreview(element: HTMLDivElement) {
    element.innerHTML = '';
    await Vditor.preview(element, getValue(), {
        mode: 'light',
        cdn,
        lang: 'zh_CN',
        hljs: { enable: true, style: 'github', lineNumber: true },
        math: { engine: 'KaTeX' },
        markdown: { toc: true },
    });
}

defineExpose({ getValue, getHTML, renderPreview });
</script>

<style scoped>
.article-markdown-editor {
    min-width: 0;
    padding: 14px 16px 16px;
    border: 1px solid #e5e6eb;
    border-radius: 10px;
    background: #fff;
    text-align: left;
    transition: border-color 0.2s, box-shadow 0.2s;
}

.article-markdown-editor:focus-within {
    border-color: #c9cdd4;
    box-shadow: 0 0 0 3px rgba(98, 106, 239, 0.08);
}

.markdown-toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-bottom: 12px;
    padding: 0 0 8px;
    border-bottom: 1px solid #f2f3f5;
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

.switch-button:hover {
    color: #1f2329;
}

.switch-button.is-active {
    background: #fff;
    color: #1f2329;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.08);
}

.mode-hint {
    color: #8f959e;
    font-size: 12px;
}
</style>
