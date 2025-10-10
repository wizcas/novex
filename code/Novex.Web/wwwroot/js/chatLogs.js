let keyboardHandler = null;
let dotNetRef = null;

export function addKeyboardListener(dotNetReference) {
    dotNetRef = dotNetReference;

    // 移除之前的监听器（如果存在）
    if (keyboardHandler) {
        document.removeEventListener("keydown", keyboardHandler);
    }

    // 创建新的键盘事件处理器
    keyboardHandler = function (event) {
        // 只处理我们关心的按键
        if (event.key === "ArrowLeft" || event.key === "ArrowRight" || event.key === "Escape") {
            // 阻止默认行为
            event.preventDefault();
            event.stopPropagation();

            // 调用 .NET 方法
            dotNetRef.invokeMethodAsync("OnKeyDown", event.key);
        }
    };

    // 添加键盘事件监听器
    document.addEventListener("keydown", keyboardHandler);
}

export function removeKeyboardListener() {
    if (keyboardHandler) {
        document.removeEventListener("keydown", keyboardHandler);
        keyboardHandler = null;
    }
    dotNetRef = null;
}
