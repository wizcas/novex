# Rules V2 实现计划

## 实现阶段

整个实现分为 5 个阶段，每个阶段都可以独立测试和验证。

```
Phase 1: Core Abstractions (核心抽象)
   ↓
Phase 2: Processor Registry (处理器注册表)
   ↓
Phase 3: Built-in Processors (内置处理器)
   ↓
Phase 4: Rule Engine (规则引擎)
   ↓
Phase 5: Migration & Testing (迁移和测试)
```

## Phase 1: Core Abstractions (核心抽象层)

**目标**: 定义所有核心接口和基础类型

**工作项**:

1. 创建项目结构
   ```
   code/Novex.Analyzer.V2/
   ├── Core/
   │   ├── IProcessor.cs
   │   ├── ProcessContext.cs
   │   ├── ProcessorParameters.cs
   │   ├── ProcessResult.cs
   │   └── ProcessError.cs
   ├── Metadata/
   │   ├── IProcessorMetadata.cs
   │   ├── ParameterDefinition.cs
   │   └── ProcessorExample.cs
   ├── Registry/
   │   ├── IProcessorRegistry.cs
   │   └── IProcessorDiscovery.cs
   ├── Models/
   │   ├── RuleBook.cs
   │   ├── RuleBase.cs
   │   ├── ProcessRule.cs
   │   └── RuleTemplate.cs
   └── Attributes/
       └── ProcessorAttribute.cs
   ```

2. 实现核心接口
   - `IProcessor` - 处理器基础接口
   - `ProcessContext` - 处理上下文
   - `ProcessorParameters` - 参数访问器
   - `ProcessResult` - 处理结果

3. 实现元数据接口
   - `IProcessorMetadata` - 处理器元数据
   - `ParameterDefinition` - 参数定义
   - `ProcessorExample` - 使用示例

4. 实现规则模型
   - `RuleBook` - 规则书
   - `RuleBase` - 规则基类
   - `ProcessRule` - 处理规则
   - `RuleTemplate` - 规则模板

**验收标准**:
- [ ] 所有接口和类都有完整的 XML 文档注释
- [ ] 所有类型都有单元测试
- [ ] 代码覆盖率 > 90%

**预计时间**: 2-3 天

## Phase 2: Processor Registry (处理器注册表)

**目标**: 实现处理器的注册、发现和解析机制

**工作项**:

1. 实现 `ProcessorRegistry`
   ```csharp
   public class ProcessorRegistry : IProcessorRegistry
   {
       private readonly Dictionary<string, Func<IProcessor>> _factories;
       private readonly Dictionary<string, IProcessorMetadata> _metadata;
       
       public void Register(string name, Type processorType);
       public void Register(string name, Func<IProcessor> factory);
       public void RegisterSingleton(string name, IProcessor instance);
       public IProcessor Resolve(string name);
       public bool TryResolve(string name, out IProcessor processor);
       // ...
   }
   ```

2. 实现 `ProcessorDiscovery`
   ```csharp
   public class ProcessorDiscovery : IProcessorDiscovery
   {
       public IEnumerable<ProcessorInfo> DiscoverFromAssembly(Assembly assembly);
       public IEnumerable<ProcessorInfo> DiscoverFromDirectory(string directory);
   }
   ```

3. 实现 `ProcessorAttribute`
   ```csharp
   [AttributeUsage(AttributeTargets.Class)]
   public class ProcessorAttribute : Attribute
   {
       public string Name { get; }
       public string? Category { get; set; }
       public string? Description { get; set; }
   }
   ```

4. 添加依赖注入支持
   ```csharp
   public static class ServiceCollectionExtensions
   {
       public static IServiceCollection AddNovexAnalyzerV2(
           this IServiceCollection services)
       {
           services.AddSingleton<IProcessorRegistry, ProcessorRegistry>();
           services.AddSingleton<IProcessorDiscovery, ProcessorDiscovery>();
           services.AddSingleton<IRuleEngine, RuleEngine>();
           return services;
       }
   }
   ```

**验收标准**:
- [ ] 可以注册和解析处理器
- [ ] 可以从程序集自动发现处理器
- [ ] 可以从外部 DLL 加载处理器
- [ ] 支持依赖注入
- [ ] 单元测试覆盖率 > 90%

**预计时间**: 3-4 天

## Phase 3: Built-in Processors (内置处理器)

**目标**: 实现所有内置处理器

**工作项**:

### 3.1 Text Processors (5 个)
- [ ] `Text.Trim` - 修剪空白
- [ ] `Text.Truncate` - 截断文本
- [ ] `Text.NormalizeWhitespace` - 规范化空白
- [ ] `Text.Replace` - 简单替换
- [ ] `Text.LimitLines` - 限制行数

