using Novex.Data.Models;

namespace Novex.Analyzer;

/// <summary>
/// 聊天记录分析器
/// </summary>
public class ChatLogAnalyzer
{
  private readonly RuleEngine _ruleEngine;

  public ChatLogAnalyzer()
  {
    _ruleEngine = new RuleEngine();
  }

  /// <summary>
  /// 分析聊天消息
  /// </summary>
  /// <param name="message">要分析的消息内容</param>
  /// <param name="rule">分析规则（YAML格式或简单字符串）</param>
  /// <returns>分析结果</returns>
  public async Task<ChatLogAnalysisResult> AnalyzeAsync(string message, string rule)
  {
    // 基本验证
    if (string.IsNullOrWhiteSpace(message))
    {
      throw new ArgumentException("消息内容不能为空", nameof(message));
    }

    if (string.IsNullOrWhiteSpace(rule))
    {
      throw new ArgumentException("分析规则不能为空", nameof(rule));
    }

    try
    {
      // 尝试解析为YAML规则书
      if (IsYamlRuleBook(rule))
      {
        var ruleBook = _ruleEngine.ParseRuleBook(rule);
        return await _ruleEngine.ExecuteRulesAsync(message, ruleBook);
      }
      else
      {
        // 使用传统的简单规则处理
        return await AnalyzeWithSimpleRuleAsync(message, rule);
      }
    }
    catch (Exception)
    {
      // 如果规则解析失败，回退到简单规则
      return await AnalyzeWithSimpleRuleAsync(message, rule);
    }
  }

  /// <summary>
  /// 检查是否为YAML规则书格式
  /// </summary>
  private bool IsYamlRuleBook(string rule)
  {
    var ruleLower = rule.ToLower();
    return ruleLower.TrimStart().StartsWith("version:") ||
           ruleLower.Contains("extractionrules:") ||
           ruleLower.Contains("transformationrules:");
  }

  /// <summary>
  /// 使用简单规则进行分析（兼容性保持）
  /// </summary>
  private async Task<ChatLogAnalysisResult> AnalyzeWithSimpleRuleAsync(string message, string rule)
  {
    // 模拟异步分析过程，添加短暂延迟
    await Task.Delay(100);

    // 创建分析结果
    var result = new ChatLogAnalysisResult {
      Title = await ExtractTitleAsync(message),
      Summary = await ExtractSummaryAsync(message, rule),
      MainBody = await AnalyzeContentAsync(message, rule),
      CreatedAt = DateTime.Now
    };

    return result;
  }

  /// <summary>
  /// 提取标题（取消息的前50个字符）
  /// </summary>
  private async Task<string?> ExtractTitleAsync(string message)
  {
    // 模拟异步处理
    await Task.Delay(10);

    if (string.IsNullOrEmpty(message))
      return null;

    return message.Length > 50
        ? message.Substring(0, 50) + "..."
        : message;
  }

  /// <summary>
  /// 生成摘要
  /// </summary>
  private async Task<string?> ExtractSummaryAsync(string message, string rule)
  {
    // 模拟异步处理
    await Task.Delay(20);

    var summary = $"根据规则 '{rule}' 分析消息";

    if (message.Length > 100)
    {
      summary += $"，消息较长（{message.Length}字符）";
    }
    else
    {
      summary += $"，消息长度：{message.Length}字符";
    }

    return summary;
  }

