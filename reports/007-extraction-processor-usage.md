# 提取结构化数据处理器使用指南

## 概述

`Extraction.ExtractStructuredData` 处理器用于从文本中提取结构化数据。它支持：

- 从特定标签内容中提取（如 `<plot>` 标签）
- 从全文中提取（不需要标签）
- 灵活的字段前缀配置
- 多个字段的组合和分隔

## 处理器信息

- **名称**: `Extraction.ExtractStructuredData`
- **类别**: Extraction
- **描述**: 从文本中提取结构化数据（支持标签和全文提取）

## 参数说明

### 必需参数

| 参数名 | 类型 | 说明 | 示例 |
|--------|------|------|------|
| `Fields` | string | 字段定义，格式: `FieldName1:Prefix1,FieldName2:Prefix2` | `Chapter:当前章节,Event:事件名` |

### 可选参数

| 参数名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `TagName` | string | 无 | 要提取内容的标签名（如 'plot'），不指定则从全文提取 |
| `Separator` | string | `/` | 字段分隔符 |
| `OutputField` | string | `ExtractedData` | 输出字段名，用于保存提取结果 |

## 使用示例

### 示例 1: 从 plot 标签中提取章节和事件名

**输入文本**:
```
<plot>
计算耗时:[5分钟]

当前章节: 初次接触
个人线:[顾云-朋友的女友[$1/100](初识)、送其回家]
当前角色内/衣物:[顾云:上身(白色吊带)，下身(超短百褶裙)，内衣(无)，真空状态]
长期事件：无

事件名:虚假的小白兔(1/5)
事件号码:1
摘要:陈晨开车送林晨的女友顾云回家。途中，陈晨因顾云过于暴露的穿着而开口提醒...
</plot>
```

**YAML 规则**:
```yaml
Version: "2.0"
Description: "从 plot 标签中提取章节和事件名"
Rules:
  - Id: extract_chapter_and_event
    Name: "提取章节和事件名"
    Processor: "Extraction.ExtractStructuredData"
    Scope: "Content"
    Priority: 1
    Enabled: true
    Parameters:
      TagName: "plot"
      Fields: "Chapter:当前章节,Event:事件名"
      Separator: "/"
      OutputField: "Title"
```

**输出**:
```
初次接触/虚假的小白兔(1/5)
```

### 示例 2: 从全文中提取多个字段

**输入文本**:
```
当前章节: 第一章
事件名: 开始的故事
摘要: 这是一个故事的开始
```

**YAML 规则**:
```yaml
Version: "2.0"
Description: "从全文中提取多个字段"
Rules:
  - Id: extract_multiple_fields
    Name: "提取多个字段"
    Processor: "Extraction.ExtractStructuredData"
    Scope: "Content"
    Priority: 1
    Enabled: true
    Parameters:
      Fields: "Chapter:当前章节,Event:事件名,Summary:摘要"
      Separator: " | "
      OutputField: "FullInfo"
```

**输出**:
```
第一章 | 开始的故事 | 这是一个故事的开始
```

### 示例 3: 从自定义标签中提取

**输入文本**:
```
<content>
当前章节: 第二章
事件名: 中间的故事
</content>
```

**YAML 规则**:
```yaml
Version: "2.0"
Description: "从自定义标签中提取"
Rules:
  - Id: extract_from_custom_tag
    Name: "从 content 标签中提取"
    Processor: "Extraction.ExtractStructuredData"
    Scope: "Content"
    Priority: 1
    Enabled: true
    Parameters:
      TagName: "content"
      Fields: "Chapter:当前章节,Event:事件名"
      Separator: "-"
```

**输出**:
```
第二章-中间的故事
```

## 字段定义格式

字段定义使用以下格式：

```
FieldName1:Prefix1,FieldName2:Prefix2,FieldName3:Prefix3
```

- `FieldName`: 字段的逻辑名称（用于识别）
- `Prefix`: 在文本中要查找的前缀（如 "当前章节"、"事件名" 等）

### 前缀匹配规则

处理器会查找以下格式的前缀：
- `前缀:` 后跟内容
- `前缀: ` 后跟内容（带空格）

例如，对于前缀 "当前章节"，以下都会被匹配：
```
当前章节: 初次接触
当前章节:初次接触
```

## 特殊行为

### 1. 空字段处理

如果某个字段的值为空（即前缀后没有内容），该字段会被跳过，不会出现在输出中。

**示例**:
```
当前章节: 第一章
事件名: 
摘要: 有摘要
```

使用 `Fields: "Chapter:当前章节,Event:事件名,Summary:摘要"` 和 `Separator: "/"` 会输出：
```
第一章/有摘要
```

### 2. 标签不存在处理

如果指定了 `TagName` 但文本中不存在该标签，处理器会返回失败。

### 3. 字段顺序

输出中的字段顺序与 `Fields` 参数中定义的顺序相同。

## 常见用途

1. **提取小说章节信息** - 从 `<plot>` 标签中提取章节名和事件名
2. **生成标题** - 组合多个字段生成文档标题
3. **数据结构化** - 将非结构化文本转换为结构化数据
4. **内容分类** - 根据提取的字段对内容进行分类

## 错误处理

处理器会在以下情况返回失败：

1. **缺少 Fields 参数** - 返回错误信息："Fields 参数必需"
2. **指定的标签不存在** - 返回错误信息："未找到标签 <tagname>"
3. **处理异常** - 返回详细的异常信息

## 性能考虑

- 处理器使用正则表达式进行匹配，性能与文本大小和字段数量成正比
- 对于大型文本（>1MB），建议先使用其他处理器进行预处理
- 字段数量建议不超过 20 个

## 与其他处理器的组合

可以与其他处理器组合使用：

```yaml
Rules:
  - Id: cleanup_and_extract
    Name: "清理并提取"
    Processor: "Text.Cleanup"
    Parameters: {}
  
  - Id: extract_data
    Name: "提取数据"
    Processor: "Extraction.ExtractStructuredData"
    Parameters:
      TagName: "plot"
      Fields: "Chapter:当前章节,Event:事件名"
      Separator: "/"
```

## 注意事项

1. 前缀匹配是**不区分大小写**的
2. 提取的值会自动去除首尾空白
3. 尾部的逗号和分号会被自动移除
4. 标签名称匹配是**不区分大小写**的

