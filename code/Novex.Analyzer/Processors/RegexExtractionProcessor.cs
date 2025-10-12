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
          // 如果正则表达式失败，保持原始输入
        }
      }
    }

    return Task.FromResult(result?.Trim() ?? "");
  }
}