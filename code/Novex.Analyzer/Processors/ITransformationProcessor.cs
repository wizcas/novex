namespace Novex.Analyzer.Processors;

/// <summary>
/// 转换处理器接口
/// </summary>
public interface ITransformationProcessor
{
  Task<string> ProcessAsync(string input, Dictionary<string, object> parameters);
}