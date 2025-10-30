using System.Text.RegularExpressions;

namespace Novex.Analyzer.V2.Processors.Regex;

/// <summary>
/// 正则匹配处理器 - 使用正则表达式匹配文本
/// </summary>
[Processor("Regex.Match", Category = "Regex", Description = "使用正则表达式匹配文本")]
public class MatchProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Regex.Match";
    public string DisplayName => "正则匹配";
    public string Description => "使用正则表达式匹配文本";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);

        if (!parameters.TryGet<string>("Pattern", out var pattern) || pattern == null)
            return Task.FromResult(ProcessResult.Fail("Pattern 参数必需"));

        try
        {
            var options = RegexOptions.None;
            if (parameters.TryGet<bool>("IgnoreCase", out var ignoreCase) && ignoreCase)
                options |= RegexOptions.IgnoreCase;

            var match = System.Text.RegularExpressions.Regex.Match(input, pattern, options);
            
            if (!match.Success)
                return Task.FromResult(ProcessResult.Fail("未找到匹配项"));
            
            var output = parameters.TryGet<int>("GroupIndex", out var groupIndex)
                ? match.Groups[groupIndex].Value
                : match.Value;
            
            return Task.FromResult(ProcessResult.Ok(output));
        }
        catch (ArgumentException ex)
        {
            return Task.FromResult(ProcessResult.Fail($"正则表达式错误: {ex.Message}", ex));
        }
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "Pattern",
                Type = typeof(string),
                Required = true,
                Description = "正则表达式模式"
            },
            new ParameterDefinition
            {
                Name = "GroupIndex",
                Type = typeof(int),
                Required = false,
                DefaultValue = 0,
                Description = "要提取的组索引"
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
                Description = "提取电子邮件地址",
                Parameters = new Dictionary<string, object> { { "Pattern", @"[\w\.-]+@[\w\.-]+\.\w+" } },
                Input = "Contact: john@example.com",
                ExpectedOutput = "john@example.com"
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

