using System.Text.RegularExpressions;

namespace Novex.Analyzer.Processors;

/// <summary>
/// HTML 清理处理器
/// </summary>
public class CleanHtmlProcessor : IPostProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    // 简单的HTML清理，实际应用中可以使用HtmlAgilityPack
    var result = Regex.Replace(input, @"<[^>]+>", "");
    return Task.FromResult(result);
  }
}