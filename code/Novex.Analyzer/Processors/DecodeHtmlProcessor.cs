namespace Novex.Analyzer.Processors;

/// <summary>
/// HTML 解码处理器
/// </summary>
public class DecodeHtmlProcessor : IPostProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input
        .Replace("&amp;", "&")
        .Replace("&lt;", "<")
        .Replace("&gt;", ">")
        .Replace("&quot;", "\"")
        .Replace("&#39;", "'");
    return Task.FromResult(result);
  }
}