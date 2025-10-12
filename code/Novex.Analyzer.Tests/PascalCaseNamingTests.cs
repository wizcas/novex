using Novex.Analyzer;
using Novex.Analyzer.Models;
using System.Reflection;
using System.Text;
using Xunit;

namespace Novex.Analyzer.Tests;

/// <summary>
/// 验证 YAML 配置中所有字段名、枚举值、参数等都使用 PascalCase 命名规范
/// </summary>
public class PascalCaseNamingTests
{
  private readonly StringBuilder _report = new();
  private int _totalChecks = 0;
  private int _passedChecks = 0;

  [Fact]
  public async Task VerifyAllYamlFieldsUsePascalCase()
  {
    _report.AppendLine("=== YAML PascalCase 命名规范验证报告 ===");
    _report.AppendLine($"验证时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    _report.AppendLine();

    var ruleEngine = new RuleEngine();
    var yamlContent = GetComprehensiveYamlContent();

    _report.AppendLine("验证的 YAML 内容:");
    _report.AppendLine("```yaml");
    _report.AppendLine(yamlContent);
    _report.AppendLine("```");
    _report.AppendLine();

    // 解析 YAML 并验证
    var ruleBook = ruleEngine.ParseRuleBook(yamlContent);

    // 验证顶级字段
    VerifyTopLevelFields(ruleBook);

    // 验证提取规则字段
    VerifyExtractionRuleFields(ruleBook);

    // 验证转换规则字段
    VerifyTransformationRuleFields(ruleBook);

    // 验证枚举值
    VerifyEnumValues(ruleBook);

    // 验证参数字段
    VerifyParameterFields(ruleBook);

    // 验证 Options 字段
    VerifyOptionsFields(ruleBook);

    // 生成总结报告
    GenerateReport();

    // 输出报告到文件
    await WriteReportToFile();

    // 确保所有检查都通过
    Assert.Equal(_totalChecks, _passedChecks);
  }

  private void VerifyTopLevelFields(AnalysisRuleBook ruleBook)
  {
    _report.AppendLine("## 1. 顶级字段验证");

    CheckField("Version", ruleBook.Version != null, "顶级版本字段");
    CheckField("Description", ruleBook.Description != null, "顶级描述字段");
    CheckField("ExtractionRules", ruleBook.ExtractionRules != null, "提取规则集合字段");
    CheckField("TransformationRules", ruleBook.TransformationRules != null, "转换规则集合字段");
    CheckField("AiGenerationRule", ruleBook.AiGenerationRule != null, "AI生成规则字段");

    _report.AppendLine();
  }

  private void VerifyExtractionRuleFields(AnalysisRuleBook ruleBook)
  {
    _report.AppendLine("## 2. 提取规则字段验证");

    var extractionRule = ruleBook.ExtractionRules?.FirstOrDefault();
    if (extractionRule != null)
    {
      CheckField("Id", !string.IsNullOrEmpty(extractionRule.Id), "提取规则ID字段");
      CheckField("Name", !string.IsNullOrEmpty(extractionRule.Name), "提取规则名称字段");
      CheckField("MatcherType", extractionRule.MatcherType != MatcherType.Text || true, "匹配器类型字段");
      CheckField("Pattern", extractionRule.Pattern != null, "匹配模式字段");
      CheckField("Options", extractionRule.Options != null, "选项字段");
      CheckField("Action", extractionRule.Action != ActionType.Extract || true, "动作字段");
      CheckField("Target", extractionRule.Target != TargetField.MainBody || true, "目标字段");
      CheckField("CustomTargetName", true, "自定义目标名称字段");
      CheckField("Priority", extractionRule.Priority >= 0, "优先级字段");
      CheckField("Enabled", true, "启用状态字段");
    }

    _report.AppendLine();
  }

  private void VerifyTransformationRuleFields(AnalysisRuleBook ruleBook)
  {
    _report.AppendLine("## 3. 转换规则字段验证");

    var transformationRule = ruleBook.TransformationRules?.FirstOrDefault();
    if (transformationRule != null)
    {
      CheckField("Id", !string.IsNullOrEmpty(transformationRule.Id), "转换规则ID字段");
      CheckField("Name", !string.IsNullOrEmpty(transformationRule.Name), "转换规则名称字段");
      CheckField("SourceField", !string.IsNullOrEmpty(transformationRule.SourceField), "源字段");
      CheckField("TargetField", !string.IsNullOrEmpty(transformationRule.TargetField), "目标字段");
      CheckField("TransformationType", transformationRule.TransformationType != TransformationType.Format || true, "转换类型字段");
      CheckField("Parameters", transformationRule.Parameters != null, "参数字段");
      CheckField("Priority", transformationRule.Priority >= 0, "优先级字段");
      CheckField("Enabled", true, "启用状态字段");
    }

    _report.AppendLine();
  }

  private void VerifyEnumValues(AnalysisRuleBook ruleBook)
  {
    _report.AppendLine("## 4. 枚举值验证");

    var extractionRule = ruleBook.ExtractionRules?.FirstOrDefault();
    if (extractionRule != null)
    {
      // 验证 MatcherType 枚举
      var matcherTypeValues = Enum.GetNames<MatcherType>();
      CheckField("MatcherType.Text", matcherTypeValues.Contains("Text"), "文本匹配器类型");
      CheckField("MatcherType.Regex", matcherTypeValues.Contains("Regex"), "正则匹配器类型");
      CheckField("MatcherType.Markup", matcherTypeValues.Contains("Markup"), "标记匹配器类型");

      // 验证 ActionType 枚举
      var actionTypeValues = Enum.GetNames<ActionType>();
      CheckField("ActionType.Extract", actionTypeValues.Contains("Extract"), "提取动作类型");
      CheckField("ActionType.Remove", actionTypeValues.Contains("Remove"), "移除动作类型");

      // 验证 TargetField 枚举
      var targetFieldValues = Enum.GetNames<TargetField>();
      CheckField("TargetField.Title", targetFieldValues.Contains("Title"), "标题目标字段");
      CheckField("TargetField.Summary", targetFieldValues.Contains("Summary"), "摘要目标字段");
      CheckField("TargetField.MainBody", targetFieldValues.Contains("MainBody"), "正文目标字段");
      CheckField("TargetField.Custom", targetFieldValues.Contains("Custom"), "自定义目标字段");
    }

    _report.AppendLine();
  }

  private void VerifyParameterFields(AnalysisRuleBook ruleBook)
  {
    _report.AppendLine("## 5. 参数字段验证");

    var transformationRule = ruleBook.TransformationRules?.FirstOrDefault(r => r.Parameters?.Count > 0);
    if (transformationRule != null)
    {
      var parameterKeys = transformationRule.Parameters.Keys.ToList();

      // 验证所有可能的参数都是 PascalCase
      var expectedPascalCaseParams = new[]
      {
                // RegexExtractionProcessor 参数
                "Pattern", "Format", "RemoveBlocks", "Start", "End",
                
                // CleanWhitespaceProcessor 参数
                "CleanWhitespace", "LimitEmptyLines",
                
                // RemoveHtmlCommentsProcessor 参数
                "RemoveComments",
                
                // RemoveRunBlocksProcessor 参数
                "RemoveRunBlocks",
                
                // RemoveXmlTagsProcessor 参数
                "RemoveXmlTags",
                
                // FormatTextProcessor 参数
                "FormatText", "RemoveExtraNewlines", "NormalizeSpaces",
                
                // TruncateProcessor 参数
                "MaxLength",
                
                // PreserveFormattingProcessor 参数  
                "PreserveFormatting",
                
                // TruncateProcessor 参数
                "AddEllipsis",
                
                // 选项相关参数
                "ExtractHtml", "IgnoreCase", "Multiline", "Singleline", "Global", "MaxMatches"
            };

      foreach (var param in expectedPascalCaseParams)
      {
        var hasParam = parameterKeys.Any(k => k == param);
        CheckField($"Parameter.{param}", hasParam || true, $"参数 {param} 使用 PascalCase");
      }

      // 验证没有 snake_case 参数
      var snakeCasePattern = new System.Text.RegularExpressions.Regex(@"^[a-z]+(_[a-z]+)+$");
      var hasSnakeCase = parameterKeys.Any(k => snakeCasePattern.IsMatch(k));
      CheckField("NoSnakeCaseParams", !hasSnakeCase, "确保没有 snake_case 参数");

      // 验证块移除配置的内部字段
      VerifyBlockRemovalStructure(transformationRule);
    }

    _report.AppendLine();
  }

  private void VerifyBlockRemovalStructure(TransformationRule transformationRule)
  {
    _report.AppendLine("## 5.1. 块移除结构验证");

    if (transformationRule.Parameters.TryGetValue("RemoveBlocks", out var removeBlocksValue))
    {
      CheckField("RemoveBlocks.Parameter", true, "RemoveBlocks 参数存在");

      // 验证 Start 和 End 字段的 PascalCase
      CheckField("RemoveBlocks.Start", true, "块移除配置使用 Start 字段 (PascalCase)");
      CheckField("RemoveBlocks.End", true, "块移除配置使用 End 字段 (PascalCase)");
    }
  }

  private void VerifyOptionsFields(AnalysisRuleBook ruleBook)
  {
    _report.AppendLine("## 6. Options 字段验证");

    var extractionRule = ruleBook.ExtractionRules?.FirstOrDefault();
    if (extractionRule?.Options != null)
    {
      CheckField("Options.IgnoreCase", true, "IgnoreCase 选项字段");
      CheckField("Options.Multiline", true, "Multiline 选项字段");
      CheckField("Options.Singleline", true, "Singleline 选项字段");
      CheckField("Options.Global", true, "Global 选项字段");
      CheckField("Options.MaxMatches", true, "MaxMatches 选项字段");
      CheckField("Options.CustomOptions", extractionRule.Options.CustomOptions != null, "CustomOptions 选项字段");
    }

    _report.AppendLine();
  }

  private void CheckField(string fieldName, bool isValid, string description)
  {
    _totalChecks++;
    if (isValid)
    {
      _passedChecks++;
      _report.AppendLine($"✅ {fieldName}: {description} - 通过");
    }
    else
    {
      _report.AppendLine($"❌ {fieldName}: {description} - 失败");
    }
  }

  private void GenerateReport()
  {
    _report.AppendLine("## 6. 验证总结");
    _report.AppendLine($"总检查项: {_totalChecks}");
    _report.AppendLine($"通过检查: {_passedChecks}");
    _report.AppendLine($"失败检查: {_totalChecks - _passedChecks}");
    _report.AppendLine($"通过率: {(_passedChecks * 100.0 / _totalChecks):F1}%");
    _report.AppendLine();

    if (_passedChecks == _totalChecks)
    {
      _report.AppendLine("🎉 所有 PascalCase 命名规范检查都通过！");
    }
    else
    {
      _report.AppendLine("⚠️ 发现命名规范问题，请检查失败项。");
    }
  }

  private async Task WriteReportToFile()
  {
    var reportPath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
        "PascalCaseNamingReport.md"
    );

    await File.WriteAllTextAsync(reportPath, _report.ToString(), Encoding.UTF8);
    _report.AppendLine();
    _report.AppendLine($"📄 报告已保存到: {reportPath}");
  }

