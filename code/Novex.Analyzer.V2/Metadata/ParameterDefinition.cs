namespace Novex.Analyzer.V2.Metadata;

/// <summary>
/// 参数定义 - 描述处理器的参数
/// </summary>
public class ParameterDefinition
{
    /// <summary>
    /// 参数名
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 参数类型
    /// </summary>
    public Type Type { get; set; } = typeof(string);
    
    /// <summary>
    /// 是否必需
    /// </summary>
    public bool Required { get; set; }
    
    /// <summary>
    /// 默认值
    /// </summary>
    public object? DefaultValue { get; set; }
    
    /// <summary>
    /// 参数描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 允许的值列表（用于枚举类型）
    /// </summary>
    public string[]? AllowedValues { get; set; }
}

