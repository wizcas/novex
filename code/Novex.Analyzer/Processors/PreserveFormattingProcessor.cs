using System.Text.RegularExpressions;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 格式化保持处理器
/// </summary>
public class PreserveFormattingProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input;

    // 保持格式化（保留段落结构）
    var preserveFormattingValue = parameters.GetValueOrDefault("PreserveFormatting");
    if (preserveFormattingValue is bool preserveFormatting && preserveFormatting)
    {
      // 保持基本的段落结构，但清理过多的空行
      result = Regex.Replace(result, @"\n{3,}", "\n\n");
    }

    return Task.FromResult(result);
  }
}