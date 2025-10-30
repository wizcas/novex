using YamlDotNet.Serialization;

namespace Novex.Analyzer.V2.Processors.Json;

/// <summary>
/// JSON 提取处理器 - 从 JSON 中提取值
/// </summary>
[Processor("Json.Extract", Category = "Json", Description = "从 JSON 中提取值")]
public class ExtractProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Json.Extract";
    public string DisplayName => "提取 JSON 值";
    public string Description => "从 JSON 中提取值";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);
        
        if (!parameters.TryGet<string>("Path", out var path))
            return Task.FromResult(ProcessResult.Fail("Path 参数必需"));
        
        try
        {
            var deserializer = new DeserializerBuilder().Build();
            var obj = deserializer.Deserialize<dynamic>(input);
            
            var value = ExtractValue(obj, path);
            if (value == null)
                return Task.FromResult(ProcessResult.Fail($"路径未找到: {path}"));
            
            return Task.FromResult(ProcessResult.Ok(value.ToString() ?? ""));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ProcessResult.Fail($"JSON 解析错误: {ex.Message}", ex));
        }
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "Path",
                Type = typeof(string),
                Required = true,
                Description = "JSON 路径（点分隔，如 'user.name'）"
            }
        };
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "提取嵌套值",
                Parameters = new Dictionary<string, object> { { "Path", "user.name" } },
                Input = "{ \"user\": { \"name\": \"John\" } }",
                ExpectedOutput = "John"
            }
        };
    }
    
    private object? ExtractValue(dynamic obj, string path)
    {
        var parts = path.Split('.');
        var current = obj;
        
        foreach (var part in parts)
        {
            if (current == null)
                return null;
            
            if (current is Dictionary<object, object> dict)
            {
                if (!dict.TryGetValue(part, out current))
                    return null;
            }
            else
            {
                return null;
            }
        }
        
        return current;
    }
    
    private string GetInput(ProcessContext context, ProcessorParameters parameters)
    {
        if (parameters.TryGet<string>("Field", out var field) && !string.IsNullOrEmpty(field))
            return context.GetField(field);
        
        return context.SourceContent;
    }
}

