# Novex.Analyzer 当前架构问题分析

## 概述

本文档分析 Novex.Analyzer 项目当前实现中存在的架构问题，为 Rules V2 重新设计提供依据。

## 1. 职责重叠问题 (Overlapping Responsibilities)

### 1.1 处理器接口重叠

当前存在三个处理器接口，职责边界模糊：

```csharp
// 后处理器 - 用于提取规则后的内容清理
public interface IPostProcessor
{
    Task<string> ProcessAsync(string input, Dictionary<string, object> parameters);
}

// 转换处理器 - 用于转换规则
public interface ITransformationProcessor
{
    Task<string> ProcessAsync(string input, Dictionary<string, object> parameters);
}

// 后处理规则处理器 - 用于最终的后处理
public interface IPostProcessingRuleProcessor
{
    Task ProcessAsync(Dictionary<string, string> extractedData, Dictionary<string, object> parameters);
}
```

**问题**：
- `IPostProcessor` 和 `ITransformationProcessor` 接口签名完全相同，但用途不同
- 用户难以理解何时使用哪个接口
- 同样的功能需要实现多次（如 `TruncateProcessor` vs `TruncateTransformationProcessor`）

### 1.2 重复的处理器实现

以下处理器功能重复或高度相似：

| 处理器 1 | 处理器 2 | 重叠功能 |
|---------|---------|---------|
| `TruncateProcessor` | `TruncateTransformationProcessor` | 文本截断 |
| `FormatTextProcessor` | `FormatTransformationProcessor` | 文本格式化 |
| `CleanWhitespaceProcessor` | `TrimWhitespaceProcessor` | 空白字符清理 |
| `RemoveXmlTagsProcessor` | `CleanHtmlProcessor` | 标签移除 |

### 1.3 枚举类型重叠

```csharp
// TransformationType 包含 17 个值
public enum TransformationType
{
    Format, Truncate, Merge, Split, Map, Calculate,
    RegexExtraction, RemoveHtmlComments, RemoveRunBlocks,
    RemoveXmlTags, CleanWhitespace, PreserveFormatting,
    GenerateTitle, CleanUrl, FixUnclosedTags
}

// ProcessorType 包含 8 个值
public enum ProcessorType
{
    CleanHtml, TrimWhitespace, DecodeHtml, FormatText,
    Validate, Custom, SummaryFallback
}
```

**问题**：
- 两个枚举都表示"处理类型"，但用于不同的规则阶段
- 功能有重叠（如 `CleanHtml` vs `RemoveXmlTags`）
- 添加新功能时不清楚应该加到哪个枚举

## 2. API 设计不一致问题 (Inconsistent API Design)

### 2.1 参数命名不统一

不同处理器使用不同的参数命名约定：

```yaml
# 有的使用 PascalCase
Parameters:
  MaxLength: 50
  AddEllipsis: true

# 有的使用 snake_case
Parameters:
  max_length: 50
  params_to_remove: ["utm_source"]

# 有的使用布尔参数名
Parameters:
  CleanWhitespace: true
  RemoveXmlTags: true
```

### 2.2 规则结构不一致

三种规则类型的结构差异很大：

```csharp
// ExtractionRule - 14 个属性
public class ExtractionRule
{
    public string Id { get; set; }
    public string Name { get; set; }
    public MatcherType MatcherType { get; set; }
    public string Pattern { get; set; }
    public MatchOptions Options { get; set; }
    public ActionType Action { get; set; }
    public TargetField Target { get; set; }
    public string? CustomTargetName { get; set; }
    public string? ReplacementValue { get; set; }
    public int Priority { get; set; }
    public bool Enabled { get; set; }
    public string? Condition { get; set; }
    public List<PostProcessor> PostProcessors { get; set; }
}

// TransformationRule - 7 个属性
public class TransformationRule
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string SourceField { get; set; }
    public string TargetField { get; set; }
    public TransformationType TransformationType { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public int Priority { get; set; }
    public bool Enabled { get; set; }
}

// PostProcessingRule - 6 个属性
public class PostProcessingRule
{
    public string Id { get; set; }
    public string Name { get; set; }
    public ProcessorType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public int Priority { get; set; }
    public bool Enabled { get; set; }
    public string? Condition { get; set; }
}
```

**问题**：
- 共同属性（Id, Name, Priority, Enabled）重复定义
- 缺少统一的基类或接口
- 条件表达式只在部分规则中支持

### 2.3 处理器注册硬编码

