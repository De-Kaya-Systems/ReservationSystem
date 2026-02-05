const handlers = new WeakMap();

export function showDialog(dialogElement, dotnetComponent, clickOutsideToClose = true) {
    if (!dialogElement) return;

    const existing = handlers.get(dialogElement);
    if (existing) {
        dialogElement.removeEventListener("click", existing.onClick);
        handlers.delete(dialogElement);
    }

    // açarken olası kapanış class'ını temizle
    dialogElement.classList.remove("is-closing");

    if (!dialogElement.open) {
        dialogElement.showModal();
    }

    const onClick = (event) => {
        if (!clickOutsideToClose) return;
        if (!dotnetComponent) return;

        const rect = dialogElement.getBoundingClientRect();

        const clickedOutside =
            event.clientX < rect.left ||
            event.clientX > rect.right ||
            event.clientY < rect.top ||
            event.clientY > rect.bottom;

        if (clickedOutside) {
            dotnetComponent.invokeMethodAsync("ClickOutsideTriggerAsync");
        }
    };

    dialogElement.addEventListener("click", onClick);
    handlers.set(dialogElement, { onClick });
}

export function hideDialog(dialogElement) {
    if (!dialogElement) return;

    const existing = handlers.get(dialogElement);
    if (existing) {
        dialogElement.removeEventListener("click", existing.onClick);
        handlers.delete(dialogElement);
    }

    if (!dialogElement.open) return;

    // Close animasyonu oynat
    dialogElement.classList.add("is-closing");

    const prefersReducedMotion =
        window.matchMedia &&
        window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    if (prefersReducedMotion) {
        dialogElement.classList.remove("is-closing");
        dialogElement.close();
        return;
    }

    const closeAfterAnimation = () => {
        dialogElement.removeEventListener("animationend", closeAfterAnimation);
        dialogElement.classList.remove("is-closing");
        dialogElement.close();
    };

    dialogElement.addEventListener("animationend", closeAfterAnimation, { once: true });
}