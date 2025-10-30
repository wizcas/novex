using Microsoft.Extensions.Logging;

namespace Novex.Analyzer.V2.Registry;

/// <summary>
/// 处理器注册表实现
/// </summary>
public class ProcessorRegistry : IProcessorRegistry
{
    private readonly Dictionary<string, Func<IProcessor>> _factories = new();
    private readonly Dictionary<string, IProcessor> _singletons = new();
    private readonly Dictionary<string, IProcessorMetadata?> _metadata = new();
    private readonly ILogger? _logger;
    
    /// <summary>
    /// 初始化处理器注册表
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public ProcessorRegistry(ILogger? logger = null)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// 注册处理器类型
    /// </summary>
    public void Register(string name, Type processorType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("处理器名称不能为空", nameof(name));
        
        if (processorType == null)
            throw new ArgumentNullException(nameof(processorType));
        
        if (!typeof(IProcessor).IsAssignableFrom(processorType))
            throw new ArgumentException($"类型 {processorType.Name} 必须实现 IProcessor 接口", nameof(processorType));
        
        _factories[name] = () => (IProcessor)Activator.CreateInstance(processorType)!;
        _logger?.LogInformation($"已注册处理器: {name} ({processorType.Name})");
    }
    
    /// <summary>
    /// 注册处理器工厂
    /// </summary>
    public void Register(string name, Func<IProcessor> factory)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("处理器名称不能为空", nameof(name));
        
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));
        
        _factories[name] = factory;
        _logger?.LogInformation($"已注册处理器工厂: {name}");
    }
    
    /// <summary>
    /// 注册处理器实例（单例）
    /// </summary>
    public void RegisterSingleton(string name, IProcessor instance)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("处理器名称不能为空", nameof(name));
        
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));
        
        _singletons[name] = instance;
        _factories[name] = () => instance;
        _logger?.LogInformation($"已注册处理器单例: {name}");
    }
    
    /// <summary>
    /// 解析处理器
    /// </summary>
    public IProcessor Resolve(string name)
    {
        if (!TryResolve(name, out var processor))
            throw new KeyNotFoundException($"处理器未找到: {name}");
        
        return processor;
    }
    
    /// <summary>
    /// 尝试解析处理器
    /// </summary>
    public bool TryResolve(string name, out IProcessor processor)
    {
        processor = null!;
        
        if (string.IsNullOrWhiteSpace(name))
            return false;
        
        // 先检查单例
        if (_singletons.TryGetValue(name, out var singleton))
        {
            processor = singleton;
            return true;
        }
        
        // 再检查工厂
        if (_factories.TryGetValue(name, out var factory))
        {
            try
            {
                processor = factory();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"创建处理器 {name} 时出错: {ex.Message}");
                return false;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取所有已注册的处理器名称
    /// </summary>
    public IEnumerable<string> GetRegisteredNames()
    {
        return _factories.Keys.AsEnumerable();
    }
    
    /// <summary>
    /// 获取处理器元数据
    /// </summary>
    public IProcessorMetadata? GetMetadata(string name)
    {
        if (!_metadata.ContainsKey(name))
        {
            if (TryResolve(name, out var processor))
            {
                _metadata[name] = processor as IProcessorMetadata;
            }
            else
            {
                _metadata[name] = null;
            }
        }
        
        return _metadata[name];
    }
}

