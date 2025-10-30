# V1 到 V2 规则迁移工具

## 概述

本目录包含 V1 到 V2 规则迁移的所有工具和文档。

## 文件说明

### 核心文件

- **V1RuleBookMigrator.cs** - 迁移工具的主要实现
  - 将 V1 `AnalysisRuleBook` 转换为 V2 `RuleBook`
  - 支持 ExtractionRule、TransformationRule、PreparationRules 的迁移
  - 自动处理处理器映射和参数转换

### 文档文件

- **QUICK_START.md** - 快速开始指南 (推荐首先阅读)
  - 3 行代码的最简单用法
  - 完整工作流程示例
  - 常见问题解答

- **V1RuleBookMigrator_Usage_Guide.md** - 详细使用指南
  - 基本用法步骤
  - 完整的处理器映射表
  - 重要注意事项
  - 常见场景示例
  - 故障排除

- **V1RuleBookMigrator_Usage_Example.cs** - 代码示例
  - 示例 1: 基本迁移
  - 示例 2: 迁移后执行规则
  - 示例 3: 查看迁移映射
  - 示例 4: 处理迁移失败的规则

## 快速使用

### 最简单的方式

```csharp
var migrator = new V1RuleBookMigrator();
var v2RuleBook = migrator.MigrateRuleBook(v1RuleBook);
```

### 完整示例

```csharp
// 1. 创建 V1 规则书
var v1RuleBook = new AnalysisRuleBook { /* ... */ };

// 2. 迁移到 V2
var migrator = new V1RuleBookMigrator();
var v2RuleBook = migrator.MigrateRuleBook(v1RuleBook);

// 3. 使用 V2 规则引擎执行
var registry = new ProcessorRegistry();
// 注册处理器...
var engine = new RuleEngine(registry);
var result = await engine.ExecuteRuleBookAsync(v2RuleBook, context);
```

## 支持的迁移

### V1 规则类型

✅ **ExtractionRule** - 提取规则
- 支持所有 MatcherType: Regex, Markup, Text, XPath, JsonPath, CssSelector, Custom
- 支持所有 ActionType: Extract, Remove, Replace, Transform, Mark, Skip
- 支持所有 TargetField: Title, Summary, MainBody, Source, Custom, Ignore

✅ **TransformationRule** - 转换规则
- 支持所有 TransformationType: Format, Truncate, Merge, Split, Map, Calculate, RegexExtraction, RemoveHtmlComments, RemoveRunBlocks, RemoveXmlTags, CleanWhitespace 等

✅ **PreparationRules** - 预处理规则
- 在所有其他规则之前执行

## 处理器映射

### 提取规则 (ExtractionRule)

| V1 MatcherType | V1 ActionType | V2 处理器 |
|---|---|---|
| Regex | Extract | Regex.Match |
| Regex | Remove/Replace | Regex.Replace |
| Markup | Extract | Markup.SelectNode |
| Markup | Remove | Markup.RemoveNode |
| Text | Extract | Text.Trim |
| Text | Remove/Replace | Text.Replace |
| JsonPath | Extract | Json.SelectToken |
| XPath | Extract | Markup.SelectNode |
| CssSelector | Extract | Markup.SelectNode |

### 转换规则 (TransformationRule)

| V1 TransformationType | V2 处理器 |
|---|---|
| Format | Text.Trim |
| Truncate | Text.Truncate |
| CleanWhitespace | Text.Trim |
| RemoveHtmlComments | Regex.Replace |
| RemoveXmlTags | Regex.Replace |

## 重要特性

### 1. 自动优先级分配
迁移工具会自动为规则分配优先级，确保执行顺序正确。

### 2. 禁用规则跳过
`Enabled = false` 的规则不会被迁移。

### 3. 参数保留
V1 规则的所有参数都会被复制到 V2 规则中。

### 4. 错误处理
所有迁移的规则都使用 `ErrorHandlingStrategy.Skip`，失败时会跳过继续执行。

## 集成测试

集成测试位于 `code/Novex.Analyzer.V2.Tests/Integration/CleanupProcessorIntegrationTests.cs`

运行测试:
```bash
dotnet test code/Novex.Analyzer.V2.Tests/Novex.Analyzer.V2.Tests.csproj
```

## 下一步

1. 阅读 `QUICK_START.md` 快速上手
2. 查看 `V1RuleBookMigrator_Usage_Example.cs` 了解代码示例
3. 参考 `V1RuleBookMigrator_Usage_Guide.md` 获取详细文档
4. 运行集成测试验证功能

## 常见问题

**Q: 如何处理不支持的规则类型？**
A: 不支持的规则会被跳过，迁移工具会返回 null。

**Q: 迁移后的规则可以直接使用吗？**
A: 是的，但需要确保所有必需的处理器都已在注册表中注册。

**Q: 如何验证迁移是否成功？**
A: 检查迁移后的规则数量，并运行集成测试验证功能。

## 支持

如有问题，请参考文档或查看示例代码。

