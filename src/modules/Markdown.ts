const HTML_ESCAPES: Record<string, string> = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;'
};

const TOKEN = '\u0000';
const BLOCKED_PROTOCOL = /^(?:javascript|vbscript|file|data):/i;

export function escapeHtml(text: string): string {
    return text.replace(/[&<>"']/g, char => HTML_ESCAPES[char] ?? char);
}

function sanitizeUrl(raw: string, allowImageData = false): string | null {
    const url = raw.trim().replace(/^</, '').replace(/>$/, '');
    if (!url) {
        return null;
    }
    if (BLOCKED_PROTOCOL.test(url)) {
        const isImageData = allowImageData && /^data:image\//i.test(url);
        if (!isImageData) {
            return null;
        }
    }
    return url;
}

function renderInline(source: string): string {
    const tokens: string[] = [];
    const stash = (html: string): string => `${TOKEN}${tokens.push(html) - 1}${TOKEN}`;

    let text = source.replace(/(`+)([\s\S]*?)\1/g, (_match, _ticks: string, code: string) =>
        stash(`<code>${escapeHtml(code.trim())}</code>`)
    );

    text = text.replace(/!\[([^\]]*)\]\(\s*((?:[^()\s]|\([^()\s]*\))+)(?:\s+"([^"]*)")?\s*\)/g,
        (match, alt: string, url: string, title?: string) => {
            const safeUrl = sanitizeUrl(url, true);
            if (!safeUrl) {
                return match;
            }
            const titleAttr = title ? ` title="${escapeHtml(title)}"` : '';
            return stash(`<img src="${escapeHtml(safeUrl)}" alt="${escapeHtml(alt)}"${titleAttr} loading="lazy" />`);
        });

    text = text.replace(/\[([^\]]+)\]\(\s*((?:[^()\s]|\([^()\s]*\))+)(?:\s+"([^"]*)")?\s*\)/g,
        (match, label: string, url: string, title?: string) => {
            const safeUrl = sanitizeUrl(url);
            if (!safeUrl) {
                return match;
            }
            const titleAttr = title ? ` title="${escapeHtml(title)}"` : '';
            const open = stash(`<a href="${escapeHtml(safeUrl)}"${titleAttr} target="_blank" rel="noopener noreferrer">`);
            return `${open}${label}${stash('</a>')}`;
        });

    text = escapeHtml(text);

    text = text.replace(/(^|[\s(（])(https?:\/\/[^\s<>)）]+)/g, (match, prefix: string, rawUrl: string) => {
        const trailing = /[.,;:!?，。；：！？、]+$/.exec(rawUrl)?.[0] ?? '';
        const url = trailing ? rawUrl.slice(0, -trailing.length) : rawUrl;
        const safeUrl = sanitizeUrl(url);
        if (!safeUrl) {
            return match;
        }
        const open = stash(`<a href="${safeUrl}" target="_blank" rel="noopener noreferrer">`);
        return `${prefix}${open}${url}${stash('</a>')}${trailing}`;
    });

    text = text
        .replace(/\*\*\*([^*]+)\*\*\*/g, '<strong><em>$1</em></strong>')
        .replace(/___([^_]+)___/g, '<strong><em>$1</em></strong>')
        .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
        .replace(/__([^_]+)__/g, '<strong>$1</strong>')
        .replace(/~~([^~]+)~~/g, '<del>$1</del>')
        .replace(/(^|[^*])\*([^*\n]+)\*(?!\*)/g, '$1<em>$2</em>')
        .replace(/(^|[^_\w])_([^_\n]+)_(?![_\w])/g, '$1<em>$2</em>');

    return text.replace(/\u0000(\d+)\u0000/g, (_match, index: string) => tokens[Number(index)] ?? '');
}

const FENCE_RE = /^ {0,3}(`{3,}|~{3,})\s*([^\s`]*)\s*$/;
const HEADING_RE = /^ {0,3}(#{1,6})\s+(.*?)\s*#*\s*$/;
const HR_RE = /^ {0,3}([-*_])(?:\s*\1){2,}\s*$/;
const QUOTE_RE = /^ {0,3}> ?(.*)$/;
const UNORDERED_RE = /^(\s*)([-*+])\s+(.*)$/;
const ORDERED_RE = /^(\s*)(\d{1,9})[.)]\s+(.*)$/;

function isListLine(line: string): boolean {
    return UNORDERED_RE.test(line) || ORDERED_RE.test(line);
}

function startsBlock(line: string): boolean {
    return FENCE_RE.test(line)
        || HEADING_RE.test(line)
        || HR_RE.test(line)
        || QUOTE_RE.test(line)
        || isListLine(line);
}

function isTableSeparator(line: string): boolean {
    const trimmed = line.trim();
    if (!trimmed || !trimmed.includes('-')) {
        return false;
    }
    const cells = trimmed.replace(/^\|/, '').replace(/\|$/, '').split('|');
    return cells.length > 0 && cells.every(cell => /^\s*:?-{1,}:?\s*$/.test(cell));
}

function splitTableRow(line: string): string[] {
    const trimmed = line.trim().replace(/^\|/, '').replace(/\|$/, '');
    const cells: string[] = [];
    let current = '';
    let escaped = false;

    for (const char of trimmed) {
        if (escaped) {
            current += char === '|' ? '|' : char;
            escaped = false;
            continue;
        }
        if (char === '\\') {
            escaped = true;
            continue;
        }
        if (char === '|') {
            cells.push(current.trim());
            current = '';
            continue;
        }
        current += char;
    }

    cells.push(current.trim());
    return cells;
}

function cellAlign(separator: string): string {
    const trimmed = separator.trim();
    const left = trimmed.startsWith(':');
    const right = trimmed.endsWith(':');
    if (left && right) {
        return 'center';
    }
    if (right) {
        return 'right';
    }
    return '';
}

function renderTable(lines: string[], start: number): { html: string; next: number } | null {
    const headerLine = lines[start];
    if (!headerLine.includes('|') || !isTableSeparator(lines[start + 1] ?? '')) {
        return null;
    }

    const headers = splitTableRow(headerLine);
    const separators = splitTableRow(lines[start + 1]);
    if (headers.length !== separators.length) {
        return null;
    }

    const aligns = separators.map(cellAlign);
    const alignAttr = (index: number) => {
        const align = aligns[index] ?? '';
        return align ? ` style="text-align:${align}"` : '';
    };

    const head = headers
        .map((cell, index) => `<th${alignAttr(index)}>${renderInline(cell)}</th>`)
        .join('');

    const rows: string[] = [];
    let i = start + 2;
    while (i < lines.length && lines[i].trim() && lines[i].includes('|')) {
        const cells = splitTableRow(lines[i]);
        const tds = headers
            .map((_header, index) => `<td${alignAttr(index)}>${renderInline(cells[index] ?? '')}</td>`)
            .join('');
        rows.push(`<tr>${tds}</tr>`);
        i++;
    }

    const body = rows.length ? `<tbody>${rows.join('')}</tbody>` : '';
    return {
        html: `<table><thead><tr>${head}</tr></thead>${body}</table>`,
        next: i
    };
}

function renderList(lines: string[], start: number): { html: string; next: number } {
    const firstLine = lines[start];
    const firstMatch = (UNORDERED_RE.exec(firstLine) ?? ORDERED_RE.exec(firstLine)) as RegExpExecArray;
    const baseIndent = firstMatch[1].length;
    const ordered = UNORDERED_RE.test(firstLine) ? false : true;
    const startNumber = ordered ? Number(firstMatch[2]) : 1;

    const items: string[][] = [];
    let current: string[] | null = null;
    let i = start;

    const dedent = (line: string): string => {
        let count = 0;
        const limit = baseIndent + 2;
        while (count < limit && line[count] === ' ') {
            count++;
        }
        return line.slice(count);
    };

    while (i < lines.length) {
        const line = lines[i];

        if (!line.trim()) {
            let next = i + 1;
            while (next < lines.length && !lines[next].trim()) {
                next++;
            }
            const nextListMatch = next < lines.length ? (UNORDERED_RE.exec(lines[next]) ?? ORDERED_RE.exec(lines[next])) : null;
            if (nextListMatch && nextListMatch[1].length >= baseIndent) {
                i = next;
                continue;
            }
            break;
        }

        const listMatch = UNORDERED_RE.exec(line) ?? ORDERED_RE.exec(line);
        if (listMatch && listMatch[1].length === baseIndent) {
            const lineOrdered = !UNORDERED_RE.test(line);
            if (lineOrdered !== ordered) {
                break;
            }
            current = [listMatch[3]];
            items.push(current);
            i++;
            continue;
        }

        if (listMatch && listMatch[1].length > baseIndent) {
            if (!current) {
                break;
            }
            current.push(dedent(line));
            i++;
            continue;
        }

        if (!current) {
            break;
        }
        current.push(dedent(line));
        i++;
    }

    const listItems = items.map(itemLines => {
        const lines$ = [...itemLines];
        let prefix = '';
        const taskMatch = /^\[([ xX])\]\s+([\s\S]*)$/.exec(lines$[0] ?? '');
        if (taskMatch) {
            prefix = `<input type="checkbox" disabled${taskMatch[1].toLowerCase() === 'x' ? ' checked' : ''} /> `;
            lines$[0] = taskMatch[2];
        }
        let inner = renderBlocks(lines$);
        const singleParagraph = /^<p>([\s\S]*)<\/p>$/.exec(inner);
        if (singleParagraph) {
            inner = singleParagraph[1];
        }
        return `<li>${prefix}${inner}</li>`;
    });

    const tag = ordered ? 'ol' : 'ul';
    const startAttr = ordered && startNumber !== 1 ? ` start="${startNumber}"` : '';
    return {
        html: `<${tag}${startAttr}>${listItems.join('')}</${tag}>`,
        next: i
    };
}

function renderBlocks(lines: string[]): string {
    const output: string[] = [];
    let i = 0;

    while (i < lines.length) {
        const line = lines[i];

        if (!line.trim()) {
            i++;
            continue;
        }

        const fence = FENCE_RE.exec(line);
        if (fence) {
            const marker = fence[1];
            const lang = fence[2];
            const closer = new RegExp(`^ {0,3}${marker[0] === '`' ? '`' : '~'}{${marker.length},}\\s*$`);
            const body: string[] = [];
            i++;
            while (i < lines.length && !closer.test(lines[i])) {
                body.push(lines[i]);
                i++;
            }
            if (i < lines.length) {
                i++;
            }
            const langClass = lang ? ` class="language-${escapeHtml(lang)}"` : '';
            output.push(`<pre><code${langClass}>${escapeHtml(body.join('\n'))}</code></pre>`);
            continue;
        }

        const heading = HEADING_RE.exec(line);
        if (heading) {
            const level = heading[1].length;
            output.push(`<h${level}>${renderInline(heading[2])}</h${level}>`);
            i++;
            continue;
        }

        if (HR_RE.test(line)) {
            output.push('<hr />');
            i++;
            continue;
        }

        if (QUOTE_RE.test(line)) {
            const quoted: string[] = [];
            while (i < lines.length && lines[i].trim() && QUOTE_RE.test(lines[i])) {
                quoted.push((QUOTE_RE.exec(lines[i]) as RegExpExecArray)[1]);
                i++;
            }
            output.push(`<blockquote>${renderBlocks(quoted)}</blockquote>`);
            continue;
        }

        const table = renderTable(lines, i);
        if (table) {
            output.push(table.html);
            i = table.next;
            continue;
        }

        if (isListLine(line)) {
            const list = renderList(lines, i);
            output.push(list.html);
            i = list.next;
            continue;
        }

        const paragraph: string[] = [];
        while (i < lines.length && lines[i].trim() && !startsBlock(lines[i])) {
            paragraph.push(lines[i]);
            i++;
        }
        if (!paragraph.length) {
            paragraph.push(lines[i]);
            i++;
        }
        output.push(`<p>${renderInline(paragraph.join('\n')).replace(/\n/g, '<br />\n')}</p>`);
    }

    return output.join('\n');
}

export function renderMarkdown(source: string): string {
    if (!source || !source.trim()) {
        return '';
    }
    const lines = source.replace(/\r\n?/g, '\n').split('\n');
    return renderBlocks(lines);
}
