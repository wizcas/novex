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
    if (GetBooleanParameter(cleanWhitespaceValue))
    {
      // 只清理行内的空白字符，保留换行符
      result = result.Trim();
      // 将行内的多个空白字符（除了换行）替换为单个空格
      result = Regex.Replace(result, @"[ \t\r\f\v]+", " ");
      // 清理行末的空白字符
      result = Regex.Replace(result, @"[ \t\r\f\v]+\n", "\n");
    }

    // 限制连续空行
    var limitEmptyLinesValue = parameters.GetValueOrDefault("LimitEmptyLines") ?? parameters.GetValueOrDefault("limit_empty_lines");
    if (GetBooleanParameter(limitEmptyLinesValue))
    {
      // 将连续的多个空行替换为最多一个空行
      // 匹配两个或更多连续的换行（可能包含空白字符），替换为最多一个空行
      result = Regex.Replace(result, @"\n\s*\n(\s*\n)+", "\n\n", RegexOptions.Multiline);
    }

    return Task.FromResult(result);
  }

  private bool GetBooleanParameter(object? value)
  {
    if (value == null) return false;
    if (value is bool boolValue) return boolValue;
    if (value is string stringValue)
      return string.Equals(stringValue, "true", StringComparison.OrdinalIgnoreCase);
    return false;
  }
}