(function () {
    const toastRoot = document.getElementById("toast-section");
    if (!toastRoot) {
        console.error("Toast: không tìm thấy #toast-section");
        return;
    }

    class ToastManager {
        constructor() {
            this.toasts = new Map();
            this.container = toastRoot.querySelector("#toastContainer");
            if (!this.container) {
                this.container = document.createElement("div");
                this.container.id = "toastContainer";
                this.container.className = "toast-container top-right";
                toastRoot.appendChild(this.container);
            }
            this.toastId = 0;
        }

        show(options = {}) {
            const id = ++this.toastId;
            const toast = this.createToast(id, options);

            this.toasts.set(id, toast);
            this.container.appendChild(toast.element);

            requestAnimationFrame(() => {
                toast.element.classList.add("show");
            });

            if (options.duration !== 0 && options.duration !== false) {
                toast.timer = setTimeout(() => {
                    this.dismiss(id);
                }, options.duration || 4000);

                if (toast.progressBar) {
                    toast.progressBar.style.transitionDuration = `${options.duration || 4000}ms`;
                    requestAnimationFrame(() => {
                        toast.progressBar.style.width = "100%";
                    });
                }
            }

            return id;
        }

        createToast(id, options) {
            const element = document.createElement("div");
            element.className = `toast ${options.theme || "light"} ${options.type || ""}`;
            element.dataset.toastId = id;

            let iconSvg = this.getIcon(options.type, options.loading);

            element.innerHTML = `
                ${iconSvg ? `<div class="toast-icon">${iconSvg}</div>` : ""}
                <div class="toast-content">
                    ${options.title ? `<div class="toast-title">${options.title}</div>` : ""}
                    ${options.message ? `<div class="toast-message">${options.message}</div>` : ""}
                </div>
                ${options.closable !== false ? `<button class="toast-close">×</button>` : ""}
                ${options.showProgress !== false && options.duration !== 0 ? '<div class="toast-progress"></div>' : ""}
            `;

            const progressBar = element.querySelector(".toast-progress");
            const closeBtn = element.querySelector(".toast-close");

            if (closeBtn) {
                closeBtn.addEventListener("click", () => this.dismiss(id));
            }

            element.addEventListener("mouseenter", () => {
                const toast = this.toasts.get(id);
                if (toast && toast.timer) {
                    clearTimeout(toast.timer);
                    toast.timer = null;
                }
            });

            element.addEventListener("mouseleave", () => {
                const toast = this.toasts.get(id);
                if (toast && options.duration !== 0 && !toast.timer) {
                    const remainingTime = (options.duration || 4000) * 0.3;
                    toast.timer = setTimeout(() => {
                        this.dismiss(id);
                    }, remainingTime);
                }
            });

            return { element, progressBar, timer: null };
        }

        getIcon(type, loading = false) {
            if (loading) {
                return '<div class="spinner"></div>';
            }

            const icons = {
                success:
                    '<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path></svg>',
                error:
                    '<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path></svg>',
                warning:
                    '<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path></svg>',
                info:
                    '<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"></path></svg>',
            };

            return icons[type] || "";
        }

        dismiss(id) {
            const toast = this.toasts.get(id);
            if (!toast) return;

            if (toast.timer) {
                clearTimeout(toast.timer);
            }

            toast.element.classList.add("hide");

            setTimeout(() => {
                if (toast.element.parentNode) {
                    toast.element.parentNode.removeChild(toast.element);
                }
                this.toasts.delete(id);
            }, 400);
        }

        clear() {
            this.toasts.forEach((_, id) => this.dismiss(id));
        }

        setPosition(position) {
            this.container.className = `toast-container ${position}`;
        }
    }

    const toastManager = new ToastManager();

    function showToast(type, title, message, options = {}) {
        const theme = options.theme || "light";
        const duration = options.duration ?? 4000;
        toastManager.show({
            type,
            title,
            message,
            theme,
            duration,
        });
    }

    window.toastManager = toastManager;
    window.showToast = showToast;
    window.addEventListener("DOMContentLoaded", function () {
        setTimeout(function () {
            showToast(
                "info",
                "Welcome! 👋",
                "Try out the different toast options above."
            );
        }, 1000);
    });
})();
