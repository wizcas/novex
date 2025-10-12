using Novex.Analyzer;
using Novex.Analyzer.Models;
using System.IO;
using Xunit;

namespace Novex.Analyzer.Tests;

public class DreamPhoneDebugTests
{
  [Fact]
  public async Task DebugDreamPhoneExtraction()
  {
    // Arrange
    var ruleEngine = new RuleEngine();

    // 获取项目目录路径
    var testProjectDir = Path.GetDirectoryName(typeof(DreamPhoneDebugTests).Assembly.Location);
    var projectRoot = Path.GetFullPath(Path.Combine(testProjectDir!, "..", "..", ".."));
    var mockDataPath = Path.Combine(projectRoot, "MockData", "dream-phone");

    // 尝试不同的编码方式读取文件
    var rawBytes = await File.ReadAllBytesAsync(Path.Combine(mockDataPath, "raw.md"));
    var rawContent = System.Text.Encoding.UTF8.GetString(rawBytes);

    var rulesYaml = await File.ReadAllTextAsync(Path.Combine(mockDataPath, "rules.yaml"), System.Text.Encoding.UTF8);

    var ruleBook = ruleEngine.ParseRuleBook(rulesYaml);

    // Act
    var result = await ruleEngine.ExecuteRulesAsync(rawContent, ruleBook);

    // Debug: 检查提取和转换过程
    System.Console.WriteLine($"Raw content length: {rawContent.Length}");
    System.Console.WriteLine($"Raw content contains '<plot>': {rawContent.Contains("<plot>")}");
    System.Console.WriteLine($"Raw content contains '<dream>': {rawContent.Contains("<dream>")}");

    // 手动测试正则表达式
    var plotRegex = new System.Text.RegularExpressions.Regex(@"<plot>(.*?)</plot>", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.Singleline);
    var plotMatch = plotRegex.Match(rawContent);
    System.Console.WriteLine($"Plot regex match success: {plotMatch.Success}");
    System.Console.WriteLine($"Plot content length: {plotMatch.Groups[1].Value.Length}");

    // 测试转换规则中的正则表达式
    var titleRegex = new System.Text.RegularExpressions.Regex(@"当前章节: ([^\n]+).*?事件名:([^\n\(]+)", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.Singleline);
    var titleMatch = titleRegex.Match(plotMatch.Groups[1].Value);
    System.Console.WriteLine($"Title regex match success: {titleMatch.Success}");
    if (titleMatch.Success)
    {
      System.Console.WriteLine($"Title match groups count: {titleMatch.Groups.Count}");
      System.Console.WriteLine($"Title Group 1 length: {titleMatch.Groups[1].Value.Length}");
      System.Console.WriteLine($"Title Group 2 length: {titleMatch.Groups[2].Value.Length}");
    }

    var summaryRegex = new System.Text.RegularExpressions.Regex(@"摘要:([^\n]*(?:\n(?!当前章节|个人线|当前角色|长期事件|事件名)[^\n]*)*)", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.Singleline);
    var summaryMatch = summaryRegex.Match(plotMatch.Groups[1].Value);
    System.Console.WriteLine($"Summary regex match success: {summaryMatch.Success}");
    if (summaryMatch.Success)
    {
      System.Console.WriteLine($"Summary content length: {summaryMatch.Groups[1].Value.Length}");
    }

    // Debug: 输出结果查看（只显示长度，避免乱码）
    System.Console.WriteLine($"Result - Title length: {result.Title?.Length ?? 0}");
    System.Console.WriteLine($"Result - Summary length: {result.Summary?.Length ?? 0}");
    System.Console.WriteLine($"Result - MainBody length: {result.MainBody?.Length ?? 0}");

    // 至少确保结果不为空
    Assert.NotNull(result);
  }
}