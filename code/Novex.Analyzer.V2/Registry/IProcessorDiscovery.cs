using System.Reflection;

namespace Novex.Analyzer.V2.Registry;

/// <summary>
/// 处理器发现接口
/// </summary>
public interface IProcessorDiscovery
{
    /// <summary>
    /// 从程序集发现处理器
    /// </summary>
    /// <param name="assembly">程序集</param>
    /// <returns>发现的处理器信息集合</returns>
    IEnumerable<ProcessorInfo> DiscoverFromAssembly(Assembly assembly);
    
    /// <summary>
    /// 从目录发现处理器（加载 DLL）
    /// </summary>
    /// <param name="directory">目录路径</param>
    /// <returns>发现的处理器信息集合</returns>
    IEnumerable<ProcessorInfo> DiscoverFromDirectory(string directory);
}

