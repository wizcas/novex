# 内置处理器目录

## 处理器分类

Rules V2 提供以下类别的内置处理器：

1. **Text** - 文本处理
2. **Regex** - 正则表达式操作
3. **Markup** - 标记语言处理（HTML/XML）
4. **Json** - JSON 处理
5. **Pipeline** - 组合和流程控制
6. **Conditional** - 条件执行
7. **Transform** - 数据转换

## 1. Text 处理器

### Text.Trim

移除字符串首尾的空白字符。

**参数：**
- `TrimChars` (string, 可选) - 要移除的字符集，默认为空白字符

**示例：**
```yaml
- Processor: "Text.Trim"
  Parameters:
    TrimChars: " \t\n"
```

### Text.Truncate

截断文本到指定长度。

**参数：**
- `MaxLength` (int, 必需) - 最大长度
- `AddEllipsis` (bool, 可选) - 是否添加省略号，默认 true
- `Ellipsis` (string, 可选) - 省略号文本，默认 "..."
- `BreakOnWord` (bool, 可选) - 是否在单词边界截断，默认 false

**示例：**
```yaml
- Processor: "Text.Truncate"
  Parameters:
    MaxLength: 100
    AddEllipsis: true
    BreakOnWord: true
```

### Text.NormalizeWhitespace

规范化空白字符（将多个空白字符替换为单个空格）。

**参数：**
- `PreserveNewlines` (bool, 可选) - 是否保留换行符，默认 false
- `MaxConsecutiveNewlines` (int, 可选) - 最大连续换行数，默认 2

**示例：**
```yaml
- Processor: "Text.NormalizeWhitespace"
  Parameters:
    PreserveNewlines: true
    MaxConsecutiveNewlines: 1
```

### Text.Replace

简单的字符串替换。

**参数：**
- `Find` (string, 必需) - 要查找的文本
- `Replace` (string, 必需) - 替换文本
- `IgnoreCase` (bool, 可选) - 是否忽略大小写，默认 false
- `ReplaceAll` (bool, 可选) - 是否替换所有匹配，默认 true

**示例：**
```yaml
- Processor: "Text.Replace"
  Parameters:
    Find: "old text"
    Replace: "new text"
    IgnoreCase: true
```

### Text.ToUpper / Text.ToLower

转换大小写。

**参数：** 无

**示例：**
```yaml
- Processor: "Text.ToUpper"
```

### Text.LimitLines

限制文本行数。

**参数：**
- `MaxLines` (int, 可选) - 最大行数
- `MaxConsecutiveEmpty` (int, 可选) - 最大连续空行数

**示例：**
```yaml
- Processor: "Text.LimitLines"
  Parameters:
    MaxLines: 100
    MaxConsecutiveEmpty: 1
```

## 2. Regex 处理器

### Regex.Replace

使用正则表达式替换文本。

**参数：**
- `Pattern` (string, 必需) - 正则表达式模式
- `Replacement` (string, 必需) - 替换文本（支持捕获组引用 $1, $2 等）
- `IgnoreCase` (bool, 可选) - 是否忽略大小写，默认 false
- `Multiline` (bool, 可选) - 多行模式，默认 false
- `Singleline` (bool, 可选) - 单行模式（. 匹配换行符），默认 false

**示例：**
```yaml
- Processor: "Regex.Replace"
  Parameters:
    Pattern: "<!--.*?-->"
    Replacement: ""
    Singleline: true
```

### Regex.Extract

使用正则表达式提取文本。

**参数：**
- `Pattern` (string, 必需) - 正则表达式模式
- `Group` (int, 可选) - 捕获组索引，默认 1
- `GroupName` (string, 可选) - 命名捕获组名称
- `AllMatches` (bool, 可选) - 是否提取所有匹配，默认 false
- `Separator` (string, 可选) - 多个匹配的分隔符，默认 "\n"

**示例：**
```yaml
- Processor: "Regex.Extract"
  Parameters:
    Pattern: "<title>(.*?)</title>"
    Group: 1
```

### Regex.Match

检查文本是否匹配正则表达式（返回布尔值到变量）。

**参数：**
- `Pattern` (string, 必需) - 正则表达式模式
- `VariableName` (string, 必需) - 存储结果的变量名
- `IgnoreCase` (bool, 可选) - 是否忽略大小写

