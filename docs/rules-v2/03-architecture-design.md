# Rules V2 架构设计

## 架构概览

```
┌─────────────────────────────────────────────────────────────┐
│                     Rule Engine Core                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Rule Parser  │  │ Rule Executor│  │ Result Builder│      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Processor Registry                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Auto-Discovery  │  Manual Registration  │  Factory  │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        ▼                   ▼                   ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│   Built-in   │  │   Domain     │  │   Custom     │
│  Processors  │  │  Processors  │  │  Processors  │
└──────────────┘  └──────────────┘  └──────────────┘
```

## 核心组件设计

### 1. 处理器接口 (Processor Interface)

#### 1.1 基础处理器接口

```csharp
namespace Novex.Analyzer.V2.Core;

/// <summary>
/// 处理器基础接口 - 所有处理器必须实现
/// </summary>
public interface IProcessor
{
    /// <summary>
    /// 处理器名称（用于注册和引用）
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 执行处理
    /// </summary>
    Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters);
}
```

#### 1.2 处理上下文

```csharp
/// <summary>
/// 处理上下文 - 包含处理所需的所有信息
/// </summary>
public class ProcessContext
{
    /// <summary>
    /// 原始输入内容
    /// </summary>
    public string SourceContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 已提取的字段
    /// </summary>
    public Dictionary<string, string> Fields { get; set; } = new();
    
    /// <summary>
    /// 变量存储（用于规则间传递数据）
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new();
    
    /// <summary>
    /// 日志记录器
    /// </summary>
    public ILogger Logger { get; set; }
    
    /// <summary>
    /// 取消令牌
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
    
    /// <summary>
    /// 获取或设置字段值
    /// </summary>
    public string GetField(string name, string defaultValue = "")
    {
        return Fields.TryGetValue(name, out var value) ? value : defaultValue;
    }
    
    public void SetField(string name, string value)
    {
        Fields[name] = value;
    }
}
```

#### 1.3 处理器参数

```csharp
/// <summary>
/// 处理器参数 - 类型安全的参数访问
/// </summary>
public class ProcessorParameters
{
    private readonly Dictionary<string, object> _parameters;
    
    public ProcessorParameters(Dictionary<string, object> parameters)
    {
        _parameters = parameters ?? new();
    }
    
    public T Get<T>(string name, T defaultValue = default)
    {
        if (!_parameters.TryGetValue(name, out var value))
            return defaultValue;
            
        if (value is T typedValue)
            return typedValue;
            
        // 尝试类型转换
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
    
    public bool TryGet<T>(string name, out T value)
    {
        value = default;
        if (!_parameters.TryGetValue(name, out var objValue))
            return false;
            
        if (objValue is T typedValue)
        {
            value = typedValue;
            return true;
        }
        
        return false;
    }
    
    public bool Has(string name) => _parameters.ContainsKey(name);
}
```

#### 1.4 处理结果

```csharp
/// <summary>
/// 处理结果
/// </summary>
public class ProcessResult
{
    public bool Success { get; set; }
    public string? Output { get; set; }
    public Dictionary<string, string>? ModifiedFields { get; set; }
    public List<ProcessError> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public static ProcessResult Ok(string output)
    {
        return new ProcessResult { Success = true, Output = output };
    }
    
    public static ProcessResult Fail(string error)
    {
        return new ProcessResult 
        { 
            Success = false, 
            Errors = new List<ProcessError> 
            { 
                new ProcessError { Message = error } 
            } 
        };
    }
}

public class ProcessError
{
    public string Message { get; set; } = string.Empty;
    public string? ProcessorName { get; set; }
    public Exception? Exception { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}
```

### 2. 处理器元数据接口 (Optional)

```csharp
/// <summary>
/// 处理器元数据接口 - 提供处理器的自描述能力
/// </summary>
public interface IProcessorMetadata
{
    /// <summary>
    /// 处理器显示名称
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// 处理器描述
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 参数定义
    /// </summary>
    IEnumerable<ParameterDefinition> GetParameters();
    
    /// <summary>
    /// 使用示例
    /// </summary>
    IEnumerable<ProcessorExample> GetExamples();
}

public class ParameterDefinition
{
    public string Name { get; set; } = string.Empty;
    public Type Type { get; set; } = typeof(string);
    public bool Required { get; set; }
    public object? DefaultValue { get; set; }
    public string? Description { get; set; }
    public string[]? AllowedValues { get; set; }
}

public class ProcessorExample
{
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
}
```

