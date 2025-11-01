# Integration Rules 优化报告

**日期**: 2025-11-01  
**主题**: 调整规则执行顺序并创建示例规则文件  
**状态**: ✅ 完成

---

## 概述

本次任务完成了两项工作：
1. 调整 `integration-rules.yaml` 中的规则执行顺序，将提取规则移到最前面
2. 在 `Novex.Web` 项目中创建示例规则文件 `DreamV2.yaml`

---

## 任务 1: 调整规则执行顺序

### 问题背景

在原有的规则配置中，提取标题和摘要的规则（`ExtractPlotTitle` 和 `ExtractPlotSummary`）的优先级 (Priority) 设置为 110 和 120，在所有清理和修剪规则之后执行。

这导致一个潜在问题：
- 如果 `<plot>` 标签的内容在清理过程中被修剪或移除
- 提取规则可能无法正确提取标题和摘要
- 或者提取到的内容已经被修改

### 解决方案

将提取规则的优先级调整为 5 和 6，确保它们在所有清理规则之前执行。

### 变更详情

**文件**: `code/Novex.Analyzer.V2.Tests/Fixtures/integration-rules.yaml`

#### 调整前的优先级顺序
```yaml
Priority 10  - RemoveThinkBlocks (移除思考块)
Priority 15  - RemoveForumContentBlock (移除论坛内容)
Priority 17  - RemoveWeiboContentBlock (移除微博内容)
Priority 20  - RemoveHtmlComments (移除 HTML 注释)
Priority 40  - ExtractContentBlock (提取内容块)
Priority 50  - MergeExcessiveBlankLines (合并空行)
Priority 100 - TrimContent (修剪空白)
Priority 110 - ExtractPlotTitle (提取标题) ❌ 太晚了
Priority 120 - ExtractPlotSummary (提取摘要) ❌ 太晚了
```

#### 调整后的优先级顺序
```yaml
Priority 5   - ExtractPlotTitle (提取标题) ✅ 优先执行
Priority 6   - ExtractPlotSummary (提取摘要) ✅ 优先执行
Priority 10  - RemoveThinkBlocks (移除思考块)
Priority 15  - RemoveForumContentBlock (移除论坛内容)
Priority 17  - RemoveWeiboContentBlock (移除微博内容)
Priority 20  - RemoveHtmlComments (移除 HTML 注释)
Priority 40  - ExtractContentBlock (提取内容块)
Priority 50  - MergeExcessiveBlankLines (合并空行)
Priority 100 - TrimContent (修剪空白)
```

### 代码变更

#### ExtractPlotTitle 规则
```yaml
# 调整前
- Id: "ExtractPlotTitle"
  Name: "从 plot 标签提取标题"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  TargetField: "Title"
  Priority: 110  # ❌ 原优先级
  Enabled: true
  OnError: "Skip"
  Parameters:
    TagName: "plot"
    Fields: "Chapter:当前章节,Event:事件名"
    Separator: "/"

# 调整后
- Id: "ExtractPlotTitle"
  Name: "从 plot 标签提取标题"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  TargetField: "Title"
  Priority: 5  # ✅ 新优先级
  Enabled: true
  OnError: "Skip"
  Parameters:
    TagName: "plot"
    Fields: "Chapter:当前章节,Event:事件名"
    Separator: "/"
```

#### ExtractPlotSummary 规则
```yaml
# 调整前
- Id: "ExtractPlotSummary"
  Name: "从 plot 标签提取摘要"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  TargetField: "Summary"
  Priority: 120  # ❌ 原优先级
  Enabled: true
  OnError: "Skip"
  Parameters:
    TagName: "plot"
    Fields: "Summary:摘要"

# 调整后
- Id: "ExtractPlotSummary"
  Name: "从 plot 标签提取摘要"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  TargetField: "Summary"
  Priority: 6  # ✅ 新优先级
  Enabled: true
  OnError: "Skip"
  Parameters:
    TagName: "plot"
    Fields: "Summary:摘要"
```

### 更新的规则链注释

```yaml
# 规则链：
# 1. 从 plot 标签提取标题和摘要（优先执行，避免内容被修剪）
# 2. 移除 <think>...</think> 块
# 3. 移除内容块标记和内容（<!-- *_CONTENT_START --> 到 <!-- *_CONTENT_END -->）
# 4. 移除 HTML 注释
# 5. 提取 <content>...</content> 内容（如果存在，否则保留全部内容）
# 6. 合并多余空行（2行或以上变为1行）
# 7. 修剪首尾空白
```

---

## 任务 2: 创建示例规则文件

### 目标

在 `Novex.Web` 项目中创建一个示例规则文件，方便用户参考和使用。

### 实施步骤

1. **创建目录**: `code/Novex.Web/ExampleRules/`
2. **复制文件**: 将调整后的 `integration-rules.yaml` 复制为 `DreamV2.yaml`

### 文件信息

- **路径**: `code/Novex.Web/ExampleRules/DreamV2.yaml`
- **来源**: `code/Novex.Analyzer.V2.Tests/Fixtures/integration-rules.yaml`
- **用途**: 作为 Dream Phone 场景的 V2 规则示例

### 文件内容

