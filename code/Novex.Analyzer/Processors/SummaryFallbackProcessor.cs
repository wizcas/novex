using Novex.Analyzer.Processors;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 摘要回退处理器 - 当Summary为空或null时，从MainBody前N字生成摘要
/// </summary>
public class SummaryFallbackProcessor : IPostProcessingRuleProcessor
{
  /// <summary>
  /// 处理提取的数据字典
  /// </summary>
  /// <param name="extractedData">提取的数据字典</param>
  /// <param name="parameters">处理参数</param>
  /// <returns>处理完成的任务</returns>
  public async Task ProcessAsync(Dictionary<string, string> extractedData, Dictionary<string, object> parameters)
  {
    // 获取参数
    var maxLength = GetParameter<int>(parameters, "MaxLength", 50);
    var addEllipsis = GetParameter<bool>(parameters, "AddEllipsis", true);
    var sourceField = GetParameter<string>(parameters, "SourceField", "MainBody");
    var targetField = GetParameter<string>(parameters, "TargetField", "Summary");

    // 检查目标字段是否为空
    var currentSummary = extractedData.GetValueOrDefault(targetField, "").Trim();
    if (!string.IsNullOrWhiteSpace(currentSummary))
    {
      // 已有摘要，不需要回退
      return;
    }

    // 从源字段获取内容
    var sourceContent = extractedData.GetValueOrDefault(sourceField, "").Trim();
    if (string.IsNullOrWhiteSpace(sourceContent))
    {
      // 源字段也为空，无法生成摘要
      return;
    }

    // 生成回退摘要
    var fallbackSummary = GenerateFallbackSummary(sourceContent, maxLength, addEllipsis);
    extractedData[targetField] = fallbackSummary;

    await Task.CompletedTask;
  }

  /// <summary>
  /// 生成回退摘要
  /// </summary>
  /// <param name="content">源内容</param>
  /// <param name="maxLength">最大长度</param>
  /// <param name="addEllipsis">是否添加省略号</param>
  /// <returns>生成的摘要</returns>
  private static string GenerateFallbackSummary(string content, int maxLength, bool addEllipsis)
  {
    if (string.IsNullOrWhiteSpace(content))
      return string.Empty;

    // 清理内容：移除多余的换行符和空白字符
    var cleanedContent = content.Trim()
        .Replace("\r\n", " ")
        .Replace("\r", " ")
        .Replace("\n", " ")
        .Replace("\t", " ");

    // 合并多个空格为单个空格
    while (cleanedContent.Contains("  "))
    {
      cleanedContent = cleanedContent.Replace("  ", " ");
    }

    cleanedContent = cleanedContent.Trim();

    if (cleanedContent.Length <= maxLength)
    {
      return cleanedContent;
    }

    // 截断内容
    var truncated = cleanedContent.Substring(0, maxLength);

    // 尝试在单词边界截断
    var lastSpaceIndex = truncated.LastIndexOf(' ');
    if (lastSpaceIndex > maxLength * 0.7) // 如果空格位置不是太靠前
    {
      truncated = truncated.Substring(0, lastSpaceIndex);
    }

    // 添加省略号
    if (addEllipsis && cleanedContent.Length > truncated.Length)
    {
      truncated += "...";
    }

    return truncated.Trim();
  }

  /// <summary>
  /// 获取参数值
  /// </summary>
  /// <typeparam name="T">参数类型</typeparam>
  /// <param name="parameters">参数字典</param>
  /// <param name="key">参数键</param>
  /// <param name="defaultValue">默认值</param>
  /// <returns>参数值</returns>
  private static T GetParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue)
  {
    if (!parameters.TryGetValue(key, out var value))
      return defaultValue;

    try
    {
      if (value is T directValue)
        return directValue;

      // 尝试类型转换
      return (T)Convert.ChangeType(value, typeof(T));
    }
    catch
    {
      return defaultValue;
    }
  }
}