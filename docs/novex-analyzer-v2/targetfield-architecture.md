# TargetField 架构设计

## 核心概念

### TargetField 是什么？

`TargetField` 是规则级别的属性，用于指定处理器的输出应该保存到哪个字段。

```csharp
public class ProcessRule
{
    public string? TargetField { get; set; }  // 目标字段名
}
```

### 为什么需要 TargetField？

在规则引擎中，处理器可能需要：
1. 更新源内容 (SourceContent)
2. 保存到特定字段 (Fields)
3. 保存到多个字段 (通过多个规则)

`TargetField` 提供了灵活的方式来指定输出目标。

---

## RuleEngine 的逻辑

### ExecuteRuleAsync 方法

```csharp
// 第 60-62 行：准备源内容
var sourceContent = rule.Scope == ProcessorScope.Field && !string.IsNullOrWhiteSpace(rule.SourceField)
    ? context.GetField(rule.SourceField)
    : context.SourceContent;

// 第 74-75 行：执行处理器
var result = await processor.ProcessAsync(processorContext, processorParams);

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

### 逻辑流程

```
1. 准备源内容
   ├─ 如果 Scope=Field 且 SourceField 被设置
   │  └─ 从 context.Fields[SourceField] 读取
   └─ 否则
      └─ 使用 context.SourceContent

2. 执行处理器
   └─ processor.ProcessAsync(processorContext, processorParams)

3. 保存结果
   ├─ 如果 TargetField 被设置
   │  └─ context.SetField(TargetField, result.Output)
   └─ 否则如果 Scope=Source
      └─ context.SourceContent = result.Output
```

---

## Scope 和 TargetField 的组合

### 组合 1: Scope=Source, TargetField=null
```yaml
Scope: "Source"
TargetField: null
```
**行为**: 处理器处理 SourceContent，结果更新 SourceContent
**用途**: 链式处理，每个规则都修改源内容

### 组合 2: Scope=Source, TargetField="Title"
```yaml
Scope: "Source"
TargetField: "Title"
```
**行为**: 处理器处理 SourceContent，结果保存到 Title 字段
**用途**: 从源内容中提取特定字段

### 组合 3: Scope=Field, SourceField="X", TargetField="Y"
```yaml
Scope: "Field"
SourceField: "X"
TargetField: "Y"
```
**行为**: 处理器处理 Fields["X"]，结果保存到 Fields["Y"]
**用途**: 字段转换或处理

### 组合 4: Scope=Field, SourceField="X", TargetField=null
```yaml
Scope: "Field"
SourceField: "X"
TargetField: null
```
**行为**: 处理器处理 Fields["X"]，结果更新 SourceContent
**用途**: 字段转换为源内容

---

## 处理器的职责

### 处理器应该做什么

```csharp
public async Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
{
    // 1. 读取输入
    var input = context.SourceContent;
    
    // 2. 处理数据
    var output = ProcessData(input);
    
    // 3. 返回结果
    return ProcessResult.Ok(output);
}
```

### 处理器不应该做什么

❌ **不应该设置字段**
```csharp
// 错误做法
context.SetField("Title", output);  // ❌ 处理器不应该管理字段
```

✅ **应该只返回结果**
```csharp
// 正确做法
return ProcessResult.Ok(output);  // ✅ 让规则引擎管理字段
```

---

## 实际例子

### 提取标题规则

```yaml
- Id: "ExtractPlotTitle"
  Name: "从 plot 标签提取标题"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  TargetField: "Title"
  Priority: 110
  Parameters:
    TagName: "plot"
    Fields: "Chapter:当前章节,Event:事件名"
    Separator: "/"
```

**执行流程**:
```
1. 读取 SourceContent
   └─ <plot>当前章节:背德的终焉\n事件名:崩坏的终曲(3/5)</plot>

2. 处理器执行
   └─ 提取 "当前章节" 和 "事件名"
   └─ 用 "/" 连接
   └─ 返回 "背德的终焉/崩坏的终曲(3/5)"

3. 规则引擎保存结果
   └─ context.SetField("Title", "背德的终焉/崩坏的终曲(3/5)")

4. 结果
   └─ context.Fields["Title"] = "背德的终焉/崩坏的终曲(3/5)"
```

---

## 与 V2RuleEngineService 的集成

### V2RuleEngineService 如何读取字段

```csharp
public async Task<ChatLogAnalysisResult> AnalyzeAsync(string sourceContent, string ruleBookId)
{
    // 1. 执行规则
    var result = await _ruleEngine.ExecuteAsync(sourceContent, ruleBookId);
    
    // 2. 读取提取的字段
    var title = result.Context.GetField("Title", null);
    var summary = result.Context.GetField("Summary", null);
    
    // 3. 返回结果
    return new ChatLogAnalysisResult
    {
        Title = !string.IsNullOrEmpty(title) ? title : ExtractTitle(sourceContent),
        Summary = !string.IsNullOrEmpty(summary) ? summary : ExtractSummary(sourceContent),
        MainBody = result.Output ?? sourceContent,
        CreatedAt = DateTime.Now
    };
}
```

---

## 最佳实践

### 1. 使用 TargetField 指定输出目标
```yaml
✅ 正确
TargetField: "Title"

❌ 错误
Parameters:
  OutputField: "Title"  # 不应该在参数中指定
```

### 2. 处理器只返回结果
```csharp
✅ 正确
return ProcessResult.Ok(output);

❌ 错误
context.SetField("Title", output);
return ProcessResult.Ok(output);
```

### 3. 清晰的规则设计
```yaml
✅ 清晰
- Id: "ExtractTitle"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  TargetField: "Title"
  Parameters:
    TagName: "plot"
    Fields: "Chapter:当前章节,Event:事件名"

❌ 混淆
- Id: "ExtractTitle"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  Parameters:
    TagName: "plot"
    Fields: "Chapter:当前章节,Event:事件名"
    OutputField: "Title"  # 混淆的参数
```

---

## 总结

| 概念 | 说明 |
|------|------|
| **TargetField** | 规则级别属性，指定输出目标 |
| **Scope** | 指定处理器的输入来源 |
| **SourceField** | 当 Scope=Field 时，指定输入字段 |
| **处理器职责** | 处理数据，返回结果 |
| **规则引擎职责** | 管理字段存储，协调处理器 |

这种设计提供了清晰的职责分离和灵活的字段管理方式。