### 3.2 Regex Processors (4 个)
- [ ] `Regex.Replace` - 正则替换
- [ ] `Regex.Extract` - 正则提取
- [ ] `Regex.Match` - 正则匹配
- [ ] `Regex.Split` - 正则分割

### 3.3 Markup Processors (5 个)
- [ ] `Markup.Extract` - 提取元素
- [ ] `Markup.Remove` - 移除元素
- [ ] `Markup.StripTags` - 移除所有标签
- [ ] `Markup.DecodeEntities` - 解码实体
- [ ] `Markup.FixUnclosed` - 修复未闭合标签

### 3.4 Pipeline Processors (2 个)
- [ ] `Pipeline` - 管道组合
- [ ] `Pipeline.Conditional` - 条件管道

### 3.5 Conditional Processors (2 个)
- [ ] `Conditional.IfEmpty` - 空值处理
- [ ] `Conditional.IfMatches` - 条件匹配

### 3.6 Transform Processors (3 个)
- [ ] `Transform.Template` - 模板生成
- [ ] `Transform.Merge` - 字段合并
- [ ] `Transform.Map` - 值映射

**每个处理器需要**:
- 实现 `IProcessor` 接口
- 实现 `IProcessorMetadata` 接口
- 添加 `[Processor]` 特性
- 完整的 XML 文档注释
- 至少 3 个单元测试
- 至少 1 个使用示例

**验收标准**:
- [ ] 所有处理器都已实现
- [ ] 所有处理器都有元数据
- [ ] 所有处理器都有单元测试
- [ ] 代码覆盖率 > 85%
- [ ] 所有处理器都能被自动发现

**预计时间**: 7-10 天

## Phase 4: Rule Engine (规则引擎)

**目标**: 实现规则引擎核心逻辑

**工作项**:

1. 实现 `RuleEngine`
   ```csharp
   public class RuleEngine : IRuleEngine
   {
       private readonly IProcessorRegistry _registry;
       
       public RuleEngine(IProcessorRegistry registry);
       
       public Task<RuleBook> ParseRuleBookAsync(string yamlPath);
       public Task<ProcessResult> ExecuteAsync(ProcessContext context, RuleBook ruleBook);
       public Task<ProcessResult> ExecuteRuleAsync(ProcessContext context, ProcessRule rule);
   }
   ```

2. 实现 YAML 解析
   - 使用 YamlDotNet
   - 支持 Includes（引用其他文件）
   - 支持 Templates（模板）
   - 支持 Variables（变量）

3. 实现规则执行逻辑
   - 按 Priority 排序
   - 支持 Condition 条件表达式
   - 支持 Scope (Source/Field/Global)
   - 错误处理策略 (Throw/Skip/UseDefault/Retry)

4. 实现条件表达式解析器
   ```csharp
   public class ConditionEvaluator
   {
       public bool Evaluate(string condition, ProcessContext context);
   }
   ```
   支持的表达式：
   - `Field.Length > 100`
   - `Field.IsEmpty`
   - `Field.Contains("text")`
   - `Variable.Name == "value"`

5. 实现规则验证
   ```csharp
   public class RuleValidator
   {
       public ValidationResult Validate(RuleBook ruleBook);
       public ValidationResult ValidateRule(ProcessRule rule);
   }
   ```

**验收标准**:
- [ ] 可以解析 YAML 规则文件
- [ ] 可以执行规则并返回结果
- [ ] 支持所有 Scope 类型
- [ ] 支持条件表达式
- [ ] 支持错误处理策略
- [ ] 支持规则模板
- [ ] 支持文件引用
- [ ] 单元测试覆盖率 > 85%
- [ ] 集成测试覆盖主要场景

**预计时间**: 5-7 天

## Phase 5: Migration & Testing (迁移和测试)

**目标**: 迁移现有规则文件并进行全面测试

**工作项**:

1. 创建迁移工具
   ```csharp
   public class RuleMigrationTool
   {
       public RuleBookV2 MigrateFromV1(string v1YamlPath);
       public void SaveRuleBook(RuleBookV2 ruleBook, string outputPath);
   }
   ```

2. 迁移测试数据
   - [ ] `dream-phone/rules.yaml`
   - [ ] 其他测试规则文件

