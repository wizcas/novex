# ProcessorScope 作用域详解

**日期**: 2025-11-01  
**主题**: ProcessorScope 枚举的三个值及其对执行逻辑的影响  
**相关组件**: Novex.Analyzer.V2

---

## 概述

本次对话详细解释了 `ProcessorScope` 枚举的三个值（Source、Field、Global）及其在规则引擎 (RuleEngine) 中的执行逻辑差异。

---

## ProcessorScope 的三个值

### 1. Source - 处理原始源内容

**定义**: 处理器 (processor) 直接作用于 `context.SourceContent`（原始输入内容）

**执行逻辑**:
- **输入来源**: 处理器接收 `context.SourceContent` 作为输入
- **输出目标**: 处理结果会直接更新 `context.SourceContent`（除非指定了 `TargetField`）
- **字段访问**: 只能访问源内容，无法直接访问其他字段

**典型应用场景**:
- 文档预处理 (preprocessing)
- 全文清理 (cleanup)
- 格式标准化 (normalization)
- HTML 注释移除
- 空行规范化

**代码实现** (RuleEngine.cs):
```csharp
// 第 60-62 行：确定输入来源
var sourceContent = rule.Scope == ProcessorScope.Field && !string.IsNullOrWhiteSpace(rule.SourceField)
    ? context.GetField(rule.SourceField)
    : context.SourceContent;  // Source 作用域使用原始内容

// 第 88-91 行：更新源内容
else if (rule.Scope == ProcessorScope.Source)
{
    context.SourceContent = result.Output ?? string.Empty;
}
```

**YAML 配置示例**:
```yaml
- Id: "cleanup_source"
  Processor: "Text.Cleanup"
  Scope: Source
  Priority: 10
  # 处理结果会更新 context.SourceContent
```

---

### 2. Field - 处理特定字段

**定义**: 处理器作用于某个特定的字段 (field)，从 `SourceField` 读取，写入到 `TargetField`

**执行逻辑**:
- **输入来源**: 
  - 如果指定了 `SourceField`，从 `context.Fields[SourceField]` 读取
  - 否则使用 `context.SourceContent`
- **输出目标**: 如果指定了 `TargetField`，结果保存到 `context.Fields[TargetField]`
- **字段访问**: 只能访问指定的源字段

**典型应用场景**:
- 字段提取 (extraction)
- 字段转换 (transformation)
- 字段清理 (cleanup)
- 数据格式化 (formatting)
- 结构化数据提取

**代码实现** (RuleEngine.cs):
```csharp
// 第 60-62 行：从字段读取
var sourceContent = rule.Scope == ProcessorScope.Field && !string.IsNullOrWhiteSpace(rule.SourceField)
    ? context.GetField(rule.SourceField)  // 从指定字段读取
    : context.SourceContent;

// 第 84-87 行：写入字段
if (!string.IsNullOrWhiteSpace(rule.TargetField))
{
    context.SetField(rule.TargetField, result.Output ?? string.Empty);
}
```

**YAML 配置示例**:
```yaml
# 示例 1: 提取字段
- Id: "extract_title"
  Processor: "Markup.Extract"
  Scope: Field
  TargetField: "Title"
  Parameters:
    TagName: "h1"

# 示例 2: 清理字段
- Id: "clean_title"
  Processor: "Text.Trim"
  Scope: Field
  SourceField: "Title"
  TargetField: "Title"

# 示例 3: 字段转换
- Id: "format_date"
  Processor: "Transform.FormatDate"
  Scope: Field
  SourceField: "RawDate"
  TargetField: "FormattedDate"
```

---

### 3. Global - 处理整个上下文

**定义**: 处理器可以访问整个上下文 (context)，包括所有字段 (fields)、变量 (variables) 等

**执行逻辑**:
- **输入来源**: 处理器接收完整的 `ProcessContext`
- **输出目标**: 处理器可以修改多个字段或执行复杂的跨字段操作
- **字段访问**: 可以访问所有字段和变量

**典型应用场景**:
- 数据聚合 (aggregation)
- 跨字段条件判断
- 复杂的数据组合
- 多字段验证
- 全局状态管理

**代码实现** (RuleEngine.cs):
```csharp
// 第 64-71 行：传递完整上下文
var processorContext = new ProcessContext
{
    SourceContent = sourceContent,
    Fields = new Dictionary<string, string>(context.Fields),      // 所有字段
    Variables = new Dictionary<string, object>(context.Variables), // 所有变量
    Logger = context.Logger,
    CancellationToken = context.CancellationToken
};
```

