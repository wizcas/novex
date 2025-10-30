namespace Novex.Analyzer.V2.Attributes;

/// <summary>
/// 处理器特性 - 用于标记和自动发现处理器
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ProcessorAttribute : Attribute
{
    /// <summary>
    /// 处理器名称
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// 处理器分类
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// 处理器描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 初始化处理器特性
    /// </summary>
    /// <param name="name">处理器名称</param>
    public ProcessorAttribute(string name)
    {
        Name = name;
    }
}

