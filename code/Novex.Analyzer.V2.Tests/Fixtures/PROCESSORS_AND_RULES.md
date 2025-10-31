# 集成测试处理器和规则说明

## 处理器清单

### 已有处理器

#### 1. Text 处理器
- **Text.Cleanup** - 清理处理器
  - 功能：移除 HTML 注释、`<think>...</think>` 块、多余空行
  - 位置：`code/Novex.Analyzer.V2/Processors/Text/CleanupProcessor.cs`

- **Text.Trim** - 修剪处理器
  - 功能：修剪首尾空白
  - 位置：`code/Novex.Analyzer.V2/Processors/Text/TrimProcessor.cs`

- **Text.Replace** - 替换处理器
  - 功能：替换文本内容
  - 位置：`code/Novex.Analyzer.V2/Processors/Text/ReplaceProcessor.cs`

- **Text.RemoveContentBlocks** - 移除内容块处理器 ✨ 新增
  - 功能：根据配置的开始和结束标记移除内容块（通用处理器）
  - 支持单个块：`Start` 和 `End` 参数
  - 支持多个块：`Blocks` 参数（数组，每个包含 `Start` 和 `End`）
  - 位置：`code/Novex.Analyzer.V2/Processors/Text/RemoveContentBlocksProcessor.cs`
  - 示例：
    - 移除 HTML 注释风格：`Start: "<!-- WEIBO_CONTENT_START -->"`, `End: "<!-- WEIBO_CONTENT_END -->"`
    - 移除方括号风格：`Start: "[START]"`, `End: "[END]"`

#### 2. Regex 处理器
- **Regex.Match** - 正则匹配处理器
  - 功能：使用正则表达式匹配文本
  - 位置：`code/Novex.Analyzer.V2/Processors/Regex/MatchProcessor.cs`

- **Regex.Replace** - 正则替换处理器
  - 功能：使用正则表达式替换文本
  - 位置：`code/Novex.Analyzer.V2/Processors/Regex/ReplaceProcessor.cs`

#### 3. Markup 处理器
- **Markup.ExtractText** - 提取文本处理器
  - 功能：从 HTML/XML 中提取纯文本
  - 位置：`code/Novex.Analyzer.V2/Processors/Markup/ExtractTextProcessor.cs`

- **Markup.SelectNode** - 选择节点处理器
  - 功能：使用 XPath 选择 HTML/XML 节点
  - 位置：`code/Novex.Analyzer.V2/Processors/Markup/SelectNodeProcessor.cs`

#### 4. Transform 处理器
- **Transform.ToUpper** - 转换为大写处理器
- **Transform.ToLower** - 转换为小写处理器

#### 5. Json 处理器
- **Json.Extract** - 从 JSON 提取值

#### 6. Conditional 处理器
- **Conditional.If** - 条件判断处理器

## 集成测试规则

### 规则文件位置
`code/Novex.Analyzer.V2.Tests/Fixtures/integration-rules.yaml`

### 规则链说明

集成测试规则按以下顺序处理内容：

1. **RemoveThinkBlocks** (优先级 10)
   - 处理器：`Regex.Replace`
   - 功能：移除 `<think>...</think>` 块（包括标签）
   - 正则：`<think>.*?</think>`

2. **ExtractContentBlock** (优先级 20)
   - 处理器：`Markup.SelectNode`
   - 功能：提取 `<content>...</content>` 内容
   - XPath：`//content`
   - 返回：InnerHtml

3. **RemoveHtmlComments** (优先级 30)
   - 处理器：`Regex.Replace`
   - 功能：移除所有 HTML 注释 `<!-- ... -->`
   - 正则：`<!--.*?-->`

4. **RemoveContentBlocks** (优先级 40)
   - 处理器：`Text.RemoveContentBlocks`
   - 功能：移除 `<!-- ` 到 ` -->` 的内容块（即所有 HTML 注释风格的块）
   - 包括标记行本身
   - 配置：`Blocks: [{ Start: "<!-- ", End: " -->" }]`

5. **CleanupWhitespace** (优先级 50)
   - 处理器：`Regex.Replace`
   - 功能：清理多余空行（3个以上连续空行变为2个）
   - 正则：`\n\s*\n\s*\n`

6. **TrimContent** (优先级 60)
   - 处理器：`Text.Trim`
   - 功能：修剪首尾空白

## 处理流程示例

### 输入示例
```
<think>
这是思考块
</think>

<content>
<!-- WEIBO_CONTENT_START -->
微博内容
<!-- WEIBO_CONTENT_END -->

实际内容
<!-- 注释 -->
更多内容
</content>
```

### 处理步骤
1. 移除 `<think>...</think>` → 移除思考块
2. 提取 `<content>...</content>` → 只保留内容块内部
3. 移除 HTML 注释 → 移除 `<!-- 注释 -->`
4. 移除内容块标记 → 移除 `<!-- WEIBO_CONTENT_START -->` 到 `<!-- WEIBO_CONTENT_END -->`
5. 清理空行 → 规范化空行
6. 修剪空白 → 移除首尾空白

### 最终输出
```
实际内容
更多内容
```

## 测试用例

### Case 1
- 输入：`code/Novex.Analyzer.V2.Tests/Fixtures/case1.input.md`
- 预期输出：`code/Novex.Analyzer.V2.Tests/Fixtures/case1.output.md`

### Case 2
- 输入：`code/Novex.Analyzer.V2.Tests/Fixtures/case2.input.md`
- 预期输出：`code/Novex.Analyzer.V2.Tests/Fixtures/case2.output.md`

### Case 3
- 输入：`code/Novex.Analyzer.V2.Tests/Fixtures/case3.input.md`
- 预期输出：`code/Novex.Analyzer.V2.Tests/Fixtures/case3.output.md`

## RemoveContentBlocks 处理器的通用性

`Text.RemoveContentBlocks` 处理器是通用的，可以移除任何风格的内容块标记。

### 配置示例

#### 1. 单个块移除
```yaml
Parameters:
  Start: "[START]"
  End: "[END]"
```

#### 2. 多个块移除
```yaml
Parameters:
  Blocks:
    - Start: "<!-- WEIBO_CONTENT_START -->"
      End: "<!-- WEIBO_CONTENT_END -->"
    - Start: "[START]"
      End: "[END]"
    - Start: "{{BEGIN}}"
      End: "{{END}}"
```

### 工作原理

1. 对每个块配置，转义特殊正则字符
2. 创建模式：`{escapedStart}.*?{escapedEnd}`
3. 使用 `Singleline` 模式匹配（`.` 匹配换行符）
4. 移除所有匹配的块（包括标记本身）

## 验证方式

集成测试通过以下方式验证：
1. 加载规则定义（YAML 格式）
2. 处理每个 `case*.input.md` 文件
3. 比较处理结果与 `case*.output.md` 的内容
4. 验证输出完全一致

