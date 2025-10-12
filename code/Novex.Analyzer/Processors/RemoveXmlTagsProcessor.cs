using System.Text.RegularExpressions;

namespace Novex.Analyzer.Processors;

/// <summary>
/// XML 标签移除处理器
/// </summary>
public class RemoveXmlTagsProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input;

    // 处理 RemoveXmlTags 参数（可能是 bool 或 string）
    var shouldRemoveXmlTags = GetBooleanParameter(parameters, "RemoveXmlTags");

    if (shouldRemoveXmlTags)
    {
      // 移除常见的XML标签（plot, phone, input, body 等），但保留内容
      result = Regex.Replace(result, @"<(plot|phone|input|body|div|span|p)\b[^>]*>(.*?)</\1>", "$2", RegexOptions.Singleline | RegexOptions.IgnoreCase);
      // 移除自闭合标签
      result = Regex.Replace(result, @"<(plot|phone|input|body|div|span|p)\b[^>]*/>", "", RegexOptions.IgnoreCase);
      // 移除其他可能的XML标签（更通用的方式）
      result = Regex.Replace(result, @"<[^>]+>", "", RegexOptions.Singleline);
    }

    return Task.FromResult(result);
  }

  private static bool GetBooleanParameter(Dictionary<string, object> parameters, string primaryKey)
  {
    var value = parameters.GetValueOrDefault(primaryKey);
    if (value != null)
    {
      if (value is bool boolValue)
      {
        return boolValue;
      }
      else if (value is string stringValue &&
              (stringValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
      {
        return true;
      }
    }
    return false;
  }
}