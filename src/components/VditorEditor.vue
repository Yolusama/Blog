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
    modelValue?: string;
    mode?: VditorMode;
    placeholder?: string;
    minHeight?: number;
    onUpload?: (file: File) => Promise<string>
}

type VditorMode = 'ir' | 'wysiwyg' | 'sv';

const props = withDefaults(
    defineProps<VditorProps>(),
    { modelValue: '', mode: 'ir', placeholder: '', minHeight: 520 }
);

const emit = defineEmits<{ 'update:modelValue': [value: string] }>();

const cdn = `${import.meta.env.BASE_URL.replace(/\/$/, '')}/vditor`;
const hostRef = ref<HTMLDivElement | null>(null);
const hostKey = ref(0);
let vditor: Vditor | null = null;

function createEditor(value: string) {
    if (!hostRef.value) {
        return;
    }
    vditor = new Vditor(hostRef.value, {
        cdn,
        mode: props.mode,
        value,
        placeholder: props.placeholder,
        lang: 'zh_CN',
        icon: 'ant',
        height: 'auto',
        minHeight: props.minHeight,
        cache: { enable: false },
        counter: { enable: true, type: 'markdown' },
        toolbarConfig: { pin: true },
        outline: { enable: false, position: 'left' },
        preview: {
            hljs: { enable: true, style: 'github', lineNumber: true },
            math: { engine: 'KaTeX' },
            markdown: { toc: true },
            actions: []
        },
        input: (value: string) => {
            if (value !== props.modelValue) {
                emit('update:modelValue', value);
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

onMounted(() => createEditor(props.modelValue));

onBeforeUnmount(destroyEditor);

async function uploadHandler(files: File[])  {
    if (!props.onUpload) return null;
    for (const file of files) {
        const url = await props.onUpload(file);
        vditor?.insertValue(`![](${url})`);
    }
    return null;
}

watch(
    () => props.modelValue,
    value => {
        if (!vditor || value === vditor.getValue()) {
            return;
        }
        vditor.setValue(value);
    }
);

// Vditor 没有公开的切换模式 API，改动模式时按当前内容重建实例
watch(
    () => props.mode,
    async () => {
        const value = vditor?.getValue() ?? props.modelValue;
        destroyEditor();
        hostKey.value += 1;
        await nextTick();
        createEditor(value);
    }
);

defineExpose({
    getValue: () => vditor?.getValue() ?? props.modelValue,
    getHTML: () => vditor?.getHTML() ?? ''
});
</script>

<style scoped>
.vditor-editor {
    min-width: 0;
    text-align: left;
}

.vditor-host :deep(.vditor) {
    border: 0;
    background: transparent;
}

.vditor-host :deep(.vditor-toolbar) {
    padding-left: 0;
    background: transparent;
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