### 3. 处理器注册表

```csharp
/// <summary>
/// 处理器注册表接口
/// </summary>
public interface IProcessorRegistry
{
    /// <summary>
    /// 注册处理器类型
    /// </summary>
    void Register(string name, Type processorType);
    
    /// <summary>
    /// 注册处理器工厂
    /// </summary>
    void Register(string name, Func<IProcessor> factory);
    
    /// <summary>
    /// 注册处理器实例（单例）
    /// </summary>
    void RegisterSingleton(string name, IProcessor instance);
    
    /// <summary>
    /// 解析处理器
    /// </summary>
    IProcessor Resolve(string name);
    
    /// <summary>
    /// 尝试解析处理器
    /// </summary>
    bool TryResolve(string name, out IProcessor processor);
    
    /// <summary>
    /// 获取所有已注册的处理器名称
    /// </summary>
    IEnumerable<string> GetRegisteredNames();
    
    /// <summary>
    /// 获取处理器元数据
    /// </summary>
    IProcessorMetadata? GetMetadata(string name);
}
```

### 4. 处理器发现机制

```csharp
/// <summary>
/// 处理器发现接口
/// </summary>
public interface IProcessorDiscovery
{
    /// <summary>
    /// 从程序集发现处理器
    /// </summary>
    IEnumerable<ProcessorInfo> DiscoverFromAssembly(Assembly assembly);
    
    /// <summary>
    /// 从目录发现处理器（加载 DLL）
    /// </summary>
    IEnumerable<ProcessorInfo> DiscoverFromDirectory(string directory);
}

public class ProcessorInfo
{
    public string Name { get; set; } = string.Empty;
    public Type Type { get; set; } = null!;
    public ProcessorAttribute? Attribute { get; set; }
}

/// <summary>
/// 处理器特性 - 用于标记和自动发现
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ProcessorAttribute : Attribute
{
    public string Name { get; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    
    public ProcessorAttribute(string name)
    {
        Name = name;
    }
}
```

### 5. 统一的规则模型

```csharp
/// <summary>
/// 规则书 - 顶层配置
/// </summary>
public class RuleBook
{
    public string Version { get; set; } = "2.0";
    public string? Description { get; set; }
    public Dictionary<string, object> Variables { get; set; } = new();
    public List<string> Includes { get; set; } = new();
    public Dictionary<string, RuleTemplate> Templates { get; set; } = new();
    public List<ProcessRule> Rules { get; set; } = new();
}

/// <summary>
/// 规则基类
/// </summary>
public abstract class RuleBase
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; } = 100;
    public bool Enabled { get; set; } = true;
    public string? Condition { get; set; }
    public ErrorHandlingStrategy OnError { get; set; } = ErrorHandlingStrategy.Throw;
}

/// <summary>
/// 处理规则
/// </summary>
public class ProcessRule : RuleBase
{
    /// <summary>
    /// 处理器类型名称
    /// </summary>
    public string Processor { get; set; } = string.Empty;
    
    /// <summary>
    /// 处理范围
    /// </summary>
    public ProcessorScope Scope { get; set; } = ProcessorScope.Field;
    
    /// <summary>
    /// 源字段（Scope=Field 时使用）
    /// </summary>
    public string? SourceField { get; set; }
    
    /// <summary>
    /// 目标字段（Scope=Field 时使用）
    /// </summary>
    public string? TargetField { get; set; }
    
    /// <summary>
    /// 处理器参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public enum ProcessorScope
{
    /// <summary>
    /// 处理原始源内容
    /// </summary>
    Source,
    
    /// <summary>
    /// 处理特定字段
    /// </summary>
    Field,
    
    /// <summary>
    /// 处理整个上下文（可访问所有字段）
    /// </summary>
    Global
}

public enum ErrorHandlingStrategy
{
    Throw,
    Skip,
    UseDefault,
    Retry
}

/// <summary>
/// 规则模板
/// </summary>
public class RuleTemplate
{
    public string Processor { get; set; } = string.Empty;
    public ProcessorScope Scope { get; set; } = ProcessorScope.Field;
    public Dictionary<string, object> Parameters { get; set; } = new();
}
```

## 内置处理器设计

### 1. 文本处理器 (Text Processors)

