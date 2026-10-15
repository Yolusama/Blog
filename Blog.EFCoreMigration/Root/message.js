/**
 * 轻量级全局提示组件（类 Element UI 的 Message）
 * 依赖：message.css
 *
 * 用法：
 *   Message.success("保存成功");
 *   Message.error("操作失败", { duration: 5000, closable: true });
 *   Message({ type: "warning", message: "请注意" });
 *   const instance = Message.info("处理中...", { duration: 0 });  // 0 表示不自动关闭
 *   instance.close();
 *   Message.closeAll();
 */
(function (global) {
    "use strict";

    const TYPES = {
        success: { className: "message--success", icon: "✓" },
        warning: { className: "message--warning", icon: "!" },
        error: { className: "message--error", icon: "✕" },
        info: { className: "message--info", icon: "i" }
    };

    const GAP = 12;
    const OFFSET_TOP = 20;
    const LEAVE_DURATION = 240;

    const defaults = {
        type: "info",
        duration: 3000,
        max: 5,
        closable: false
    };

    let container = null;

    function ensureContainer() {
        if (container && document.body.contains(container)) {
            return container;
        }
        container = document.createElement("div");
        container.className = "message-container";
        container.setAttribute("role", "status");
        container.setAttribute("aria-live", "polite");
        document.body.appendChild(container);
        return container;
    }

    function getMessages() {
        return container ? Array.from(container.children) : [];
    }

    // 依次堆叠，避免多条提示互相重叠
    function reposition() {
        let top = OFFSET_TOP;
        getMessages().forEach(el => {
            el.style.top = `${top}px`;
            top += el.offsetHeight + GAP;
        });
    }

    function close(el) {
        if (!el || el.dataset.closing === "true") {
            return;
        }
        el.dataset.closing = "true";
        global.clearTimeout(Number(el.dataset.timerId));
        el.classList.remove("message--enter");
        el.classList.add("message--leave");
        global.setTimeout(() => {
            el.remove();
            reposition();
        }, LEAVE_DURATION);
    }

    function closeAll() {
        getMessages().forEach(close);
    }

    function show(options) {
        const opts = typeof options === "string" ? { message: options } : { ...options };
        const type = TYPES[opts.type] ? opts.type : defaults.type;
        const config = TYPES[type];
        const duration = Number.isFinite(opts.duration) ? opts.duration : defaults.duration;
        const closable = opts.closable ?? defaults.closable;

        const host = ensureContainer();

        // 超出上限时，先关掉最早的一条
        const alive = getMessages().filter(el => el.dataset.closing !== "true");
        if (defaults.max > 0 && alive.length >= defaults.max) {
            close(alive[0]);
        }

        const el = document.createElement("div");
        el.className = `message ${config.className}`;
        el.style.top = `${OFFSET_TOP}px`;

        const icon = document.createElement("span");
        icon.className = "message__icon";
        icon.setAttribute("aria-hidden", "true");
        icon.textContent = config.icon;

        // 使用 textContent 写入，避免消息内容被当作 HTML 解析
        const text = document.createElement("span");
        text.className = "message__text";
        text.textContent = String(opts.message ?? "");

        el.append(icon, text);

        if (closable) {
            const closeBtn = document.createElement("button");
            closeBtn.type = "button";
            closeBtn.className = "message__close";
            closeBtn.setAttribute("aria-label", "关闭");
            closeBtn.textContent = "×";
            closeBtn.addEventListener("click", () => close(el));
            el.appendChild(closeBtn);
        }

        host.appendChild(el);

        // 强制回流后再加 enter 类，确保进场过渡能触发
        void el.offsetHeight;
        el.classList.add("message--enter");
        reposition();

        if (duration > 0) {
            el.dataset.timerId = String(global.setTimeout(() => close(el), duration));
        }

        return {
            close: () => close(el),
            element: el
        };
    }

    function message(options) {
        return show(options);
    }

    Object.keys(TYPES).forEach(type => {
        message[type] = (text, options) => show({ ...options, type, message: text });
    });

    message.show = show;
    message.closeAll = closeAll;
    message.defaults = defaults;

    global.Message = message;
})(window);

