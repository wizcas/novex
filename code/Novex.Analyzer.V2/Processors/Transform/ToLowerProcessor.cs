namespace Novex.Analyzer.V2.Processors.Transform;

/// <summary>
/// 转换为小写处理器
/// </summary>
[Processor("Transform.ToLower", Category = "Transform", Description = "将文本转换为小写")]
public class ToLowerProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Transform.ToLower";
    public string DisplayName => "转换为小写";
    public string Description => "将文本转换为小写";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);
        var output = input.ToLower();
        return Task.FromResult(ProcessResult.Ok(output));
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return Enumerable.Empty<ParameterDefinition>();
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "转换为小写",
                Parameters = new Dictionary<string, object>(),
                Input = "HELLO WORLD",
                ExpectedOutput = "hello world"
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

