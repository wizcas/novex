using System.Text.Json;
using System.Text.RegularExpressions;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 自定义转换处理器
/// </summary>
public class CustomTransformationProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input;

    // Debug: 输出参数信息（已注释）
    // Console.WriteLine($"CustomTransformationProcessor - Input length: {input.Length}");

    // 处理正则表达式提取和格式化
    var patternObj = parameters.GetValueOrDefault("Pattern") ?? parameters.GetValueOrDefault("pattern");
    if (patternObj != null)
    {
      var pattern = patternObj.ToString();
      if (!string.IsNullOrEmpty(pattern))
      {
        try
        {
          var regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.Singleline);
          var match = regex.Match(input);



          if (match.Success)
          {
            var formatObj = parameters.GetValueOrDefault("Format") ?? parameters.GetValueOrDefault("format");
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
                  Console.WriteLine($"  String.Format failed: {ex.Message}");
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
          // 如果正则表达式失败，保持原始输入
        }
      }
    }

    // 移除 HTML 注释
    // 处理 RemoveComments 参数（可能是 bool 或 string）
    var shouldRemoveComments = false;
    var removeCommentsValue = parameters.GetValueOrDefault("RemoveComments") ?? parameters.GetValueOrDefault("remove_comments");
    if (removeCommentsValue != null)
    {
      if (removeCommentsValue is bool boolValue)
      {
        shouldRemoveComments = boolValue;
      }
      else if (removeCommentsValue is string stringValue &&
              (stringValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
      {
        shouldRemoveComments = true;
      }
    }

    if (shouldRemoveComments)
    {
      result = Regex.Replace(result, @"<!--.*?-->", "", RegexOptions.Singleline);
    }

    // 移除 run 块
    var shouldRemoveRunBlocks = false;
    var removeRunBlocksValue = parameters.GetValueOrDefault("RemoveRunBlocks") ?? parameters.GetValueOrDefault("remove_run_blocks");
    if (removeRunBlocksValue != null)
    {
      if (removeRunBlocksValue is bool boolValue)
      {
        shouldRemoveRunBlocks = boolValue;
      }
      else if (removeRunBlocksValue is string stringValue &&
              (stringValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
      {
        shouldRemoveRunBlocks = true;
      }
    }

    if (shouldRemoveRunBlocks)
    {
      result = Regex.Replace(result, @"<!--run:.*?-->", "", RegexOptions.Singleline);
    }

    // 移除 XML 标签
    var shouldRemoveXmlTags = false;
    var removeXmlTagsValue = parameters.GetValueOrDefault("RemoveXmlTags") ?? parameters.GetValueOrDefault("remove_xml_tags");
    if (removeXmlTagsValue != null)
    {
      if (removeXmlTagsValue is bool boolValue)
      {
        shouldRemoveXmlTags = boolValue;
      }
      else if (removeXmlTagsValue is string stringValue &&
              (stringValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
      {
        shouldRemoveXmlTags = true;
      }
    }

    if (shouldRemoveXmlTags)
    {
      // 移除常见的XML标签（plot, phone, input, body 等），但保留内容
      result = Regex.Replace(result, @"<(plot|phone|input|body|div|span|p)\b[^>]*>(.*?)</\1>", "$2", RegexOptions.Singleline | RegexOptions.IgnoreCase);
      // 移除自闭合标签
      result = Regex.Replace(result, @"<(plot|phone|input|body|div|span|p)\b[^>]*/>", "", RegexOptions.IgnoreCase);
      // 移除其他可能的XML标签（更通用的方式）
      result = Regex.Replace(result, @"<[^>]+>", "", RegexOptions.Singleline);
    }

    // 清理空白字符
    var cleanWhitespaceValue = parameters.GetValueOrDefault("CleanWhitespace") ?? parameters.GetValueOrDefault("clean_whitespace");
    if (cleanWhitespaceValue is bool cleanWhitespace && cleanWhitespace)
    {
      result = Regex.Replace(result.Trim(), @"\s+", " ");
      result = Regex.Replace(result, @"\n\s*\n\s*\n", "\n\n"); // 减少多余的空行
    }

    // 保持格式化（保留段落结构）
    var preserveFormattingValue = parameters.GetValueOrDefault("PreserveFormatting") ?? parameters.GetValueOrDefault("preserve_formatting");
    if (preserveFormattingValue is bool preserveFormatting && preserveFormatting)
    {
      // 保持基本的段落结构，但清理过多的空行
      result = Regex.Replace(result, @"\n{3,}", "\n\n");
    }

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


    var finalResult = result?.Trim() ?? "";

    return Task.FromResult(finalResult);
  }
}