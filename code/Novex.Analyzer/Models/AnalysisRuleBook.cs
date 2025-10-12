using System.Text.Json.Serialization;

namespace Novex.Analyzer.Models;

/// <summary>
/// 聊天记录分析规则书的根结构
/// </summary>
public class AnalysisRuleBook
{
  /// <summary>
  /// 规则书版本号
  /// </summary>
  public string Version { get; set; } = "1.0";

  /// <summary>
  /// 规则书描述
  /// </summary>
  public string Description { get; set; } = string.Empty;

  /// <summary>
  /// 提取规则列表
  /// </summary>
  public List<ExtractionRule> ExtractionRules { get; set; } = new();

  /// <summary>
  /// 转换规则列表
  /// </summary>
  public List<TransformationRule> TransformationRules { get; set; } = new();

  /// <summary>
  /// AI生成规则
  /// </summary>
  public AiGenerationRule? AiGenerationRule { get; set; }
}

/// <summary>
/// 提取规则 - 用于从原文中提取特定内容
/// </summary>
public class ExtractionRule
{
  /// <summary>
  /// 规则ID
  /// </summary>
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// 规则名称
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// 匹配器类型：regex, markup, text, xpath, json_path
  /// </summary>
  public MatcherType MatcherType { get; set; }

  /// <summary>
  /// 匹配模式
  /// </summary>
  public string Pattern { get; set; } = string.Empty;

  /// <summary>
  /// 匹配选项（如正则表达式的标志位）
  /// </summary>
  public MatchOptions Options { get; set; } = new();

  /// <summary>
  /// 操作类型：extract, remove, replace, transform
  /// </summary>
  public ActionType Action { get; set; }

  /// <summary>
  /// 目标字段：title, summary, main_body, custom
  /// </summary>
  public TargetField Target { get; set; }

  /// <summary>
  /// 自定义目标字段名（当Target为Custom时使用）
  /// </summary>
  public string? CustomTargetName { get; set; }

  /// <summary>
  /// 替换值（当Action为Replace时使用）
  /// </summary>
  public string? ReplacementValue { get; set; }

  /// <summary>
  /// 优先级（数值越小优先级越高）
  /// </summary>
  public int Priority { get; set; } = 100;

  /// <summary>
  /// 是否启用此规则
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// 条件表达式（可选，用于条件性执行）
  /// </summary>
  public string? Condition { get; set; }

  /// <summary>
  /// 后处理器链
  /// </summary>
  public List<PostProcessor> PostProcessors { get; set; } = new();
}

/// <summary>
/// 转换规则 - 用于对提取的内容进行进一步处理
/// </summary>
public class TransformationRule
{
  /// <summary>
  /// 规则ID
  /// </summary>
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// 规则名称
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// 源字段
  /// </summary>
  public string SourceField { get; set; } = string.Empty;

  /// <summary>
  /// 目标字段
  /// </summary>
  public string TargetField { get; set; } = string.Empty;

  /// <summary>
  /// 转换类型
  /// </summary>
  public TransformationType TransformationType { get; set; }

  /// <summary>
  /// 转换参数
  /// </summary>
  public Dictionary<string, object> Parameters { get; set; } = new();

  /// <summary>
  /// 优先级
  /// </summary>
  public int Priority { get; set; } = 100;

  /// <summary>
  /// 是否启用
  /// </summary>
  public bool Enabled { get; set; } = true;
}

/// <summary>
/// AI生成规则
/// </summary>
public class AiGenerationRule
{
  /// <summary>
  /// 是否启用AI生成
  /// </summary>
  public bool Enabled { get; set; } = false;

  /// <summary>
  /// AI提供商：openai, anthropic, custom
  /// </summary>
  public string Provider { get; set; } = "custom";

  /// <summary>
  /// 模型名称
  /// </summary>
  public string Model { get; set; } = string.Empty;

  /// <summary>
  /// 系统提示词
  /// </summary>
  public string SystemPrompt { get; set; } = string.Empty;

  /// <summary>
  /// 用户提示词模板
  /// </summary>
  public string UserPromptTemplate { get; set; } = string.Empty;

  /// <summary>
  /// 生成参数
  /// </summary>
  public Dictionary<string, object> Parameters { get; set; } = new();

  /// <summary>
  /// 生成目标（title, summary, main_body 或多个的组合）
  /// </summary>
  public List<TargetField> Targets { get; set; } = new();

  /// <summary>
  /// 优先级（相对于提取规则）
  /// </summary>
  public int Priority { get; set; } = 1000;
}

/// <summary>
/// 后处理器
/// </summary>
public class PostProcessor
{
  /// <summary>
  /// 处理器类型
  /// </summary>
  public ProcessorType Type { get; set; }

