# Novex.Analyzer V2 - Rules V2 实现

这是 Novex.Analyzer Rules V2 系统的实现项目。

## 项目结构

```
Novex.Analyzer.V2/
├── Core/                    # 核心抽象层
│   ├── IProcessor.cs       # 处理器基础接口
│   ├── ProcessContext.cs   # 处理上下文
│   ├── ProcessorParameters.cs  # 参数访问器
│   ├── ProcessResult.cs    # 处理结果
│   └── ProcessError.cs     # 错误信息
├── Metadata/               # 元数据
│   ├── IProcessorMetadata.cs   # 处理器元数据接口
│   ├── ParameterDefinition.cs  # 参数定义
│   └── ProcessorExample.cs     # 使用示例
├── Models/                 # 数据模型
│   ├── RuleBook.cs        # 规则书
│   ├── RuleBase.cs        # 规则基类
│   ├── ProcessRule.cs     # 处理规则
│   ├── RuleTemplate.cs    # 规则模板
│   ├── ProcessorScope.cs  # 处理器作用域
│   └── ErrorHandlingStrategy.cs  # 错误处理策略
├── Registry/              # 处理器注册表
│   ├── IProcessorRegistry.cs    # 注册表接口
│   ├── IProcessorDiscovery.cs   # 发现接口
│   └── ProcessorInfo.cs         # 处理器信息
├── Attributes/            # 特性
│   └── ProcessorAttribute.cs    # 处理器特性
└── Processors/            # 处理器实现（后续添加）
```

## Phase 1: Core Abstractions (✅ 完成)

### 已实现的组件

1. **核心接口和类**
   - ✅ `IProcessor` - 处理器基础接口
   - ✅ `ProcessContext` - 处理上下文
   - ✅ `ProcessorParameters` - 参数访问器（支持类型转换）
   - ✅ `ProcessResult` - 处理结果
   - ✅ `ProcessError` - 错误信息

2. **元数据接口**
   - ✅ `IProcessorMetadata` - 处理器元数据接口
   - ✅ `ParameterDefinition` - 参数定义
   - ✅ `ProcessorExample` - 使用示例

3. **规则模型**
   - ✅ `RuleBook` - 规则书
   - ✅ `RuleBase` - 规则基类
   - ✅ `ProcessRule` - 处理规则
   - ✅ `RuleTemplate` - 规则模板
   - ✅ `ProcessorScope` - 处理器作用域枚举
   - ✅ `ErrorHandlingStrategy` - 错误处理策略枚举

4. **处理器注册表**
   - ✅ `IProcessorRegistry` - 注册表接口
   - ✅ `IProcessorDiscovery` - 发现接口
   - ✅ `ProcessorInfo` - 处理器信息
   - ✅ `ProcessorAttribute` - 处理器特性

## Phase 2: Processor Registry (✅ 完成)

### 已实现的组件

1. **注册表实现**
   - ✅ `ProcessorRegistry` - 处理器注册表实现
   - ✅ `ProcessorDiscovery` - 处理器发现实现

2. **依赖注入**
   - ✅ `ServiceCollectionExtensions` - DI 扩展方法

## Phase 3: Built-in Processors (✅ 完成)

### 已实现的处理器

1. **Text 处理器**
   - ✅ `Text.Trim` - 修剪空白
   - ✅ `Text.Truncate` - 截断文本
   - ✅ `Text.Replace` - 替换文本

2. **Regex 处理器**
   - ✅ `Regex.Match` - 正则匹配
   - ✅ `Regex.Replace` - 正则替换

3. **Markup 处理器**
   - ✅ `Markup.ExtractText` - 提取文本
   - ✅ `Markup.SelectNode` - 选择节点

4. **Json 处理器**
   - ✅ `Json.Extract` - 提取值

5. **Transform 处理器**
   - ✅ `Transform.ToUpper` - 转换为大写
   - ✅ `Transform.ToLower` - 转换为小写

6. **Conditional 处理器**
   - ✅ `Conditional.If` - 条件判断

7. **Pipeline 处理器**
   - ✅ `Pipeline.Chain` - 管道链接

## Phase 4: Rule Engine (✅ 完成)

### 已实现的组件

1. **规则引擎**
   - ✅ `RuleEngine` - 规则执行引擎
   - ✅ `RuleValidator` - 规则验证器
   - ✅ `ConditionEvaluator` - 条件评估器
   - ✅ `YamlRuleLoader` - YAML 规则加载器

### 设计特点

- **类型安全**: `ProcessorParameters` 提供类型安全的参数访问
- **灵活的错误处理**: 支持多种错误处理策略
- **元数据支持**: 处理器可以提供自描述的元数据
- **可扩展的规则模型**: 支持模板、变量、条件等高级特性

## 下一步

- **Phase 5**: 迁移和测试
  - 创建 V1 到 V2 的迁移工具
  - 编写单元测试
  - 性能测试和优化

## 相关文档

- 详见 `/docs/rules-v2/` 目录下的设计文档