```csharp
public RuleEngine()
{
    _postProcessors = new Dictionary<ProcessorType, IPostProcessor>
    {
        { ProcessorType.TrimWhitespace, new TrimWhitespaceProcessor() },
        { ProcessorType.FormatText, new FormatTextProcessor() },
        { ProcessorType.CleanHtml, new CleanHtmlProcessor() },
        { ProcessorType.DecodeHtml, new DecodeHtmlProcessor() }
    };

    _transformationProcessors = new Dictionary<TransformationType, ITransformationProcessor>
    {
        { TransformationType.Format, new FormatTransformationProcessor() },
        { TransformationType.Truncate, new TruncateProcessor() },
        // ... 11 more entries
    };
}
```

**问题**：
- 添加新处理器需要修改 RuleEngine 代码
- 无法在运行时动态加载处理器
- 违反开闭原则 (Open-Closed Principle)

## 3. 通用性不足问题 (Lack of Generality)

### 3.1 过于具体的处理器

许多处理器针对特定场景，缺少通用性：

```csharp
// 只能移除特定的 XML 标签
public class RemoveXmlTagsProcessor : ITransformationProcessor
{
    // 硬编码标签列表: plot, phone, input, body, div, span, p
}

// 只能移除 <!--run:...--> 格式的注释
public class RemoveRunBlocksProcessor : ITransformationProcessor
{
    // 硬编码正则: @"<!--run:.*?-->"
}

// 只能修复未闭合的标签
public class FixUnclosedTagsProcessor : ITransformationProcessor
{
    // 特定的标签修复逻辑
}
```

**建议**：应该有通用的"正则替换"、"标签操作"处理器，通过参数配置具体行为。

### 3.2 缺少组合能力

当前架构难以组合多个简单操作：

```yaml
# 想要：移除特定块 + 清理空白 + 截断
# 现在需要：定义 3 个独立的规则
TransformationRules:
  - Id: "remove_blocks"
    TransformationType: "RegexExtraction"
    Parameters:
      RemoveBlocks: [...]
  - Id: "clean"
    TransformationType: "CleanWhitespace"
  - Id: "truncate"
    TransformationType: "Truncate"
```

## 4. 扩展性问题 (Extensibility Issues)

### 4.1 无法从外部加载处理器

当前架构不支持：
- 从外部 DLL 加载自定义处理器
- 插件式的处理器发现机制
- 第三方扩展

### 4.2 添加新处理器的步骤繁琐

要添加一个新处理器，需要：

1. 创建处理器类实现接口
2. 在对应的枚举中添加新值
3. 在 RuleEngine 构造函数中注册
4. 在验证逻辑中添加支持
5. 更新文档

**问题**：修改点过多，容易遗漏。

### 4.3 缺少处理器元数据

处理器缺少自描述能力：
- 没有参数定义/验证
- 没有文档说明
- 没有版本信息
- 无法生成规则文件模板

## 5. YAML 配置问题

### 5.1 规则文件冗长

```yaml
# 当前需要为每个规则指定所有属性
TransformationRules:
  - Id: "clean1"
    Name: "清理标题"
    SourceField: "Title"
    TargetField: "Title"
    TransformationType: "CleanWhitespace"
    Parameters:
      CleanWhitespace: true
    Priority: 160
    Enabled: true
```

### 5.2 缺少规则复用机制

- 无法定义规则模板
- 无法引用其他规则文件
- 无法定义变量或常量

## 6. 测试和调试问题

### 6.1 难以单独测试规则

- 规则必须在完整的 RuleEngine 上下文中执行
- 无法单独测试某个处理器的行为
- 缺少规则执行的中间状态查看

### 6.2 错误信息不够详细

```csharp
catch (Exception ex)
{
    Console.WriteLine($"Error executing transformation rule {rule.Id}: {ex.Message}");
}
```

- 只打印到控制台，不返回给调用者
- 缺少详细的错误上下文
- 难以定位问题规则

## 总结

当前架构的主要问题：

| 问题类别 | 严重程度 | 影响 |
|---------|---------|------|
| 职责重叠 | 高 | 代码重复、维护困难 |
| API 不一致 | 高 | 学习曲线陡峭、易出错 |
| 通用性不足 | 中 | 需要频繁添加新处理器 |
| 扩展性差 | 高 | 无法支持插件、第三方扩展 |
| 配置冗长 | 中 | 规则文件难以维护 |
| 调试困难 | 中 | 问题定位耗时 |

这些问题需要在 Rules V2 中系统性地解决。

