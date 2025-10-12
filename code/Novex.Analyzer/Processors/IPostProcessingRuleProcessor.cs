namespace Novex.Analyzer.Processors;

/// <summary>
/// 后处理规则处理器接口 - 用于在所有提取和转换完成后进行最终处理
/// </summary>
public interface IPostProcessingRuleProcessor
{
  /// <summary>
  /// 处理提取的数据字典
  /// </summary>
  /// <param name="extractedData">提取的数据字典</param>
  /// <param name="parameters">处理参数</param>
  /// <returns>处理完成的任务</returns>
  Task ProcessAsync(Dictionary<string, string> extractedData, Dictionary<string, object> parameters);
}