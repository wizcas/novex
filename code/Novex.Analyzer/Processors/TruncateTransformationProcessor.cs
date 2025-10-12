using System.Text.Json;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 截断转换处理器
/// </summary>
public class TruncateTransformationProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    if (parameters.TryGetValue("MaxLength", out var maxLengthObj) && maxLengthObj is JsonElement maxLengthElement && maxLengthElement.TryGetInt32(out var maxLength))
    {
      return Task.FromResult(input.Length > maxLength ? input.Substring(0, maxLength) : input);
    }

    return Task.FromResult(input);
  }
}