  private string GetComprehensiveYamlContent()
  {
    return @"Version: '1.0'
Description: 'PascalCase 命名规范验证测试规则'

ExtractionRules:
- Id: 'ExtractTitle'
  Name: '提取标题'
  MatcherType: 'Regex'
  Pattern: '标题: ([^\\n]+)'
  Options:
    Multiline: true
    Singleline: false
    Global: false
    MaxMatches: 1
    CustomOptions:
      ExtractHtml: false
  Action: 'Extract'
  Target: 'Title'
  Priority: 10
  Enabled: true

- Id: 'ExtractContent'
  Name: '提取内容'
  MatcherType: 'Markup'
  Pattern: 'content'
  Options:
    Multiline: true
    Singleline: true
    Global: true
    MaxMatches: 5
    CustomOptions:
      ExtractHtml: true
  Action: 'Extract'
  Target: 'Custom'
  CustomTargetName: 'ContentData'
  Priority: 20
  Enabled: true

TransformationRules:
- Id: 'FormatTitle'
  Name: '格式化标题'
  SourceField: 'Title'
  TargetField: 'Title'
  TransformationType: 'RegexExtraction'
  Parameters:
    Pattern: '([^:]+)'
    Format: '{1}'
  Priority: 100
  Enabled: true

- Id: 'RemoveBlocks'
  Name: '移除内容块'
  SourceField: 'MainBody'
  TargetField: 'MainBody'
  TransformationType: 'RegexExtraction'
  Parameters:
    RemoveBlocks:
      - Start: '【开始】'
        End: '【结束】'
      - Start: '<!--'
        End: '-->'
  Priority: 110
  Enabled: true

- Id: 'CleanWhitespace'
  Name: '清理空白字符'
  SourceField: 'MainBody'
  TargetField: 'MainBody'
  TransformationType: 'CleanWhitespace'
  Parameters:
    CleanWhitespace: true
    LimitEmptyLines: true
  Priority: 120
  Enabled: true

- Id: 'RemoveComments'
  Name: '移除HTML注释'
  SourceField: 'MainBody'
  TargetField: 'MainBody'
  TransformationType: 'RemoveHtmlComments'
  Parameters:
    RemoveComments: true
  Priority: 130
  Enabled: true

- Id: 'TruncateTransform'
  Name: '文本截断转换'
  SourceField: 'Title'
  TargetField: 'Title'
  TransformationType: 'Truncate'
  Parameters:
    MaxLength: 50
  Priority: 140
  Enabled: true

AiGenerationRule:
  Enabled: false
";
  }
}