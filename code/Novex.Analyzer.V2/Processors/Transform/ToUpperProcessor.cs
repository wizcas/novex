namespace Novex.Analyzer.V2.Processors.Transform;

/// <summary>
/// 转换为大写处理器
/// </summary>
[Processor("Transform.ToUpper", Category = "Transform", Description = "将文本转换为大写")]
public class ToUpperProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Transform.ToUpper";
    public string DisplayName => "转换为大写";
    public string Description => "将文本转换为大写";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);
        var output = input.ToUpper();
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
                Description = "转换为大写",
                Parameters = new Dictionary<string, object>(),
                Input = "hello world",
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

