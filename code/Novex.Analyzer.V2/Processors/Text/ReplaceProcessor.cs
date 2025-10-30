namespace Novex.Analyzer.V2.Processors.Text;

/// <summary>
/// 替换处理器 - 替换文本中的指定内容
/// </summary>
[Processor("Text.Replace", Category = "Text", Description = "替换文本中的指定内容")]
public class ReplaceProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Text.Replace";
    public string DisplayName => "替换文本";
    public string Description => "替换文本中的指定内容";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);

        if (!parameters.TryGet<string>("OldValue", out var oldValue) || oldValue == null)
            return Task.FromResult(ProcessResult.Fail("OldValue 参数必需"));

        var newValue = parameters.TryGet<string>("NewValue", out var nv) ? nv : string.Empty;

        var ignoreCase = parameters.TryGet<bool>("IgnoreCase", out var ic) && ic;

        var output = ignoreCase
            ? System.Text.RegularExpressions.Regex.Replace(input, System.Text.RegularExpressions.Regex.Escape(oldValue), newValue ?? string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            : input.Replace(oldValue, newValue);

        return Task.FromResult(ProcessResult.Ok(output));
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "OldValue",
                Type = typeof(string),
                Required = true,
                Description = "要替换的文本"
            },
            new ParameterDefinition
            {
                Name = "NewValue",
                Type = typeof(string),
                Required = false,
                DefaultValue = "",
                Description = "替换为的文本"
            },
            new ParameterDefinition
            {
                Name = "IgnoreCase",
                Type = typeof(bool),
                Required = false,
                DefaultValue = false,
                Description = "是否忽略大小写"
            }
        };
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "替换文本",
                Parameters = new Dictionary<string, object> { { "OldValue", "world" }, { "NewValue", "universe" } },
                Input = "hello world",
                ExpectedOutput = "hello universe"
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

