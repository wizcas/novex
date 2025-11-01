# Novex.Analyzer.V2 规则书生成指南

**版本**: 2.0  
**目标受众**: AI 助手  
**用途**: 用于指导 AI 生成符合 Novex.Analyzer.V2 规范的规则书 (RuleBook) YAML 文件

---

## 目录

1. [概述](#概述)
2. [规则书结构](#规则书结构)
3. [处理器作用域 (ProcessorScope)](#处理器作用域-processorscope)
4. [错误处理策略](#错误处理策略)
5. [内置处理器参考](#内置处理器参考)
6. [规则编写最佳实践](#规则编写最佳实践)
7. [完整示例](#完整示例)
8. [扩展性说明](#扩展性说明)

---

## 概述

Novex.Analyzer.V2 是一个基于规则的文本处理引擎，通过 YAML 格式的规则书 (RuleBook) 定义处理流程。规则书由多个规则 (Rules) 组成，每个规则调用一个处理器 (Processor) 来执行特定的文本处理操作。

### 核心概念

- **RuleBook**: 规则书，包含版本、描述、变量、规则列表等
- **Rule**: 规则，定义单个处理操作
- **Processor**: 处理器，执行具体的文本处理逻辑
- **ProcessContext**: 处理上下文，包含源内容、字段、变量等
- **Priority**: 优先级，数字越小越先执行

---

## 规则书结构

### 基本结构

```yaml
Version: "2.0"                    # 必需，规则书版本
Description: "规则书描述"          # 可选，规则书说明

Variables:                        # 可选，全局变量
  MaxLength: 100
  DefaultSeparator: "/"

Rules:                            # 必需，规则列表
  - Id: "rule_id"                 # 必需，规则唯一标识
    Name: "规则名称"               # 可选，规则显示名称
    Processor: "Text.Trim"        # 必需，处理器名称
    Scope: "Source"               # 可选，默认 Field
    Priority: 10                  # 可选，默认 0，数字越小越先执行
    Enabled: true                 # 可选，默认 true
    OnError: "Skip"               # 可选，错误处理策略
    SourceField: "FieldName"      # 可选，源字段名（Scope=Field 时）
    TargetField: "FieldName"      # 可选，目标字段名
    Parameters:                   # 可选，处理器参数
      ParamName: "value"
```

### 字段说明

| 字段 | 类型 | 必需 | 默认值 | 说明 |
|------|------|------|--------|------|
| `Version` | string | ✅ | - | 规则书版本，当前为 "2.0" |
| `Description` | string | ❌ | null | 规则书描述 |
| `Variables` | object | ❌ | {} | 全局变量字典 |
| `Rules` | array | ✅ | - | 规则列表 |
| `Rules[].Id` | string | ✅ | - | 规则唯一标识 |
| `Rules[].Name` | string | ❌ | null | 规则显示名称 |
| `Rules[].Processor` | string | ✅ | - | 处理器名称 |
| `Rules[].Scope` | enum | ❌ | Field | 处理器作用域 |
| `Rules[].Priority` | int | ❌ | 0 | 执行优先级（越小越先执行） |
| `Rules[].Enabled` | bool | ❌ | true | 是否启用 |
| `Rules[].OnError` | enum | ❌ | Throw | 错误处理策略 |
| `Rules[].SourceField` | string | ❌ | null | 源字段名 |
| `Rules[].TargetField` | string | ❌ | null | 目标字段名 |
| `Rules[].Parameters` | object | ❌ | {} | 处理器参数 |

---

## 处理器作用域 (ProcessorScope)

作用域定义了处理器作用的范围，影响输入来源和输出目标。

### Source - 处理原始源内容

**输入**: `context.SourceContent`（原始输入内容）  
**输出**: 直接更新 `context.SourceContent`（除非指定了 `TargetField`）

**使用场景**:
- 对整个文档进行预处理
- 清理、格式化原始内容
- 提取主要内容块

**示例**:
```yaml
- Id: "cleanup_source"
  Processor: "Text.Trim"
  Scope: Source              # 处理整个源内容
  Priority: 10
```

### Field - 处理特定字段

**输入**: `context.GetField(SourceField)` 或 `context.SourceContent`  
**输出**: 更新 `context.Fields[TargetField]`

**使用场景**:
- 从源内容提取特定字段
- 转换已提取的字段
- 字段间的数据处理

**示例**:
```yaml
- Id: "extract_title"
  Processor: "Extraction.ExtractStructuredData"
  Scope: Field               # 处理字段
  TargetField: "Title"       # 保存到 Title 字段
  Parameters:
    TagName: "plot"
    Fields: "Chapter:当前章节,Event:事件名"
```

### Global - 处理整个上下文

**输入**: 整个 `context`（包括所有字段和变量）  
**输出**: 可以修改多个字段

**使用场景**:
- 复杂的跨字段处理
- 需要访问多个字段的逻辑
- 条件性的字段操作

**注意**: 目前大多数内置处理器使用 Source 或 Field 作用域。Global 作用域为未来扩展预留。

---

## 错误处理策略

定义规则执行失败时的行为。

| 策略 | 说明 | 使用场景 |
|------|------|----------|
| `Throw` | 抛出异常，停止执行 | 关键规则，必须成功 |
| `Skip` | 跳过当前规则，继续执行 | 可选规则，失败不影响后续 |
| `UseDefault` | 使用默认值 | 有备选方案的规则 |
| `Retry` | 重试执行 | 临时性错误可能恢复 |

**示例**:
```yaml
- Id: "optional_extraction"
  Processor: "Markup.SelectNode"
  OnError: "Skip"            # 如果节点不存在，跳过此规则
  Parameters:
    XPath: "//optional-tag"
```

---

## 内置处理器参考

### 处理器快速参考表

| 处理器 | 类别 | 功能 | 常用场景 |
|--------|------|------|----------|
| Text.Trim | Text | 修剪空白 | 清理首尾空白 |
| Text.Truncate | Text | 截断文本 | 限制长度 |
| Text.Replace | Text | 替换文本 | 简单文本替换 |
| Text.Cleanup | Text | 清理文本 | 移除注释、思考块 |
| Text.RemoveContentBlocks | Text | 移除内容块 | 移除特定标记的内容 |
| Regex.Match | Regex | 正则匹配 | 提取匹配内容 |
| Regex.Replace | Regex | 正则替换 | 复杂模式替换 |
| Markup.ExtractText | Markup | 提取文本 | HTML 转纯文本 |
| Markup.SelectNode | Markup | 选择节点 | XPath 提取 |
| Extraction.ExtractStructuredData | Extraction | 提取结构化数据 | 从标签提取字段 |
| Transform.ToUpper | Transform | 转大写 | 文本转换 |
| Transform.ToLower | Transform | 转小写 | 文本转换 |
| Json.Extract | Json | 提取 JSON | JSON 数据提取 |
| Conditional.If | Conditional | 条件判断 | 条件处理 |
| Pipeline.Chain | Pipeline | 管道链接 | 串联处理器 |

### Text 处理器

#### Text.Trim - 修剪空白

**功能**: 移除字符串首尾的空白字符

**参数**:
- `TrimChars` (string, 可选): 要移除的字符集，默认为空白字符

**示例**:
```yaml
- Id: "trim_content"
  Processor: "Text.Trim"
  Scope: Source
```

#### Text.Truncate - 截断文本

**功能**: 截断文本到指定长度

**参数**:
- `MaxLength` (int, 必需): 最大长度
- `AddEllipsis` (bool, 可选): 是否添加省略号，默认 true
- `Ellipsis` (string, 可选): 省略号文本，默认 "..."

**示例**:
```yaml
- Id: "truncate_summary"
  Processor: "Text.Truncate"
  SourceField: "Summary"
  TargetField: "ShortSummary"
  Parameters:
    MaxLength: 100
    AddEllipsis: true
```

#### Text.Replace - 替换文本

**功能**: 替换文本中的指定内容

**参数**:
- `OldValue` (string, 必需): 要替换的文本
- `NewValue` (string, 可选): 替换为的文本，默认 ""
- `IgnoreCase` (bool, 可选): 是否忽略大小写，默认 false

**示例**:
```yaml
- Id: "replace_text"
  Processor: "Text.Replace"
  Scope: Source
  Parameters:
    OldValue: "old"
    NewValue: "new"
    IgnoreCase: true
```

#### Text.Cleanup - 清理文本

**功能**: 移除 HTML 注释、思考块等（一站式清理）

**参数**: 无

**清理内容**:
- HTML 注释 `<!-- ... -->`
- 思考块 `<think>...</think>`
- 对话思考注释 `<!-- dialogue_antThinking: ... -->`
- 多余空行

**示例**:
```yaml
- Id: "cleanup"
  Processor: "Text.Cleanup"
  Scope: Source
```

#### Text.RemoveContentBlocks - 移除内容块

**功能**: 根据配置的开始和结束标记移除内容块

**参数**:
- `Start` (string, 可选): 内容块开始标记
- `End` (string, 可选): 内容块结束标记
- `Blocks` (array, 可选): 多个内容块配置（每个包含 Start 和 End）

**示例 1 - 单个块**:
```yaml
- Id: "remove_weibo"
  Processor: "Text.RemoveContentBlocks"
  Scope: Source
  Parameters:
    Start: "<!-- WEIBO_CONTENT_START -->"
    End: "<!-- WEIBO_CONTENT_END -->"
```

**示例 2 - 多个块**:
```yaml
- Id: "remove_multiple_blocks"
  Processor: "Text.RemoveContentBlocks"
  Scope: Source
  Parameters:
    Blocks:
      - Start: "<!-- FORUM_CONTENT_START -->"
        End: "<!-- FORUM_CONTENT_END -->"
      - Start: "<!-- WEIBO_CONTENT_START -->"
        End: "<!-- WEIBO_CONTENT_END -->"
```

---

### Regex 处理器

#### Regex.Match - 正则匹配

**功能**: 使用正则表达式匹配文本

**参数**:
- `Pattern` (string, 必需): 正则表达式模式
- `IgnoreCase` (bool, 可选): 是否忽略大小写
- `GroupIndex` (int, 可选): 捕获组索引，默认 0（整个匹配）

**示例**:
```yaml
- Id: "extract_number"
  Processor: "Regex.Match"
  Scope: Source
  Parameters:
    Pattern: '\d+'
    GroupIndex: 0
```

#### Regex.Replace - 正则替换

**功能**: 使用正则表达式替换文本

**参数**:
- `Pattern` (string, 必需): 正则表达式模式
- `Replacement` (string, 可选): 替换文本，默认 ""
- `RegexOptions` (string, 可选): 正则选项，如 "Singleline,IgnoreCase"

**重要**: 支持转义序列 `\n`, `\r`, `\t`, `\\`

**示例**:
```yaml
- Id: "remove_think_blocks"
  Processor: "Regex.Replace"
  Scope: Source
  Parameters:
    Pattern: '<think>.*?</think>'
    Replacement: ''
    RegexOptions: "Singleline,IgnoreCase"
```

---

### Markup 处理器

#### Markup.ExtractText - 提取文本

**功能**: 从 HTML/XML 中提取纯文本

**参数**:
- `CleanWhitespace` (bool, 可选): 是否清理多余空白

**示例**:
```yaml
- Id: "extract_text"
  Processor: "Markup.ExtractText"
  Scope: Source
  Parameters:
    CleanWhitespace: true
```

#### Markup.SelectNode - 选择节点

**功能**: 使用 XPath 选择 HTML/XML 节点

**参数**:
- `XPath` (string, 必需): XPath 表达式
- `InnerHtml` (bool, 可选): 是否返回 InnerHtml，默认 false（返回 OuterHtml）

**示例**:
```yaml
- Id: "extract_content"
  Processor: "Markup.SelectNode"
  Scope: Source
  OnError: "Skip"
  Parameters:
    XPath: "//content"
    InnerHtml: true
```

---

### Extraction 处理器

#### Extraction.ExtractStructuredData - 提取结构化数据

**功能**: 从文本中提取结构化字段，支持从标签内容或全文中提取

**参数**:
- `TagName` (string, 可选): 要提取内容的标签名（如 "plot"），不指定则从全文提取
- `Fields` (string, 必需): 字段定义，格式: "FieldName1:Prefix1,FieldName2:Prefix2"
- `Separator` (string, 可选): 字段分隔符，默认 "/"

**示例**:
```yaml
- Id: "extract_plot_title"
  Processor: "Extraction.ExtractStructuredData"
  Scope: Source
  TargetField: "Title"
  Parameters:
    TagName: "plot"
    Fields: "Chapter:当前章节,Event:事件名"
    Separator: "/"
```

**输入示例**:
```xml
<plot>
当前章节: 初次接触
事件名: 虚假的小白兔(1/5)
摘要: 这是摘要内容
</plot>
```

**输出**: `初次接触/虚假的小白兔(1/5)`

---

### Transform 处理器

#### Transform.ToUpper - 转换为大写

**功能**: 将文本转换为大写

**参数**: 无

**示例**:
```yaml
- Id: "to_upper"
  Processor: "Transform.ToUpper"
  SourceField: "Title"
  TargetField: "UpperTitle"
```

#### Transform.ToLower - 转换为小写

**功能**: 将文本转换为小写

**参数**: 无

**示例**:
```yaml
- Id: "to_lower"
  Processor: "Transform.ToLower"
  Scope: Source
```

---

### Json 处理器

#### Json.Extract - 提取 JSON 值

**功能**: 从 JSON 中提取值

**参数**:
- `Path` (string, 必需): JSON 路径（点分隔，如 "user.name"）

**示例**:
```yaml
- Id: "extract_json"
  Processor: "Json.Extract"
  Scope: Source
  Parameters:
    Path: "user.name"
```

---

### Conditional 处理器

#### Conditional.If - 条件判断

**功能**: 根据条件返回不同的值

**参数**:
- `Condition` (string, 必需): 条件表达式（支持: empty, notempty, equals, contains）
- `TrueValue` (string, 可选): 条件为真时的值
- `FalseValue` (string, 可选): 条件为假时的值

**支持的条件**:
- `empty`: 输入为空
- `notempty`: 输入不为空

**示例**:
```yaml
- Id: "check_empty"
  Processor: "Conditional.If"
  SourceField: "Title"
  TargetField: "DisplayTitle"
  Parameters:
    Condition: "empty"
    TrueValue: "无标题"
    FalseValue: "{{input}}"  # 保留原值
```

---

### Pipeline 处理器

#### Pipeline.Chain - 管道链接

**功能**: 按顺序执行多个处理器

**参数**:
- `Processors` (string, 必需): 处理器名称列表（逗号分隔）

**示例**:
```yaml
- Id: "chain_processors"
  Processor: "Pipeline.Chain"
  Scope: Source
  Parameters:
    Processors: "Text.Trim,Transform.ToUpper"
```

**注意**: 此处理器适用于简单的处理器链接。对于复杂的处理流程，建议使用多个独立规则。

---

## 规则编写最佳实践

### 1. 优先级规划

**原则**: 数据提取优先于数据清理

```yaml
Rules:
  # Priority 5-10: 数据提取（从原始内容）
  - Id: "extract_title"
    Priority: 5
    Processor: "Extraction.ExtractStructuredData"
  
  # Priority 20-50: 内容清理
  - Id: "remove_comments"
    Priority: 20
    Processor: "Regex.Replace"
  
  # Priority 100+: 最终处理
  - Id: "trim_final"
    Priority: 100
    Processor: "Text.Trim"
```

### 2. 错误处理

**原则**: 关键规则使用 Throw，可选规则使用 Skip

```yaml
Rules:
  # 关键规则：必须成功
  - Id: "critical_extraction"
    Processor: "Markup.SelectNode"
    OnError: "Throw"
  
  # 可选规则：失败不影响后续
  - Id: "optional_field"
    Processor: "Extraction.ExtractStructuredData"
    OnError: "Skip"
```

### 3. 字段命名

**原则**: 使用清晰、一致的字段名

```yaml
# ✅ 推荐
TargetField: "Title"
TargetField: "Summary"
TargetField: "MainBody"

# ❌ 不推荐
TargetField: "field1"
TargetField: "temp"
```

### 4. 作用域选择

**原则**: 根据处理目标选择合适的作用域

```yaml
# 处理整个文档 → Source
- Scope: Source
  Processor: "Text.Trim"

# 提取或转换字段 → Field
- Scope: Field
  TargetField: "Title"
  Processor: "Extraction.ExtractStructuredData"
```

---

## 完整示例

### 示例 1: Dream Phone 场景处理

```yaml
Version: "2.0"
Description: "Dream Phone 对话记录处理规则"

Rules:
  # 第1步：提取标题（优先执行）
  - Id: "ExtractPlotTitle"
    Name: "从 plot 标签提取标题"
    Processor: "Extraction.ExtractStructuredData"
    Scope: Source
    TargetField: "Title"
    Priority: 5
    Enabled: true
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Chapter:当前章节,Event:事件名"
      Separator: "/"

  # 第2步：提取摘要（优先执行）
  - Id: "ExtractPlotSummary"
    Name: "从 plot 标签提取摘要"
    Processor: "Extraction.ExtractStructuredData"
    Scope: Source
    TargetField: "Summary"
    Priority: 6
    Enabled: true
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Summary:摘要"

  # 第3步：移除思考块
  - Id: "RemoveThinkBlocks"
    Name: "移除思考块"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 10
    Enabled: true
    Parameters:
      Pattern: '<think>.*?</think>'
      Replacement: ''
      RegexOptions: "Singleline,IgnoreCase"

  # 第4步：移除 HTML 注释
  - Id: "RemoveHtmlComments"
    Name: "移除 HTML 注释"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 20
    Enabled: true
    Parameters:
      Pattern: '<!--.*?-->'
      Replacement: ''
      RegexOptions: "Singleline"

  # 第5步：提取内容块（如果存在）
  - Id: "ExtractContentBlock"
    Name: "提取内容块"
    Processor: "Markup.SelectNode"
    Scope: Source
    Priority: 40
    Enabled: true
    OnError: "Skip"
    Parameters:
      XPath: "//content"
      InnerHtml: true

  # 第6步：合并多余空行
  - Id: "MergeBlankLines"
    Name: "合并多余空行"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 50
    Enabled: true
    Parameters:
      Pattern: "\\n\\n+"
      Replacement: "\\n\\n"

  # 第7步：修剪首尾空白
  - Id: "TrimContent"
    Name: "修剪首尾空白"
    Processor: "Text.Trim"
    Scope: Source
    Priority: 100
    Enabled: true
```

---

## 扩展性说明

### 动态处理器注册

Novex.Analyzer.V2 支持动态发现和注册处理器：

1. **程序集发现**: 从指定程序集自动发现标记了 `[Processor]` 特性的处理器
2. **目录发现**: 从指定目录加载 DLL 并发现处理器
3. **手动注册**: 通过 `IProcessorRegistry` 手动注册处理器

### 第三方处理器

第三方可以通过以下方式添加自定义处理器：

1. 实现 `IProcessor` 接口
2. 添加 `[Processor("YourCategory.YourProcessor")]` 特性
3. 可选实现 `IProcessorMetadata` 接口提供元数据
4. 将 DLL 放入指定目录或通过代码注册

**示例**:
```csharp
[Processor("Custom.MyProcessor", Category = "Custom", Description = "我的自定义处理器")]
public class MyProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Custom.MyProcessor";
    public string DisplayName => "我的处理器";
    public string Description => "自定义处理逻辑";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        // 实现处理逻辑
        return Task.FromResult(ProcessResult.Ok(result));
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        // 返回参数定义
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        // 返回使用示例
    }
}
```

### 已实现的处理器类别

当前系统包含以下类别的处理器：

1. **Text** (文本处理): Trim, Truncate, Replace, Cleanup, RemoveContentBlocks
2. **Regex** (正则表达式): Match, Replace
3. **Markup** (标记语言): ExtractText, SelectNode
4. **Extraction** (数据提取): ExtractStructuredData
5. **Transform** (转换): ToUpper, ToLower
6. **Json** (JSON 处理): Extract
7. **Conditional** (条件): If
8. **Pipeline** (管道): Chain

### 未来可能添加的处理器

以下是可能在未来版本中添加的处理器：

- **File.Read**: 文件读取处理器
- **File.Write**: 文件写入处理器
- **Http.Fetch**: HTTP 请求处理器
- **Template.Render**: 模板渲染处理器（如 Liquid, Mustache）
- **Crypto.Hash**: 加密哈希处理器（MD5, SHA256 等）
- **Crypto.Encrypt**: 加密处理器
- **DateTime.Format**: 日期时间格式化处理器
- **DateTime.Parse**: 日期时间解析处理器
- **Math.Calculate**: 数学计算处理器
- **String.Split**: 字符串分割处理器
- **String.Join**: 字符串连接处理器
- **Array.Map**: 数组映射处理器
- **Array.Filter**: 数组过滤处理器
- **Validation.Email**: 邮箱验证处理器
- **Validation.Url**: URL 验证处理器
- **Cache.Get**: 缓存读取处理器
- **Cache.Set**: 缓存写入处理器

---

## 生成规则书时的注意事项

### 1. 必需字段检查

确保以下字段存在：
- ✅ `Version: "2.0"`
- ✅ `Rules` 数组
- ✅ 每个规则的 `Id` 和 `Processor`

### 2. 优先级分配

- 数据提取: 1-10
- 内容清理: 11-50
- 格式化: 51-99
- 最终处理: 100+

### 3. 错误处理

- 关键规则: `OnError: "Throw"`
- 可选规则: `OnError: "Skip"`
- 有默认值: `OnError: "UseDefault"`

### 4. 参数验证

确保处理器参数符合要求：
- 必需参数必须提供
- 参数类型正确（string, int, bool 等）
- 正则表达式语法正确
- XPath 表达式有效

### 5. YAML 格式

- 使用 2 空格缩进
- 字符串包含特殊字符时使用引号
- 布尔值使用 `true`/`false`
- 数字不使用引号

---

## 总结

本指南提供了生成 Novex.Analyzer.V2 规则书所需的所有信息：

1. ✅ 规则书结构和字段说明
2. ✅ 处理器作用域详解
3. ✅ 错误处理策略
4. ✅ 所有内置处理器的完整参考
5. ✅ 规则编写最佳实践
6. ✅ 完整的实际示例
7. ✅ 扩展性说明

使用本指南，AI 助手可以：
- 理解规则书的结构和语法
- 选择合适的处理器完成任务
- 设置正确的优先级和作用域
- 处理错误情况
- 生成符合规范的 YAML 文件

**重要提醒**: 
- 始终从原始内容提取数据，然后再清理
- 使用清晰的规则 ID 和名称
- 合理设置优先级
- 为可选操作设置 `OnError: "Skip"`
- 保持 YAML 格式正确

---

**文档版本**: 1.0  
**最后更新**: 2025-11-01  
**维护者**: Novex.Analyzer.V2 Team

