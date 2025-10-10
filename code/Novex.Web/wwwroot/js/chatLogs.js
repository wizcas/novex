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
    if (
      event.key === "ArrowLeft" ||
      event.key === "ArrowRight" ||
      event.key === "Escape"
    ) {
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

// 页面滚动位置管理
let savedScrollPosition = 0;

export function saveScrollPosition() {
  savedScrollPosition =
    window.pageYOffset || document.documentElement.scrollTop;
}

export function restoreScrollPosition() {
  // 使用 requestAnimationFrame 确保在DOM更新后再恢复滚动位置
  requestAnimationFrame(() => {
    window.scrollTo({
      top: savedScrollPosition,
      behavior: "instant",
    });
  });
}

// 防止页面闪烁的辅助函数
export function preventScrollFlash() {
  // 临时固定页面位置，防止滚动时的视觉跳跃
  const currentScroll =
    window.pageYOffset || document.documentElement.scrollTop;
  document.body.style.position = "fixed";
  document.body.style.top = `-${currentScroll}px`;
  document.body.style.width = "100%";
}

export function releaseScrollLock() {
  // 恢复正常滚动并保持位置
  const scrollY = document.body.style.top;
  document.body.style.position = "";
  document.body.style.top = "";
  document.body.style.width = "";

  if (scrollY) {
    const y = parseInt(scrollY || "0") * -1;
    window.scrollTo(0, y);
  }
}
