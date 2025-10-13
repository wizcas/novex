import { HighlightStyle } from '@codemirror/language';
import { EditorView } from '@codemirror/view';
import { tags as t } from '@lezer/highlight';

// 深色主题
export const markdownDarkTheme = EditorView.theme(
  {
    "&": {
      color: "#e0e6ed",
      backgroundColor: "#1e1e1e",
    },
    ".cm-content": {
      padding: "16px",
      fontSize: "16px",
      fontFamily:
        '"Inter", "SF Pro Text", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
      lineHeight: "1.6",
      caretColor: "#4fc3f7",
    },
    ".cm-focused": {
      outline: "none",
    },
    ".cm-editor": {
      borderRadius: "8px",
      border: "1px solid #333333",
      boxShadow: "0 2px 8px rgba(0, 0, 0, 0.3)",
    },
    ".cm-scroller": {
      overflow: "auto",
    },
    ".cm-line": {
      padding: "0 4px",
    },
    ".cm-activeLine": {
      backgroundColor: "#252525",
    },
    ".cm-selectionBackground, ::selection": {
      backgroundColor: "#264f78 !important",
    },
    ".cm-gutters": {
      backgroundColor: "#252525",
      borderRight: "1px solid #333333",
      color: "#858E96",
    },
    ".cm-lineNumbers .cm-gutterElement": {
      padding: "0 8px",
      fontSize: "14px",
    },
  },
  { dark: true }
);

// 深色主题语法高亮
export const markdownDarkHighlighting = HighlightStyle.define(
  [
    // 标题 - 不同级别使用不同的亮色调
    {
      tag: t.heading1,
      fontSize: "1.8em",
      fontWeight: "bold",
      color: "#ffffff",
      textDecoration: "underline",
      textDecorationColor: "#404040",
    },
    {
      tag: t.heading2,
      fontSize: "1.5em",
      fontWeight: "bold",
      color: "#74b9ff",
    },
    {
      tag: t.heading3,
      fontSize: "1.3em",
      fontWeight: "bold",
      color: "#81ecec",
    },
    {
      tag: t.heading4,
      fontSize: "1.2em",
      fontWeight: "bold",
      color: "#a29bfe",
    },
    {
      tag: t.heading5,
      fontSize: "1.1em",
      fontWeight: "bold",
      color: "#fd79a8",
    },
    {
      tag: t.heading6,
      fontSize: "1.05em",
      fontWeight: "bold",
      color: "#fdcb6e",
    },

    // 文本格式化
    {
      tag: t.strong,
      fontWeight: "bold",
      color: "#74b9ff",
    },
    {
      tag: t.emphasis,
      fontStyle: "italic",
      color: "#fd79a8",
    },
    {
      tag: t.strikethrough,
      textDecoration: "line-through",
      color: "#636e72",
    },

    // 链接
    {
      tag: t.link,
      color: "#00cec9",
      textDecoration: "underline",
    },
    {
      tag: t.url,
      color: "#00cec9",
    },

    // 代码
    {
      tag: t.monospace,
      fontFamily:
        '"JetBrains Mono", "Fira Code", "SF Mono", Consolas, monospace',
      fontSize: "0.9em",
    },
    {
      tag: t.processingInstruction,
      backgroundColor: "#2d3748",
      padding: "2px 4px",
      borderRadius: "3px",
      color: "#ff7675",
    },

    // 列表
    {
      tag: t.list,
      color: "#ddd",
    },

    // 引用
    {
      tag: t.quote,
      color: "#b2bec3",
      fontStyle: "italic",
      borderLeft: "4px solid #636e72",
      paddingLeft: "12px",
    },

    // 注释和元数据
    {
      tag: t.comment,
      color: "#636e72",
      fontStyle: "italic",
    },
    {
      tag: t.meta,
      color: "#6c5ce7",
    },

    // 标点符号
    {
      tag: t.punctuation,
      color: "#b2bec3",
    },

    // 特殊字符
    {
      tag: t.escape,
      color: "#e17055",
    },
    {
      tag: t.invalid,
      color: "#ff7675",
      textDecoration: "underline wavy",
    },
  ],
  { themeType: "dark" }
);
