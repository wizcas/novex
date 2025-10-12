namespace Novex.Analyzer.Processors;

/// <summary>
/// 后处理器接口
/// </summary>
public interface IPostProcessor
{
  Task<string> ProcessAsync(string input, Dictionary<string, object> parameters);
}