# Novex.Analyzer Rules V2 设计文档

## 概述

本目录包含 Novex.Analyzer Rules V2 的完整设计文档。Rules V2 是对现有规则系统的全面重新设计，旨在解决当前架构中的问题，并提供更灵活、可扩展、易用的规则处理框架。

## 设计目标

1. **最小职责重叠** - 每个组件职责单一、清晰
2. **一致的 API 设计** - 统一的接口、参数、命名约定
3. **通用性和可配置性** - 通过配置实现特定功能，而非硬编码
4. **易于组合和扩展** - 支持规则组合和插件化扩展
5. **自动发现机制** - 支持从外部 DLL 自动加载处理器

## 文档结构

### [01-current-problems.md](./01-current-problems.md)
**当前架构问题分析**

详细分析 Novex.Analyzer V1 中存在的问题：
- 职责重叠问题（3 个处理器接口、重复的处理器实现）
- API 设计不一致（参数命名、规则结构、硬编码注册）
- 通用性不足（过于具体的处理器、缺少组合能力）
- 扩展性问题（无法从外部加载、添加处理器步骤繁琐）
- YAML 配置冗长、测试调试困难

### [02-design-principles.md](./02-design-principles.md)
**设计原则**

定义 Rules V2 的核心设计原则：
- SOLID 原则的应用（单一职责、开闭原则、依赖倒置等）
- 架构层次（核心抽象层、通用处理器层、领域处理器层、应用处理器层）
- 统一的规则模型（规则基类、处理规则、作用域）
- 参数标准化（命名约定、通用参数、参数验证）
- 处理器发现和注册机制
- 执行管道和错误处理
- 配置文件简化（默认值、模板、引用）

### [03-architecture-design.md](./03-architecture-design.md)
**架构设计**

详细的架构设计和 API 定义：
- 架构概览图
- 核心组件设计
  - 处理器接口 (`IProcessor`, `ProcessContext`, `ProcessorParameters`, `ProcessResult`)
  - 处理器元数据接口 (`IProcessorMetadata`)
  - 处理器注册表 (`IProcessorRegistry`)
  - 处理器发现机制 (`IProcessorDiscovery`, `ProcessorAttribute`)
  - 统一的规则模型 (`RuleBook`, `ProcessRule`, `RuleTemplate`)
- 内置处理器设计示例
- YAML 配置示例

### [04-processor-catalog.md](./04-processor-catalog.md)
**内置处理器目录**

完整的内置处理器参考文档：
- **Text 处理器** (7 个): Trim, Truncate, NormalizeWhitespace, Replace, ToUpper/ToLower, LimitLines
- **Regex 处理器** (4 个): Replace, Extract, Match, Split
- **Markup 处理器** (5 个): Extract, Remove, StripTags, DecodeEntities, FixUnclosed
- **Json 处理器** (2 个): Extract, Parse
- **Pipeline 处理器** (2 个): Pipeline, Pipeline.Conditional
- **Conditional 处理器** (2 个): IfEmpty, IfMatches
- **Transform 处理器** (3 个): Template, Merge, Map

每个处理器包含：
- 功能描述
- 参数说明
- 使用示例

### [05-migration-guide.md](./05-migration-guide.md)
**迁移指南**

从 V1 迁移到 V2 的详细指南：
- 规则文件迁移（PreparationRules、ExtractionRules、TransformationRules、PostProcessingRules）
- 处理器类型映射表
- 自定义处理器迁移
- API 调用迁移
- 兼容性说明和迁移策略
- 迁移检查清单

### [06-implementation-plan.md](./06-implementation-plan.md)
**实现计划**

分阶段的实现计划：
- **Phase 1**: Core Abstractions (2-3 天)
- **Phase 2**: Processor Registry (3-4 天)
- **Phase 3**: Built-in Processors (7-10 天)
- **Phase 4**: Rule Engine (5-7 天)
- **Phase 5**: Migration & Testing (5-7 天)

包含：
- 每个阶段的工作项和验收标准
- 时间估算（总计 22-31 天）
- 风险和缓解措施
- 质量保证标准
- 发布计划

## 关键改进

### 1. 统一的处理器接口

**V1 问题**: 3 个不同的处理器接口（`IPostProcessor`, `ITransformationProcessor`, `IPostProcessingRuleProcessor`）

**V2 解决方案**: 单一的 `IProcessor` 接口

```csharp
public interface IProcessor
{
    string Name { get; }
    Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters);
}
```

### 2. 插件化架构

**V1 问题**: 硬编码的处理器注册，无法从外部加载

**V2 解决方案**: 自动发现机制

```csharp
[Processor("Custom.MyProcessor", Category = "Custom")]
public class MyProcessor : IProcessor
{
    // 自动被发现和注册
}
```

### 3. 简化的 YAML 配置

**V1 问题**: 冗长的配置文件，缺少复用机制

**V2 解决方案**: 模板、引用、默认值

```yaml
# 定义模板
Templates:
  CleanText:
    Processor: "Pipeline"
    Parameters:
      Steps:
        - Type: "Text.Trim"
        - Type: "Text.NormalizeWhitespace"

# 使用模板
Rules:
  - Template: "CleanText"
    Field: "Title"
```

### 4. 通用的处理器

**V1 问题**: 过于具体的处理器（如 `RemoveRunBlocksProcessor`）

**V2 解决方案**: 通用处理器 + 参数配置

```yaml
# V1: 需要专门的 RemoveRunBlocksProcessor
# V2: 使用通用的 Regex.Replace
- Processor: "Regex.Replace"
  Parameters:
    Pattern: "<!--run:.*?-->"
    Replacement: ""
```

### 5. 管道组合

**V1 问题**: 难以组合多个简单操作

**V2 解决方案**: Pipeline 处理器

```yaml
- Processor: "Pipeline"
  Parameters:
    Steps:
      - Type: "Markup.Extract"
        Parameters:
          TagName: "dream"
      - Type: "Text.Trim"
      - Type: "Markup.StripTags"
```

## 使用示例

### 基础使用

```csharp
// 创建处理器注册表
var registry = new ProcessorRegistry();
var discovery = new ProcessorDiscovery();

// 自动发现处理器
foreach (var processor in discovery.DiscoverFromAssembly(typeof(IProcessor).Assembly))
{
    registry.Register(processor.Name, processor.Type);
}

// 创建规则引擎
var engine = new RuleEngine(registry);

// 解析规则书
var ruleBook = await engine.ParseRuleBookAsync("rules.yaml");

// 执行规则
var context = new ProcessContext { SourceContent = htmlContent };
var result = await engine.ExecuteAsync(context, ruleBook);

// 访问结果
if (result.Success)
{
    var title = result.Fields["Title"];
    var summary = result.Fields["Summary"];
}
```

### 自定义处理器

```csharp
[Processor("Custom.MyProcessor", Category = "Custom")]
public class MyProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Custom.MyProcessor";
    
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
                DefaultValue = 100,
                Description = "Maximum length"
            }
        };
    }
}
```

## 下一步行动

1. **审查设计文档** - 收集团队反馈
2. **开始实现 Phase 1** - 核心抽象层
3. **创建原型** - 验证关键设计决策
4. **迭代改进** - 根据反馈调整设计

## 反馈和讨论

如有任何问题、建议或反馈，请：
1. 在设计文档中添加注释
2. 创建 Issue 讨论
3. 与团队成员直接沟通

## 版本历史

- **2025-10-29**: 初始版本，完成所有设计文档