  /// <summary>
  /// 分析消息内容
  /// </summary>
  private async Task<string> AnalyzeContentAsync(string message, string rule)
  {
    // 模拟异步分析处理
    await Task.Delay(50);

    var analysis = new System.Text.StringBuilder();

    analysis.AppendLine($"消息分析报告");
    analysis.AppendLine($"=================");
    analysis.AppendLine($"分析时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    analysis.AppendLine($"使用规则: {rule}");
    analysis.AppendLine();

    analysis.AppendLine($"消息统计:");
    analysis.AppendLine($"- 总字符数: {message.Length}");
    analysis.AppendLine($"- 总行数: {message.Split('\n').Length}");
    analysis.AppendLine();

    // 根据规则类型进行不同的分析
    switch (rule.ToLower())
    {
      case "length":
        await AnalyzeLengthAsync(message, analysis);
        break;
      case "sentiment":
        await AnalyzeSentimentAsync(message, analysis);
        break;
      case "keyword":
        await AnalyzeKeywordsAsync(message, analysis);
        break;
      default:
        await AnalyzeGeneralAsync(message, analysis);
        break;
    }

    analysis.AppendLine();
    analysis.AppendLine($"原始消息:");
    analysis.AppendLine($"----------");
    analysis.AppendLine(message);

    return analysis.ToString();
  }

  /// <summary>
  /// 长度分析
  /// </summary>
  private async Task AnalyzeLengthAsync(string message, System.Text.StringBuilder analysis)
  {
    // 模拟异步分析处理
    await Task.Delay(10);

    analysis.AppendLine($"长度分析:");

    if (message.Length < 10)
    {
      analysis.AppendLine($"- 消息较短，可能信息量不足");
    }
    else if (message.Length > 500)
    {
      analysis.AppendLine($"- 消息较长，信息量丰富");
    }
    else
    {
      analysis.AppendLine($"- 消息长度适中");
    }
  }

  /// <summary>
  /// 情感分析（简单版本）
  /// </summary>
  private async Task AnalyzeSentimentAsync(string message, System.Text.StringBuilder analysis)
  {
    // 模拟异步分析处理
    await Task.Delay(30);

    analysis.AppendLine($"情感分析:");

    var positiveWords = new[] { "开心", "快乐", "高兴", "喜欢", "爱", "好", "棒", "赞" };
    var negativeWords = new[] { "难过", "悲伤", "生气", "讨厌", "恨", "坏", "差", "糟" };

    int positiveCount = positiveWords.Count(word => message.Contains(word));
    int negativeCount = negativeWords.Count(word => message.Contains(word));

    if (positiveCount > negativeCount)
    {
      analysis.AppendLine($"- 情感倾向: 积极 (积极词汇: {positiveCount}, 消极词汇: {negativeCount})");
    }
    else if (negativeCount > positiveCount)
    {
      analysis.AppendLine($"- 情感倾向: 消极 (积极词汇: {positiveCount}, 消极词汇: {negativeCount})");
    }
    else
    {
      analysis.AppendLine($"- 情感倾向: 中性 (积极词汇: {positiveCount}, 消极词汇: {negativeCount})");
    }
  }

  /// <summary>
  /// 关键词分析
  /// </summary>
  private async Task AnalyzeKeywordsAsync(string message, System.Text.StringBuilder analysis)
  {
    // 模拟异步分析处理
    await Task.Delay(20);

    analysis.AppendLine($"关键词分析:");

    // 简单的关键词统计
    var commonWords = new[] { "的", "了", "是", "在", "我", "你", "他", "她", "它", "这", "那" };
    var wordCounts = commonWords
        .Select(word => new { Word = word, Count = CountOccurrences(message, word) })
        .Where(x => x.Count > 0)
        .OrderByDescending(x => x.Count)
        .Take(5);

    analysis.AppendLine($"- 常见词汇频次:");
    foreach (var wordCount in wordCounts)
    {
      analysis.AppendLine($"  '{wordCount.Word}': {wordCount.Count} 次");
    }
  }

  /// <summary>
  /// 通用分析
  /// </summary>
  private async Task AnalyzeGeneralAsync(string message, System.Text.StringBuilder analysis)
  {
    // 模拟异步分析处理
    await Task.Delay(15);

    analysis.AppendLine($"通用分析:");
    analysis.AppendLine($"- 消息类型: 文本消息");
    analysis.AppendLine($"- 包含标点符号: {(ContainsPunctuation(message) ? "是" : "否")}");
    analysis.AppendLine($"- 包含数字: {(ContainsNumbers(message) ? "是" : "否")}");
  }

  /// <summary>
  /// 计算子字符串出现次数
  /// </summary>
  private int CountOccurrences(string text, string pattern)
  {
    if (string.IsNullOrEmpty(pattern)) return 0;

    int count = 0;
    int index = 0;

    while ((index = text.IndexOf(pattern, index)) != -1)
    {
      count++;
      index += pattern.Length;
    }

    return count;
  }

  /// <summary>
  /// 检查是否包含标点符号
  /// </summary>
  private bool ContainsPunctuation(string message)
  {
    return message.Any(c => char.IsPunctuation(c));
  }

  /// <summary>
  /// 检查是否包含数字
  /// </summary>
  private bool ContainsNumbers(string message)
  {
    return message.Any(c => char.IsDigit(c));
  }
}