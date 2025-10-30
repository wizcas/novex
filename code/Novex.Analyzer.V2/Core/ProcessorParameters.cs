namespace Novex.Analyzer.V2.Core;

/// <summary>
/// 处理器参数 - 类型安全的参数访问
/// </summary>
public class ProcessorParameters
{
    private readonly Dictionary<string, object> _parameters;
    
    /// <summary>
    /// 初始化处理器参数
    /// </summary>
    /// <param name="parameters">参数字典</param>
    public ProcessorParameters(Dictionary<string, object>? parameters = null)
    {
        _parameters = parameters ?? new();
    }
    
    /// <summary>
    /// 获取参数值（带类型转换）
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="name">参数名</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>参数值或默认值</returns>
    public T Get<T>(string name, T? defaultValue = default)
    {
        if (!_parameters.TryGetValue(name, out var value))
            return defaultValue ?? throw new ArgumentException($"Parameter '{name}' not found");
            
        if (value is T typedValue)
            return typedValue;
            
        // 尝试类型转换
        try
        {
            return (T)Convert.ChangeType(value, typeof(T))!;
        }
        catch
        {
            return defaultValue ?? throw new ArgumentException($"Cannot convert parameter '{name}' to type {typeof(T).Name}");
        }
    }
    
    /// <summary>
    /// 尝试获取参数值
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="name">参数名</param>
    /// <param name="value">输出参数值</param>
    /// <returns>是否成功获取</returns>
    public bool TryGet<T>(string name, out T? value)
    {
        value = default;
        if (!_parameters.TryGetValue(name, out var objValue))
            return false;
            
        if (objValue is T typedValue)
        {
            value = typedValue;
            return true;
        }
        
        try
        {
            value = (T)Convert.ChangeType(objValue, typeof(T))!;
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 检查参数是否存在
    /// </summary>
    /// <param name="name">参数名</param>
    /// <returns>参数是否存在</returns>
    public bool Has(string name) => _parameters.ContainsKey(name);
    
    /// <summary>
    /// 获取所有参数名
    /// </summary>
    /// <returns>参数名集合</returns>
    public IEnumerable<string> GetNames() => _parameters.Keys;
    
    /// <summary>
    /// 获取原始参数字典
    /// </summary>
    /// <returns>参数字典</returns>
    public Dictionary<string, object> GetAll() => new(_parameters);
}

