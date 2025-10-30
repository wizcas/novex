# V1RuleBookMigrator 使用指南

## 概述

`V1RuleBookMigrator` 是一个工具类，用于将 Novex.Analyzer V1 的规则书 (`AnalysisRuleBook`) 自动转换为 V2 的规则书 (`RuleBook`)。

## 基本用法

### 第一步：创建 V1 规则书

```csharp
var v1RuleBook = new AnalysisRuleBook
{
    Version = "1.0",
    Description = "我的规则书",
    ExtractionRules = new List<ExtractionRule>
    {
        new ExtractionRule
        {
            Id = "rule1",
            Name = "提取标题",
            MatcherType = MatcherType.Regex,
            Pattern = @"^(.+?)$",
            Action = ActionType.Extract,
            Target = TargetField.Title,
            Enabled = true
        }
    },
    TransformationRules = new List<TransformationRule>
    {
        new TransformationRule
        {
            Id = "transform1",
            Name = "清理空白",
            SourceField = "MainBody",
            TargetField = "MainBody",
            TransformationType = TransformationType.CleanWhitespace,
            Enabled = true
        }
    }
};
```

### 第二步：执行迁移

```csharp
var migrator = new V1RuleBookMigrator();
var v2RuleBook = migrator.MigrateRuleBook(v1RuleBook);
```

### 第三步：使用迁移后的规则

```csharp
// 创建处理器注册表
var registry = new ProcessorRegistry();
registry.Register("Text.Cleanup", typeof(CleanupProcessor));
registry.Register("Regex.Match", typeof(MatchProcessor));
// ... 注册其他处理器

// 创建规则引擎
var engine = new RuleEngine(registry);

// 执行规则
var context = new ProcessContext
{
    SourceContent = "原始内容",
    Fields = new Dictionary<string, string>(),
    Variables = new Dictionary<string, object>()
};

var result = await engine.ExecuteRuleBookAsync(v2RuleBook, context);
```

## 处理器映射

### 提取规则映射 (ExtractionRule)

| V1 MatcherType | V1 ActionType | V2 处理器 |
|---|---|---|
| Regex | Extract | Regex.Match |
| Regex | Remove | Regex.Replace |
| Regex | Replace | Regex.Replace |
| Markup | Extract | Markup.SelectNode |
| Markup | Remove | Markup.RemoveNode |
| Text | Extract | Text.Trim |
| Text | Remove | Text.Replace |
| Text | Replace | Text.Replace |
| JsonPath | Extract | Json.SelectToken |
| XPath | Extract | Markup.SelectNode |
| CssSelector | Extract | Markup.SelectNode |

### 转换规则映射 (TransformationRule)

| V1 TransformationType | V2 处理器 |
|---|---|
| Format | Text.Trim |
| Truncate | Text.Truncate |
| CleanWhitespace | Text.Trim |
| RemoveHtmlComments | Regex.Replace |
| RemoveXmlTags | Regex.Replace |

## 重要注意事项

### 1. 禁用的规则会被跳过

如果 V1 规则的 `Enabled` 属性为 `false`，迁移工具会跳过该规则，不会将其添加到 V2 规则书中。

```csharp
var rule = new ExtractionRule
{
    Enabled = false  // 这个规则不会被迁移
};
```

### 2. 规则优先级

迁移工具会自动为 V2 规则分配优先级，按照以下顺序：
1. 预处理规则 (PreparationRules) - 优先级 1, 2, 3, ...
2. 提取规则 (ExtractionRules) - 优先级继续递增
3. 转换规则 (TransformationRules) - 优先级继续递增

### 3. 错误处理策略

所有迁移的规则都使用 `ErrorHandlingStrategy.Skip` 作为错误处理策略，这意味着如果规则执行失败，会跳过该规则继续执行下一个。

### 4. 字段映射

V1 的 `TargetField` 枚举值会被映射到 V2 的字符串字段名（使用 Pascal Case）：

| V1 TargetField | V2 字段名 |
|---|---|
| Title | "Title" |
| Summary | "Summary" |
| MainBody | "MainBody" |
| Source | "Source" |
| Custom | 使用 CustomTargetName |
| Ignore | "Source" (默认) |

### 5. 参数转换

V1 规则的参数会被直接复制到 V2 规则的 `Parameters` 字典中。

## 常见场景

### 场景 1: 迁移简单的正则表达式规则

```csharp
var v1Rule = new ExtractionRule
{
    Id = "extract_email",
    Name = "提取邮箱",
    MatcherType = MatcherType.Regex,
    Pattern = @"[\w\.-]+@[\w\.-]+\.\w+",
    Action = ActionType.Extract,
    Target = TargetField.Custom,
    CustomTargetName = "email",
    Enabled = true
};

// 迁移后会生成:
// Processor: "Regex.Match"
// TargetField: "email"
// Parameters: { "pattern": "[\w\.-]+@[\w\.-]+" }
```

### 场景 2: 迁移清理规则

```csharp
var v1Rule = new TransformationRule
{
    Id = "cleanup",
    Name = "清理 HTML 注释",
    SourceField = "MainBody",
    TargetField = "MainBody",
    TransformationType = TransformationType.RemoveHtmlComments,
    Enabled = true
};

// 迁移后会生成:
// Processor: "Regex.Replace"
// SourceField: "MainBody"
// TargetField: "MainBody"
```

## 故障排除

### 问题 1: 迁移后规则数量少于预期

**原因**: 禁用的规则或不支持的规则类型被跳过了。

**解决方案**: 检查 V1 规则的 `Enabled` 属性，确保规则已启用。

### 问题 2: 处理器未找到

**原因**: 处理器未在注册表中注册。

**解决方案**: 确保在创建规则引擎前，已经注册了所有必需的处理器。

```csharp
var registry = new ProcessorRegistry();
registry.Register("Regex.Match", typeof(MatchProcessor));
registry.Register("Text.Trim", typeof(TrimProcessor));
// ... 注册其他处理器
```

## 完整示例

参考 `V1RuleBookMigrator_Usage_Example.cs` 文件中的示例代码。

