# Rules V2 设计原则

## 核心设计目标

Rules V2 的设计遵循以下核心目标：

1. **最小职责重叠** (Minimum Overlapping Responsibilities)
2. **一致的 API 设计** (Consistent API Design)
3. **通用性和可配置性** (Generality and Configurability)
4. **易于组合和扩展** (Easy Composition and Extension)
5. **插件化架构** (Plugin Architecture)

## 设计原则

### 1. 单一职责原则 (Single Responsibility Principle)

每个处理器只做一件事，并做好：

```csharp
// ✅ 好的设计 - 单一职责
public class RegexReplaceProcessor : IProcessor
{
    // 只负责正则替换
}

public class TrimProcessor : IProcessor
{
    // 只负责修剪空白
}

// ❌ 不好的设计 - 职责混杂
public class CleanAndFormatProcessor : IProcessor
{
    // 既清理 HTML，又格式化文本，还截断长度
}
```

### 2. 开闭原则 (Open-Closed Principle)

对扩展开放，对修改封闭：

```csharp
// 通过插件机制添加新处理器，无需修改核心代码
public interface IProcessorRegistry
{
    void Register<T>(string name) where T : IProcessor;
    IProcessor Resolve(string name);
}
```

### 3. 依赖倒置原则 (Dependency Inversion Principle)

依赖抽象而非具体实现：

```csharp
// 规则引擎依赖抽象的处理器接口
public class RuleEngine
{
    private readonly IProcessorRegistry _registry;
    
    public RuleEngine(IProcessorRegistry registry)
    {
        _registry = registry;
    }
}
```

### 4. 接口隔离原则 (Interface Segregation Principle)

提供细粒度的接口，而非臃肿的大接口：

```csharp
// 基础处理器接口
public interface IProcessor
{
    Task<ProcessResult> ProcessAsync(ProcessContext context);
}

// 可选的元数据接口
public interface IProcessorMetadata
{
    string Name { get; }
    string Description { get; }
    IEnumerable<ParameterDefinition> Parameters { get; }
}

// 可选的验证接口
public interface IValidatableProcessor
{
    ValidationResult Validate(Dictionary<string, object> parameters);
}
```

### 5. 组合优于继承 (Composition over Inheritance)

通过组合简单处理器实现复杂功能：

```yaml
# 通过管道组合多个处理器
Processors:
  - Type: "Regex.Replace"
    Parameters:
      Pattern: "<[^>]+>"
      Replacement: ""
  - Type: "Text.Trim"
  - Type: "Text.Truncate"
    Parameters:
      MaxLength: 100
```

## 架构层次

### 层次 1: 核心抽象层 (Core Abstractions)

定义基础接口和契约：

- `IProcessor` - 处理器接口
- `IProcessorRegistry` - 处理器注册表
- `ProcessContext` - 处理上下文
- `ProcessResult` - 处理结果

### 层次 2: 通用处理器层 (Generic Processors)

提供高度通用的处理器：

- `RegexProcessor` - 正则表达式操作（匹配、替换、提取）
- `TextProcessor` - 文本操作（修剪、截断、格式化）
- `MarkupProcessor` - 标记语言操作（XPath、CSS 选择器）
- `ConditionalProcessor` - 条件执行
- `PipelineProcessor` - 管道组合

### 层次 3: 领域处理器层 (Domain Processors)

针对特定领域的处理器（可选）：

- `HtmlProcessor` - HTML 特定操作
- `JsonProcessor` - JSON 操作
- `MarkdownProcessor` - Markdown 操作

### 层次 4: 应用处理器层 (Application Processors)

应用特定的处理器（用户自定义）：

- `SummaryFallbackProcessor` - 摘要回退
- 其他业务特定处理器

## 统一的规则模型

### 规则基类

所有规则共享统一的基础结构：

