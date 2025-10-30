namespace Novex.Analyzer.V2.Processors.Pipeline;

/// <summary>
/// 管道链接处理器 - 按顺序执行多个处理器
/// </summary>
[Processor("Pipeline.Chain", Category = "Pipeline", Description = "按顺序执行多个处理器")]
public class ChainProcessor : IProcessor, IProcessorMetadata
{
    private readonly IProcessorRegistry _registry;
    
    public string Name => "Pipeline.Chain";
    public string DisplayName => "管道链接";
    public string Description => "按顺序执行多个处理器";
    
    public ChainProcessor(IProcessorRegistry? registry = null)
    {
        _registry = registry ?? new ProcessorRegistry();
    }
    
    public async Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        if (!parameters.TryGet<string>("Processors", out var processorsJson) || processorsJson == null)
            return ProcessResult.Fail("Processors 参数必需");

        var input = GetInput(context, parameters);
        var current = input;

        try
        {
            // 简单的处理器链解析（实际应该使用 JSON 解析）
            var processorNames = processorsJson.Split(',').Select(p => p.Trim()).ToList();

            foreach (var processorName in processorNames)
            {
                if (!_registry.TryResolve(processorName, out var processor))
                    return ProcessResult.Fail($"处理器未找到: {processorName}");

                var result = await processor.ProcessAsync(context, new ProcessorParameters(new Dictionary<string, object>()));
                if (!result.Success)
                    return result;

                current = result.Output ?? string.Empty;
            }

            return ProcessResult.Ok(current);
        }
        catch (Exception ex)
        {
            return ProcessResult.Fail($"管道执行错误: {ex.Message}", ex);
        }
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "Processors",
                Type = typeof(string),
                Required = true,
                Description = "处理器名称列表（逗号分隔）"
            }
        };
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "链接多个处理器",
                Parameters = new Dictionary<string, object> { { "Processors", "Text.Trim,Transform.ToUpper" } },
                Input = "  hello world  ",
                ExpectedOutput = "HELLO WORLD"
            }
        };
    }
    
    private string GetInput(ProcessContext context, ProcessorParameters parameters)
    {
        if (parameters.TryGet<string>("Field", out var field) && !string.IsNullOrEmpty(field))
            return context.GetField(field);
        
        return context.SourceContent;
    }
}

