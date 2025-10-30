namespace Novex.Analyzer.V2.Core;

/// <summary>
/// 处理错误信息
/// </summary>
public class ProcessError
{
    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 处理器名称
    /// </summary>
    public string? ProcessorName { get; set; }
    
    /// <summary>
    /// 异常对象
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// 错误上下文信息
    /// </summary>
    public Dictionary<string, object> Context { get; set; } = new();
}

