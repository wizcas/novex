using System.Reflection;

namespace Novex.Analyzer.V2.Registry;

/// <summary>
/// 处理器发现实现
/// </summary>
public class ProcessorDiscovery : IProcessorDiscovery
{
    /// <summary>
    /// 从程序集发现处理器
    /// </summary>
    public IEnumerable<ProcessorInfo> DiscoverFromAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));
        
        var processorType = typeof(IProcessor);
        var processorAttributeType = typeof(ProcessorAttribute);
        
        var types = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract && processorType.IsAssignableFrom(t));
        
        foreach (var type in types)
        {
            var attribute = type.GetCustomAttribute<ProcessorAttribute>();
            if (attribute != null)
            {
                yield return new ProcessorInfo
                {
                    Name = attribute.Name,
                    Type = type,
                    Attribute = attribute
                };
            }
        }
    }
    
    /// <summary>
    /// 从目录发现处理器（加载 DLL）
    /// </summary>
    public IEnumerable<ProcessorInfo> DiscoverFromDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("目录路径不能为空", nameof(directory));

        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"目录不存在: {directory}");

        var result = new List<ProcessorInfo>();
        var dllFiles = Directory.GetFiles(directory, "*.dll");

        foreach (var dllFile in dllFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllFile);
                result.AddRange(DiscoverFromAssembly(assembly));
            }
            catch (Exception ex)
            {
                // 记录加载失败，继续处理其他 DLL
                System.Diagnostics.Debug.WriteLine($"加载程序集 {dllFile} 失败: {ex.Message}");
            }
        }

        return result;
    }
}

