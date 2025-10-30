namespace Novex.Analyzer.V2.Processors.Text;

/// <summary>
/// 修剪空白处理器 - 移除字符串首尾的空白字符
/// </summary>
[Processor("Text.Trim", Category = "Text", Description = "移除字符串首尾的空白字符")]
public class TrimProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Text.Trim";
    public string DisplayName => "修剪空白";
    public string Description => "移除字符串首尾的空白字符";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);
        var trimChars = parameters.TryGet<string>("TrimChars", out var chars) ? chars : null;
        
        var output = trimChars != null 
            ? input.Trim(trimChars.ToCharArray())
            : input.Trim();
            
        return Task.FromResult(ProcessResult.Ok(output));
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "TrimChars",
                Type = typeof(string),
                Required = false,
                Description = "要移除的字符集，默认为空白字符"
            }
        };
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "移除首尾空白",
                Parameters = new Dictionary<string, object>(),
                Input = "  hello world  ",
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