3. 创建对比测试
   ```csharp
   [Fact]
   public async Task V1_And_V2_Should_Produce_Same_Results()
   {
       var v1Result = await _v1Engine.ExecuteAsync(input, v1Rules);
       var v2Result = await _v2Engine.ExecuteAsync(input, v2Rules);
       
       Assert.Equal(v1Result.Title, v2Result.Fields["Title"]);
       Assert.Equal(v1Result.Summary, v2Result.Fields["Summary"]);
       // ...
   }
   ```

4. 性能测试
   ```csharp
   [Fact]
   public async Task V2_Should_Be_Faster_Than_V1()
   {
       var v1Time = await MeasureExecutionTime(() => _v1Engine.ExecuteAsync(...));
       var v2Time = await MeasureExecutionTime(() => _v2Engine.ExecuteAsync(...));
       
       Assert.True(v2Time < v1Time * 1.2); // 允许 20% 的性能差异
   }
   ```

5. 创建示例项目
   - 基础使用示例
   - 自定义处理器示例
   - 插件开发示例

6. 更新文档
   - [ ] README.md
   - [ ] API 文档
   - [ ] 教程文档
   - [ ] 迁移指南

**验收标准**:
- [ ] 所有 V1 规则文件都已迁移
- [ ] V1 和 V2 产生相同的结果
- [ ] 性能测试通过
- [ ] 示例项目可以运行
- [ ] 文档完整且准确
- [ ] 端到端测试覆盖率 > 80%

**预计时间**: 5-7 天

## 总体时间估算

| 阶段 | 预计时间 | 依赖 |
|-----|---------|------|
| Phase 1: Core Abstractions | 2-3 天 | - |
| Phase 2: Processor Registry | 3-4 天 | Phase 1 |
| Phase 3: Built-in Processors | 7-10 天 | Phase 1, 2 |
| Phase 4: Rule Engine | 5-7 天 | Phase 1, 2, 3 |
| Phase 5: Migration & Testing | 5-7 天 | Phase 1-4 |
| **总计** | **22-31 天** | |

## 风险和缓解措施

### 风险 1: YAML 解析复杂度
**影响**: 高  
**概率**: 中  
**缓解**: 
- 使用成熟的 YamlDotNet 库
- 提前进行 YAML 解析原型验证
- 简化 YAML 结构设计

### 风险 2: 条件表达式解析
**影响**: 中  
**概率**: 中  
**缓解**:
- 使用现有的表达式解析库（如 DynamicExpresso）
- 限制支持的表达式类型
- 提供清晰的错误消息

### 风险 3: 性能回归
**影响**: 中  
**概率**: 低  
**缓解**:
- 在每个阶段进行性能测试
- 使用 BenchmarkDotNet 进行基准测试
- 优化热路径代码

### 风险 4: 迁移兼容性问题
**影响**: 高  
**概率**: 中  
**缓解**:
- 创建自动化迁移工具
- 提供详细的迁移指南
- 支持 V1 和 V2 并行运行

## 质量保证

### 代码质量
- [ ] 所有代码都有 XML 文档注释
- [ ] 遵循 C# 编码规范
- [ ] 通过静态代码分析（无警告）
- [ ] 代码审查通过

### 测试质量
- [ ] 单元测试覆盖率 > 85%
- [ ] 集成测试覆盖主要场景
- [ ] 端到端测试覆盖率 > 80%
- [ ] 所有测试都通过

### 文档质量
- [ ] API 文档完整
- [ ] 教程文档清晰
- [ ] 示例代码可运行
- [ ] 迁移指南详细

## 发布计划

### Alpha 版本 (Phase 1-2 完成后)
- 核心接口和注册表
- 供早期反馈

### Beta 版本 (Phase 1-3 完成后)
- 包含所有内置处理器
- 供内部测试

### RC 版本 (Phase 1-4 完成后)
- 完整的规则引擎
- 供生产前测试

### 正式版本 (Phase 1-5 完成后)
- 完整功能
- 完整文档
- 生产就绪

## 下一步行动

1. **立即**: 审查设计文档，收集反馈
2. **本周**: 开始 Phase 1 实现
3. **下周**: 完成 Phase 1，开始 Phase 2
4. **两周后**: 第一次进度审查

## 附录

### 相关文档
- `01-current-problems.md` - 当前问题分析
- `02-design-principles.md` - 设计原则
- `03-architecture-design.md` - 架构设计
- `04-processor-catalog.md` - 处理器目录
- `05-migration-guide.md` - 迁移指南

### 参考资源
- YamlDotNet: https://github.com/aaubry/YamlDotNet
- HtmlAgilityPack: https://html-agility-pack.net/
- DynamicExpresso: https://github.com/dynamicexpresso/DynamicExpresso
- BenchmarkDotNet: https://benchmarkdotnet.org/