```csharp
[Processor("Text.Trim", Category = "Text")]
public class TrimProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Text.Trim";
    public string DisplayName => "Trim Whitespace";
    public string Description => "Remove leading and trailing whitespace";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);
        var trimChars = parameters.Get<string>("TrimChars", null);
        
        var output = trimChars != null 
            ? input.Trim(trimChars.ToCharArray())
            : input.Trim();
            
        return Task.FromResult(ProcessResult.Ok(output));
    }
    
    private string GetInput(ProcessContext context, ProcessorParameters parameters)
    {
        var field = parameters.Get<string>("Field", null);
        return field != null ? context.GetField(field) : context.SourceContent;
    }
}
```

### 2. 正则处理器 (Regex Processors)

```csharp
[Processor("Regex.Replace", Category = "Regex")]
public class RegexReplaceProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Regex.Replace";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);
        var pattern = parameters.Get<string>("Pattern");
        var replacement = parameters.Get<string>("Replacement", "");
        var ignoreCase = parameters.Get<bool>("IgnoreCase", false);
        
        if (string.IsNullOrEmpty(pattern))
            return Task.FromResult(ProcessResult.Fail("Pattern is required"));
        
        var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        var output = Regex.Replace(input, pattern, replacement, options);
        
        return Task.FromResult(ProcessResult.Ok(output));
    }
}

[Processor("Regex.Extract", Category = "Regex")]
public class RegexExtractProcessor : IProcessor
{
    public string Name => "Regex.Extract";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);
        var pattern = parameters.Get<string>("Pattern");
        var group = parameters.Get<int>("Group", 1);
        
        var match = Regex.Match(input, pattern);
        if (!match.Success || match.Groups.Count <= group)
            return Task.FromResult(ProcessResult.Ok(""));
            
        return Task.FromResult(ProcessResult.Ok(match.Groups[group].Value));
    }
}
```

### 3. 管道处理器 (Pipeline Processor)

```csharp
[Processor("Pipeline", Category = "Composition")]
public class PipelineProcessor : IProcessor
{
    private readonly IProcessorRegistry _registry;
    
    public PipelineProcessor(IProcessorRegistry registry)
    {
        _registry = registry;
    }
    
    public string Name => "Pipeline";
    
    public async Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var steps = parameters.Get<List<object>>("Steps");
        if (steps == null || steps.Count == 0)
            return ProcessResult.Fail("Pipeline requires at least one step");
        
        var currentOutput = GetInput(context, parameters);
        
        foreach (var stepObj in steps)
        {
            var step = ParseStep(stepObj);
            if (!_registry.TryResolve(step.Type, out var processor))
                return ProcessResult.Fail($"Processor not found: {step.Type}");
            
            // 创建临时上下文
            var tempContext = new ProcessContext
            {
                SourceContent = currentOutput,
                Fields = context.Fields,
                Variables = context.Variables,
                Logger = context.Logger,
                CancellationToken = context.CancellationToken
            };
            
            var result = await processor.ProcessAsync(tempContext, new ProcessorParameters(step.Parameters));
            if (!result.Success)
                return result;
                
            currentOutput = result.Output ?? "";
        }
        
        return ProcessResult.Ok(currentOutput);
    }
}
```

## YAML 配置示例

### 基础示例

```yaml
Version: "2.0"
Description: "Dream Phone Rules - V2"

Rules:
  # 使用简化语法
  - Id: "extract_dream"
    Processor: "Markup.Extract"
    Scope: Field
    TargetField: "MainBody"
    Parameters:
      TagName: "dream"
      
  - Id: "clean_body"
    Processor: "Pipeline"
    Scope: Field
    SourceField: "MainBody"
    TargetField: "MainBody"
    Parameters:
      Steps:
        - Type: "Regex.Replace"
          Parameters:
            Pattern: "<!--.*?-->"
            Replacement: ""
        - Type: "Text.Trim"
        - Type: "Text.NormalizeWhitespace"
```

### 使用模板

```yaml
Templates:
  CleanText:
    Processor: "Pipeline"
    Parameters:
      Steps:
        - Type: "Text.Trim"
        - Type: "Text.NormalizeWhitespace"
        - Type: "Text.LimitLines"
          Parameters:
            MaxConsecutiveEmpty: 1

Rules:
  - Id: "clean_title"
    Template: "CleanText"
    Field: "Title"
    
  - Id: "clean_summary"
    Template: "CleanText"
    Field: "Summary"
```

## 下一步

详见：
- `04-processor-catalog.md` - 内置处理器目录
- `05-migration-guide.md` - 从 V1 迁移指南
- `06-implementation-plan.md` - 实现计划

