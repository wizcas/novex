# Novex.Analyzer.V2 常见场景示例

**版本**: 1.0  
**目标受众**: AI 助手  
**用途**: 提供常见文本处理场景的规则书示例

---

## 目录

1. [场景 1: 清理 AI 对话记录](#场景-1-清理-ai-对话记录)
2. [场景 2: 提取结构化信息](#场景-2-提取结构化信息)
3. [场景 3: HTML 内容处理](#场景-3-html-内容处理)
4. [场景 4: 多格式内容提取](#场景-4-多格式内容提取)
5. [场景 5: 条件性内容处理](#场景-5-条件性内容处理)
6. [场景 6: 文本格式化](#场景-6-文本格式化)
7. [场景 7: 数据验证和清理](#场景-7-数据验证和清理)

---

## 场景 1: 清理 AI 对话记录

### 需求
从 AI 对话记录中移除思考块、HTML 注释、多余空行，保留纯净的对话内容。

### 规则书
```yaml
Version: "2.0"
Description: "清理 AI 对话记录"

Rules:
  # 第1步：移除思考块
  - Id: "remove_think"
    Name: "移除思考块"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 10
    Parameters:
      Pattern: '<think>.*?</think>'
      Replacement: ''
      RegexOptions: "Singleline,IgnoreCase"

  # 第2步：移除 HTML 注释
  - Id: "remove_comments"
    Name: "移除 HTML 注释"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 20
    Parameters:
      Pattern: '<!--.*?-->'
      Replacement: ''
      RegexOptions: "Singleline"

  # 第3步：合并多余空行
  - Id: "merge_blank_lines"
    Name: "合并多余空行"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 30
    Parameters:
      Pattern: "\\n\\n+"
      Replacement: "\\n\\n"

  # 第4步：修剪首尾空白
  - Id: "trim"
    Name: "修剪首尾空白"
    Processor: "Text.Trim"
    Scope: Source
    Priority: 100
```

### 输入示例
```
<think>
这是 AI 的思考过程...
</think>

<!-- 这是一个注释 -->

用户: 你好！
AI: 你好！很高兴见到你。


<!-- 另一个注释 -->

用户: 今天天气怎么样？
```

### 输出结果
```
用户: 你好！
AI: 你好！很高兴见到你。

用户: 今天天气怎么样？
```

---

## 场景 2: 提取结构化信息

### 需求
从包含 `<plot>` 标签的文本中提取标题、摘要和其他结构化字段。

### 规则书
```yaml
Version: "2.0"
Description: "提取 plot 标签中的结构化信息"

Rules:
  # 第1步：提取标题（章节 + 事件名）
  - Id: "extract_title"
    Name: "提取标题"
    Processor: "Extraction.ExtractStructuredData"
    Scope: Source
    TargetField: "Title"
    Priority: 5
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Chapter:当前章节,Event:事件名"
      Separator: " / "

  # 第2步：提取摘要
  - Id: "extract_summary"
    Name: "提取摘要"
    Processor: "Extraction.ExtractStructuredData"
    Scope: Source
    TargetField: "Summary"
    Priority: 6
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Summary:摘要"

  # 第3步：提取事件号码
  - Id: "extract_event_number"
    Name: "提取事件号码"
    Processor: "Extraction.ExtractStructuredData"
    Scope: Source
    TargetField: "EventNumber"
    Priority: 7
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Number:事件号码"

  # 第4步：移除 plot 标签，保留其他内容
  - Id: "remove_plot_tag"
    Name: "移除 plot 标签"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 50
    Parameters:
      Pattern: '<plot>.*?</plot>'
      Replacement: ''
      RegexOptions: "Singleline,IgnoreCase"

  # 第5步：清理空白
  - Id: "cleanup"
    Name: "清理空白"
    Processor: "Text.Trim"
    Scope: Source
    Priority: 100
```

### 输入示例
```xml
<plot>
计算耗时:[40分钟]

当前章节:背德的终焉
个人线:[顾云-朋友的女友[$100/100](车内交合)、彻底沉沦]
当前角色内/衣物:[陈晨:上身(衬衫、西装)，下身(西裤)，衣物完整]
长期事件：无

事件名:崩坏的终曲(3/5)
事件号码:25
摘要:陈晨独自驾车行驶在城市夜色中，内心充满了对未来的迷茫。
</plot>

这是正文内容...
```

### 输出结果
- **Title**: `背德的终焉 / 崩坏的终曲(3/5)`
- **Summary**: `陈晨独自驾车行驶在城市夜色中，内心充满了对未来的迷茫。`
- **EventNumber**: `25`
- **MainBody**: `这是正文内容...`

---

## 场景 3: HTML 内容处理

### 需求
从 HTML 文档中提取特定节点的内容，并转换为纯文本。

### 规则书
```yaml
Version: "2.0"
Description: "HTML 内容提取和处理"

Rules:
  # 第1步：提取 <content> 标签内容
  - Id: "extract_content_tag"
    Name: "提取 content 标签"
    Processor: "Markup.SelectNode"
    Scope: Source
    Priority: 10
    OnError: "Skip"
    Parameters:
      XPath: "//content"
      InnerHtml: true

  # 第2步：提取 <dream> 标签内容（如果存在）
  - Id: "extract_dream_tag"
    Name: "提取 dream 标签"
    Processor: "Markup.SelectNode"
    Scope: Source
    TargetField: "DreamContent"
    Priority: 15
    OnError: "Skip"
    Parameters:
      XPath: "//dream"
      InnerHtml: true

  # 第3步：转换为纯文本
  - Id: "to_plain_text"
    Name: "转换为纯文本"
    Processor: "Markup.ExtractText"
    Scope: Source
    Priority: 50
    Parameters:
      CleanWhitespace: true

  # 第4步：清理空白
  - Id: "trim"
    Name: "修剪空白"
    Processor: "Text.Trim"
    Scope: Source
    Priority: 100
```

### 输入示例
```html
<html>
  <body>
    <content>
      <h1>标题</h1>
      <p>这是第一段内容。</p>
      <p>这是第二段内容。</p>
    </content>
    <dream>
      梦境内容...
    </dream>
  </body>
</html>
```

### 输出结果
- **MainBody**: `标题 这是第一段内容。 这是第二段内容。`
- **DreamContent**: `梦境内容...`

---

## 场景 4: 多格式内容提取

### 需求
处理包含多种格式标记的内容，移除不需要的部分，保留主要内容。

### 规则书
```yaml
Version: "2.0"
Description: "多格式内容提取"

Rules:
  # 第1步：移除论坛内容块
  - Id: "remove_forum"
    Name: "移除论坛内容"
    Processor: "Text.RemoveContentBlocks"
    Scope: Source
    Priority: 10
    Parameters:
      Start: "<!-- FORUM_CONTENT_START -->"
      End: "<!-- FORUM_CONTENT_END -->"

  # 第2步：移除微博内容块
  - Id: "remove_weibo"
    Name: "移除微博内容"
    Processor: "Text.RemoveContentBlocks"
    Scope: Source
    Priority: 11
    Parameters:
      Start: "<!-- WEIBO_CONTENT_START -->"
      End: "<!-- WEIBO_CONTENT_END -->"

  # 第3步：提取主要内容
  - Id: "extract_main"
    Name: "提取主要内容"
    Processor: "Markup.SelectNode"
    Scope: Source
    Priority: 20
    OnError: "Skip"
    Parameters:
      XPath: "//content"
      InnerHtml: true

  # 第4步：清理
  - Id: "cleanup"
    Name: "清理"
    Processor: "Text.Cleanup"
    Scope: Source
    Priority: 100
```

---

## 场景 5: 条件性内容处理

### 需求
根据字段是否为空，设置默认值或进行不同的处理。

### 规则书
```yaml
Version: "2.0"
Description: "条件性内容处理"

Rules:
  # 第1步：提取标题
  - Id: "extract_title"
    Name: "提取标题"
    Processor: "Extraction.ExtractStructuredData"
    Scope: Source
    TargetField: "Title"
    Priority: 10
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Event:事件名"

  # 第2步：检查标题是否为空，设置默认值
  - Id: "check_title"
    Name: "检查标题"
    Processor: "Conditional.If"
    SourceField: "Title"
    TargetField: "DisplayTitle"
    Priority: 20
    Parameters:
      Condition: "empty"
      TrueValue: "未命名事件"
      FalseValue: "{{input}}"

  # 第3步：提取摘要
  - Id: "extract_summary"
    Name: "提取摘要"
    Processor: "Extraction.ExtractStructuredData"
    Scope: Source
    TargetField: "Summary"
    Priority: 30
    OnError: "Skip"
    Parameters:
      TagName: "plot"
      Fields: "Summary:摘要"

  # 第4步：截断摘要（如果太长）
  - Id: "truncate_summary"
    Name: "截断摘要"
    Processor: "Text.Truncate"
    SourceField: "Summary"
    TargetField: "ShortSummary"
    Priority: 40
    Parameters:
      MaxLength: 100
      AddEllipsis: true
```

---

## 场景 6: 文本格式化

### 需求
对文本进行格式化处理，包括大小写转换、替换特定内容等。

### 规则书
```yaml
Version: "2.0"
Description: "文本格式化"

Rules:
  # 第1步：替换特定词汇
  - Id: "replace_words"
    Name: "替换词汇"
    Processor: "Text.Replace"
    Scope: Source
    Priority: 10
    Parameters:
      OldValue: "旧词"
      NewValue: "新词"
      IgnoreCase: true

  # 第2步：移除多余空格
  - Id: "remove_extra_spaces"
    Name: "移除多余空格"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 20
    Parameters:
      Pattern: ' +'
      Replacement: ' '

  # 第3步：标题转大写
  - Id: "title_to_upper"
    Name: "标题转大写"
    Processor: "Transform.ToUpper"
    SourceField: "Title"
    TargetField: "UpperTitle"
    Priority: 30

  # 第4步：修剪空白
  - Id: "trim"
    Name: "修剪空白"
    Processor: "Text.Trim"
    Scope: Source
    Priority: 100
```

---

## 场景 7: 数据验证和清理

### 需求
验证和清理用户输入的数据，确保数据质量。

### 规则书
```yaml
Version: "2.0"
Description: "数据验证和清理"

Rules:
  # 第1步：移除 HTML 标签
  - Id: "remove_html"
    Name: "移除 HTML 标签"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 10
    Parameters:
      Pattern: '<[^>]+>'
      Replacement: ''

  # 第2步：移除特殊字符
  - Id: "remove_special_chars"
    Name: "移除特殊字符"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 20
    Parameters:
      Pattern: '[^\w\s\u4e00-\u9fa5]'
      Replacement: ''
      RegexOptions: "None"

  # 第3步：合并多余空格
  - Id: "merge_spaces"
    Name: "合并多余空格"
    Processor: "Regex.Replace"
    Scope: Source
    Priority: 30
    Parameters:
      Pattern: '\s+'
      Replacement: ' '

  # 第4步：修剪空白
  - Id: "trim"
    Name: "修剪空白"
    Processor: "Text.Trim"
    Scope: Source
    Priority: 100

  # 第5步：限制长度
  - Id: "limit_length"
    Name: "限制长度"
    Processor: "Text.Truncate"
    Scope: Source
    Priority: 110
    Parameters:
      MaxLength: 500
      AddEllipsis: false
```

---

## 常见模式总结

### 1. 数据提取优先模式
```yaml
Rules:
  # Priority 1-10: 提取数据
  - Priority: 5
    Processor: "Extraction.ExtractStructuredData"
  
  # Priority 11-50: 清理内容
  - Priority: 20
    Processor: "Regex.Replace"
  
  # Priority 100+: 最终处理
  - Priority: 100
    Processor: "Text.Trim"
```

### 2. 多步骤清理模式
```yaml
Rules:
  - Processor: "Regex.Replace"  # 移除思考块
  - Processor: "Regex.Replace"  # 移除注释
  - Processor: "Regex.Replace"  # 合并空行
  - Processor: "Text.Trim"      # 修剪空白
```

### 3. 条件处理模式
```yaml
Rules:
  - Processor: "Extraction.ExtractStructuredData"  # 提取
  - Processor: "Conditional.If"                    # 检查
  - Processor: "Text.Replace"                      # 处理
```

### 4. 字段转换模式
```yaml
Rules:
  - SourceField: "RawTitle"
    TargetField: "Title"
    Processor: "Text.Trim"
  
  - SourceField: "Title"
    TargetField: "UpperTitle"
    Processor: "Transform.ToUpper"
```

---

## 最佳实践提醒

1. **优先级规划**: 数据提取 < 内容清理 < 格式化 < 最终处理
2. **错误处理**: 可选操作使用 `OnError: "Skip"`
3. **字段命名**: 使用清晰、一致的字段名
4. **作用域选择**: Source 用于整体处理，Field 用于字段操作
5. **正则表达式**: 注意转义序列和 RegexOptions
6. **测试验证**: 使用实际数据测试规则书

---

**文档版本**: 1.0  
**最后更新**: 2025-11-01  
**维护者**: Novex.Analyzer.V2 Team

