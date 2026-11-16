import { mergeAttributes } from '@tiptap/core';
import Image from '@tiptap/extension-image';

export type ImageAlign = 'left' | 'center' | 'right';

const ALIGNMENTS: ImageAlign[] = ['left', 'center', 'right'];

export function normalizeAlign(value: unknown): ImageAlign {
    return ALIGNMENTS.includes(value as ImageAlign) ? value as ImageAlign : 'center';
}

function escapeAttribute(value: unknown): string {
    return String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}

/**
 * 支持拖拽缩放与左/中/右对齐的图片节点。
 * 默认居中的普通图片仍序列化为标准 Markdown 语法；
 * 一旦调整了宽高或对齐方式，则以 <figure><img /></figure> 的形式写入 Markdown，
 * 保证排版信息可持久化，同时后端仍能解析到 img 的 src。
 */
export const ResizableImage = Image.extend({
    addAttributes() {
        return {
            ...(this.parent?.() ?? {}),
            align: {
                default: 'center',
                parseHTML: element => {
                    const align = element.getAttribute('data-align');
                    if (ALIGNMENTS.includes(align as ImageAlign)) {
                        return align;
                    }
                    const float = element.style.float;
                    if (float === 'left' || float === 'right') {
                        return float;
                    }
                    return 'center';
                },
                renderHTML: attributes => ({
                    'data-align': normalizeAlign(attributes.align),
                }),
            },
        };
    },

    renderHTML({ HTMLAttributes }) {
        return ['img', mergeAttributes(this.options.HTMLAttributes, HTMLAttributes)];
    },

    renderMarkdown(node) {
        const { src, alt, title, width, height } = node.attrs ?? {};
        if (!src) {
            return '';
        }
        const align = normalizeAlign(node.attrs?.align);
        const hasLayout = width != null || height != null || align !== 'center';
        if (!hasLayout) {
            const altText = String(alt ?? '');
            return title
                ? `![${altText}](${src} "${title}")`
                : `![${altText}](${src})`;
        }
        const marginStyle = align === 'left'
            ? 'margin-right:auto;margin-left:0'
            : align === 'right'
                ? 'margin-right:0;margin-left:auto'
                : 'margin-right:auto;margin-left:auto';
        const attributes = [
            `src="${escapeAttribute(src)}"`,
            alt != null && alt !== '' ? `alt="${escapeAttribute(alt)}"` : '',
            title != null && title !== '' ? `title="${escapeAttribute(title)}"` : '',
            width != null ? `width="${escapeAttribute(width)}"` : '',
            height != null ? `height="${escapeAttribute(height)}"` : '',
            `data-align="${align}"`,
            `style="display:block;max-width:100%;height:auto;${marginStyle}"`,
        ].filter(Boolean);
        return `<figure>${`<img ${attributes.join(' ')}>`}</figure>`;
    },
});
