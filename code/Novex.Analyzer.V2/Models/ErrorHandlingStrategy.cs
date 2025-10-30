namespace Novex.Analyzer.V2.Models;

/// <summary>
/// 错误处理策略
/// </summary>
public enum ErrorHandlingStrategy
{
    /// <summary>
    /// 抛出异常，停止执行
    /// </summary>
    Throw,
    
    /// <summary>
    /// 跳过当前规则，继续执行
    /// </summary>
    Skip,
    
    /// <summary>
    /// 使用默认值
    /// </summary>
    UseDefault,
    
    /// <summary>
    /// 重试
    /// </summary>
    Retry
}

