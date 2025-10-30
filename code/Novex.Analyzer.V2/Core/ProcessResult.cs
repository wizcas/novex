namespace Novex.Analyzer.V2.Core;

/// <summary>
/// 处理结果
/// </summary>
public class ProcessResult
{
    /// <summary>
    /// 处理是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 处理输出内容
    /// </summary>
    public string? Output { get; set; }
    
    /// <summary>
    /// 修改的字段（用于 Scope=Global 的处理器）
    /// </summary>
    public Dictionary<string, string>? ModifiedFields { get; set; }
    
    /// <summary>
    /// 错误列表
    /// </summary>
    public List<ProcessError> Errors { get; set; } = new();
    
    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// 创建成功结果
    /// </summary>
    /// <param name="output">输出内容</param>
    /// <returns>成功的处理结果</returns>
    public static ProcessResult Ok(string output)
    {
        return new ProcessResult 
        { 
            Success = true, 
            Output = output 
        };
    }
    
    /// <summary>
    /// 创建失败结果
    /// </summary>
    /// <param name="error">错误消息</param>
    /// <returns>失败的处理结果</returns>
    public static ProcessResult Fail(string error)
    {
        return new ProcessResult 
        { 
            Success = false, 
            Errors = new List<ProcessError> 
            { 
                new ProcessError { Message = error } 
            } 
        };
    }
    
    /// <summary>
    /// 创建失败结果（带异常）
    /// </summary>
    /// <param name="error">错误消息</param>
    /// <param name="exception">异常对象</param>
    /// <returns>失败的处理结果</returns>
    public static ProcessResult Fail(string error, Exception exception)
    {
        return new ProcessResult 
        { 
            Success = false, 
            Errors = new List<ProcessError> 
            { 
                new ProcessError 
                { 
                    Message = error,
                    Exception = exception
                } 
            } 
        };
    }
}

