namespace Novex.Analyzer.V2.Metadata;

/// <summary>
/// 处理器使用示例
/// </summary>
public class ProcessorExample
{
    /// <summary>
    /// 示例描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 处理器参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    /// <summary>
    /// 输入内容
    /// </summary>
    public string Input { get; set; } = string.Empty;
    
    /// <summary>
    /// 预期输出
    /// </summary>
    public string ExpectedOutput { get; set; } = string.Empty;
}

