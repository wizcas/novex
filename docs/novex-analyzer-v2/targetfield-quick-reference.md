# TargetField 快速参考指南

## 核心概念

### TargetField 是什么？
规则级别的属性，指定处理器的输出应该保存到哪个字段。

### 为什么需要 TargetField？
提供灵活的方式来指定处理器的输出目标。

---

## 快速对比

### ❌ 错误做法（旧版本）
```yaml
- Id: "ExtractTitle"
  Processor: "Extraction.ExtractStructuredData"
  Parameters:
    OutputField: "Title"  # ❌ 在参数中指定输出字段
```

### ✅ 正确做法（新版本）
```yaml
- Id: "ExtractTitle"
  Processor: "Extraction.ExtractStructuredData"
  TargetField: "Title"  # ✅ 在规则级别指定输出字段
  Parameters:
    # ✅ 不需要 OutputField 参数
```

---

## 常见场景

### 场景 1: 从源内容提取字段
```yaml
- Id: "ExtractTitle"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  TargetField: "Title"
  Parameters:
    TagName: "plot"
    Fields: "Chapter:当前章节,Event:事件名"
    Separator: "/"
```

**流程**:
1. 处理器读取 SourceContent
2. 处理器返回提取的结果
3. 规则引擎保存到 Title 字段

---

### 场景 2: 处理字段并保存到另一个字段
```yaml
- Id: "ProcessField"
  Processor: "Text.Trim"
  Scope: "Field"
  SourceField: "RawTitle"
  TargetField: "Title"
```

**流程**:
1. 处理器读取 Fields["RawTitle"]
2. 处理器返回处理后的结果
3. 规则引擎保存到 Title 字段

---

### 场景 3: 更新源内容
```yaml
- Id: "CleanupContent"
  Processor: "Text.Trim"
  Scope: "Source"
  # TargetField 为空
```

**流程**:
1. 处理器读取 SourceContent
2. 处理器返回处理后的结果
3. 规则引擎更新 SourceContent

---

## 规则配置模板

### 基础模板
```yaml
- Id: "RuleId"
  Name: "规则名称"
  Processor: "Processor.Name"
  Scope: "Source"  # 或 "Field"
  TargetField: "FieldName"  # 可选
  Priority: 100
  Enabled: true
  OnError: "Skip"
  Parameters:
    # 处理器参数
```

### 提取规则模板
```yaml
- Id: "ExtractXxx"
  Name: "提取 Xxx"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  TargetField: "Xxx"
  Priority: 100
  Parameters:
    TagName: "tag"
    Fields: "Field1:Prefix1,Field2:Prefix2"
    Separator: "/"
```

---

## 常见错误

### ❌ 错误 1: 在参数中指定 OutputField
```yaml
Parameters:
  OutputField: "Title"  # ❌ 错误
```

**正确做法**:
```yaml
TargetField: "Title"  # ✅ 正确
```

---

### ❌ 错误 2: 忘记设置 TargetField
```yaml
- Id: "ExtractTitle"
  Processor: "Extraction.ExtractStructuredData"
  # ❌ 没有 TargetField
```

**结果**: 结果会更新 SourceContent 而不是保存到字段

**正确做法**:
```yaml
- Id: "ExtractTitle"
  Processor: "Extraction.ExtractStructuredData"
  TargetField: "Title"  # ✅ 指定目标字段
```

---

### ❌ 错误 3: TargetField 名称错误
```yaml
- Id: "ExtractSummary"
  TargetField: "Title"  # ❌ 应该是 "Summary"
```

**结果**: 摘要被保存到 Title 字段

**正确做法**:
```yaml
- Id: "ExtractSummary"
  TargetField: "Summary"  # ✅ 正确的字段名
```

---

## 调试技巧

### 1. 验证 TargetField 是否被设置
```csharp
// 在规则执行后
var title = context.GetField("Title");
if (string.IsNullOrEmpty(title))
{
    // TargetField 可能没有被正确设置
}
```

### 2. 检查规则优先级
```yaml
- Id: "Rule1"
  Priority: 100  # 先执行

- Id: "Rule2"
  Priority: 110  # 后执行
```

### 3. 验证处理器返回值
```csharp
// 处理器应该返回正确的结果
return ProcessResult.Ok(output);  // ✅ 正确
```

---

## 性能提示

1. **避免不必要的字段设置**
   - 只在需要时设置 TargetField
   - 不需要的字段不要设置

2. **合理安排规则优先级**
   - 优先级低的规则先执行
   - 优先级高的规则后执行

3. **使用 Scope 优化性能**
   - Scope=Field 比 Scope=Source 更高效
   - 只处理需要的字段

---

## 最佳实践

### ✅ DO
- 使用 TargetField 指定输出目标
- 处理器只返回结果
- 规则引擎管理字段存储
- 清晰的规则命名
- 合理的优先级设置

### ❌ DON'T
- 在参数中指定 OutputField
- 处理器直接设置字段
- 混淆 TargetField 和 SourceField
- 使用不清晰的字段名
- 不合理的优先级设置

---

## 相关文档

- **TARGETFIELD_ARCHITECTURE.md** - 详细的架构设计
- **TARGETFIELD_ANALYSIS.md** - 问题分析
- **TARGETFIELD_VERIFICATION_REPORT.md** - 验证报告

---

## 快速查询

| 问题 | 答案 |
|------|------|
| 如何指定输出字段？ | 使用 `TargetField` 属性 |
| 处理器应该设置字段吗？ | 不应该，只返回结果 |
| OutputField 参数还存在吗？ | 不存在，已移除 |
| 如何更新 SourceContent？ | 不设置 TargetField，Scope=Source |
| 如何处理字段？ | Scope=Field, SourceField, TargetField |

---

## 示例代码

### 完整的规则配置
```yaml
Rules:
  # 提取标题
  - Id: "ExtractPlotTitle"
    Name: "从 plot 标签提取标题"
    Processor: "Extraction.ExtractStructuredData"
    Scope: "Source"
    TargetField: "Title"
    Priority: 110
    Enabled: true
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Chapter:当前章节,Event:事件名"
      Separator: "/"

  # 提取摘要
  - Id: "ExtractPlotSummary"
    Name: "从 plot 标签提取摘要"
    Processor: "Extraction.ExtractStructuredData"
    Scope: "Source"
    TargetField: "Summary"
    Priority: 120
    Enabled: true
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Summary:摘要"
```

---

## 总结

- ✅ 使用 `TargetField` 指定输出字段
- ✅ 处理器只返回结果
- ✅ 规则引擎管理字段存储
- ✅ 清晰的职责分离

就这么简单！🎉

