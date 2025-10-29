# 从 V1 迁移到 V2 指南

## 概述

本文档提供从 Novex.Analyzer V1 迁移到 V2 的详细指南，包括：

1. 规则文件迁移
2. 自定义处理器迁移
3. API 调用迁移
4. 兼容性说明

## 1. 规则文件迁移

### 1.1 PreparationRules → Rules (Scope: Source)

**V1:**
```yaml
PreparationRules:
  - Id: "remove_comments"
    Name: "Remove HTML Comments"
    MatcherType: "Regex"
    Pattern: "<!--.*?-->"
    Action: "Remove"
    Priority: 10
```

**V2:**
```yaml
Rules:
  - Id: "remove_comments"
    Name: "Remove HTML Comments"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 10
    Parameters:
      Pattern: "<!--.*?-->"
      Replacement: ""
      Singleline: true
```

### 1.2 ExtractionRules → Rules (Scope: Field)

**V1:**
```yaml
ExtractionRules:
  - Id: "extract_dream"
    Name: "Extract Dream Content"
    MatcherType: "Markup"
    Pattern: "dream"
    Action: "Extract"
    Target: "MainBody"
    Priority: 100
    PostProcessors:
      - Type: "TrimWhitespace"
      - Type: "CleanHtml"
```

**V2:**
```yaml
Rules:
  - Id: "extract_dream"
    Name: "Extract Dream Content"
    Processor: "Markup.Extract"
    Scope: Field
    TargetField: "MainBody"
    Priority: 100
    Parameters:
      TagName: "dream"
  
  # PostProcessors 变成独立的规则
  - Id: "clean_dream"
    Processor: "Pipeline"
    Scope: Field
    SourceField: "MainBody"
    TargetField: "MainBody"
    Priority: 101
    Parameters:
      Steps:
        - Type: "Text.Trim"
        - Type: "Markup.StripTags"
```

**或者使用管道简化：**
```yaml
Rules:
  - Id: "extract_and_clean_dream"
    Processor: "Pipeline"
    Scope: Field
    TargetField: "MainBody"
    Priority: 100
    Parameters:
      Steps:
        - Type: "Markup.Extract"
          Parameters:
            TagName: "dream"
        - Type: "Text.Trim"
        - Type: "Markup.StripTags"
```

### 1.3 TransformationRules → Rules (Scope: Field)

**V1:**
```yaml
TransformationRules:
  - Id: "truncate_summary"
    Name: "Truncate Summary"
    SourceField: "Summary"
    TargetField: "Summary"
    TransformationType: "Truncate"
    Priority: 160
    Parameters:
      MaxLength: 50
      AddEllipsis: true
```

**V2:**
```yaml
Rules:
  - Id: "truncate_summary"
    Name: "Truncate Summary"
    Processor: "Text.Truncate"
    Scope: Field
    SourceField: "Summary"
    TargetField: "Summary"
    Priority: 160
    Parameters:
      MaxLength: 50
      AddEllipsis: true
```

**简化版（SourceField = TargetField 时）：**
```yaml
Rules:
  - Id: "truncate_summary"
    Processor: "Text.Truncate"
    Field: "Summary"  # 自动设置 SourceField 和 TargetField
    Priority: 160
    Parameters:
      MaxLength: 50
      AddEllipsis: true
```

### 1.4 PostProcessingRules → Rules (Scope: Global)

**V1:**
```yaml
PostProcessingRules:
  - Id: "summary_fallback"
    Name: "Summary Fallback"
    Type: "SummaryFallback"
    Priority: 200
    Parameters:
      MaxLength: 100
      SourceField: "MainBody"
```

**V2:**
```yaml
Rules:
  - Id: "summary_fallback"
    Name: "Summary Fallback"
    Processor: "Conditional.IfEmpty"
    Scope: Field
    SourceField: "Summary"
    TargetField: "Summary"
    Priority: 200
    Parameters:
      FallbackProcessor: "Text.Truncate"
      FallbackParameters:
        SourceField: "MainBody"
        MaxLength: 100
        AddEllipsis: true
```

### 1.5 完整迁移示例

**V1 (dream-phone/rules.yaml):**
```yaml
Version: "1.0"
Description: "Dream Phone Analysis Rules"

PreparationRules:
  - Id: "remove_run_blocks"
    MatcherType: "Regex"
    Pattern: "<!--run:.*?-->"
    Action: "Remove"
    Priority: 10

ExtractionRules:
  - Id: "extract_dream"
    MatcherType: "Markup"
    Pattern: "dream"
    Action: "Extract"
    Target: "MainBody"
    Priority: 100
    PostProcessors:
      - Type: "TrimWhitespace"

TransformationRules:
  - Id: "clean_body"
    SourceField: "MainBody"
    TargetField: "MainBody"
    TransformationType: "RemoveXmlTags"
    Priority: 150

PostProcessingRules:
  - Id: "summary_fallback"
    Type: "SummaryFallback"
    Priority: 200
```

