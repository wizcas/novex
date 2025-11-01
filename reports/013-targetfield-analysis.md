# TargetField 问题分析

## 问题总结

你提出的三个问题都是有效的。当前实现存在以下问题：

### 问题 1: TargetField 在 YAML 中被正确定义但在 RuleEngine 中未被正确使用

**现象**:
- 在 `integration-rules.yaml` 中设置了 `TargetField: "Summary"`
- 但在 `ExecuteRuleAsync()` 中，`rule.TargetField` 仍然是 null

**根本原因**:
YAML 加载器使用 `PascalCaseNamingConvention`，但 YAML 文件中使用的是 `TargetField`（PascalCase）。这应该能正确解析。

**实际问题**:
在 `integration-rules.yaml` 中，`ExtractPlotSummary` 规则的 `TargetField` 被错误地设置为 `"Title"` 而不是 `"Summary"`。

---

### 问题 2: OutputField 参数与 TargetField 的重复

**现象**:
```yaml
Parameters:
  OutputField: "Summary"  # 处理器参数
TargetField: "Summary"    # 规则级别
```

**问题**:
- 处理器的 `OutputField` 参数和规则的 `TargetField` 功能重复
- 处理器会将结果保存到 `OutputField` 指定的字段
- 规则引擎也会将结果保存到 `TargetField` 指定的字段
- 这导致混淆和不必要的复杂性

**设计缺陷**:
- 处理器不应该知道目标字段的概念
- 目标字段应该由规则引擎统一管理

---

### 问题 3: Scope=Source 时 TargetField 应该被使用

**现象**:
```csharp
// 当前代码 (RuleEngine.cs 第 84-91 行)
if (!string.IsNullOrWhiteSpace(rule.TargetField))
{
    context.SetField(rule.TargetField, result.Output ?? string.Empty);
}
else if (rule.Scope == ProcessorScope.Source)
{
    context.SourceContent = result.Output ?? string.Empty;
}
```

**问题**:
- 逻辑是正确的，但有一个隐藏的问题
- 当 `Scope=Source` 且 `TargetField` 被设置时，结果会被保存到字段
- 但 `SourceContent` 不会被更新
- 这可能导致后续规则无法获取更新的内容

**设计问题**:
- `Scope=Source` 和 `TargetField` 的组合语义不清楚
- 应该明确定义：
  - `Scope=Source` + `TargetField=null` → 更新 SourceContent
  - `Scope=Source` + `TargetField="X"` → 保存到字段 X，SourceContent 不变
  - `Scope=Field` + `SourceField="X"` + `TargetField="Y"` → 从 X 读取，保存到 Y

---

## 当前实现分析

### RuleEngine.ExecuteRuleAsync() 的逻辑

```csharp
// 第 60-62 行：准备源内容
var sourceContent = rule.Scope == ProcessorScope.Field && !string.IsNullOrWhiteSpace(rule.SourceField)
    ? context.GetField(rule.SourceField)
    : context.SourceContent;

// 第 84-91 行：保存结果
if (!string.IsNullOrWhiteSpace(rule.TargetField))
{
    context.SetField(rule.TargetField, result.Output ?? string.Empty);
}
else if (rule.Scope == ProcessorScope.Source)
{
    context.SourceContent = result.Output ?? string.Empty;
}
```

**问题**:
1. 当 `Scope=Source` 且 `TargetField` 被设置时，逻辑是正确的
2. 但处理器的 `OutputField` 参数会导致重复设置

---

## 处理器的 OutputField 参数问题

### ExtractStructuredDataProcessor 的实现

```csharp
// 第 61-64 行
if (!string.IsNullOrEmpty(outputField))
{
    context.SetField(outputField, result);
}
```

**问题**:
- 处理器在执行时就已经将结果保存到字段
- 然后规则引擎又会根据 `TargetField` 再次保存
- 这导致两次保存，可能覆盖彼此

---

## 建议的解决方案

### 方案 1: 统一使用 TargetField（推荐）

**修改**:
1. 移除处理器的 `OutputField` 参数
2. 处理器只返回 `ProcessResult.Ok(output)`
3. 规则引擎根据 `TargetField` 决定如何保存结果

**优点**:
- 清晰的职责分离
- 处理器只负责处理，不负责存储
- 规则引擎统一管理字段

**缺点**:
- 需要修改现有处理器

### 方案 2: 处理器优先，规则引擎后备

**修改**:
1. 保留处理器的 `OutputField` 参数
2. 如果处理器设置了 `OutputField`，规则引擎不再设置 `TargetField`
3. 如果处理器没有设置 `OutputField`，规则引擎使用 `TargetField`

**优点**:
- 向后兼容
- 处理器可以自定义输出字段

**缺点**:
- 逻辑复杂，容易混淆

---

## 当前 YAML 配置的问题

### ExtractPlotSummary 规则

```yaml
- Id: "ExtractPlotSummary"
  TargetField: "Title"  # ❌ 应该是 "Summary"
  Parameters:
    OutputField: "Summary"  # ❌ 与 TargetField 重复
```

**应该改为**:
```yaml
- Id: "ExtractPlotSummary"
  TargetField: "Summary"  # ✅ 正确的目标字段
  Parameters:
    # ❌ 移除 OutputField 参数
```

---

## 建议的修复步骤

1. **修复 YAML 配置**
   - 将 `ExtractPlotSummary` 的 `TargetField` 改为 `"Summary"`
   - 移除 `Parameters` 中的 `OutputField`

2. **修改处理器**
   - 移除 `OutputField` 参数
   - 处理器只返回结果，不保存到字段

3. **验证规则引擎**
   - 确保 `TargetField` 被正确使用
   - 添加测试验证字段被正确保存

4. **更新文档**
   - 明确 `Scope` 和 `TargetField` 的语义
   - 提供最佳实践指南

---

## 测试验证

需要添加测试来验证：
1. `TargetField` 被正确读取和使用
2. 结果被保存到正确的字段
3. 后续规则可以访问保存的字段

