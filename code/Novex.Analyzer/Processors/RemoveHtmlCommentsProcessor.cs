using System.Text.RegularExpressions;

namespace Novex.Analyzer.Processors;

/// <summary>
/// HTML 注释移除处理器
/// </summary>
public class RemoveHtmlCommentsProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input;

    // 处理 RemoveComments 参数（可能是 bool 或 string）
    var shouldRemoveComments = GetBooleanParameter(parameters, "RemoveComments", "remove_comments");

    if (shouldRemoveComments)
    {
      result = Regex.Replace(result, @"<!--.*?-->", "", RegexOptions.Singleline);
    }

    return Task.FromResult(result);
  }

  private static bool GetBooleanParameter(Dictionary<string, object> parameters, string primaryKey, string fallbackKey)
  {
    var value = parameters.GetValueOrDefault(primaryKey) ?? parameters.GetValueOrDefault(fallbackKey);
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