**V2 (dream-phone/rules.yaml):**
```yaml
Version: "2.0"
Description: "Dream Phone Analysis Rules"

Rules:
  # 准备阶段
  - Id: "remove_run_blocks"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 10
    Parameters:
      Pattern: "<!--run:.*?-->"
      Replacement: ""
      Singleline: true
  
  # 提取阶段
  - Id: "extract_dream"
    Processor: "Pipeline"
    Scope: Field
    TargetField: "MainBody"
    Priority: 100
    Parameters:
      Steps:
        - Type: "Markup.Extract"
          Parameters:
            TagName: "dream"
        - Type: "Text.Trim"
  
  # 转换阶段
  - Id: "clean_body"
    Processor: "Markup.Remove"
    Field: "MainBody"
    Priority: 150
    Parameters:
      TagNames: ["plot", "phone", "input", "body", "div", "span", "p"]
  
  # 后处理阶段
  - Id: "summary_fallback"
    Processor: "Conditional.IfEmpty"
    Field: "Summary"
    Priority: 200
    Parameters:
      FallbackProcessor: "Text.Truncate"
      FallbackParameters:
        SourceField: "MainBody"
        MaxLength: 100
```

## 2. 处理器类型映射

### 2.1 MatcherType + Action 映射

| V1 MatcherType | V1 Action | V2 Processor |
|---------------|-----------|--------------|
| Regex | Extract | Regex.Extract |
| Regex | Remove | Regex.Replace (Replacement="") |
| Regex | Replace | Regex.Replace |
| Markup | Extract | Markup.Extract |
| Text | Extract | Text.Replace 或 Regex.Extract |
| XPath | Extract | Markup.Extract (XPath=...) |
| CssSelector | Extract | Markup.Extract (Selector=...) |

### 2.2 TransformationType 映射

| V1 TransformationType | V2 Processor |
|----------------------|--------------|
| Format | Text.NormalizeWhitespace |
| Truncate | Text.Truncate |
| Merge | Transform.Merge |
| RegexExtraction | Regex.Extract |
| RemoveHtmlComments | Regex.Replace |
| RemoveRunBlocks | Regex.Replace |
| RemoveXmlTags | Markup.Remove |
| CleanWhitespace | Text.NormalizeWhitespace |
| GenerateTitle | Transform.Template |
| CleanUrl | Url.Clean (自定义) |
| FixUnclosedTags | Markup.FixUnclosed |

### 2.3 ProcessorType 映射

| V1 ProcessorType | V2 Processor |
|-----------------|--------------|
| TrimWhitespace | Text.Trim |
| FormatText | Text.NormalizeWhitespace |
| CleanHtml | Markup.StripTags |
| DecodeHtml | Markup.DecodeEntities |
| SummaryFallback | Conditional.IfEmpty |

## 3. 自定义处理器迁移

### 3.1 V1 处理器接口

**V1:**
```csharp
public interface IPostProcessor
{
    Task<string> ProcessAsync(string input, Dictionary<string, object> parameters);
}

public class MyCustomProcessor : IPostProcessor
{
    public async Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
    {
        var maxLength = parameters.ContainsKey("MaxLength") 
            ? (int)parameters["MaxLength"] 
            : 100;
        
        return input.Length > maxLength 
            ? input.Substring(0, maxLength) 
            : input;
    }
}
```

### 3.2 V2 处理器接口

**V2:**
```csharp
[Processor("Custom.MyProcessor", Category = "Custom")]
public class MyCustomProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Custom.MyProcessor";
    public string DisplayName => "My Custom Processor";
    public string Description => "Custom text processing";
    
    public Task<ProcessResult> ProcessAsync(
        ProcessContext context, 
        ProcessorParameters parameters)
    {
        var input = context.SourceContent;
        var maxLength = parameters.Get<int>("MaxLength", 100);
        
        var output = input.Length > maxLength 
            ? input.Substring(0, maxLength) 
            : input;
        
        return Task.FromResult(ProcessResult.Ok(output));
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "MaxLength",
                Type = typeof(int),
                Required = false,
                DefaultValue = 100,
                Description = "Maximum length of output"
            }
        };
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "Truncate to 50 characters",
                Parameters = new Dictionary<string, object> { { "MaxLength", 50 } },
                Input = "This is a very long text that needs to be truncated",
                ExpectedOutput = "This is a very long text that needs to be trunca"
            }
        };
    }
}
```

### 3.3 注册自定义处理器

