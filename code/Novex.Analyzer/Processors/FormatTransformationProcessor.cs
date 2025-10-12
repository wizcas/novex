using System.Text.Json;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 格式化转换处理器
/// </summary>
public class FormatTransformationProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input;

    if (parameters.TryGetValue("max_length", out var maxLengthObj) && maxLengthObj is JsonElement maxLengthElement && maxLengthElement.TryGetInt32(out var maxLength))
    {
      if (result.Length > maxLength)
      {
        result = result.Substring(0, maxLength) + "...";
      }
    }

    return Task.FromResult(result.Trim());
  }
}