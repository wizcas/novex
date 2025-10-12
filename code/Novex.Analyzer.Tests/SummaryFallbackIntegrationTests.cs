using Novex.Analyzer;
using Xunit;

namespace Novex.Analyzer.Tests;

public class SummaryFallbackIntegrationTests
{
  [Fact]
  public async Task SummaryFallback_ShouldGenerateFromMainBody_WhenSummaryIsEmpty()
  {
    // Arrange
    var ruleEngine = new RuleEngine();

    var yamlContent = @"
Version: '1.0'
Description: '摘要回退测试规则'

ExtractionRules:
  - Id: 'ExtractMainBody'
    Name: '提取主体内容'
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
  - Id: 'SummaryFallback'
    Name: '摘要回退处理'
    Type: 'SummaryFallback'
    Parameters:
      MaxLength: 30
      AddEllipsis: true
      SourceField: 'MainBody'
      TargetField: 'Summary'
    Priority: 200
    Enabled: true
";

    var sourceContent = "这是一个很长的梦境内容，用于测试摘要回退功能。当没有提取到摘要时，系统应该自动从MainBody的前30字生成一个回退摘要。";

    // Act
    var ruleBook = ruleEngine.ParseRuleBook(yamlContent);
    var result = await ruleEngine.ExecuteRulesAsync(sourceContent, ruleBook);

    // Assert
    Assert.NotNull(result);
    Assert.NotNull(result.Summary);
    Assert.NotEmpty(result.Summary);

    // 摘要应该是从MainBody生成的回退摘要
    Assert.Contains("这是一个很长的梦境内容", result.Summary);
    Assert.True(result.Summary.Length <= 33); // 30 + "..." = 33
    Assert.Contains("...", result.Summary); // 应该添加省略号
  }

  [Fact]
  public async Task SummaryFallback_ShouldNotOverrideExistingSummary()
  {
    // Arrange
    var ruleEngine = new RuleEngine();

    var yamlContent = @"
Version: '1.0'
Description: '测试不覆盖现有摘要'

ExtractionRules:
  - Id: 'ExtractSummary'
    Name: '提取摘要'
    MatcherType: 'Regex'
    Pattern: '摘要:([^\\n]+)'
    Options:
      Global: false
      MaxMatches: 1
    Action: 'Extract'
    Target: 'Summary'
    Priority: 5
    Enabled: true

  - Id: 'ExtractMainBody'
    Name: '提取主体内容'
    MatcherType: 'Regex'
    Pattern: '正文:(.+)'
    Action: 'Extract'
    Target: 'MainBody'
    Priority: 10
    Enabled: true

PostProcessingRules:
  - Id: 'SummaryFallback'
    Name: '摘要回退处理'
    Type: 'SummaryFallback'
    Parameters:
      MaxLength: 30
      AddEllipsis: true
    Priority: 200
    Enabled: true
";

    var sourceContent = "摘要:已存在的摘要内容\n正文:这是正文内容，应该不会被用作摘要";

    // Act
    var ruleBook = ruleEngine.ParseRuleBook(yamlContent);
    var result = await ruleEngine.ExecuteRulesAsync(sourceContent, ruleBook);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("已存在的摘要内容", result.Summary);
    Assert.Equal("这是正文内容，应该不会被用作摘要", result.MainBody);
  }
}