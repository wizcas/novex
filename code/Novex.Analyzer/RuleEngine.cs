using HtmlAgilityPack;
using Novex.Analyzer.Models;
using Novex.Analyzer.Processors;
using Novex.Data.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Novex.Analyzer;

/// <summary>
/// 规则引擎 - 负责解析和执行分析规则
/// </summary>
public class RuleEngine
{
  private readonly IDeserializer _yamlDeserializer;
  private readonly Dictionary<ProcessorType, IPostProcessor> _postProcessors;
  private readonly Dictionary<TransformationType, ITransformationProcessor> _transformationProcessors;

  public RuleEngine()
  {
    _yamlDeserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    _postProcessors = new Dictionary<ProcessorType, IPostProcessor>
    {
            { ProcessorType.TrimWhitespace, new TrimWhitespaceProcessor() },
            { ProcessorType.FormatText, new FormatTextProcessor() },
            { ProcessorType.CleanHtml, new CleanHtmlProcessor() },
            { ProcessorType.DecodeHtml, new DecodeHtmlProcessor() }
        };

    _transformationProcessors = new Dictionary<TransformationType, ITransformationProcessor>
    {
            { TransformationType.Format, new FormatTransformationProcessor() },
            { TransformationType.Truncate, new TruncateProcessor() },
            { TransformationType.Custom, new CustomTransformationProcessor() }, // Keep for backward compatibility
            { TransformationType.RegexExtraction, new RegexExtractionProcessor() },
            { TransformationType.RemoveHtmlComments, new RemoveHtmlCommentsProcessor() },
            { TransformationType.RemoveRunBlocks, new RemoveRunBlocksProcessor() },
            { TransformationType.RemoveXmlTags, new RemoveXmlTagsProcessor() },
            { TransformationType.CleanWhitespace, new CleanWhitespaceProcessor() },
            { TransformationType.PreserveFormatting, new PreserveFormattingProcessor() },
            { TransformationType.GenerateTitle, new GenerateTitleProcessor() },
            { TransformationType.CleanUrl, new CleanUrlProcessor() }
        };
  }

