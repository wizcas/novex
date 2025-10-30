namespace Novex.Analyzer.V2.Models;

/// <summary>
/// 规则书 - 顶层配置，包含所有规则和模板
/// </summary>
public class RuleBook
{
    /// <summary>
    /// 规则书版本
    /// </summary>
    public string Version { get; set; } = "2.0";
    
    /// <summary>
    /// 规则书描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 全局变量
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new();
    
    /// <summary>
    /// 包含的其他规则文件
    /// </summary>
    public List<string> Includes { get; set; } = new();
    
    /// <summary>
    /// 规则模板
    /// </summary>
    public Dictionary<string, RuleTemplate> Templates { get; set; } = new();
    
    /// <summary>
    /// 处理规则列表
    /// </summary>
    public List<ProcessRule> Rules { get; set; } = new();
}

