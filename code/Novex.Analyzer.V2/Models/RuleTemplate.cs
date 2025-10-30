namespace Novex.Analyzer.V2.Models;

/// <summary>
/// 规则模板 - 用于复用规则配置
/// </summary>
public class RuleTemplate
{
    /// <summary>
    /// 处理器类型名称
    /// </summary>
    public string Processor { get; set; } = string.Empty;
    
    /// <summary>
    /// 处理范围
    /// </summary>
    public ProcessorScope Scope { get; set; } = ProcessorScope.Field;
    
    /// <summary>
    /// 处理器参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

