namespace Novex.Analyzer.V2.Models;

/// <summary>
/// 规则基类 - 所有规则的基础
/// </summary>
public abstract class RuleBase
{
    /// <summary>
    /// 规则ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 规则名称
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// 规则描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 优先级（数字越小优先级越高）
    /// </summary>
    public int Priority { get; set; } = 100;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// 条件表达式（可选）
    /// </summary>
    public string? Condition { get; set; }
    
    /// <summary>
    /// 错误处理策略
    /// </summary>
    public ErrorHandlingStrategy OnError { get; set; } = ErrorHandlingStrategy.Throw;
}