**V1:**
```csharp
// 需要修改 RuleEngine 构造函数
public RuleEngine()
{
    _postProcessors = new Dictionary<ProcessorType, IPostProcessor>
    {
        { ProcessorType.Custom, new MyCustomProcessor() }
    };
}
```

**V2:**
```csharp
// 方式 1: 自动发现（推荐）
// 将处理器放在 plugins 目录，自动加载

// 方式 2: 手动注册
var registry = new ProcessorRegistry();
registry.Register("Custom.MyProcessor", typeof(MyCustomProcessor));

var engine = new RuleEngine(registry);

// 方式 3: 使用工厂
registry.Register("Custom.MyProcessor", () => new MyCustomProcessor());
```

## 4. API 调用迁移

### 4.1 V1 API

```csharp
var engine = new RuleEngine();
var ruleBook = engine.ParseRuleBook("path/to/rules.yaml");
var result = await engine.ExecuteRulesAsync(htmlContent, ruleBook);

var title = result.ExtractedData["Title"];
var summary = result.ExtractedData["Summary"];
```

### 4.2 V2 API

```csharp
// 创建处理器注册表
var registry = new ProcessorRegistry();
var discovery = new ProcessorDiscovery();

// 自动发现内置处理器
foreach (var processor in discovery.DiscoverFromAssembly(typeof(IProcessor).Assembly))
{
    registry.Register(processor.Name, processor.Type);
}

// 自动发现插件处理器
foreach (var processor in discovery.DiscoverFromDirectory("plugins"))
{
    registry.Register(processor.Name, processor.Type);
}

// 创建规则引擎
var engine = new RuleEngine(registry);

// 解析规则书
var ruleBook = await engine.ParseRuleBookAsync("path/to/rules.yaml");

// 执行规则
var context = new ProcessContext
{
    SourceContent = htmlContent,
    Logger = logger
};

var result = await engine.ExecuteAsync(context, ruleBook);

// 访问结果
if (result.Success)
{
    var title = result.Fields["Title"];
    var summary = result.Fields["Summary"];
}
else
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error.Message}");
    }
}
```

## 5. 兼容性说明

### 5.1 破坏性变更

1. **规则文件格式**：V2 不兼容 V1 的 YAML 格式，需要手动迁移
2. **处理器接口**：所有处理器接口都已更改
3. **枚举类型**：`MatcherType`、`ActionType`、`TransformationType`、`ProcessorType` 已移除
4. **API 签名**：`RuleEngine` 的构造函数和方法签名已更改

### 5.2 迁移策略

**选项 1: 一次性迁移**
- 适用于小型项目
- 一次性更新所有规则文件和代码

**选项 2: 渐进式迁移**
- 适用于大型项目
- 保留 V1 和 V2 并行运行
- 逐步迁移规则文件

```csharp
// 同时支持 V1 和 V2
public class HybridRuleEngine
{
    private readonly RuleEngineV1 _v1Engine;
    private readonly RuleEngineV2 _v2Engine;
    
    public async Task<AnalysisResult> ExecuteAsync(string content, string rulesPath)
    {
        // 根据规则文件版本选择引擎
        var version = DetectVersion(rulesPath);
        
        if (version == "1.0")
            return await _v1Engine.ExecuteRulesAsync(content, rulesPath);
        else
            return await _v2Engine.ExecuteAsync(content, rulesPath);
    }
}
```

### 5.3 迁移工具

建议创建自动迁移工具：

```csharp
public class RuleMigrationTool
{
    public RuleBookV2 MigrateFromV1(RuleBookV1 v1RuleBook)
    {
        var v2RuleBook = new RuleBookV2
        {
            Version = "2.0",
            Description = v1RuleBook.Description
        };
        
        // 迁移 PreparationRules
        foreach (var rule in v1RuleBook.PreparationRules)
        {
            v2RuleBook.Rules.Add(MigratePreparationRule(rule));
        }
        
        // 迁移 ExtractionRules
        foreach (var rule in v1RuleBook.ExtractionRules)
        {
            v2RuleBook.Rules.AddRange(MigrateExtractionRule(rule));
        }
        
        // ... 其他规则类型
        
        return v2RuleBook;
    }
}
```

## 6. 迁移检查清单

- [ ] 备份所有 V1 规则文件
- [ ] 创建 V2 规则文件目录
- [ ] 迁移 PreparationRules
- [ ] 迁移 ExtractionRules
- [ ] 迁移 TransformationRules
- [ ] 迁移 PostProcessingRules
- [ ] 迁移自定义处理器
- [ ] 更新 API 调用代码
- [ ] 测试所有迁移的规则
- [ ] 更新文档
- [ ] 部署到生产环境

## 下一步

查看 `06-implementation-plan.md` 了解 V2 的实现计划。

