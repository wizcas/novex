using Novex.Analyzer;
using Novex.Analyzer.Models;
using System.IO;
using Xunit;

namespace Novex.Analyzer.Tests;

public class DreamPhoneRulesTests
{
  [Fact]
  public async Task ProcessDreamPhoneContent_ShouldExtractCorrectStructure()
  {
    // Arrange
    var ruleEngine = new RuleEngine();

    // 获取项目目录路径
    var testProjectDir = Path.GetDirectoryName(typeof(DreamPhoneRulesTests).Assembly.Location);
    var projectRoot = Path.GetFullPath(Path.Combine(testProjectDir!, "..", "..", ".."));
    var mockDataPath = Path.Combine(projectRoot, "MockData", "dream-phone");

    var rawContent = await File.ReadAllTextAsync(Path.Combine(mockDataPath, "raw.md"));
    var expectedResult = await File.ReadAllTextAsync(Path.Combine(mockDataPath, "result.md"));
    var rulesYaml = await File.ReadAllTextAsync(Path.Combine(mockDataPath, "rules.yaml"));

    var ruleBook = ruleEngine.ParseRuleBook(rulesYaml);

    // Act
    var result = await ruleEngine.ExecuteRulesAsync(rawContent, ruleBook);

    // Assert
    Assert.NotNull(result);

    // 验证标题提取
    Assert.Contains("初次接触", result.Title ?? "");
    Assert.Contains("虚假的小白兔", result.Title ?? "");

    // 验证摘要提取
    Assert.Contains("陈晨开车送林晨的女友顾云回家", result.Summary ?? "");
    Assert.Contains("展现出其真实火爆的性格", result.Summary ?? "");

    // 验证正文提取
    Assert.Contains("*这女人……搞什么鬼？", result.MainBody ?? "");
    Assert.Contains("顾云发出了一声短促的音节", result.MainBody ?? "");

    // 验证注释已被移除
    Assert.DoesNotContain("<!--run:", result.MainBody ?? "");
    Assert.DoesNotContain("复述:", result.MainBody ?? "");
  }

  [Fact]
  public async Task ProcessContentWithoutDreamTag_ShouldUseFullContentWithXmlTagsRemoved()
  {
    // Arrange
    var ruleEngine = new RuleEngine();

    // 获取项目目录路径
    var testProjectDir = Path.GetDirectoryName(typeof(DreamPhoneRulesTests).Assembly.Location);
    var projectRoot = Path.GetFullPath(Path.Combine(testProjectDir!, "..", "..", ".."));
    var mockDataPath = Path.Combine(projectRoot, "MockData", "dream-phone");
    var rulesYaml = await File.ReadAllTextAsync(Path.Combine(mockDataPath, "rules.yaml"));

    // 创建没有 <dream> 标签的测试内容
    var contentWithoutDreamTag = @"
<plot>
当前章节: 第二章
事件名: 测试事件
摘要: 这是一个测试摘要。
</plot>

这是正文的第一段。

<!--run: 这是一个注释，应该被移除 -->

<phone>这里有一个手机标签，应该被移除但内容保留</phone>

<input>用户输入内容，标签应该被移除</input>

这是正文的最后一段。
";

    var ruleBook = ruleEngine.ParseRuleBook(rulesYaml);

    // Act
    var result = await ruleEngine.ExecuteRulesAsync(contentWithoutDreamTag, ruleBook);

    // Assert
    Assert.NotNull(result);

    // 验证应该提取标题和摘要（因为有 <plot> 标签）
    Assert.Contains("第二章", result.Title ?? "");
    Assert.Contains("测试事件", result.Title ?? "");
    Assert.Contains("这是一个测试摘要", result.Summary ?? "");

    // 验证正文内容
    Assert.NotNull(result.MainBody);
    Assert.Contains("这是正文的第一段", result.MainBody);
    Assert.Contains("这里有一个手机标签，应该被移除但内容保留", result.MainBody);
    Assert.Contains("用户输入内容，标签应该被移除", result.MainBody);
    Assert.Contains("这是正文的最后一段", result.MainBody);

    // 验证 XML 标签已被移除
    Assert.DoesNotContain("<phone>", result.MainBody);
    Assert.DoesNotContain("</phone>", result.MainBody);
    Assert.DoesNotContain("<input>", result.MainBody);
    Assert.DoesNotContain("</input>", result.MainBody);
    Assert.DoesNotContain("<plot>", result.MainBody);
    Assert.DoesNotContain("</plot>", result.MainBody);

    // 验证注释已被移除
    Assert.DoesNotContain("<!--run:", result.MainBody);
    Assert.DoesNotContain("这是一个注释，应该被移除", result.MainBody);
  }

  [Fact]
  public async Task ParseDreamPhoneRules_ShouldParseSuccessfully()
  {
    // Arrange
    var ruleEngine = new RuleEngine();

    // 获取项目目录路径
    var testProjectDir = Path.GetDirectoryName(typeof(DreamPhoneRulesTests).Assembly.Location);
    var projectRoot = Path.GetFullPath(Path.Combine(testProjectDir!, "..", "..", ".."));
    var mockDataPath = Path.Combine(projectRoot, "MockData", "dream-phone");
    var rulesYaml = await File.ReadAllTextAsync(Path.Combine(mockDataPath, "rules.yaml"));

    // Act
    var ruleBook = ruleEngine.ParseRuleBook(rulesYaml);

    // Assert
    Assert.NotNull(ruleBook);
    Assert.Equal("1.0", ruleBook.Version);
    Assert.Equal("Dream Phone 规则 - 使用混合方式处理", ruleBook.Description);

    // 验证提取规则
    Assert.Equal(3, ruleBook.ExtractionRules.Count);
    // 前两个规则是 Markup，第三个兜底规则是 Regex
    Assert.Equal(MatcherType.Markup, ruleBook.ExtractionRules[0].MatcherType);
    Assert.Equal(MatcherType.Markup, ruleBook.ExtractionRules[1].MatcherType);
    Assert.Equal(MatcherType.Regex, ruleBook.ExtractionRules[2].MatcherType);

    // 验证转换规则
    Assert.Equal(3, ruleBook.TransformationRules.Count);
    Assert.All(ruleBook.TransformationRules, rule => Assert.Equal(TransformationType.Custom, rule.TransformationType));
  }
}