```csharp
public abstract class RuleBase
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; } = 100;
    public bool Enabled { get; set; } = true;
    public string? Condition { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

### 处理规则

```csharp
public class ProcessRule : RuleBase
{
    public string ProcessorType { get; set; }
    public ProcessorScope Scope { get; set; }  // Source, Field, Global
    public string? SourceField { get; set; }
    public string? TargetField { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public enum ProcessorScope
{
    Source,      // 处理原始输入
    Field,       // 处理特定字段
    Global       // 处理所有提取的数据
}
```

## 参数标准化

### 参数命名约定

统一使用 PascalCase：

```yaml
Parameters:
  Pattern: "regex pattern"
  Replacement: "replacement text"
  MaxLength: 100
  AddEllipsis: true
  IgnoreCase: false
```

### 通用参数

所有处理器支持的通用参数：

- `Enabled` - 是否启用（默认 true）
- `OnError` - 错误处理策略（Throw, Skip, UseDefault）
- `Timeout` - 超时时间（毫秒）

### 参数验证

处理器应提供参数定义：

```csharp
public class ParameterDefinition
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public bool Required { get; set; }
    public object? DefaultValue { get; set; }
    public string? Description { get; set; }
    public Func<object, bool>? Validator { get; set; }
}
```

## 处理器发现和注册

### 自动发现机制

```csharp
public interface IProcessorDiscovery
{
    IEnumerable<ProcessorInfo> DiscoverProcessors(Assembly assembly);
    IEnumerable<ProcessorInfo> DiscoverProcessors(string directory);
}

[Processor("Text.Trim")]
public class TrimProcessor : IProcessor
{
    // 通过特性标记，支持自动发现
}
```

### 注册表设计

```csharp
public interface IProcessorRegistry
{
    void Register(string name, Type processorType);
    void Register(string name, Func<IProcessor> factory);
    IProcessor Resolve(string name);
    bool TryResolve(string name, out IProcessor processor);
    IEnumerable<string> GetRegisteredNames();
}
```

## 执行管道

### 管道模型

```
Input → [Preparation] → [Extraction] → [Transformation] → [Validation] → Output
         ↓                ↓               ↓                 ↓
      Processors       Processors      Processors        Processors
```

### 执行上下文

```csharp
public class ProcessContext
{
    public string SourceContent { get; set; }
    public Dictionary<string, string> ExtractedFields { get; set; }
    public Dictionary<string, object> Variables { get; set; }
    public ILogger Logger { get; set; }
    public CancellationToken CancellationToken { get; set; }
}
```

### 执行结果

```csharp
public class ProcessResult
{
    public bool Success { get; set; }
    public string? Output { get; set; }
    public Dictionary<string, string>? Fields { get; set; }
    public List<ProcessError> Errors { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

## 错误处理

### 错误策略

```csharp
public enum ErrorHandlingStrategy
{
    Throw,          // 抛出异常，停止执行
    Skip,           // 跳过当前规则，继续执行
    UseDefault,     // 使用默认值
    Retry           // 重试
}
```

### 错误信息

```csharp
public class ProcessError
{
    public string RuleId { get; set; }
    public string ProcessorType { get; set; }
    public string Message { get; set; }
    public Exception? Exception { get; set; }
    public Dictionary<string, object> Context { get; set; }
}
```

## 配置文件简化

### 默认值和约定

```yaml
# 简化前
Rules:
  - Id: "trim_title"
    Name: "Trim Title"
    ProcessorType: "Text.Trim"
    Scope: "Field"
    SourceField: "Title"
    TargetField: "Title"
    Priority: 100
    Enabled: true

# 简化后（使用默认值和约定）
Rules:
  - Id: "trim_title"
    Processor: "Text.Trim"
    Field: "Title"  # 默认 Scope=Field, SourceField=TargetField
```

### 规则模板

```yaml
# 定义模板
Templates:
  CleanField:
    Processor: "Pipeline"
    Parameters:
      Steps:
        - Type: "Text.Trim"
        - Type: "Text.NormalizeWhitespace"

# 使用模板
Rules:
  - Template: "CleanField"
    Field: "Title"
  - Template: "CleanField"
    Field: "Summary"
```

### 规则引用

```yaml
# 引用其他规则文件
Includes:
  - "common-rules.yaml"
  - "html-cleanup.yaml"

Rules:
  # 本文件特定的规则
```

## 总结

Rules V2 设计原则的核心是：

1. **简单性** - 每个组件职责单一、易于理解
2. **一致性** - 统一的接口、参数、命名约定
3. **通用性** - 通过配置实现特定功能，而非硬编码
4. **可组合性** - 通过组合简单组件实现复杂功能
5. **可扩展性** - 插件化架构，支持第三方扩展
6. **可维护性** - 清晰的层次结构，易于测试和调试

这些原则将指导 Rules V2 的具体实现设计。

