# Novex Editor Build

这个项目用于构建 CodeMirror 编辑器，为 Novex.Web Blazor 项目提供 Markdown 编辑功能。

## 特征

- 使用 TypeScript 开发
- 基于 CodeMirror 6
- 支持 Markdown 语法高亮
- 支持明暗主题切换
- 与 Blazor 集成的事件系统
- 使用 esbuild 进行快速打包

## 安装

```bash
npm install
```

## 构建

```bash
# 开发构建（带 source map）
npm run build

# 生产构建（压缩）
npm run build:prod

# 监听模式
npm run dev

# 构建并复制到 Blazor 项目
npm run copy
```

## 使用

构建后的文件会输出到 `dist/codemirror-bundle.js`，可以在 Blazor 项目中引用：

```html
<script src="~/js/codemirror-bundle.js"></script>
```

## API

全局对象 `window.CodeMirrorSetup` 提供以下方法：

- `createEditor(elementId, initialValue, options)` - 创建编辑器
- `setValue(elementId, value)` - 设置编辑器内容
- `getValue(elementId)` - 获取编辑器内容
- `focus(elementId)` - 聚焦编辑器
- `destroy(elementId)` - 销毁编辑器
- `getEditorInfo(elementId)` - 获取编辑器信息