namespace Novex.Analyzer.V2.Core;

/// <summary>
/// 处理器基础接口 - 所有处理器必须实现
/// </summary>
public interface IProcessor
{
    /// <summary>
    /// 处理器名称（用于注册和引用）
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 执行处理
    /// </summary>
    /// <param name="context">处理上下文</param>
    /// <param name="parameters">处理器参数</param>
    /// <returns>处理结果</returns>
    Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters);
}