**示例：**
```yaml
- Processor: "Regex.Match"
  Parameters:
    Pattern: "^\\d{4}-\\d{2}-\\d{2}$"
    VariableName: "IsDateFormat"
```

### Regex.Split

使用正则表达式分割文本。

**参数：**
- `Pattern` (string, 必需) - 分割模式
- `MaxSplits` (int, 可选) - 最大分割次数
- `RemoveEmpty` (bool, 可选) - 是否移除空项，默认 true

**示例：**
```yaml
- Processor: "Regex.Split"
  Parameters:
    Pattern: "\\s*,\\s*"
    RemoveEmpty: true
```

## 3. Markup 处理器

### Markup.Extract

使用 XPath、CSS 选择器或标签名提取内容。

**参数：**
- `Selector` (string, 可选) - CSS 选择器
- `XPath` (string, 可选) - XPath 表达式
- `TagName` (string, 可选) - 标签名
- `Attribute` (string, 可选) - 提取属性值而非文本内容
- `InnerHtml` (bool, 可选) - 提取 HTML 而非纯文本，默认 false
- `AllMatches` (bool, 可选) - 提取所有匹配，默认 false
- `Separator` (string, 可选) - 多个匹配的分隔符

**示例：**
```yaml
# 使用 CSS 选择器
- Processor: "Markup.Extract"
  Parameters:
    Selector: "div.content > p"
    AllMatches: true
    Separator: "\n\n"

# 使用 XPath
- Processor: "Markup.Extract"
  Parameters:
    XPath: "//dream/text()"

# 使用标签名
- Processor: "Markup.Extract"
  Parameters:
    TagName: "dream"
```

### Markup.Remove

移除指定的 HTML/XML 元素。

**参数：**
- `Selector` (string, 可选) - CSS 选择器
- `XPath` (string, 可选) - XPath 表达式
- `TagNames` (string[], 可选) - 要移除的标签名列表

**示例：**
```yaml
- Processor: "Markup.Remove"
  Parameters:
    TagNames: ["script", "style", "iframe"]
```

### Markup.StripTags

移除所有 HTML/XML 标签，保留纯文本。

**参数：**
- `KeepTags` (string[], 可选) - 要保留的标签列表
- `DecodeEntities` (bool, 可选) - 是否解码 HTML 实体，默认 true

**示例：**
```yaml
- Processor: "Markup.StripTags"
  Parameters:
    KeepTags: ["p", "br"]
    DecodeEntities: true
```

### Markup.DecodeEntities

解码 HTML 实体（如 `&amp;` → `&`）。

**参数：** 无

**示例：**
```yaml
- Processor: "Markup.DecodeEntities"
```

### Markup.FixUnclosed

修复未闭合的 HTML 标签。

**参数：**
- `SelfClosingTags` (string[], 可选) - 自闭合标签列表

**示例：**
```yaml
- Processor: "Markup.FixUnclosed"
  Parameters:
    SelfClosingTags: ["br", "hr", "img", "input"]
```

## 4. Json 处理器

### Json.Extract

从 JSON 中提取值。

**参数：**
- `Path` (string, 必需) - JSONPath 表达式
- `DefaultValue` (string, 可选) - 未找到时的默认值

**示例：**
```yaml
- Processor: "Json.Extract"
  Parameters:
    Path: "$.data.title"
    DefaultValue: "Untitled"
```

### Json.Parse

解析 JSON 并存储到变量。

**参数：**
- `VariableName` (string, 必需) - 存储解析结果的变量名

**示例：**
```yaml
- Processor: "Json.Parse"
  Parameters:
    VariableName: "ParsedData"
```

## 5. Pipeline 处理器

### Pipeline

按顺序执行多个处理器。

**参数：**
- `Steps` (array, 必需) - 处理步骤列表

**示例：**
```yaml
- Processor: "Pipeline"
  Parameters:
    Steps:
      - Type: "Markup.StripTags"
      - Type: "Text.Trim"
      - Type: "Text.NormalizeWhitespace"
      - Type: "Text.Truncate"
        Parameters:
          MaxLength: 200
```

### Pipeline.Conditional

根据条件执行不同的处理器。

