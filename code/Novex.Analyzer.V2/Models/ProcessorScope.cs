namespace Novex.Analyzer.V2.Models;

/// <summary>
/// 处理器作用域 - 定义处理器作用的范围
/// </summary>
public enum ProcessorScope
{
    /// <summary>
    /// 处理原始源内容
    /// </summary>
    Source,
    
    /// <summary>
    /// 处理特定字段
    /// </summary>
    Field,
    
    /// <summary>
    /// 处理整个上下文（可访问所有字段）
    /// </summary>
    Global
}

