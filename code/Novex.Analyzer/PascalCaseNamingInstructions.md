# YAML PascalCase 命名规范维护指南

## 📋 概述

本文档为 Novex Rule Engine 系统的 YAML 配置命名规范提供维护指南。当添加新的 YAML 字段、参数或枚举值时，请遵循此指南确保命名一致性。

## 🎯 命名规范

### ✅ **PascalCase 标准**
- **所有 YAML 字段名**: 使用 PascalCase （首字母大写的驼峰命名）
- **所有参数名**: 使用 PascalCase
- **所有枚举值**: 使用 PascalCase
- **配置块内字段**: 使用 PascalCase

### ❌ **禁止使用**
- snake_case: `max_length`, `remove_blocks`, `extract_html`
- camelCase: `maxLength`, `removeBlocks`, `extractHtml`
- kebab-case: `max-length`, `remove-blocks`, `extract-html`

## 🏗️ 当前验证的字段类别

### 1. 顶级字段
```yaml
Version: '1.0'
Description: '描述'
ExtractionRules: []
TransformationRules: []
AiGenerationRule: {}
```

### 2. 提取规则字段
```yaml
ExtractionRules:
- Id: 'rule_id'
  Name: '规则名'
  MatcherType: 'Regex'
  Pattern: '模式'
  Options:
    Multiline: true
    Singleline: false
    Global: false
    MaxMatches: 1
    IgnoreCase: false
    CustomOptions:
      ExtractHtml: false
  Action: 'Extract'
  Target: 'Title'
  CustomTargetName: 'custom_field'
  Priority: 10
  Enabled: true
```

### 3. 转换规则字段
```yaml
TransformationRules:
- Id: 'transform_id'
  Name: '转换名'
  SourceField: 'source'
  TargetField: 'target'
  TransformationType: 'RegexExtraction'
  Parameters: {}
  Priority: 100
  Enabled: true
```

### 4. 处理器参数

#### RegexExtractionProcessor
```yaml
Parameters:
  Pattern: '正则表达式'
  Format: '{1}'
  RemoveBlocks:
    - Start: '开始标记'
      End: '结束标记'
```

#### CleanWhitespaceProcessor
```yaml
Parameters:
  CleanWhitespace: true
  LimitEmptyLines: true
```

#### 其他处理器参数
```yaml
Parameters:
  # RemoveHtmlCommentsProcessor
  RemoveComments: true
  
  # RemoveRunBlocksProcessor
  RemoveRunBlocks: true
  
  # RemoveXmlTagsProcessor
  RemoveXmlTags: true
  
  # FormatTextProcessor
  FormatText: true
  RemoveExtraNewlines: true
  NormalizeSpaces: true
  
  # TruncateProcessor
  AddEllipsis: true
```

## 🔄 添加新字段的步骤

### 步骤 1: 代码实现
1. 在相应的 Processor 或 RuleEngine 中添加新字段处理逻辑
2. 确保使用 PascalCase 命名：`parameters.GetValueOrDefault("NewField")`
3. 避免使用 snake_case 回退：~~`parameters.GetValueOrDefault("new_field")`~~

### 步骤 2: 模型更新
1. 如果是新的枚举值，在 `AnalysisRuleBook.cs` 中添加
2. 确保枚举值使用 PascalCase
3. 使用 `[JsonConverter(typeof(JsonStringEnumConverter))]` 属性

### 步骤 3: 测试更新
1. 更新 `PascalCaseNamingTests.cs` 中的验证逻辑
2. 在 `expectedPascalCaseParams` 数组中添加新参数
3. 在 `GetComprehensiveYamlContent()` 方法中添加测试用例

### 步骤 4: 验证
1. 运行测试确保新字段被正确验证：
   ```bash
   dotnet test --filter "VerifyAllYamlFieldsUsePascalCase"
   ```
2. 检查生成的报告确认新字段被包含

## 🛠️ 自动化更新测试

### 快速添加新参数验证
在 `PascalCaseNamingTests.cs` 中找到 `expectedPascalCaseParams` 数组，添加新参数：

```csharp
var expectedPascalCaseParams = new[]
{
    // 现有参数...
    "Pattern", "Format", "RemoveBlocks",
    
    // 添加你的新参数
    "YourNewParameter",
    "AnotherNewField"
};
```

### 添加新的测试 YAML 内容
在 `GetComprehensiveYamlContent()` 方法中添加新的测试规则：

```csharp
- Id: 'TestNewParameter'
  Name: '测试新参数'
  TransformationType: 'YourNewProcessor'
  Parameters:
    YourNewParameter: true
    AnotherNewField: 'value'
```

## 📊 当前验证覆盖范围

### 字段类型验证 (共 ~60+ 项)
- ✅ 顶级字段 (5项)
- ✅ 提取规则字段 (10项) 
- ✅ 转换规则字段 (8项)
- ✅ 枚举值 (9项)
- ✅ 处理器参数 (25+ 项)
- ✅ Options 字段 (6项)
- ✅ 块移除结构 (3项)

### 处理器覆盖
- ✅ RegexExtractionProcessor
- ✅ CleanWhitespaceProcessor  
- ✅ RemoveHtmlCommentsProcessor
- ✅ RemoveRunBlocksProcessor
- ✅ RemoveXmlTagsProcessor
- ✅ FormatTextProcessor
- ✅ TruncateProcessor
- ✅ PreserveFormattingProcessor

## 🚨 常见错误和修复

### 1. snake_case 遗留
```csharp
// ❌ 错误
parameters.GetValueOrDefault("max_length")

// ✅ 正确
parameters.GetValueOrDefault("MaxLength")
```

### 2. 双重命名支持
```csharp
// ❌ 错误 - 不要提供回退
parameters.GetValueOrDefault("NewField") ?? parameters.GetValueOrDefault("new_field")

// ✅ 正确 - 只使用 PascalCase
parameters.GetValueOrDefault("NewField")
```

### 3. JSON 属性处理
```csharp
// ❌ 错误
if (blockElement.TryGetProperty("start", out var startElement))

// ✅ 正确
if (blockElement.TryGetProperty("Start", out var startElement))
```

## 📝 维护检查清单

- [ ] 新字段使用 PascalCase 命名
- [ ] 代码中没有 snake_case 引用
- [ ] 测试文件已更新包含新字段
- [ ] 运行验证测试并通过
- [ ] 检查生成的报告确认覆盖范围
- [ ] 更新相关的 YAML 示例文件

## 🔍 验证命令

```bash
# 运行 PascalCase 验证测试
dotnet test --filter "VerifyAllYamlFieldsUsePascalCase" -v n

# 运行所有测试
dotnet test --verbosity normal

# 搜索潜在的 snake_case 问题
grep -r "_[a-z]" --include="*.cs" Novex.Analyzer/
```

## 📈 持续改进

1. **定期审查**: 每次添加新功能时检查命名一致性
2. **自动化检测**: 利用测试套件自动检测命名不规范
3. **文档同步**: 保持本指南与实际代码同步
4. **团队培训**: 确保所有开发者了解并遵循命名规范

---

*最后更新: 2025-10-12*  
*维护者: Novex Development Team*