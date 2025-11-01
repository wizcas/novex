# 提取规则集成到 Analyze 页面

## 概述

已成功将 `Extraction.ExtractStructuredData` 处理器集成到 Novex.Web 的 Analyze 页面中。现在用户可以使用自定义规则书来自动提取标题和摘要。

## 集成内容

### 1. 规则文件更新

**文件**: `code/Novex.Analyzer.V2.Tests/Fixtures/integration-rules.yaml`

添加了两个新规则：

#### 规则 1: 从 plot 标签提取标题
```yaml
- Id: "ExtractPlotTitle"
  Name: "从 plot 标签提取标题"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  Priority: 110
  Enabled: true
  OnError: "Skip"
  Parameters:
    TagName: "plot"
    Fields: "Chapter:当前章节,Event:事件名"
    Separator: "/"
    OutputField: "Title"
```

**功能**: 从 `<plot>` 标签中提取"当前章节"和"事件名"，用 `/` 连接，保存到 `Title` 字段。

#### 规则 2: 从 plot 标签提取摘要
```yaml
- Id: "ExtractPlotSummary"
  Name: "从 plot 标签提取摘要"
  Processor: "Extraction.ExtractStructuredData"
  Scope: "Source"
  Priority: 120
  Enabled: true
  OnError: "Skip"
  Parameters:
    TagName: "plot"
    Fields: "Summary:摘要"
    OutputField: "Summary"
```

**功能**: 从 `<plot>` 标签中提取"摘要"字段，保存到 `Summary` 字段。

### 2. V2RuleEngineService 更新

**文件**: `code/Novex.Web/Services/V2RuleEngineService.cs`

更新了 `AnalyzeAsync` 方法，使其能够正确读取规则提取的字段：

```csharp
// 优先使用规则提取的字段，如果没有则使用默认提取方法
var title = context.GetField("Title", null);
var summary = context.GetField("Summary", null);

return new ChatLogAnalysisResult
{
    Title = !string.IsNullOrEmpty(title) ? title : ExtractTitle(sourceContent),
    Summary = !string.IsNullOrEmpty(summary) ? summary : ExtractSummary(sourceContent),
    MainBody = result.Output ?? sourceContent,
    CreatedAt = DateTime.Now
};
```

**功能**: 
- 优先使用规则提取的 `Title` 和 `Summary` 字段
- 如果规则没有提取到这些字段，则使用默认的提取方法
- 确保向后兼容性

## 使用流程

### 在 Analyze 页面上使用提取规则

1. **打开 Analyze 页面**
   - 导航到对话记录的分析页面
   - URL: `/chatlogs/{bookId}/{chatLogId}/analyze`

2. **选择规则书**
   - 在右侧"规则书"卡片中选择包含提取规则的规则书
   - 或使用默认分析引擎

3. **点击"分析"按钮**
   - 系统会使用选中的规则书进行分析
   - 如果规则书包含提取规则，会自动提取标题和摘要

4. **查看结果**
   - 提取的标题会显示在"标题"字段
   - 提取的摘要会显示在"摘要"字段
   - 处理后的内容会显示在"正文"字段

5. **保存结果**
   - 点击"保存"按钮保存分析结果

## 示例

### 输入内容

```
<plot>
计算耗时:[5分钟]

当前章节: 初次接触
个人线:[顾云-朋友的女友[$1/100](初识)、送其回家]
当前角色内/衣物:[顾云:上身(白色吊带)，下身(超短百褶裙)，内衣(无)，真空状态]
长期事件：无

事件名:虚假的小白兔(1/5)
事件号码:1
摘要:陈晨开车送林晨的女友顾云回家。途中，陈晨因顾云过于暴露的穿着而开口提醒，担心会引发交通事故。顾云听到后，一改在林晨面前的甜美形象，用粗俗的语言直接回怼陈晨，展现出其真实火爆的性格。陈晨对此感到震惊，并在随后的对话中选择退让。顾云在沉默片刻后，开始主动询问陈晨关于林晨过往情史的问题，言语直接，让陈晨陷入了短暂的为难。
</plot>
```

### 处理结果

**标题字段**:
```
初次接触/虚假的小白兔(1/5)
```

**摘要字段**:
```
陈晨开车送林晨的女友顾云回家。途中，陈晨因顾云过于暴露的穿着而开口提醒，担心会引发交通事故。顾云听到后，一改在林晨面前的甜美形象，用粗俗的语言直接回怼陈晨，展现出其真实火爆的性格。陈晨对此感到震惊，并在随后的对话中选择退让。顾云在沉默片刻后，开始主动询问陈晨关于林晨过往情史的问题，言语直接，让陈晨陷入了短暂的为难。
```

## 创建自定义规则书

用户可以创建自己的规则书来提取不同的字段。示例：

```yaml
Version: "2.0"
Description: "自定义提取规则"

Rules:
  # 清理内容
  - Id: "CleanContent"
    Name: "清理内容"
    Processor: "Text.Trim"
    Scope: "Source"
    Priority: 10
    Enabled: true

  # 提取标题
  - Id: "ExtractTitle"
    Name: "提取标题"
    Processor: "Extraction.ExtractStructuredData"
    Scope: "Source"
    Priority: 20
    Enabled: true
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Chapter:当前章节,Event:事件名"
      Separator: " - "
      OutputField: "Title"

  # 提取摘要
  - Id: "ExtractSummary"
    Name: "提取摘要"
    Processor: "Extraction.ExtractStructuredData"
    Scope: "Source"
    Priority: 30
    Enabled: true
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Summary:摘要"
      OutputField: "Summary"
```

## 技术细节

### 字段映射

| 规则字段 | Analyze 页面字段 | 说明 |
|---------|-----------------|------|
| `Title` | 标题 | 自动填充到标题输入框 |
| `Summary` | 摘要 | 自动填充到摘要文本框 |
| 其他字段 | 正文 | 规则处理后的内容 |

### 错误处理

- 如果规则提取失败（`OnError: "Skip"`），系统会使用默认的提取方法
- 如果规则书中没有提取规则，系统会使用默认的提取方法
- 所有错误都会被记录到浏览器控制台

### 性能考虑

- 提取规则在规则书执行时自动运行
- 不需要额外的配置或手动操作
- 性能取决于规则书的复杂度和内容大小

## 向后兼容性

- 现有的规则书继续工作
- 如果规则书中没有提取规则，使用默认的提取方法
- 用户可以逐步迁移到新的提取规则

## 测试状态

✅ 所有 75 个 V2 测试通过
✅ Novex.Web 编译成功
✅ 集成测试通过

## 下一步

1. **创建更多规则书示例** - 为不同的内容格式创建规则书
2. **用户文档** - 为用户提供详细的规则书编写指南
3. **规则书模板** - 提供常用的规则书模板
4. **性能优化** - 对大型内容进行性能优化

