using Novex.Analyzer.V2.Attributes;

namespace Novex.Analyzer.V2.Registry;

/// <summary>
/// 处理器信息 - 用于处理器发现
/// </summary>
public class ProcessorInfo
{
    /// <summary>
    /// 处理器名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 处理器类型
    /// </summary>
    public Type Type { get; set; } = null!;
    
    /// <summary>
    /// 处理器特性
    /// </summary>
    public ProcessorAttribute? Attribute { get; set; }
}

