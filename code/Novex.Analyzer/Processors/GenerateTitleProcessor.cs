using System.Text.Json;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 标题生成处理器（从内容生成标题）
/// </summary>
public class GenerateTitleProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input;

    // 处理标题为空的情况（兼容旧逻辑）
    if (parameters.TryGetValue("condition", out var conditionObj) && conditionObj.ToString() == "title_is_empty")
    {
      if (parameters.TryGetValue("max_length", out var maxLengthObj) && maxLengthObj is JsonElement maxLengthElement && maxLengthElement.TryGetInt32(out var maxLength))
      {
        var words = result.Split(new[] { ' ', '，', '。', '、' }, StringSplitOptions.RemoveEmptyEntries);
        var title = string.Join("", words.Take(3));
        result = title.Length > maxLength ? title.Substring(0, maxLength) : title;
      }
    }

    return Task.FromResult(result?.Trim() ?? "");
  }
}