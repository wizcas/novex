using Microsoft.Extensions.DependencyInjection;

namespace Novex.Analyzer.V2.DependencyInjection;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Novex.Analyzer V2 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddNovexAnalyzerV2(this IServiceCollection services)
    {
        services.AddSingleton<IProcessorRegistry, ProcessorRegistry>();
        services.AddSingleton<IProcessorDiscovery, ProcessorDiscovery>();
        
        return services;
    }
    
    /// <summary>
    /// 添加 Novex.Analyzer V2 服务并自动发现处理器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="autoDiscoverAssemblies">是否自动发现程序集中的处理器</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddNovexAnalyzerV2WithAutoDiscovery(
        this IServiceCollection services,
        bool autoDiscoverAssemblies = true)
    {
        services.AddNovexAnalyzerV2();
        
        if (autoDiscoverAssemblies)
        {
            services.AddSingleton(provider =>
            {
                var registry = provider.GetRequiredService<IProcessorRegistry>();
                var discovery = provider.GetRequiredService<IProcessorDiscovery>();
                
                // 从当前程序集发现处理器
                var currentAssembly = typeof(ServiceCollectionExtensions).Assembly;
                foreach (var processorInfo in discovery.DiscoverFromAssembly(currentAssembly))
                {
                    registry.Register(processorInfo.Name, processorInfo.Type);
                }
                
                return registry;
            });
        }
        
        return services;
    }
}

