<template>
    <div class="vditor-editor">
        <div :key="hostKey" ref="hostRef" class="vditor-host"></div>
    </div>
</template>

<script lang="ts" setup>
import { nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import Vditor from 'vditor';
import 'vditor/dist/index.css';

interface VditorProps {
    /** HTML 内容，Vditor 内部会转成 Markdown 再编辑 */
    modelValue?: string;
    mode?: VditorMode;
    placeholder?: string;
    minHeight?: number;
    onUpload?: (file: File) => Promise<string>
}

type VditorMode = 'ir' | 'wysiwyg' | 'sv';

interface LuteInstance {
    HTML2Md(html: string): string;
}

interface LuteConstructor {
    New(): LuteInstance;
}

type LuteGlobal = Window & { Lute?: LuteConstructor };

const props = withDefaults(
    defineProps<VditorProps>(),
    { modelValue: '', mode: 'ir', placeholder: '', minHeight: 520 }
);

const emit = defineEmits<{ 'update:modelValue': [value: string] }>();

const cdn = `${import.meta.env.BASE_URL.replace(/\/$/, '')}/vditor`;
const LUTE_SCRIPT_ID = 'vditorLuteScript';
const hostRef = ref<HTMLDivElement | null>(null);
const hostKey = ref(0);
let vditor: Vditor | null = null;
let createToken = 0;
let unmounted = false;
let lutePromise: Promise<LuteConstructor> | null = null;

// Vditor 只在创建完成后才异步加载 Lute，提前加载后才能在创建前完成 HTML → Markdown 转译
function loadLute(): Promise<LuteConstructor> {
    const loaded = (window as LuteGlobal).Lute;
    if (loaded) {
        return Promise.resolve(loaded);
    }
    if (lutePromise) {
        return lutePromise;
    }
    lutePromise = new Promise<LuteConstructor>((resolve, reject) => {
        const script = document.createElement('script');
        script.src = `${cdn}/dist/js/lute/lute.min.js`;
        script.async = true;
        script.onload = () => {
            script.id = LUTE_SCRIPT_ID;
            const lute = (window as LuteGlobal).Lute;
            if (lute) {
                resolve(lute);
                return;
            }
            reject(new Error('Lute 加载失败'));
        };
        script.onerror = () => reject(new Error('Lute 加载失败'));
        document.head.appendChild(script);
    }).catch(error => {
        lutePromise = null;
        throw error;
    });
    return lutePromise;
}

async function htmlToMarkdown(html: string): Promise<string> {
    if (!html.trim()) {
        return '';
    }
    // 历史草稿已经是 Markdown，直接复用，避免再次转译带来的格式损失
    if (!/^\s*<(?!\/?(?:https?:|mailto:))/i.test(html)) {
        return html;
    }
    try {
        const lute = await loadLute();
        return lute.New().HTML2Md(html);
    } catch {
        // Markdown 本身允许内嵌 HTML，转译失败时退回原文
        return html;
    }
}

function getCurrentHTML(): string {
    return vditor?.getHTML() ?? props.modelValue ?? '';
}

async function createEditor(html: string) {
    if (!hostRef.value) {
        return;
    }
    const token = ++createToken;
    const value = await htmlToMarkdown(html);
    if (unmounted || token !== createToken || !hostRef.value) {
        return;
    }
    vditor = new Vditor(hostRef.value, {
        cdn,
        mode: props.mode,
        value,
        resize:{enable:true},
        placeholder: props.placeholder,
        lang: 'zh_CN',
        icon: 'ant',
        height: 'auto',
        minHeight: props.minHeight,
        cache: { enable: false },
        counter: { enable: true, type: 'markdown' },
        toolbarConfig: { pin: false },
        outline: { enable: false, position: 'left' },
        preview: {
            hljs: { enable: true, style: 'github', lineNumber: true },
            math: { engine: 'KaTeX' },
            markdown: { toc: true },
            actions: []
        },
        input: () => {
            const html = vditor?.getHTML() ?? '';
            if (html !== props.modelValue) {
                emit('update:modelValue', html);
            }
        },
        upload: {
            handler: uploadHandler
        }
    });
}

function destroyEditor() {
    if (!vditor) {
        return;
    }
    try {
        vditor.destroy();
    } catch {
        // Vditor 在销毁过程中可能清理失败，忽略即可
    }
    vditor = null;
}

onMounted(() => {
    void createEditor(props.modelValue);
});

onBeforeUnmount(() => {
    unmounted = true;
    destroyEditor();
});

async function uploadHandler(files: File[])  {
    if (!props.onUpload) return null;
    for (const file of files) {
        const url = await props.onUpload(file);
        if (url) {
            vditor?.insertValue(`![Image](${url})`);
        }
    }
    return null;
}

watch(
    () => props.modelValue,
    async value => {
        const instance = vditor;
        if (!instance) {
            return;
        }
        const next = value ?? '';
        if (next === instance.getHTML()) {
            return;
        }
        const markdown = await htmlToMarkdown(next);
        // 转译期间内容可能已再次变化或实例已重建，此时丢弃本次同步
        if (vditor !== instance || next === instance.getHTML()) {
            return;
        }
        instance.setValue(markdown, true);
    }
);

// Vditor 没有公开的切换模式 API，改动模式时按当前内容重建实例
watch(
    () => props.mode,
    async () => {
        const html = getCurrentHTML();
        destroyEditor();
        hostKey.value += 1;
        await nextTick();
        await createEditor(html);
    }
);

defineExpose({
    getValue: getCurrentHTML,
    getHTML: getCurrentHTML,
    getMarkdown: () => vditor?.getValue() ?? ''
});
</script>

<style scoped>
.vditor-editor {
    min-width: 0;
    text-align: left;
}

.vditor-host :deep(.vditor) {
    display: flex;
    flex-direction: column;
    border: 0;
    background: transparent;
}

/* 工具栏置于编辑区顶部 */
.vditor-host :deep(.vditor-toolbar) {
    order: -1;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 2px;
    margin-bottom: 12px;
    padding: 0 0 8px;
    border-top: 0;
    border-bottom: 1px solid #f2f3f5;
    background: transparent;
}

.vditor-host :deep(.vditor-toolbar__item),
.vditor-host :deep(.vditor-toolbar__divider) {
    float: none;
}

.vditor-host :deep(.vditor-toolbar__br) {
    display: none;
}

.vditor-host :deep(.vditor-counter) {
    float: none;
    margin: 0 0 0 auto;
}

.vditor-host :deep(.vditor-toolbar__item button) {
    color: #8f959e;
}

.vditor-host :deep(.vditor-toolbar__item button:hover) {
    color: #1f2329;
}

.vditor-host :deep(.vditor-reset) {
    color: #333;
    font-size: 15px;
    line-height: 1.9;
}

.vditor-host :deep(.vditor-ir pre.vditor-reset),
.vditor-host :deep(.vditor-wysiwyg pre.vditor-reset) {
    padding-left: 0;
    background: transparent;
}

.vditor-host :deep(.vditor-sv) {
    border-left: 0;
}

.vditor-host :deep(.vditor-counter) {
    font-size: 12px;
    color: #8f959e;
}
</style>
