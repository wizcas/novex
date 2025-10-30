using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Metadata;

namespace Novex.Analyzer.V2.Registry;

/// <summary>
/// 处理器注册表接口
/// </summary>
public interface IProcessorRegistry
{
    /// <summary>
    /// 注册处理器类型
    /// </summary>
    /// <param name="name">处理器名称</param>
    /// <param name="processorType">处理器类型</param>
    void Register(string name, Type processorType);
    
    /// <summary>
    /// 注册处理器工厂
    /// </summary>
    /// <param name="name">处理器名称</param>
    /// <param name="factory">处理器工厂函数</param>
    void Register(string name, Func<IProcessor> factory);
    
    /// <summary>
    /// 注册处理器实例（单例）
    /// </summary>
    /// <param name="name">处理器名称</param>
    /// <param name="instance">处理器实例</param>
    void RegisterSingleton(string name, IProcessor instance);
    
    /// <summary>
    /// 解析处理器
    /// </summary>
    /// <param name="name">处理器名称</param>
    /// <returns>处理器实例</returns>
    /// <exception cref="KeyNotFoundException">处理器未找到</exception>
    IProcessor Resolve(string name);
    
    /// <summary>
    /// 尝试解析处理器
    /// </summary>
    /// <param name="name">处理器名称</param>
    /// <param name="processor">输出处理器实例</param>
    /// <returns>是否成功解析</returns>
    bool TryResolve(string name, out IProcessor processor);
    
    /// <summary>
    /// 获取所有已注册的处理器名称
    /// </summary>
    /// <returns>处理器名称集合</returns>
    IEnumerable<string> GetRegisteredNames();
    
    /// <summary>
    /// 获取处理器元数据
    /// </summary>
    /// <param name="name">处理器名称</param>
    /// <returns>处理器元数据或 null</returns>
    IProcessorMetadata? GetMetadata(string name);
}

