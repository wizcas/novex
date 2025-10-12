using System.Text.RegularExpressions;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 文本格式化处理器
/// </summary>
public class FormatTextProcessor : IPostProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input;

    if (parameters.GetValueOrDefault("RemoveExtraNewlines") is true)
    {
      result = Regex.Replace(result, @"\n\s*\n\s*\n", "\n\n");
    }

    if (parameters.GetValueOrDefault("NormalizeSpaces") is true)
    {
      result = Regex.Replace(result, @"[ \t]+", " ");
    }

    return Task.FromResult(result.Trim());
  }
}