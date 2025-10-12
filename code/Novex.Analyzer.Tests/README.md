# Novex.Analyzer 测试说明

本项目包含的测试文件及其用途：

## 测试文件

### `ChatLogAnalyzerTests.cs`

- **用途**: 测试 `ChatLogAnalyzer` 的基础功能
- **覆盖内容**:
  - 基本分析功能
  - 参数验证
  - 错误处理
  - YAML规则和传统规则的兼容性

### `RuleEngineTests.cs`

- **用途**: 测试 `RuleEngine` 的核心功能
- **覆盖内容**:
  - YAML规则书解析
  - 提取规则执行
  - 转换规则执行
  - 错误处理和验证

### `DreamPhoneRulesTests.cs`

- **用途**: 测试特定场景（Dream Phone）的复杂规则处理
- **覆盖内容**:
  - 复杂的多步骤提取和转换
  - 实际使用场景的端到端测试
  - 标记语言处理

### `BlockRemovalIntegrationTests.cs`

- **用途**: 测试讨论块删除功能的集成测试
- **覆盖内容**:
  - 端到端的块删除功能
  - ChatLogAnalyzer 与 RuleEngine 的集成
  - YAML规则的实际应用效果

## 测试数据

### `MockData/`

包含各种测试场景的模拟数据：

- `dream-phone/`: Dream Phone 场景的测试数据
- 各种规则文件 (`.yaml`)
- 预期结果文件

### `../TestData/`

包含实际的测试数据文件：

- `1.jsonl`: 包含需要处理讨论块的实际数据
- `test.jsonl`: 其他测试数据

## 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定的测试类
dotnet test --filter "ChatLogAnalyzerTests"
dotnet test --filter "BlockRemovalIntegrationTests"

# 查看详细输出
dotnet test --verbosity detailed
```

## 已删除的调试文件

以下调试用的测试文件已被清理：

- `DebugParameterPassingTests.cs`
- `DebugTransformationRulesTests.cs`
- `DebugYamlParameterFormatTests.cs`
- `BlockRemovalDemoTests.cs`
- `RegexExtractionProcessorBlockRemovalTests.cs`
- `DreamPhoneDebugTests.cs`
- 各种临时调试文件 (`SimpleDebugTest.cs`, `test_*.cs*`)

这些文件的功能已经被整合到正式的测试文件中，或者不再需要。