  /// <summary>
  /// 从YAML字符串解析规则书，并进行验证，只允许解析合规的规则书内容
  /// </summary>
  public AnalysisRuleBook ParseRuleBook(string yamlContent)
  {
    if (string.IsNullOrWhiteSpace(yamlContent))
    {
      throw new ArgumentException("规则书内容不能为空", nameof(yamlContent));
    }

    try
    {
      // 解析YAML
      var ruleBook = _yamlDeserializer.Deserialize<AnalysisRuleBook>(yamlContent);

      // 如果解析结果为null，创建一个默认对象
      if (ruleBook == null)
      {
        throw new InvalidOperationException("YAML内容解析为空，请检查格式是否正确");
      }

      // 验证规则书内容
      ValidateRuleBook(ruleBook);

      return ruleBook;
    }
    catch (ArgumentException)
    {
      // 重新抛出参数异常
      throw;
    }
    catch (InvalidOperationException)
    {
      // 重新抛出验证异常
      throw;
    }
    catch (YamlDotNet.Core.YamlException ex)
    {
      throw new InvalidOperationException($"YAML格式错误: {ex.Message}。请检查缩进、引号和语法是否正确。", ex);
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"规则书解析失败: {ex.GetType().Name} - {ex.Message}", ex);
    }
  }

  /// <summary>
  /// 验证规则书内容的有效性
  /// </summary>
  private void ValidateRuleBook(AnalysisRuleBook ruleBook)
  {
    if (ruleBook == null)
    {
      throw new InvalidOperationException("规则书不能为空");
    }

    // 验证版本号
    if (string.IsNullOrWhiteSpace(ruleBook.Version))
    {
      throw new InvalidOperationException("规则书版本号不能为空");
    }

    // 验证版本号格式
    if (!IsValidVersion(ruleBook.Version))
    {
      throw new InvalidOperationException($"无效的版本号格式: {ruleBook.Version}");
    }

    // 验证提取规则
    ValidateExtractionRules(ruleBook.ExtractionRules);

    // 验证转换规则
    ValidateTransformationRules(ruleBook.TransformationRules);

    // 验证AI生成规则
    if (ruleBook.AiGenerationRule != null)
    {
      ValidateAiGenerationRule(ruleBook.AiGenerationRule);
    }
  }

  /// <summary>
  /// 验证版本号格式
  /// </summary>
  private bool IsValidVersion(string version)
  {
    // 支持的版本格式: x.x, x.x.x, x.x.x.x
    var versionRegex = new Regex(@"^\d+(\.\d+){1,3}$");
    return versionRegex.IsMatch(version);
  }

  /// <summary>
  /// 验证提取规则
  /// </summary>
  private void ValidateExtractionRules(List<ExtractionRule> rules)
  {
    if (rules == null) return;

    var ruleIds = new HashSet<string>();

    foreach (var rule in rules)
    {
      // 验证规则ID唯一性
      if (string.IsNullOrWhiteSpace(rule.Id))
      {
        throw new InvalidOperationException("提取规则ID不能为空");
      }

      if (!ruleIds.Add(rule.Id))
      {
        throw new InvalidOperationException($"重复的提取规则ID: {rule.Id}");
      }

      // 验证规则名称
      if (string.IsNullOrWhiteSpace(rule.Name))
      {
        throw new InvalidOperationException($"提取规则 '{rule.Id}' 的名称不能为空");
      }

      // 验证匹配模式
      if (rule.MatcherType == MatcherType.Regex && !string.IsNullOrWhiteSpace(rule.Pattern))
      {
        try
        {
          _ = new Regex(rule.Pattern);
        }
        catch (ArgumentException ex)
        {
          throw new InvalidOperationException($"提取规则 '{rule.Id}' 的正则表达式无效: {ex.Message}");
        }
      }

      // 验证优先级
      if (rule.Priority < 0)
      {
        throw new InvalidOperationException($"提取规则 '{rule.Id}' 的优先级不能为负数");
      }

      // 验证自定义目标字段
      if (rule.Target == TargetField.Custom && string.IsNullOrWhiteSpace(rule.CustomTargetName))
      {
        throw new InvalidOperationException($"提取规则 '{rule.Id}' 使用自定义目标时必须指定目标字段名");
      }
    }
  }

  /// <summary>
  /// 验证转换规则
  /// </summary>
  private void ValidateTransformationRules(List<TransformationRule> rules)
  {
    if (rules == null) return;

    var ruleIds = new HashSet<string>();

    foreach (var rule in rules)
    {
      // 验证规则ID唯一性
      if (string.IsNullOrWhiteSpace(rule.Id))
      {
        throw new InvalidOperationException("转换规则ID不能为空");
      }

      if (!ruleIds.Add(rule.Id))
      {
        throw new InvalidOperationException($"重复的转换规则ID: {rule.Id}");
      }

      // 验证规则名称
      if (string.IsNullOrWhiteSpace(rule.Name))
      {
        throw new InvalidOperationException($"转换规则 '{rule.Id}' 的名称不能为空");
      }

      // 验证源字段和目标字段
      if (string.IsNullOrWhiteSpace(rule.SourceField))
      {
        throw new InvalidOperationException($"转换规则 '{rule.Id}' 的源字段不能为空");
      }

      if (string.IsNullOrWhiteSpace(rule.TargetField))
      {
        throw new InvalidOperationException($"转换规则 '{rule.Id}' 的目标字段不能为空");
      }

      // 验证优先级
      if (rule.Priority < 0)
      {
        throw new InvalidOperationException($"转换规则 '{rule.Id}' 的优先级不能为负数");
      }
    }
  }

  /// <summary>
  /// 验证AI生成规则
  /// </summary>
  private void ValidateAiGenerationRule(AiGenerationRule rule)
  {
    if (!rule.Enabled) return;

    // 验证提供商
    var validProviders = new[] { "openai", "anthropic", "custom" };
    if (string.IsNullOrWhiteSpace(rule.Provider) || !validProviders.Contains(rule.Provider.ToLower()))
    {
      throw new InvalidOperationException($"无效的AI提供商: {rule.Provider}. 有效值: {string.Join(", ", validProviders)}");
    }

    // 验证模型名称（当启用AI生成时）
    if (string.IsNullOrWhiteSpace(rule.Model))
    {
      throw new InvalidOperationException("启用AI生成时，模型名称不能为空");
    }

    // 验证生成目标
    if (rule.Targets == null || rule.Targets.Count == 0)
    {
      throw new InvalidOperationException("AI生成规则必须指定至少一个生成目标");
    }

    // 验证优先级
    if (rule.Priority < 0)
    {
      throw new InvalidOperationException("AI生成规则的优先级不能为负数");
    }
  }

  /// <summary>
  /// 执行分析规则
  /// </summary>
  public async Task<ChatLogAnalysisResult> ExecuteRulesAsync(string sourceContent, AnalysisRuleBook ruleBook)
  {
    var result = new ChatLogAnalysisResult();
    var workingContent = sourceContent;
    var extractedData = new Dictionary<string, string>();

    // 1. 执行提取规则
    await ExecuteExtractionRulesAsync(workingContent, ruleBook.ExtractionRules, extractedData);

    // 2. 执行转换规则
    await ExecuteTransformationRulesAsync(extractedData, ruleBook.TransformationRules);

    // 3. 执行AI生成规则（如果启用）
    if (ruleBook.AiGenerationRule?.Enabled == true)
    {
      await ExecuteAiGenerationRuleAsync(extractedData, ruleBook.AiGenerationRule);
    }

    // 4. 将结果映射到ChatLogAnalysisResult
    result.Title = extractedData.GetValueOrDefault("Title", extractedData.GetValueOrDefault("title", "")).Trim();
    result.Summary = extractedData.GetValueOrDefault("Summary", extractedData.GetValueOrDefault("summary", "")).Trim();
    result.MainBody = extractedData.GetValueOrDefault("MainBody", "").Trim();

    return result;
  }

  private async Task ExecuteExtractionRulesAsync(string sourceContent, List<ExtractionRule> rules, Dictionary<string, string> extractedData)
  {
    var workingContent = sourceContent;
    var sortedRules = rules.Where(r => r.Enabled).OrderBy(r => r.Priority).ToList();

    // 按Target分组，跟踪哪些目标已经成功提取
    var extractedTargets = new HashSet<string>();

    foreach (var rule in sortedRules)
    {
      try
      {
        var matches = await FindMatchesAsync(workingContent, rule);
        var processedMatches = await ApplyPostProcessorsAsync(matches, rule.PostProcessors);

        switch (rule.Action)
        {
          case ActionType.Extract:
            var targetKey = GetTargetKey(rule.Target, rule.CustomTargetName);

            // 只有当目标字段还没有被成功提取时，才执行提取
            if (processedMatches.Any() && !extractedTargets.Contains(targetKey))
            {
              extractedData[targetKey] = string.Join("\n", processedMatches);
              extractedTargets.Add(targetKey);
            }
            break;

          case ActionType.Remove:
            workingContent = await RemoveMatchesAsync(workingContent, rule);
            break;

          case ActionType.Replace:
            workingContent = await ReplaceMatchesAsync(workingContent, rule);
            break;
        }

        // 更新source内容用于后续规则
        extractedData["source"] = workingContent;
      }
      catch (Exception ex)
      {
        // 记录错误但继续处理其他规则
        Console.WriteLine($"Error executing rule {rule.Id}: {ex.Message}");
      }
    }
  }

  private async Task ExecuteTransformationRulesAsync(Dictionary<string, string> extractedData, List<TransformationRule> rules)
  {
    var sortedRules = rules.Where(r => r.Enabled).OrderBy(r => r.Priority).ToList();

    foreach (var rule in sortedRules)
    {
      try
      {
        if (extractedData.TryGetValue(rule.SourceField, out var sourceValue))
        {
          var processor = _transformationProcessors[rule.TransformationType];
          var transformedValue = await processor.ProcessAsync(sourceValue, rule.Parameters);
          extractedData[rule.TargetField] = transformedValue;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error executing transformation rule {rule.Id}: {ex.Message}");
      }
    }
  }

  private async Task ExecuteAiGenerationRuleAsync(Dictionary<string, string> extractedData, AiGenerationRule rule)
  {
    // TODO: 实现AI生成逻辑
    // 这里暂时跳过，后续可以集成实际的AI服务
    await Task.CompletedTask;
  }

  private async Task<List<string>> FindMatchesAsync(string content, ExtractionRule rule)
  {
    var matches = new List<string>();

    switch (rule.MatcherType)
    {
      case MatcherType.Regex:
        matches = await FindRegexMatchesAsync(content, rule);
        break;

      case MatcherType.Markup:
        matches = await FindMarkupMatchesAsync(content, rule);
        break;

      case MatcherType.Text:
        matches = await FindTextMatchesAsync(content, rule);
        break;

      default:
        throw new NotSupportedException($"Matcher type {rule.MatcherType} is not supported yet.");
    }

    return matches;
  }

  private async Task<List<string>> FindRegexMatchesAsync(string content, ExtractionRule rule)
  {
    var options = RegexOptions.None;
    if (rule.Options.IgnoreCase) options |= RegexOptions.IgnoreCase;
    if (rule.Options.Multiline) options |= RegexOptions.Multiline;
    if (rule.Options.Singleline) options |= RegexOptions.Singleline;

    var regex = new Regex(rule.Pattern, options);
    var matches = regex.Matches(content);

    var results = new List<string>();
    var maxMatches = rule.Options.MaxMatches > 0 ? rule.Options.MaxMatches : int.MaxValue;

    for (int i = 0; i < Math.Min(matches.Count, maxMatches); i++)
    {
      var match = matches[i];
      // 如果有捕获组，使用第一个捕获组，否则使用整个匹配
      var value = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
      results.Add(value);

      if (!rule.Options.Global) break;
    }

    return await Task.FromResult(results);
  }

  private async Task<List<string>> FindMarkupMatchesAsync(string content, ExtractionRule rule)
  {
    var results = new List<string>();

    try
    {
      var doc = new HtmlDocument();
      doc.LoadHtml(content);

      // 如果Pattern是XPath选择器
      if (rule.Pattern.StartsWith("//") || rule.Pattern.StartsWith("/"))
      {
        var nodes = doc.DocumentNode.SelectNodes(rule.Pattern);
        if (nodes != null)
        {
          var maxMatches = rule.Options.MaxMatches > 0 ? rule.Options.MaxMatches : int.MaxValue;
          foreach (var node in nodes.Take(maxMatches))
          {
            // 根据需要返回节点的内容或HTML
            var extractHtml = rule.Options.CustomOptions.GetValueOrDefault("ExtractHtml", false);
            var value = (extractHtml is bool boolVal && boolVal)
              ? node.OuterHtml
              : node.InnerText;
            results.Add(value);

            if (!rule.Options.Global) break;
          }
        }
      }
      // 如果Pattern是标签名
      else if (!rule.Pattern.Contains("<") && !rule.Pattern.Contains(">"))
      {
        var nodes = doc.DocumentNode.SelectNodes($"//{rule.Pattern}");
        if (nodes != null)
        {
          var maxMatches = rule.Options.MaxMatches > 0 ? rule.Options.MaxMatches : int.MaxValue;
          foreach (var node in nodes.Take(maxMatches))
          {
            var extractHtml = rule.Options.CustomOptions.GetValueOrDefault("ExtractHtml", false);
            var value = (extractHtml is bool boolVal && boolVal)
              ? node.OuterHtml
              : node.InnerText;
            results.Add(value);

            if (!rule.Options.Global) break;
          }
        }
      }
      // 如果Pattern是HTML标签格式（如<tag>content</tag>），使用正则表达式作为备用
      else
      {
        return await FindRegexMatchesAsync(content, rule);
      }
    }
    catch (Exception ex)
    {
      // 如果HtmlAgilityPack解析失败，回退到正则表达式
      Console.WriteLine($"HtmlAgilityPack parsing failed for rule {rule.Id}: {ex.Message}. Falling back to regex.");
      return await FindRegexMatchesAsync(content, rule);
    }

    return await Task.FromResult(results);
  }

  private async Task<List<string>> FindTextMatchesAsync(string content, ExtractionRule rule)
  {
    var results = new List<string>();
    var comparison = rule.Options.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    var index = content.IndexOf(rule.Pattern, comparison);
    while (index >= 0)
    {
      results.Add(rule.Pattern);
      if (!rule.Options.Global) break;

      index = content.IndexOf(rule.Pattern, index + rule.Pattern.Length, comparison);
    }

    return await Task.FromResult(results);
  }

  private async Task<List<string>> ApplyPostProcessorsAsync(List<string> matches, List<PostProcessor> postProcessors)
  {
    var results = matches.ToList();

    foreach (var postProcessor in postProcessors)
    {
      if (_postProcessors.TryGetValue(postProcessor.Type, out var processor))
      {
        for (int i = 0; i < results.Count; i++)
        {
          results[i] = await processor.ProcessAsync(results[i], postProcessor.Parameters);
        }
      }
    }

    return results;
  }

  private async Task<string> RemoveMatchesAsync(string content, ExtractionRule rule)
  {
    var options = RegexOptions.None;
    if (rule.Options.IgnoreCase) options |= RegexOptions.IgnoreCase;
    if (rule.Options.Multiline) options |= RegexOptions.Multiline;
    if (rule.Options.Singleline) options |= RegexOptions.Singleline;

    var regex = new Regex(rule.Pattern, options);
    return await Task.FromResult(regex.Replace(content, ""));
  }

  private async Task<string> ReplaceMatchesAsync(string content, ExtractionRule rule)
  {
    var options = RegexOptions.None;
    if (rule.Options.IgnoreCase) options |= RegexOptions.IgnoreCase;
    if (rule.Options.Multiline) options |= RegexOptions.Multiline;
    if (rule.Options.Singleline) options |= RegexOptions.Singleline;

    var regex = new Regex(rule.Pattern, options);
    var replacement = rule.ReplacementValue ?? "";
    return await Task.FromResult(regex.Replace(content, replacement));
  }

  private static string GetTargetKey(TargetField target, string? customTargetName)
  {
    return target switch {
      TargetField.Title => "Title",
      TargetField.Summary => "Summary",
      TargetField.MainBody => "MainBody",
      TargetField.Source => "Source",
      TargetField.Custom => customTargetName ?? "Custom",
      TargetField.Ignore => "Ignore",
      _ => "Unknown"
    };
  }
}

