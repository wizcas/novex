namespace Novex.Analyzer.Processors;

/// <summary>
/// 后处理器接口
/// </summary>
public interface IPostProcessor
{
  /// <summary>
  /// 处理单个文本内容
  /// </summary>
  /// <param name="input">输入文本</param>
  /// <param name="parameters">处理参数</param>
  /// <returns>处理后的文本</returns>
  Task<string> ProcessAsync(string input, Dictionary<string, object> parameters);

  /// <summary>
  /// 处理提取的数据字典（可选实现）
  /// </summary>
  /// <param name="extractedData">提取的数据字典</param>
  /// <param name="parameters">处理参数</param>
  /// <returns>处理完成的任务</returns>
  Task ProcessDataAsync(Dictionary<string, string> extractedData, Dictionary<string, object> parameters)
  {
    // 默认实现：不进行任何处理
    return Task.CompletedTask;
  }
}