**参数：**
- `Condition` (string, 必需) - 条件表达式
- `ThenSteps` (array, 必需) - 条件为真时执行的步骤
- `ElseSteps` (array, 可选) - 条件为假时执行的步骤

**示例：**
```yaml
- Processor: "Pipeline.Conditional"
  Parameters:
    Condition: "Length > 100"
    ThenSteps:
      - Type: "Text.Truncate"
        Parameters:
          MaxLength: 100
    ElseSteps:
      - Type: "Text.Trim"
```

## 6. Conditional 处理器

### Conditional.IfEmpty

如果输入为空，使用默认值或执行其他处理器。

**参数：**
- `DefaultValue` (string, 可选) - 默认值
- `FallbackProcessor` (string, 可选) - 回退处理器
- `FallbackParameters` (object, 可选) - 回退处理器参数

**示例：**
```yaml
- Processor: "Conditional.IfEmpty"
  Parameters:
    FallbackProcessor: "Text.Extract"
    FallbackParameters:
      SourceField: "MainBody"
      MaxLength: 100
```

### Conditional.IfMatches

如果输入匹配条件，执行处理器。

**参数：**
- `Pattern` (string, 必需) - 正则表达式模式
- `ThenProcessor` (string, 必需) - 匹配时执行的处理器
- `ThenParameters` (object, 可选) - 处理器参数
- `ElseProcessor` (string, 可选) - 不匹配时执行的处理器
- `ElseParameters` (object, 可选) - 处理器参数

**示例：**
```yaml
- Processor: "Conditional.IfMatches"
  Parameters:
    Pattern: "^https?://"
    ThenProcessor: "Url.Clean"
    ElseProcessor: "Text.Trim"
```

## 7. Transform 处理器

### Transform.Template

使用模板生成文本。

**参数：**
- `Template` (string, 必需) - 模板字符串（支持 {FieldName} 占位符）
- `Fields` (object, 可选) - 额外的字段映射

**示例：**
```yaml
- Processor: "Transform.Template"
  Parameters:
    Template: "Title: {Title}\nSummary: {Summary}"
```

### Transform.Merge

合并多个字段。

**参数：**
- `SourceFields` (string[], 必需) - 源字段列表
- `Separator` (string, 可选) - 分隔符，默认 "\n"
- `SkipEmpty` (bool, 可选) - 是否跳过空字段，默认 true

**示例：**
```yaml
- Processor: "Transform.Merge"
  Parameters:
    SourceFields: ["Title", "Summary", "MainBody"]
    Separator: "\n\n"
    SkipEmpty: true
```

### Transform.Map

映射值（字典查找）。

**参数：**
- `Mappings` (object, 必需) - 映射字典
- `DefaultValue` (string, 可选) - 未找到时的默认值
- `IgnoreCase` (bool, 可选) - 是否忽略大小写

**示例：**
```yaml
- Processor: "Transform.Map"
  Parameters:
    Mappings:
      "en": "English"
      "zh": "中文"
      "ja": "日本語"
    DefaultValue: "Unknown"
```

## 处理器命名约定

所有处理器遵循以下命名约定：

- **格式**: `Category.Action`
- **Category**: 处理器类别（Text, Regex, Markup 等）
- **Action**: 具体操作（Trim, Replace, Extract 等）
- **使用 PascalCase**

**示例：**
- `Text.Trim`
- `Regex.Replace`
- `Markup.Extract`
- `Pipeline.Conditional`

## 扩展处理器

用户可以通过以下方式添加自定义处理器：

1. 实现 `IProcessor` 接口
2. 使用 `[Processor("Custom.Name")]` 特性标记
3. 将程序集放在 `plugins` 目录下
4. 处理器将自动被发现和注册

**示例：**
```csharp
[Processor("Custom.MyProcessor", Category = "Custom")]
public class MyProcessor : IProcessor
{
    public string Name => "Custom.MyProcessor";
    
    public Task<ProcessResult> ProcessAsync(
        ProcessContext context, 
        ProcessorParameters parameters)
    {
        // 自定义处理逻辑
        var input = context.SourceContent;
        var output = DoSomething(input);
        return Task.FromResult(ProcessResult.Ok(output));
    }
}
```

## 下一步

- 查看 `05-migration-guide.md` 了解如何从 V1 迁移
- 查看 `06-implementation-plan.md` 了解实现计划

