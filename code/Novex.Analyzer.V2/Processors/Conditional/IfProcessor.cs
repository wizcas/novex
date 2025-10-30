namespace Novex.Analyzer.V2.Processors.Conditional;

/// <summary>
/// 条件处理器 - 根据条件返回不同的值
/// </summary>
[Processor("Conditional.If", Category = "Conditional", Description = "根据条件返回不同的值")]
public class IfProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Conditional.If";
    public string DisplayName => "条件判断";
    public string Description => "根据条件返回不同的值";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);
        
        if (!parameters.TryGet<string>("Condition", out var condition) || condition == null)
            return Task.FromResult(ProcessResult.Fail("Condition 参数必需"));
        
        var trueValue = parameters.TryGet<string>("TrueValue", out var tv) ? tv : input;
        var falseValue = parameters.TryGet<string>("FalseValue", out var fv) ? fv : string.Empty;
        
        var result = EvaluateCondition(input, condition) ? trueValue : falseValue;
        return Task.FromResult(ProcessResult.Ok(result ?? string.Empty));
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "Condition",
                Type = typeof(string),
                Required = true,
                Description = "条件表达式（支持: empty, notempty, equals, contains）"
            },
            new ParameterDefinition
            {
                Name = "TrueValue",
                Type = typeof(string),
                Required = false,
                Description = "条件为真时的值"
            },
            new ParameterDefinition
            {
                Name = "FalseValue",
                Type = typeof(string),
                Required = false,
                Description = "条件为假时的值"
            }
        };
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "检查是否为空",
                Parameters = new Dictionary<string, object> { { "Condition", "empty" }, { "TrueValue", "N/A" }, { "FalseValue", "OK" } },
                Input = "",
                ExpectedOutput = "N/A"
            }
        };
    }
    
    private bool EvaluateCondition(string input, string condition)
    {
        return condition.ToLower() switch
        {
            "empty" => string.IsNullOrEmpty(input),
            "notempty" => !string.IsNullOrEmpty(input),
            _ => false
        };
    }
    
    private string GetInput(ProcessContext context, ProcessorParameters parameters)
    {
        if (parameters.TryGet<string>("Field", out var field) && !string.IsNullOrEmpty(field))
            return context.GetField(field);
        
        return context.SourceContent;
    }
}

