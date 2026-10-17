/**
 * 轻量级弹窗组件（类 Element UI MessageBox）
 * 依赖：message-box.css
 *
 * 用法（Promise 风格，可直接 await）：
 *   if (await MessageBox.confirm("确认删除吗？")) { ... }
 *   await MessageBox.alert("保存成功");
 *   await MessageBox.warning("请注意", { title: "警告" });
 *   const name = await MessageBox.prompt("请输入迁移名称", { inputValue: "Init" });
 *   if (name === null) return;                 // 用户取消
 *   await MessageBox.confirm("确认删除吗？", { danger: true, confirmText: "删除" });
 *   MessageBox.close();                        // 主动关闭当前弹窗（按取消处理）
 *
 * 返回值：
 *   alert / success / warning / error / info → boolean（点“确定”或 Enter 为 true，其余为 false）
 *   confirm                                  → boolean
 *   prompt                                   → string（已 trim）或 null（取消）
 */
(function (global) {
    "use strict";

    const TYPES = {
        success: { className: "message-box--success", icon: "✓" },
        warning: { className: "message-box--warning", icon: "!" },
        error: { className: "message-box--error", icon: "✕" },
        info: { className: "message-box--info", icon: "i" }
    };

    const FOCUSABLE = "button:not([disabled]), input:not([disabled]), textarea:not([disabled]), select:not([disabled]), a[href]";
    const LEAVE_DURATION = 200;

    const defaults = {
        title: "提示",
        type: "",
        message: "",
        confirmText: "确定",
        cancelText: "取消",
        showCancel: true,
        showClose: true,
        closeOnOverlayClick: true,
        danger: false,
        input: false,
        inputValue: "",
        inputPlaceholder: "",
        inputType: "text",
        inputErrorMessage: "请输入内容。"
    };

    let instance = null;

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

    /**
     * 关闭当前弹窗并 settle 对应的 Promise
     * @param {*} result confirm/alert 为 boolean，prompt 为 string 或 null
     */
    function close(result) {
        if (!instance) {
            return;
        }

        const state = instance;
        instance = null;

        document.removeEventListener("keydown", state.onKeyDown, true);
        state.overlay.classList.remove("message-box-overlay--enter");
        state.overlay.classList.add("message-box-overlay--leave");
        global.setTimeout(() => state.overlay.remove(), LEAVE_DURATION);

        if (state.restoreFocus && document.body.contains(state.restoreFocus)) {
            state.restoreFocus.focus();
        }

        state.resolve(result);
    }

    function closeAll() {
        if (instance) {
            close(instance.cancelValue);
        }
    }

    function open(options) {
        const opts = { ...defaults, ...options };
        const inputMode = opts.input === true;
        // prompt 取消时返回 null，其余返回 false
        const cancelValue = inputMode ? null : false;
        const type = TYPES[opts.type] ? opts.type : "";
        const config = type ? TYPES[type] : null;
        const hasTitle = Boolean(opts.title);
        const hasMessage = opts.message !== undefined && opts.message !== null && String(opts.message) !== "";

        // 同一时间只展示一个弹窗，旧的按“取消”处理
        closeAll();

        const overlay = createEl("div", "message-box-overlay");
        const panel = createEl("div", config ? `message-box ${config.className}` : "message-box");
        panel.setAttribute("role", "alertdialog");
        panel.setAttribute("aria-modal", "true");

        if (hasTitle) {
            const header = createEl("div", "message-box__header");
            header.appendChild(createEl("h3", "message-box__title", String(opts.title)));

            if (opts.showClose) {
                const closeBtn = createEl("button", "message-box__close", "×");
                closeBtn.type = "button";
                closeBtn.setAttribute("aria-label", "关闭");
                closeBtn.addEventListener("click", () => close(cancelValue));
                header.appendChild(closeBtn);
            }

            panel.appendChild(header);
        }

        const body = createEl("div", "message-box__body");

        if (config) {
            const icon = createEl("span", "message-box__icon", config.icon);
            icon.setAttribute("aria-hidden", "true");
            body.appendChild(icon);
        }

        const content = createEl("div", "message-box__content");

        if (hasMessage) {
            content.appendChild(createEl("p", "message-box__message", String(opts.message)));
        }

        let input = null;
        let errorText = null;

        if (inputMode) {
            input = document.createElement("input");
            input.className = "message-box__input";
            input.type = opts.inputType || "text";
            input.value = opts.inputValue ?? "";
            if (opts.inputPlaceholder) {
                input.placeholder = String(opts.inputPlaceholder);
            }
            content.appendChild(input);

            errorText = createEl("div", "message-box__error", String(opts.inputErrorMessage));
            errorText.hidden = true;
            content.appendChild(errorText);
        }

        body.appendChild(content);
        panel.appendChild(body);

        const footer = createEl("div", "message-box__footer");

        if (opts.showCancel) {
            const cancelBtn = createEl("button", "message-box__btn", String(opts.cancelText));
            cancelBtn.type = "button";
            cancelBtn.addEventListener("click", () => close(cancelValue));
            footer.appendChild(cancelBtn);
        }

        const confirmBtn = createEl(
            "button",
            opts.danger
                ? "message-box__btn message-box__btn--primary message-box__btn--danger"
                : "message-box__btn message-box__btn--primary",
            String(opts.confirmText)
        );
        confirmBtn.type = "button";
        confirmBtn.addEventListener("click", onConfirm);
        footer.appendChild(confirmBtn);
        panel.appendChild(footer);

        function onConfirm() {
            if (!input) {
                close(true);
                return;
            }

            const value = input.value.trim();
            if (!value) {
                errorText.hidden = false;
                input.focus();
                return;
            }

            close(value);
        }

        function onKeyDown(event) {
            if (!instance || instance.overlay !== overlay) {
                return;
            }

            if (event.key === "Escape") {
                event.preventDefault();
                close(cancelValue);
                return;
            }

            // 焦点在按钮上时交给按钮自身的默认行为（Enter 触发取消/确定）
            if (event.key === "Enter" && event.target?.tagName !== "BUTTON") {
                event.preventDefault();
                onConfirm();
                return;
            }

            if (event.key !== "Tab") {
                return;
            }

            const focusables = Array.from(panel.querySelectorAll(FOCUSABLE));
            if (!focusables.length) {
                return;
            }

            const first = focusables[0];
            const last = focusables[focusables.length - 1];

            if (event.shiftKey && document.activeElement === first) {
                event.preventDefault();
                last.focus();
            } else if (!event.shiftKey && document.activeElement === last) {
                event.preventDefault();
                first.focus();
            }
        }

        // 用 mousedown 判断，避免在弹窗内拖选文本到遮罩上时被误关闭
        overlay.addEventListener("mousedown", event => {
            if (opts.closeOnOverlayClick && event.target === overlay) {
                close(cancelValue);
            }
        });

        overlay.appendChild(panel);

        const active = document.activeElement;
        const restoreFocus = active instanceof HTMLElement && active !== document.body ? active : null;

        let resolveFn;
        const promise = new Promise(resolve => {
            resolveFn = resolve;
        });

        instance = { overlay, onKeyDown, cancelValue, restoreFocus, resolve: resolveFn };

        document.body.appendChild(overlay);
        document.addEventListener("keydown", onKeyDown, true);

        if (input) {
            input.focus();
            input.select();
        } else {
            confirmBtn.focus();
        }

        // 强制回流后再加 enter 类，确保进场过渡能触发
        void overlay.offsetHeight;
        overlay.classList.add("message-box-overlay--enter");

        return promise;
    }

    function messageBox(options) {
        return open(options);
    }

    // 提示型：只有“确定”按钮
    messageBox.alert = (message, options) => open({ showCancel: false, ...options, message });
    // 确认型：返回 boolean
    messageBox.confirm = (message, options) => open({ ...options, message });
    // 输入型：返回 string 或 null
    messageBox.prompt = (message, options) => open({ ...options, message, input: true });

    Object.keys(TYPES).forEach(type => {
        messageBox[type] = (message, options) => open({ showCancel: false, ...options, type, message });
    });

    messageBox.close = closeAll;
    messageBox.defaults = defaults;

    global.MessageBox = messageBox;
})(window);

