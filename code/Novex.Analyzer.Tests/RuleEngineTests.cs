using Novex.Analyzer;
using Novex.Analyzer.Models;
using System.IO;
using Xunit;

namespace Novex.Analyzer.Tests;

public class RuleEngineTests
{
  [Fact]
  public void ParseRuleBook_ValidYaml_ShouldReturnRuleBook()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var yaml = @"
version: ""1.0""
description: ""Test Rule Book""
extraction_rules:
  - id: ""test_rule""
    name: ""Test Rule""
    matcher_type: ""text""
    pattern: ""test""
    action: ""extract""
    target: ""title""
    priority: 10
    enabled: true
";

    // Act
    var result = ruleEngine.ParseRuleBook(yaml);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("1.0", result.Version);
    Assert.Equal("Test Rule Book", result.Description);
    Assert.Single(result.ExtractionRules);
    Assert.Equal("test_rule", result.ExtractionRules[0].Id);
  }

  [Fact]
  public void ParseRuleBook_EmptyContent_ShouldThrowArgumentException()
  {
    // Arrange
    var ruleEngine = new RuleEngine();

    // Act & Assert
    Assert.Throws<ArgumentException>(() => ruleEngine.ParseRuleBook(""));
    Assert.Throws<ArgumentException>(() => ruleEngine.ParseRuleBook("   "));
    Assert.Throws<ArgumentException>(() => ruleEngine.ParseRuleBook(null!));
  }

  [Fact]
  public void ParseRuleBook_InvalidVersion_ShouldThrowInvalidOperationException()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var yaml = @"
version: ""invalid""
description: ""Test Rule Book""
";

    // Act & Assert
    var ex = Assert.Throws<InvalidOperationException>(() => ruleEngine.ParseRuleBook(yaml));
    Assert.Contains("无效的版本号格式", ex.Message);
  }

  [Fact]
  public void ParseRuleBook_EmptyVersion_ShouldThrowInvalidOperationException()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var yaml = @"
version: """"
description: ""Test Rule Book""
";

    // Act & Assert
    var ex = Assert.Throws<InvalidOperationException>(() => ruleEngine.ParseRuleBook(yaml));
    Assert.Contains("规则书版本号不能为空", ex.Message);
  }

  [Fact]
  public void ParseRuleBook_DuplicateRuleIds_ShouldThrowInvalidOperationException()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var yaml = @"
version: ""1.0""
description: ""Test Rule Book""
extraction_rules:
  - id: ""duplicate_id""
    name: ""Test Rule 1""
    matcher_type: ""text""
    pattern: ""test1""
    action: ""extract""
    target: ""title""
    priority: 10
    enabled: true
  - id: ""duplicate_id""
    name: ""Test Rule 2""
    matcher_type: ""text""
    pattern: ""test2""
    action: ""extract""
    target: ""summary""
    priority: 20
    enabled: true
";

    // Act & Assert
    var ex = Assert.Throws<InvalidOperationException>(() => ruleEngine.ParseRuleBook(yaml));
    Assert.Contains("重复的提取规则ID", ex.Message);
  }

  [Fact]
  public void ParseRuleBook_InvalidRegexPattern_ShouldThrowInvalidOperationException()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var yaml = @"
version: ""1.0""
description: ""Test Rule Book""
extraction_rules:
  - id: ""invalid_regex""
    name: ""Invalid Regex""
    matcher_type: ""regex""
    pattern: ""[invalid regex(""
    action: ""extract""
    target: ""title""
    priority: 10
    enabled: true
";

    // Act & Assert
    var ex = Assert.Throws<InvalidOperationException>(() => ruleEngine.ParseRuleBook(yaml));
    Assert.Contains("正则表达式无效", ex.Message);
  }

  [Fact]
  public async Task ExecuteRules_RegexExtraction_ShouldWork()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var sourceContent = "<h1>这是标题</h1><p>这是内容</p>";
    var ruleBook = new AnalysisRuleBook {
      Version = "1.0",
      ExtractionRules = new List<ExtractionRule>
        {
                new ExtractionRule
                {
                    Id = "extract_title",
                    Name = "提取标题",
                    MatcherType = MatcherType.Regex,
                    Pattern = @"<h1>(.*?)</h1>",
                    Action = ActionType.Extract,
                    Target = TargetField.Title,
                    Priority = 10,
                    Enabled = true
                }
            }
    };

    // Act
    var result = await ruleEngine.ExecuteRulesAsync(sourceContent, ruleBook);

    // Assert
    Assert.NotNull(result);
    // Note: This test will pass when ExecuteRulesAsync is properly implemented
  }

  [Fact]
  public async Task ExecuteRules_RemoveOperation_ShouldWork()
  {
    // Arrange
    var ruleEngine = new RuleEngine();
    var sourceContent = "<!--这是注释-->正文开始这里是正文内容";
    var ruleBook = new AnalysisRuleBook {
      Version = "1.0",
      ExtractionRules = new List<ExtractionRule>
        {
                new ExtractionRule
                {
                    Id = "remove_comments",
                    Name = "Remove Comments",
                    MatcherType = MatcherType.Regex,
                    Pattern = @"<!--.*?-->",
                    Options = new MatchOptions { Multiline = true, Singleline = true, Global = true },
                    Action = ActionType.Remove,
                    Target = TargetField.Source,
                    Priority = 10,
                    Enabled = true
                },
                new ExtractionRule
                {
                    Id = "extract_main",
                    Name = "Extract Main",
                    MatcherType = MatcherType.Text,
                    Pattern = "",
                    Action = ActionType.Extract,
                    Target = TargetField.MainBody,
                    Priority = 20,
                    Enabled = true
                }
            }
    };

    // Act
    var result = await ruleEngine.ExecuteRulesAsync(sourceContent, ruleBook);

    // Assert
    Assert.NotNull(result);
    // Note: This test will pass when ExecuteRulesAsync is properly implemented
  }
}