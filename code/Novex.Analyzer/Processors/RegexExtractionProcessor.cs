using System.Text.RegularExpressions;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 正则表达式提取和格式化处理器
/// </summary>
public class RegexExtractionProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input;

    // 处理内容块移除（在其他处理之前）
    result = RemoveContentBlocks(result, parameters);

    // 处理正则表达式提取和格式化
    var patternObj = parameters.GetValueOrDefault("Pattern");
    if (patternObj != null)
    {
      var pattern = patternObj.ToString();
      if (!string.IsNullOrEmpty(pattern))
      {
        try
        {
          var regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.Singleline);
          var match = regex.Match(result);

          if (match.Success)
          {
            var formatObj = parameters.GetValueOrDefault("Format");
            if (formatObj != null)
            {
              var format = formatObj.ToString();
              if (!string.IsNullOrEmpty(format))
              {
                // 支持 {0}, {1}, {2} 等格式化占位符
                var groups = new string[match.Groups.Count];
                for (int i = 0; i < match.Groups.Count; i++)
                {
                  groups[i] = match.Groups[i].Value.Trim();
                }

                try
                {
                  result = string.Format(format, groups);
                }
                catch (Exception ex)
                {
                  Console.WriteLine($"String.Format failed: {ex.Message}");
                  result = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                }
              }
              else
              {
                result = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
              }
            }
            else
            {
              result = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Regex processing failed: {ex.Message}");
          // 如果正则表达式失败，使用已经处理过的result
        }
      }
    }
    // 如果没有Pattern参数，返回经过块移除处理的结果

    return Task.FromResult(result?.Trim() ?? "");
  }

  private string RemoveContentBlocks(string input, Dictionary<string, object> parameters)
  {
    var result = input;

    // 支持多个块的移除（通过数组配置）
    var removeBlocksValue = parameters.GetValueOrDefault("RemoveBlocks");

    if (removeBlocksValue != null)
    {
      // 处理 YAML 解析的 List<Dictionary<string, object>>
      if (removeBlocksValue is System.Collections.IList blocksList)
      {
        foreach (var blockItem in blocksList)
        {
          if (blockItem is Dictionary<string, object> blockDict)
          {
            var blockStart = blockDict.GetValueOrDefault("Start")?.ToString() ?? "";
            var blockEnd = blockDict.GetValueOrDefault("End")?.ToString() ?? "";

            if (!string.IsNullOrEmpty(blockStart) && !string.IsNullOrEmpty(blockEnd))
            {
              var escapedStart = Regex.Escape(blockStart);
              var escapedEnd = Regex.Escape(blockEnd);
              var pattern = $"{escapedStart}.*?{escapedEnd}";

              result = Regex.Replace(result, pattern, "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }
          }
          else if (blockItem is Dictionary<object, object> blockObjDict)
          {
            var blockStart = blockObjDict.GetValueOrDefault("Start")?.ToString() ?? "";
            var blockEnd = blockObjDict.GetValueOrDefault("End")?.ToString() ?? "";

            if (!string.IsNullOrEmpty(blockStart) && !string.IsNullOrEmpty(blockEnd))
            {
              var escapedStart = Regex.Escape(blockStart);
              var escapedEnd = Regex.Escape(blockEnd);
              var pattern = $"{escapedStart}.*?{escapedEnd}";

              result = Regex.Replace(result, pattern, "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }
          }
        }
      }
      // 处理 JSON 解析的 JsonElement
      else if (removeBlocksValue is System.Text.Json.JsonElement blocksElement && blocksElement.ValueKind == System.Text.Json.JsonValueKind.Array)
      {
        foreach (var blockElement in blocksElement.EnumerateArray())
        {
          if (blockElement.ValueKind == System.Text.Json.JsonValueKind.Object)
          {
            var blockStart = "";
            var blockEnd = "";

            if (blockElement.TryGetProperty("Start", out var startElement))
              blockStart = startElement.GetString() ?? "";
            if (blockElement.TryGetProperty("End", out var endElement))
              blockEnd = endElement.GetString() ?? "";

            if (!string.IsNullOrEmpty(blockStart) && !string.IsNullOrEmpty(blockEnd))
            {
              var escapedStart = Regex.Escape(blockStart);
              var escapedEnd = Regex.Escape(blockEnd);
              var pattern = $"{escapedStart}.*?{escapedEnd}";

              result = Regex.Replace(result, pattern, "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }
          }
        }
      }
    }

    return result;
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