# V1RuleBookMigrator 快速开始

## 最简单的用法 (3 行代码)

```csharp
var migrator = new V1RuleBookMigrator();
var v2RuleBook = migrator.MigrateRuleBook(v1RuleBook);
// 完成！v2RuleBook 现在可以用于 V2 规则引擎
```

## 完整工作流程

```csharp
using Novex.Analyzer.Models;
using Novex.Analyzer.V2.Migration;
using Novex.Analyzer.V2.Engine;
using Novex.Analyzer.V2.Registry;
using Novex.Analyzer.V2.Core;

// 1. 创建或加载 V1 规则书
var v1RuleBook = new AnalysisRuleBook
{
    Version = "1.0",
    Description = "我的规则",
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
    }
};

// 2. 迁移到 V2
var migrator = new V1RuleBookMigrator();
var v2RuleBook = migrator.MigrateRuleBook(v1RuleBook);

// 3. 创建处理器注册表
var registry = new ProcessorRegistry();
registry.Register("Regex.Match", typeof(MatchProcessor));
registry.Register("Text.Trim", typeof(TrimProcessor));
// ... 注册其他处理器

// 4. 创建规则引擎
var engine = new RuleEngine(registry);

// 5. 执行规则
var context = new ProcessContext
{
    SourceContent = "Hello World",
    Fields = new Dictionary<string, string>(),
    Variables = new Dictionary<string, object>()
};

var result = await engine.ExecuteRuleBookAsync(v2RuleBook, context);
Console.WriteLine($"成功: {result.Success}");
Console.WriteLine($"输出: {result.Output}");
```

## 关键概念

### V1 规则类型

| 类型 | 用途 | 迁移到 V2 |
|---|---|---|
| **ExtractionRule** | 从文本中提取内容 | ProcessRule (Regex.Match, Markup.SelectNode 等) |
| **TransformationRule** | 转换/清理内容 | ProcessRule (Text.Trim, Regex.Replace 等) |
| **PreparationRules** | 预处理规则 | ProcessRule (优先级最高) |

### YAML 格式 (使用 Pascal Case)

```yaml
Version: 2.0
Description: 我的规则书
Rules:
  - Id: rule1
    Name: 提取标题
    Processor: Regex.Match
    Scope: Field
    SourceField: Source
    TargetField: Title
    Priority: 1
    Enabled: true
    Parameters:
      Pattern: "^(.+?)$"
```

### V1 到 V2 处理器映射

```
Regex + Extract      → Regex.Match
Regex + Remove       → Regex.Replace
Regex + Replace      → Regex.Replace
Markup + Extract     → Markup.SelectNode
Markup + Remove      → Markup.RemoveNode
Text + Extract       → Text.Trim
Text + Remove        → Text.Replace
Text + Replace       → Text.Replace
JsonPath + Extract   → Json.SelectToken
XPath + Extract      → Markup.SelectNode
CssSelector + Extract → Markup.SelectNode

Format               → Text.Trim
Truncate             → Text.Truncate
CleanWhitespace      → Text.Trim
RemoveHtmlComments   → Regex.Replace
RemoveXmlTags        → Regex.Replace
```

## 常见问题

**Q: 禁用的规则会被迁移吗？**
A: 不会。只有 `Enabled = true` 的规则才会被迁移。

**Q: 迁移后的规则优先级是什么？**
A: 按照 PreparationRules → ExtractionRules → TransformationRules 的顺序，自动分配优先级 1, 2, 3, ...

**Q: 如果规则执行失败会怎样？**
A: 所有迁移的规则都使用 `ErrorHandlingStrategy.Skip`，失败时会跳过该规则继续执行下一个。

**Q: 自定义参数会被保留吗？**
A: 是的。V1 规则的所有参数都会被复制到 V2 规则的 `Parameters` 字典中。

## 下一步

1. 查看 `V1RuleBookMigrator_Usage_Example.cs` 了解更多示例
2. 阅读 `V1RuleBookMigrator_Usage_Guide.md` 获取详细文档
3. 查看 `code/Novex.Analyzer.V2/Examples/BasicExample.cs` 了解如何使用 V2 规则引擎

