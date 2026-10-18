/**
 * 全屏加载遮罩（类 Element UI Loading）
 * 依赖：loading.css
 *
 * 用法（Promise 风格）：
 *   const loading = Loading.show({ text: "正在生成迁移文件…", subText: "请勿关闭页面" });
 *   try { await doSomething(); } finally { loading.close(); }
 *
 *   // 包裹 Promise / 返回 Promise 的函数，自动 show + close
 *   const res = await Loading.wrap(apiClient.post(url, payload), { text: "处理中…" });
 *
 *   const loading = Loading.show("加载中…");
 *   loading.setText("正在应用迁移…", "请勿刷新页面");   // 中途更新文案
 *   loading.close();
 *
 *   Loading.hide();   // 强制关闭（忽略引用计数）
 *
 * 特性：
 *   - 引用计数：多次 show() 时，最后一个 handle close() 才真正关闭
 *   - delay：操作很快完成时不会闪一下遮罩；minDuration：避免一闪而过
 *   - 默认不可关闭（阻断页面交互，防止重复提交）
 *   - showElapsed：显示“已等待 N 秒”，缓解长耗时操作的焦虑
 */
(function (global) {
    "use strict";

    const defaults = {
        text: "加载中…",
        subText: "",
        showElapsed: true,
        closable: false,
        delay: 150,
        minDuration: 300
    };

    const LEAVE_DURATION = 200;

    let state = null;   // { overlay, textEl, subTextEl, elapsedEl, mountedAt, tickTimer }
    let count = 0;      // 引用计数
    let mountTimer = 0;
    let removeTimer = 0;
    let options = { ...defaults };

    function createEl(tag, className, text) {
        const el = document.createElement(tag);
        if (className) {
            el.className = className;
        }
        if (text !== undefined) {
            el.textContent = text;
        }
        return el;
    }

    function updateElapsed() {
        if (!state) {
            return;
        }
        const seconds = Math.floor((Date.now() - state.mountedAt) / 1000);
        state.elapsedEl.textContent = seconds > 0 ? `已等待 ${seconds} 秒` : "";
    }

    function applyContent() {
        if (!state) {
            return;
        }

        const text = String(options.text ?? "");
        state.textEl.textContent = text;
        state.textEl.hidden = !text;

        const subText = String(options.subText ?? "");
        state.subTextEl.textContent = subText;
        state.subTextEl.hidden = !subText;

        state.elapsedEl.hidden = !options.showElapsed;
        updateElapsed();
    }

    function mount() {
        if (state) {
            return;
        }

        const overlay = createEl("div", "loading-overlay");
        overlay.setAttribute("role", "status");
        overlay.setAttribute("aria-live", "polite");
        overlay.setAttribute("aria-busy", "true");
        if (options.closable) {
            overlay.classList.add("loading-overlay--closable");
        }

        const panel = createEl("div", "loading-box");
        const spinner = createEl("div", "loading-spinner");
        spinner.setAttribute("aria-hidden", "true");

        const textEl = createEl("div", "loading__text");
        const subTextEl = createEl("div", "loading__subtext");
        const elapsedEl = createEl("div", "loading__elapsed");
        elapsedEl.setAttribute("aria-hidden", "true");

        panel.append(spinner, textEl, subTextEl, elapsedEl);
        overlay.appendChild(panel);

        state = {
            overlay,
            textEl,
            subTextEl,
            elapsedEl,
            mountedAt: Date.now(),
            tickTimer: 0
        };
        applyContent();

        // 用 mousedown 判断，避免在面板内拖选文本到遮罩上时被误关闭
        overlay.addEventListener("mousedown", event => {
            if (options.closable && event.target === overlay) {
                hideAll();
            }
        });

        document.body.appendChild(overlay);

        if (options.showElapsed) {
            state.tickTimer = global.setInterval(updateElapsed, 1000);
        }

        // 强制回流后再加 enter 类，确保进场过渡能触发
        void overlay.offsetHeight;
        overlay.classList.add("loading-overlay--enter");
    }

    function unmount() {
        if (!state) {
            return;
        }

        const current = state;
        state = null;

        global.clearInterval(current.tickTimer);
        current.overlay.classList.remove("loading-overlay--enter");
        current.overlay.classList.add("loading-overlay--leave");
        global.setTimeout(() => current.overlay.remove(), LEAVE_DURATION);
    }

    function hide() {
        global.clearTimeout(removeTimer);
        removeTimer = 0;

        if (!state) {
            // 还处于 delay 阶段，直接取消挂载
            global.clearTimeout(mountTimer);
            mountTimer = 0;
            return;
        }

        const remain = Math.max(0, options.minDuration - (Date.now() - state.mountedAt));
        if (remain > 0) {
            removeTimer = global.setTimeout(unmount, remain);
        } else {
            unmount();
        }
    }

    function show(opts) {
        options = { ...defaults, ...(typeof opts === "string" ? { text: opts } : opts) };
        count += 1;

        if (state) {
            // 已在显示：取消可能进行中的退场，仅更新文案
            global.clearTimeout(removeTimer);
            removeTimer = 0;
            if (state.overlay.classList.contains("loading-overlay--leave")) {
                state.overlay.classList.remove("loading-overlay--leave");
                state.overlay.classList.add("loading-overlay--enter");
            }
            applyContent();
        } else {
            global.clearTimeout(mountTimer);
            mountTimer = global.setTimeout(() => {
                mountTimer = 0;
                // delay 期间如果已被全部 close，则不挂载
                if (count > 0) {
                    mount();
                }
            }, options.delay);
        }

        let released = false;
        return {
            setText(text, subText) {
                if (released) {
                    return;
                }
                options = { ...options, text, subText: subText ?? "" };
                applyContent();
            },
            close() {
                if (released) {
                    return;
                }
                released = true;
                count = Math.max(0, count - 1);
                if (count === 0) {
                    hide();
                }
            }
        };
    }

    function hideAll() {
        count = 0;
        hide();
    }

    async function wrap(target, opts) {
        const handle = show(opts);
        try {
            return await (typeof target === "function" ? target() : target);
        } finally {
            handle.close();
        }
    }

    global.Loading = {
        show,
        hide: hideAll,
        wrap,
        defaults
    };
})(window);