  /// <summary>
  /// 处理器参数
  /// </summary>
  public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// 匹配选项
/// </summary>
public class MatchOptions
{
  /// <summary>
  /// 忽略大小写
  /// </summary>
  public bool IgnoreCase { get; set; } = false;

  /// <summary>
  /// 多行模式
  /// </summary>
  public bool Multiline { get; set; } = false;

  /// <summary>
  /// 单行模式（.匹配换行符）
  /// </summary>
  public bool Singleline { get; set; } = false;

  /// <summary>
  /// 全局匹配（找到所有匹配项）
  /// </summary>
  public bool Global { get; set; } = false;

  /// <summary>
  /// 最大匹配数量（0表示无限制）
  /// </summary>
  public int MaxMatches { get; set; } = 0;

  /// <summary>
  /// 自定义选项
  /// </summary>
  public Dictionary<string, object> CustomOptions { get; set; } = new();
}

/// <summary>
/// 匹配器类型枚举
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MatcherType
{
  /// <summary>
  /// 正则表达式
  /// </summary>
  Regex,

  /// <summary>
  /// Markup标记（如HTML、XML标签）
  /// </summary>
  Markup,

  /// <summary>
  /// 纯文本匹配
  /// </summary>
  Text,

  /// <summary>
  /// XPath表达式
  /// </summary>
  XPath,

  /// <summary>
  /// JSON路径
  /// </summary>
  JsonPath,

  /// <summary>
  /// CSS选择器
  /// </summary>
  CssSelector,

  /// <summary>
  /// 自定义匹配器
  /// </summary>
  Custom
}

/// <summary>
/// 操作类型枚举
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActionType
{
  /// <summary>
  /// 提取内容
  /// </summary>
  Extract,

  /// <summary>
  /// 删除内容
  /// </summary>
  Remove,

  /// <summary>
  /// 替换内容
  /// </summary>
  Replace,

  /// <summary>
  /// 转换内容
  /// </summary>
  Transform,

  /// <summary>
  /// 标记内容（不改变内容，但添加标记）
  /// </summary>
  Mark,

  /// <summary>
  /// 跳过（匹配但不处理）
  /// </summary>
  Skip
}

/// <summary>
/// 目标字段枚举
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TargetField
{
  /// <summary>
  /// 标题
  /// </summary>
  Title,

  /// <summary>
  /// 摘要
  /// </summary>
  Summary,

  /// <summary>
  /// 正文内容
  /// </summary>
  MainBody,

  /// <summary>
  /// 原文（用于删除操作）
  /// </summary>
  Source,

  /// <summary>
  /// 自定义字段
  /// </summary>
  Custom,

  /// <summary>
  /// 忽略（不输出到任何字段）
  /// </summary>
  Ignore
}

/// <summary>
/// 转换类型枚举
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransformationType
{
  /// <summary>
  /// 格式化（去除多余空格、换行等）
  /// </summary>
  Format,

  /// <summary>
  /// 截取（按长度或规则截取）
  /// </summary>
  Truncate,

  /// <summary>
  /// 合并（将多个字段合并）
  /// </summary>
  Merge,

  /// <summary>
  /// 分割（将一个字段分割为多个）
  /// </summary>
  Split,

  /// <summary>
  /// 映射（按照映射表转换值）
  /// </summary>
  Map,

  /// <summary>
  /// 计算（执行表达式计算）
  /// </summary>
  Calculate,

  /// <summary>
  /// 自定义转换
  /// </summary>
  Custom,

  /// <summary>
  /// 正则表达式提取
  /// </summary>
  RegexExtraction,

  /// <summary>
  /// 移除HTML注释
  /// </summary>
  RemoveHtmlComments,

  /// <summary>
  /// 移除运行块
  /// </summary>
  RemoveRunBlocks,

  /// <summary>
  /// 移除XML标签
  /// </summary>
  RemoveXmlTags,

  /// <summary>
  /// 清理空白字符
  /// </summary>
  CleanWhitespace,

  /// <summary>
  /// 保持格式化
  /// </summary>
  PreserveFormatting,

  /// <summary>
  /// 生成标题
  /// </summary>
  GenerateTitle,

  /// <summary>
  /// 清理URL
  /// </summary>
  CleanUrl
}

/// <summary>
/// 后处理器类型枚举
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProcessorType
{
  /// <summary>
  /// 清理HTML标签
  /// </summary>
  CleanHtml,

  /// <summary>
  /// 去除多余空白
  /// </summary>
  TrimWhitespace,

  /// <summary>
  /// 解码HTML实体
  /// </summary>
  DecodeHtml,

  /// <summary>
  /// 格式化文本
  /// </summary>
  FormatText,

  /// <summary>
  /// 验证格式
  /// </summary>
  Validate,

  /// <summary>
  /// 自定义处理器
  /// </summary>
  Custom
}