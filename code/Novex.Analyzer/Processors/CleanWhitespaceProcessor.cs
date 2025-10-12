using System.Text.RegularExpressions;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 空白字符清理处理器
/// </summary>
public class CleanWhitespaceProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input;

    // 清理空白字符
    var cleanWhitespaceValue = parameters.GetValueOrDefault("CleanWhitespace") ?? parameters.GetValueOrDefault("clean_whitespace");
    if (cleanWhitespaceValue is bool cleanWhitespace && cleanWhitespace)
    {
      result = Regex.Replace(result.Trim(), @"\s+", " ");
      result = Regex.Replace(result, @"\n\s*\n\s*\n", "\n\n"); // 减少多余的空行
    }

    return Task.FromResult(result);
  }
}