namespace Novex.Analyzer.V2.Metadata;

/// <summary>
/// 处理器元数据接口 - 提供处理器的自描述能力（可选实现）
/// </summary>
public interface IProcessorMetadata
{
    /// <summary>
    /// 处理器显示名称
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// 处理器描述
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 获取参数定义
    /// </summary>
    /// <returns>参数定义集合</returns>
    IEnumerable<ParameterDefinition> GetParameters();
    
    /// <summary>
    /// 获取使用示例
    /// </summary>
    /// <returns>使用示例集合</returns>
    IEnumerable<ProcessorExample> GetExamples();
}

