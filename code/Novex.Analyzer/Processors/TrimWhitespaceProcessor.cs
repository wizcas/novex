using Novex.Analyzer.Processors;

namespace Novex.Analyzer.Processors;

/// <summary>
/// 修剪空白字符处理器
/// </summary>
public class TrimWhitespaceProcessor : IPostProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    return Task.FromResult(input.Trim());
  }
}