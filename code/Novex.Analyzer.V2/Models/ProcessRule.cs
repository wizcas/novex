namespace Novex.Analyzer.V2.Models;

/// <summary>
/// 处理规则 - 定义一个具体的处理操作
/// </summary>
public class ProcessRule : RuleBase
{
    /// <summary>
    /// 处理器类型名称（如 "Text.Trim", "Regex.Replace" 等）
    /// </summary>
    public string Processor { get; set; } = string.Empty;
    
    /// <summary>
    /// 处理范围
    /// </summary>
    public ProcessorScope Scope { get; set; } = ProcessorScope.Field;
    
    /// <summary>
    /// 源字段（Scope=Field 时使用）
    /// </summary>
    public string? SourceField { get; set; }
    
    /// <summary>
    /// 目标字段（Scope=Field 时使用）
    /// </summary>
    public string? TargetField { get; set; }
    
    /// <summary>
    /// 处理器参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

