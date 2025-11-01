# Pascal Case 迁移指南

## 概述

从 2025-10-30 开始，Novex.Analyzer V2 规则系统统一使用 **Pascal Case** 命名约定，而不是 camelCase。这包括：

- YAML 配置文件中的所有字段名
- 规则的 SourceField 和 TargetField 属性
- 所有与 YAML 读取相关的逻辑

## 变更内容

### 1. YAML 命名约定

#### 之前 (camelCase)
```yaml
version: 2.0
description: 示例规则书
rules:
  - id: rule1
    name: 修剪空白
    processor: Text.Trim
    scope: Source
    sourceField: source
    targetField: title
    priority: 1
    enabled: true
    parameters:
      oldValue: hello
      newValue: world
```

#### 现在 (Pascal Case)
```yaml
Version: 2.0
Description: 示例规则书
Rules:
  - Id: rule1
    Name: 修剪空白
    Processor: Text.Trim
    Scope: Source
    SourceField: Source
    TargetField: Title
    Priority: 1
    Enabled: true
    Parameters:
      OldValue: hello
      NewValue: world
```

### 2. 字段名称映射

#### 规则字段

| 属性 | 之前 | 现在 |
|---|---|---|
| 版本 | `version` | `Version` |
| 描述 | `description` | `Description` |
| 规则列表 | `rules` | `Rules` |
| 规则 ID | `id` | `Id` |
| 规则名称 | `name` | `Name` |
| 处理器 | `processor` | `Processor` |
| 处理范围 | `scope` | `Scope` |
| 源字段 | `sourceField` | `SourceField` |
| 目标字段 | `targetField` | `TargetField` |
| 优先级 | `priority` | `Priority` |
| 启用状态 | `enabled` | `Enabled` |
| 参数 | `parameters` | `Parameters` |

#### 字段值

| 字段 | 之前 | 现在 |
|---|---|---|
| Title | `"title"` | `"Title"` |
| Summary | `"summary"` | `"Summary"` |
| MainBody | `"mainBody"` | `"MainBody"` |
| Source | `"source"` | `"Source"` |
| Custom | `"custom"` | `"Custom"` |

### 3. 代码中的变更

#### V1RuleBookMigrator

```csharp
// 之前
SourceField = "source",
TargetField = targetField,

// 现在
SourceField = "Source",
TargetField = targetField,  // 使用 Pascal Case 的字段名
```

#### 字段映射函数

```csharp
// 之前
private string GetTargetField(TargetField targetField, string? customName)
{
    return targetField switch
    {
        TargetField.Title => "title",
        TargetField.Summary => "summary",
        TargetField.MainBody => "mainBody",
        TargetField.Source => "source",
        TargetField.Custom => customName ?? "custom",
        _ => "source"
    };
}

// 现在
private string GetTargetField(TargetField targetField, string? customName)
{
    return targetField switch
    {
        TargetField.Title => "Title",
        TargetField.Summary => "Summary",
        TargetField.MainBody => "MainBody",
        TargetField.Source => "Source",
        TargetField.Custom => customName ?? "Custom",
        _ => "Source"
    };
}
```

### 4. YAML 加载器变更

#### YamlRuleLoader.cs

```csharp
// 之前
var deserializer = new DeserializerBuilder()
    .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
    .Build();

// 现在
var deserializer = new DeserializerBuilder()
    .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention.Instance)
    .Build();
```

## 迁移清单

- [x] 更新 YamlRuleLoader 使用 PascalCaseNamingConvention
- [x] 更新 V1RuleBookMigrator 中的字段映射
- [x] 更新所有 YAML 测试用例
- [x] 更新示例代码 (BasicExample.cs)
- [x] 更新迁移工具示例 (V1RuleBookMigrator_Usage_Example.cs)
- [x] 更新文档 (QUICK_START.md, V1RuleBookMigrator_Usage_Guide.md)
- [x] 编译验证 (0 错误)
- [x] 测试验证 (59/59 通过)

## 向后兼容性

⚠️ **重要**: 这是一个破坏性变更。

如果你有现有的 YAML 规则文件使用 camelCase，需要进行以下更新：

1. 将所有字段名从 camelCase 转换为 Pascal Case
2. 将所有字段值（如 `"mainBody"`）转换为 Pascal Case（如 `"MainBody"`）

### 自动转换脚本示例

```csharp
// 简单的 YAML 转换脚本
var oldYaml = File.ReadAllText("old-rules.yaml");
var newYaml = oldYaml
    .Replace("version:", "Version:")
    .Replace("description:", "Description:")
    .Replace("rules:", "Rules:")
    .Replace("id:", "Id:")
    .Replace("name:", "Name:")
    .Replace("processor:", "Processor:")
    .Replace("scope:", "Scope:")
    .Replace("sourceField:", "SourceField:")
    .Replace("targetField:", "TargetField:")
    .Replace("priority:", "Priority:")
    .Replace("enabled:", "Enabled:")
    .Replace("parameters:", "Parameters:")
    .Replace("\"mainBody\"", "\"MainBody\"")
    .Replace("\"source\"", "\"Source\"")
    .Replace("\"title\"", "\"Title\"")
    .Replace("\"summary\"", "\"Summary\"");

File.WriteAllText("new-rules.yaml", newYaml);
```

## 测试结果

✅ 所有 59 个 V2 测试通过
✅ 编译成功 (0 错误)
✅ YAML 序列化/反序列化正常工作

## 相关文件

- `code/Novex.Analyzer.V2/Engine/YamlRuleLoader.cs` - YAML 加载器
- `code/Novex.Analyzer.V2/Migration/V1RuleBookMigrator.cs` - 迁移工具
- `code/Novex.Analyzer.V2.Tests/Engine/YamlRuleLoaderTests.cs` - 测试用例
- `code/Novex.Analyzer.V2/Examples/BasicExample.cs` - 示例代码

## 常见问题

**Q: 为什么要改为 Pascal Case？**
A: 为了与 C# 的命名约定保持一致，提高代码的可读性和一致性。

**Q: 我的旧 YAML 文件还能用吗？**
A: 不能。需要更新为 Pascal Case 格式。

**Q: 如何快速迁移现有的 YAML 文件？**
A: 使用文本替换工具或编写简单的转换脚本（见上面的示例）。

**Q: 这会影响 C# 代码吗？**
A: 不会。C# 代码中的属性名称保持不变（已经是 Pascal Case）。只有 YAML 文件和字段值需要更新。