**YAML 配置示例**:
```yaml
- Id: "aggregate_metadata"
  Processor: "Custom.MetadataAggregator"
  Scope: Global
  # 处理器内部可以访问所有字段
  # 例如: context.Fields["Title"], context.Fields["Summary"], context.Fields["Author"]
  # 并可以设置多个输出字段
```

---

## 三种作用域对比

| 特性 | Source | Field | Global |
|------|--------|-------|--------|
| **输入来源** | `context.SourceContent` | `context.Fields[SourceField]` 或 `SourceContent` | 完整的 `ProcessContext` |
| **输出目标** | 更新 `context.SourceContent` | 更新 `context.Fields[TargetField]` | 可修改多个字段 |
| **字段访问权限** | 只能访问源内容 | 只能访问指定字段 | 可访问所有字段和变量 |
| **典型用途** | 文档预处理、全文清理 | 字段提取、字段转换 | 复杂的跨字段处理 |
| **配置复杂度** | 简单 | 中等（需指定字段名） | 复杂（需处理器支持） |
| **性能影响** | 处理整个文档 | 只处理特定字段 | 可能需要访问多个字段 |
| **默认值** | - | `ProcessorScope.Field` | - |

---

## 实际执行流程示例

### 场景: 文档处理流水线

```yaml
Rules:
  # 步骤 1: 清理源内容
  - Id: "cleanup"
    Processor: "Text.Cleanup"
    Scope: Source
    Priority: 10
  
  # 步骤 2: 提取标题到字段
  - Id: "extract_title"
    Processor: "Markup.Extract"
    Scope: Field
    TargetField: "Title"
    Priority: 20
    Parameters:
      TagName: "h1"
  
  # 步骤 3: 清理标题字段
  - Id: "clean_title"
    Processor: "Text.Trim"
    Scope: Field
    SourceField: "Title"
    TargetField: "Title"
    Priority: 30
  
  # 步骤 4: 提取摘要到字段
  - Id: "extract_summary"
    Processor: "Markup.Extract"
    Scope: Field
    TargetField: "Summary"
    Priority: 40
    Parameters:
      TagName: "summary"
```

### 执行过程分析

1. **cleanup** (Source 作用域):
   - 输入: `context.SourceContent` = 原始文档
   - 处理: 移除 HTML 注释、规范化空行
   - 输出: 更新 `context.SourceContent`

2. **extract_title** (Field 作用域):
   - 输入: `context.SourceContent` (已清理)
   - 处理: 提取 `<h1>` 标签内容
   - 输出: 保存到 `context.Fields["Title"]`

3. **clean_title** (Field 作用域):
   - 输入: `context.Fields["Title"]`
   - 处理: 去除首尾空白
   - 输出: 更新 `context.Fields["Title"]`

4. **extract_summary** (Field 作用域):
   - 输入: `context.SourceContent` (已清理)
   - 处理: 提取 `<summary>` 标签内容
   - 输出: 保存到 `context.Fields["Summary"]`

### 最终状态

```
context.SourceContent: 清理后的完整文档
context.Fields["Title"]: 提取并清理后的标题
context.Fields["Summary"]: 提取的摘要
```

---

## 设计优势

### 1. 职责分离 (Separation of Concerns)
- **Source**: 专注于文档级别的处理
- **Field**: 专注于字段级别的处理
- **Global**: 专注于跨字段的复杂逻辑

### 2. 灵活性 (Flexibility)
- 可以根据需求选择合适的作用域
- 支持从简单到复杂的各种处理场景
- 易于组合和扩展

### 3. 可维护性 (Maintainability)
- 规则配置清晰明确
- 数据流向一目了然
- 易于调试和测试

### 4. 性能优化 (Performance)
- Field 作用域只处理必要的字段，避免不必要的全文处理
- Source 作用域适合一次性处理整个文档
- Global 作用域仅在需要时使用

---

## 相关文件

- **核心定义**: `code/Novex.Analyzer.V2/Models/ProcessorScope.cs`
- **执行引擎**: `code/Novex.Analyzer.V2/Engine/RuleEngine.cs`
- **规则模型**: `code/Novex.Analyzer.V2/Models/ProcessRule.cs`
- **测试用例**: `code/Novex.Analyzer.V2.Tests/Integration/CleanupProcessorIntegrationTests.cs`
- **设计文档**: `docs/rules-v2/02-design-principles.md`

---

## 总结

`ProcessorScope` 的三个值提供了灵活的处理范围控制：

- **Source**: 适合文档级别的预处理和清理
- **Field**: 适合字段级别的提取和转换（最常用）
- **Global**: 适合需要访问多个字段的复杂处理

这种设计使得 Novex.Analyzer V2 的规则系统能够处理从简单的文本清理到复杂的多字段数据提取和转换的各种场景，同时保持配置的简洁性和可维护性。

