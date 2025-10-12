using Novex.Analyzer;
using Novex.Analyzer.Models;
using Xunit;

namespace Novex.Analyzer.Tests;

public class PostProcessingRuleTests
{
  [Fact]
  public async Task SummaryFallbackProcessor_ShouldGenerateSummaryFromMainBody_WhenSummaryIsEmpty()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var yamlContent = @"
Version: '1.0'
Description: 'PostProcessing 测试规则'

ExtractionRules:
  - Id: 'extract_main_body'
    Name: '提取正文'
    MatcherType: 'Regex'
    Pattern: '^(.*)$'
    Options:
      Multiline: true
      Singleline: true
    Action: 'Extract'
    Target: 'MainBody'
    Priority: 10
    Enabled: true

PostProcessingRules:
  - Id: 'summary_fallback'
    Name: '摘要回退处理'
    Type: 'SummaryFallback'
    Parameters:
      MaxLength: 30
      AddEllipsis: true
      SourceField: 'MainBody'
      TargetField: 'Summary'
    Priority: 100
    Enabled: true
";

    var sourceContent = "这是一个很长的文本内容，用于测试摘要回退功能。当没有从规则中提取到摘要时，应该从正文前30字生成摘要。";

    // Act
    var ruleBook = ruleEngine.ParseRuleBook(yamlContent);
    var result = await ruleEngine.ExecuteRulesAsync(sourceContent, ruleBook);

    // Assert
    Assert.NotNull(result);
    Assert.NotNull(result.Summary);
    Assert.NotEmpty(result.Summary);
    Assert.True(result.Summary.Length <= 33); // 30 + "..." = 33
    Assert.Contains("这是一个很长的文本内容", result.Summary);
    Assert.Equal(sourceContent, result.MainBody);
  }

  [Fact]
  public async Task SummaryFallbackProcessor_ShouldNotOverrideExistingSummary()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var yamlContent = @"
Version: '1.0'
Description: 'PostProcessing 测试规则 - 不覆盖现有摘要'

ExtractionRules:
  - Id: 'extract_summary'
    Name: '提取摘要'
    MatcherType: 'Regex'
    Pattern: '摘要：(.+)'
    Action: 'Extract'
    Target: 'Summary'
    Priority: 5
    Enabled: true

  - Id: 'extract_main_body'
    Name: '提取正文'
    MatcherType: 'Regex'
    Pattern: '正文：(.+)'
    Action: 'Extract'
    Target: 'MainBody'
    Priority: 10
    Enabled: true

PostProcessingRules:
  - Id: 'summary_fallback'
    Name: '摘要回退处理'
    Type: 'SummaryFallback'
    Parameters:
      MaxLength: 30
      AddEllipsis: true
    Priority: 100
    Enabled: true
";

    var sourceContent = "摘要：已存在的摘要内容\n正文：这是正文内容，应该不会被用作摘要";

    // Act
    var ruleBook = ruleEngine.ParseRuleBook(yamlContent);
    var result = await ruleEngine.ExecuteRulesAsync(sourceContent, ruleBook);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("已存在的摘要内容", result.Summary);
    Assert.Equal("这是正文内容，应该不会被用作摘要", result.MainBody);
  }

  [Fact]
  public async Task PostProcessingRules_ShouldBeExecutedInPriorityOrder()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var yamlContent = @"
Version: '1.0'
Description: 'PostProcessing 优先级测试'

ExtractionRules:
  - Id: 'extract_main_body'
    Name: '提取正文'
    MatcherType: 'Regex'
    Pattern: '^(.*)$'
    Options:
      Multiline: true
      Singleline: true
    Action: 'Extract'
    Target: 'MainBody'
    Priority: 10
    Enabled: true

PostProcessingRules:
  - Id: 'summary_fallback_low'
    Name: '低优先级摘要回退'
    Type: 'SummaryFallback'
    Parameters:
      MaxLength: 20
      AddEllipsis: false
    Priority: 200
    Enabled: true

  - Id: 'summary_fallback_high'
    Name: '高优先级摘要回退'
    Type: 'SummaryFallback'
    Parameters:
      MaxLength: 15
      AddEllipsis: true
    Priority: 100
    Enabled: true
";

    var sourceContent = "这是测试内容用于验证优先级顺序执行";

    // Act
    var ruleBook = ruleEngine.ParseRuleBook(yamlContent);
    var result = await ruleEngine.ExecuteRulesAsync(sourceContent, ruleBook);

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Summary);
    // 高优先级的规则应该先执行，所以长度应该是15 + "..." = 18
    Assert.True(result.Summary.Length <= 18);
    Assert.Contains("...", result.Summary);
  }

  [Fact]
  public void ParseRuleBook_ShouldAcceptPostProcessingRules()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var yamlContent = @"
Version: '1.0'
Description: 'PostProcessing YAML 解析测试'

PostProcessingRules:
  - Id: 'summary_fallback'
    Name: '摘要回退处理'
    Type: 'SummaryFallback'
    Parameters:
      MaxLength: 50
      AddEllipsis: true
      SourceField: 'MainBody'
      TargetField: 'Summary'
    Priority: 100
    Enabled: true
    Condition: 'Summary=='
";

    // Act & Assert
    var ruleBook = ruleEngine.ParseRuleBook(yamlContent);

    Assert.NotNull(ruleBook);
    Assert.NotNull(ruleBook.PostProcessingRules);
    Assert.Single(ruleBook.PostProcessingRules);

    var rule = ruleBook.PostProcessingRules.First();
    Assert.Equal("summary_fallback", rule.Id);
    Assert.Equal("摘要回退处理", rule.Name);
    Assert.Equal(ProcessorType.SummaryFallback, rule.Type);
    Assert.Equal(100, rule.Priority);
    Assert.True(rule.Enabled);
    Assert.Equal("Summary==", rule.Condition);
    Assert.Contains("MaxLength", rule.Parameters.Keys);
    Assert.Equal(50, Convert.ToInt32(rule.Parameters["MaxLength"]));
  }
}