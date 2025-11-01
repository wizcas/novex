# HtmlAgilityPack 集成说明

## 概述

已成功将 HtmlAgilityPack 集成到 RuleEngine 中，用于处理 `MatcherType: "Markup"` 类型的提取规则。

## 功能特性

### 1. XPath 选择器支持
- 支持标准 XPath 表达式，如 `//dream`, `//*[@class='content']`
- 自动识别以 `//` 或 `/` 开头的模式为 XPath 选择器

### 2. 简单标签名匹配
- 支持直接使用标签名进行匹配，如 `dream`, `plot`, `content`
- 自动转换为 `//tagname` XPath 查询

### 3. 内容提取选项
- 默认提取节点的内部文本（`InnerText`）
- 可通过 `CustomOptions` 设置 `extract_html: true` 来提取完整HTML（`OuterHtml`）

### 4. 错误处理
- 如果 HtmlAgilityPack 解析失败，自动回退到正则表达式处理
- 记录错误信息便于调试

## 使用示例

### YAML 规则配置

```yaml
Version: "1.0"
Description: "HtmlAgilityPack 示例规则"

ExtractionRules:
# 使用 XPath 选择器
- Id: "extract_dream_xpath"
  Name: "使用XPath提取梦境内容"
  MatcherType: "Markup"
  Pattern: "//dream//content"
  Options:
    Global: true
    CustomOptions:
      extract_html: false  # 提取文本内容
  Action: "Extract"
  Target: "MainBody"
  Priority: 10
  Enabled: true

# 使用简单标签名
- Id: "extract_plot_summary" 
  Name: "提取情节摘要"
  MatcherType: "Markup"
  Pattern: "summary"  # 等价于 //summary
  Options:
    Global: false
    MaxMatches: 1
    CustomOptions:
      extract_html: true  # 提取HTML内容
  Action: "Extract"
  Target: "Summary"
  Priority: 20
  Enabled: true
```

### 处理的源内容示例

```xml
<root>
  <dream>
    <content>这是梦境的内容</content>
    <character>主角</character>
  </dream>
  <plot>
    <summary>这是情节摘要</summary>
    <events>重要事件列表</events>
  </plot>
</root>
```

## 安装的包版本

- **HtmlAgilityPack**: 1.12.4 (最新版本)

## 技术实现

### 核心方法
- `FindMarkupMatchesAsync()`: 处理 markup 类型的规则匹配
- 支持三种模式识别：
  1. XPath 选择器（以 `//` 或 `/` 开头）
  2. 简单标签名（不包含 `<` 和 `>` 字符）
  3. HTML 标签格式（回退到正则表达式）

### 选项设置
- `Options.CustomOptions["extract_html"]`: 控制是否提取HTML还是纯文本
- `Options.Global`: 控制是否匹配所有结果
- `Options.MaxMatches`: 控制最大匹配数量

## 测试验证

已通过单元测试验证：
- XPath 选择器提取功能
- 简单标签名匹配功能  
- 内容提取选项功能
- 错误处理和回退机制

所有 24 个测试均通过，确保功能稳定可靠。