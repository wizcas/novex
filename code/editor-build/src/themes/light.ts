import { HighlightStyle } from '@codemirror/language';
import { EditorView } from '@codemirror/view';
import { tags as t } from '@lezer/highlight';

// 优化的浅色主题 - 使用更丰富的颜色方案
export const markdownLightTheme = EditorView.theme(
  {
    "&": {
      color: "#56554f",
      backgroundColor: "#d8dfd1",
    },
    ".cm-content": {
      padding: "16px",
      fontSize: "16px",
      fontFamily:
        '"Maple Mono NF CN","Inter", "SF Pro Text", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
      lineHeight: "1.6",
      caretColor: "#517d93",
    },
    ".cm-focused": {
      outline: "none",
    },
    ".cm-editor": {
      borderRadius: "8px",
      border: "1px solid #b8c5a8",
      boxShadow: "0 2px 8px rgba(81, 125, 147, 0.15)",
    },
    ".cm-scroller": {
      overflow: "auto",
    },
    ".cm-line": {
      padding: "0 4px",
    },
    ".cm-activeLine": {
      backgroundColor: "#e2e9dc",
    },
    ".cm-selectionBackground, ::selection": {
      backgroundColor: "rgba(81, 125, 147, 0.2) !important",
    },
    ".cm-gutters": {
      backgroundColor: "#e2e9dc",
      borderRight: "1px solid #b8c5a8",
      color: "#8a8985",
    },
    ".cm-lineNumbers .cm-gutterElement": {
      padding: "0 8px",
      fontSize: "14px",
    },
    ".cm-foldGutter .cm-gutterElement": {
      padding: "0 4px",
    },
  },
  { dark: false }
);

// 优化的浅色主题语法高亮
export const markdownLightHighlighting = HighlightStyle.define([
  // 标题 - 不同级别使用不同的暖色调
  {
    tag: t.heading1,
    fontSize: "1.8em",
    fontWeight: "bold",
    color: "#8b4513", // 深棕色
    textDecoration: "underline",
    textDecorationColor: "#d2691e",
  },
  {
    tag: t.heading2,
    fontSize: "1.5em",
    fontWeight: "bold",
    color: "#cd853f", // 秘鲁色
  },
  {
    tag: t.heading3,
    fontSize: "1.3em",
    fontWeight: "bold",
    color: "#517d93", // 强调色
  },
  {
    tag: t.heading4,
    fontSize: "1.2em",
    fontWeight: "bold",
    color: "#708090", // 石板灰
  },
  {
    tag: t.heading5,
    fontSize: "1.1em",
    fontWeight: "bold",
    color: "#5f8a5f", // 深海绿
  },
  {
    tag: t.heading6,
    fontSize: "1.05em",
    fontWeight: "bold",
    color: "#9370db", // 中紫色
  },

  // 文本格式化
  {
    tag: t.strong,
    fontWeight: "bold",
    color: "#8b4513", // 深棕色，与一级标题呼应
  },
  {
    tag: t.emphasis,
    fontStyle: "italic",
    color: "#9370db", // 中紫色
  },
  {
    tag: t.strikethrough,
    textDecoration: "line-through",
    color: "#cd853f",
  },

  // 链接
  {
    tag: t.link,
    color: "#517d93", // 强调色
    textDecoration: "underline",
    textDecorationColor: "rgba(81, 125, 147, 0.6)",
  },
  {
    tag: t.url,
    color: "#4682b4", // 钢蓝色
  },

  // 代码
  {
    tag: t.monospace,
    fontFamily:
      '"Maple Mono NF CN", "JetBrains Mono", "Fira Code", "SF Mono", Consolas, monospace',
    fontSize: "0.9em",
    color: "#2f4f4f", // 深石板灰
  },
  {
    tag: t.processingInstruction,
    backgroundColor: "rgba(81, 125, 147, 0.1)",
    padding: "2px 4px",
    borderRadius: "3px",
    color: "#8b0000", // 深红色
  },

  // 列表
  {
    tag: t.list,
    color: "#556b2f", // 橄榄暗绿色
  },

  // 引用
  {
    tag: t.quote,
    color: "#708090", // 石板灰
    fontStyle: "italic",
    borderLeft: "4px solid #9370db",
    paddingLeft: "12px",
  },

  // 注释和元数据
  {
    tag: t.comment,
    color: "#8a8985",
    fontStyle: "italic",
  },
  {
    tag: t.meta,
    color: "#9370db", // 中紫色
  },

  // 标点符号
  {
    tag: t.punctuation,
    color: "#696969", // 暗灰色
  },

  // 特殊字符
  {
    tag: t.escape,
    color: "#d2691e", // 橙色
  },
  {
    tag: t.invalid,
    color: "#dc143c", // 深红色
    textDecoration: "underline wavy",
  },
]);
