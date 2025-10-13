// Minimal working editor for Blazor
window.CodeMirrorSetup = {
  editors: new Map(),

  createEditor: function (elementId, initialValue) {
    const element = document.getElementById(elementId);
    if (!element) {
      console.error("Element not found:", elementId);
      return null;
    }

    // Create simple textarea
    const textarea = document.createElement("textarea");
    textarea.value = initialValue || "";
    textarea.style.cssText = `
      width: 100%;
      height: 400px;
      min-height: 400px;
      padding: 12px;
      font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
      font-size: 14px;
      line-height: 1.5;
      border: 1px solid #d9d9d9;
      border-radius: 4px;
      resize: vertical;
      box-sizing: border-box;
      background: white;
      color: black;
      outline: none;
    `;

    // Clear and add
    element.innerHTML = "";
    element.appendChild(textarea);

    // Add event listener
    textarea.addEventListener("input", function () {
      const customEvent = new CustomEvent("codemirror-change", {
        detail: {
          elementId: elementId,
          value: textarea.value,
        },
      });
      element.dispatchEvent(customEvent);
    });

    // Store reference
    this.editors.set(elementId, textarea);

    return { elementId: elementId };
  },

  setValue: function (elementId, value) {
    const textarea = this.editors.get(elementId);
    if (textarea) {
      textarea.value = value || "";
    }
  },

  getValue: function (elementId) {
    const textarea = this.editors.get(elementId);
    return textarea ? textarea.value : "";
  },

  focus: function (elementId) {
    const textarea = this.editors.get(elementId);
    if (textarea) {
      textarea.focus();
    }
  },

  destroy: function (elementId) {
    const textarea = this.editors.get(elementId);
    if (textarea) {
      const element = document.getElementById(elementId);
      if (element && element.contains(textarea)) {
        element.removeChild(textarea);
      }
      this.editors.delete(elementId);
    }
  },
};
