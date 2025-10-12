using System.Text.Json;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 文本截断处理器
/// </summary>
public class TruncateProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input?.Trim() ?? "";

    if (string.IsNullOrEmpty(result))
      return Task.FromResult(result);

    // 获取最大长度参数
    if (parameters.TryGetValue("max_length", out var maxLengthObj) &&
        maxLengthObj is JsonElement maxLengthElement &&
        maxLengthElement.TryGetInt32(out var maxLength) &&
        maxLength > 0)
    {
      if (result.Length > maxLength)
      {
        result = result.Substring(0, maxLength);

        // 检查是否需要添加省略号
        if (GetBooleanParameter(parameters, "add_ellipsis"))
        {
          result += "...";
        }
      }
    }

    return Task.FromResult(result);
  }

  private bool GetBooleanParameter(Dictionary<string, object> parameters, string key)
  {
    if (!parameters.TryGetValue(key, out var value))
      return false;

    if (value is bool boolValue)
      return boolValue;

    if (value is JsonElement element)
    {
      if (element.ValueKind == JsonValueKind.True)
        return true;
      if (element.ValueKind == JsonValueKind.False)
        return false;
    }

    if (value is string stringValue)
    {
      return string.Equals(stringValue, "true", StringComparison.OrdinalIgnoreCase);
    }

    return false;
  }
}