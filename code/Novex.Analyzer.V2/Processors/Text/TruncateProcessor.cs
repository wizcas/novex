namespace Novex.Analyzer.V2.Processors.Text;

/// <summary>
/// 截断处理器 - 截断文本到指定长度
/// </summary>
[Processor("Text.Truncate", Category = "Text", Description = "截断文本到指定长度")]
public class TruncateProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Text.Truncate";
    public string DisplayName => "截断文本";
    public string Description => "截断文本到指定长度";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);
        
        if (!parameters.TryGet<int>("MaxLength", out var maxLength))
            return Task.FromResult(ProcessResult.Fail("MaxLength 参数必需"));
        
        if (maxLength <= 0)
            return Task.FromResult(ProcessResult.Fail("MaxLength 必须大于 0"));
        
        if (input.Length <= maxLength)
            return Task.FromResult(ProcessResult.Ok(input));
        
        var output = input.Substring(0, maxLength);
        
        if (parameters.TryGet<bool>("AddEllipsis", out var addEllipsis) && addEllipsis)
        {
            var ellipsis = parameters.TryGet<string>("Ellipsis", out var customEllipsis) 
                ? customEllipsis 
                : "...";
            output += ellipsis;
        }
        
        return Task.FromResult(ProcessResult.Ok(output));
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "MaxLength",
                Type = typeof(int),
                Required = true,
                Description = "最大长度"
            },
            new ParameterDefinition
            {
                Name = "AddEllipsis",
                Type = typeof(bool),
                Required = false,
                DefaultValue = true,
                Description = "是否添加省略号"
            },
            new ParameterDefinition
            {
                Name = "Ellipsis",
                Type = typeof(string),
                Required = false,
                DefaultValue = "...",
                Description = "省略号文本"
            }
        };
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "截断到 10 个字符并添加省略号",
                Parameters = new Dictionary<string, object> { { "MaxLength", 10 }, { "AddEllipsis", true } },
                Input = "This is a very long text",
                ExpectedOutput = "This is a ..."
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