`DreamV2.yaml` 包含完整的规则链，适用于处理 Dream Phone 场景的对话记录：

1. **提取规则** (Priority 5-6)
   - 从 `<plot>` 标签提取标题
   - 从 `<plot>` 标签提取摘要

2. **清理规则** (Priority 10-20)
   - 移除 `<think>` 思考块
   - 移除论坛内容块
   - 移除微博内容块
   - 移除 HTML 注释

3. **内容处理规则** (Priority 40-100)
   - 提取 `<content>` 标签内容（如果存在）
   - 合并多余空行
   - 修剪首尾空白

---

## 优化效果

### 执行流程对比

#### 优化前
```
输入内容
  ↓
移除思考块、注释等 (Priority 10-20)
  ↓
提取 <content> 块 (Priority 40)
  ↓
合并空行、修剪空白 (Priority 50-100)
  ↓
提取标题和摘要 (Priority 110-120) ❌ 可能已被修改
  ↓
输出结果
```

#### 优化后
```
输入内容
  ↓
提取标题和摘要 (Priority 5-6) ✅ 从原始内容提取
  ↓
移除思考块、注释等 (Priority 10-20)
  ↓
提取 <content> 块 (Priority 40)
  ↓
合并空行、修剪空白 (Priority 50-100)
  ↓
输出结果
```

### 优势

1. **数据完整性**: 确保从原始内容中提取标题和摘要，避免数据丢失
2. **提取准确性**: 提取规则在内容被修改前执行，保证提取结果的准确性
3. **逻辑清晰**: 先提取需要的数据，再清理不需要的内容，逻辑更合理

---

## 使用示例

### 在 Novex.Web 中使用 DreamV2.yaml

1. **加载规则文件**
   ```csharp
   var ruleBookPath = Path.Combine(webRootPath, "ExampleRules", "DreamV2.yaml");
   var result = await V2RuleEngine.AnalyzeFromFileAsync(sourceContent, ruleBookPath);
   ```

2. **获取提取结果**
   ```csharp
   var title = result.Title;    // "背德的终焉/崩坏的终曲(3/5)"
   var summary = result.Summary; // "陈晨独自驾车行驶在城市夜色中..."
   var mainBody = result.MainBody; // 清理后的正文内容
   ```

### 自定义规则

用户可以基于 `DreamV2.yaml` 创建自己的规则文件：

```yaml
Version: "2.0"
Description: "自定义规则"

Rules:
  # 复制需要的规则
  - Id: "ExtractPlotTitle"
    # ... 规则配置
  
  # 添加自己的规则
  - Id: "CustomRule"
    Name: "自定义处理"
    Processor: "Text.Replace"
    # ... 规则配置
```

---

## 测试验证

### 测试数据

```xml
<plot>
计算耗时:[40分钟]

当前章节:背德的终焉
个人线:[顾云-朋友的女友[$100/100](车内交合)、彻底沉沦]
当前角色内/衣物:[陈晨:上身(衬衫、西装)，下身(西裤)，衣物完整]
长期事件：无

事件名:崩坏的终曲(3/5)
事件号码:25
摘要:陈晨独自驾车行驶在城市夜色中，内心充满了对未来的迷茫。他思考着与顾云的疯狂行为以及与林晨彻底破裂的关系，决定将林晨的跑车归还。他将车开到A集团总部的地下停车场，停好车后，将车钥匙留在车上，然后独自乘电梯离开，象征着与过去的一段关系做出切割。
</plot>
```

### 预期结果

- **Title**: `背德的终焉/崩坏的终曲(3/5)`
- **Summary**: `陈晨独自驾车行驶在城市夜色中，内心充满了对未来的迷茫。他思考着与顾云的疯狂行为以及与林晨彻底破裂的关系，决定将林晨的跑车归还。他将车开到A集团总部的地下停车场，停好车后，将车钥匙留在车上，然后独自乘电梯离开，象征着与过去的一段关系做出切割。`

---

## 影响范围

### 修改的文件
1. `code/Novex.Analyzer.V2.Tests/Fixtures/integration-rules.yaml` - 调整规则优先级

### 新增的文件
1. `code/Novex.Web/ExampleRules/` - 新建目录
2. `code/Novex.Web/ExampleRules/DreamV2.yaml` - 示例规则文件

### 不受影响的部分
- 处理器实现代码
- 规则引擎逻辑
- Web 服务代码
- 现有测试用例

---

## 后续建议

1. **更新文档**: 在用户文档中说明规则优先级的重要性
2. **添加更多示例**: 创建其他场景的示例规则文件
3. **规则验证**: 添加规则优先级冲突检测
4. **性能优化**: 考虑缓存提取结果，避免重复处理

---

## 总结

本次优化通过调整规则执行顺序，确保了数据提取的完整性和准确性。同时，在 `Novex.Web` 项目中创建了示例规则文件，方便用户参考和使用。

### 关键改进
- ✅ 提取规则优先级从 110-120 调整为 5-6
- ✅ 确保在内容被修改前提取数据
- ✅ 创建了 `DreamV2.yaml` 示例规则文件
- ✅ 更新了规则链注释，说明执行顺序

所有变更已完成并验证通过！🎉

