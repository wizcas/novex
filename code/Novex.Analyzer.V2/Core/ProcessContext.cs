using Microsoft.Extensions.Logging;

namespace Novex.Analyzer.V2.Core;

/// <summary>
/// 处理上下文 - 包含处理所需的所有信息
/// </summary>
public class ProcessContext
{
    /// <summary>
    /// 原始输入内容
    /// </summary>
    public string SourceContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 已提取的字段
    /// </summary>
    public Dictionary<string, string> Fields { get; set; } = new();
    
    /// <summary>
    /// 变量存储（用于规则间传递数据）
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new();
    
    /// <summary>
    /// 日志记录器
    /// </summary>
    public ILogger? Logger { get; set; }
    
    /// <summary>
    /// 取消令牌
    /// </summary>
    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    
    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <param name="name">字段名</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>字段值或默认值</returns>
    public string GetField(string name, string defaultValue = "")
    {
        return Fields.TryGetValue(name, out var value) ? value : defaultValue;
    }
    
    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="name">字段名</param>
    /// <param name="value">字段值</param>
    public void SetField(string name, string value)
    {
        Fields[name] = value;
    }
    
    /// <summary>
    /// 获取变量值
    /// </summary>
    /// <param name="name">变量名</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>变量值或默认值</returns>
    public T? GetVariable<T>(string name, T? defaultValue = default)
    {
        if (Variables.TryGetValue(name, out var value))
        {
            if (value is T typedValue)
                return typedValue;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// 设置变量值
    /// </summary>
    /// <param name="name">变量名</param>
    /// <param name="value">变量值</param>
    public void SetVariable(string name, object value)
    {
        Variables[name] = value;
    }
}

