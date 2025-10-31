using System.Text.RegularExpressions;

namespace Novex.Analyzer.V2.Processors.Regex;

/// <summary>
/// 正则替换处理器 - 使用正则表达式替换文本
/// </summary>
[Processor("Regex.Replace", Category = "Regex", Description = "使用正则表达式替换文本")]
public class ReplaceProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Regex.Replace";
    public string DisplayName => "正则替换";
    public string Description => "使用正则表达式替换文本";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);

        if (!parameters.TryGet<string>("Pattern", out var pattern) || pattern == null)
            return Task.FromResult(ProcessResult.Fail("Pattern 参数必需"));

        var replacement = parameters.TryGet<string>("Replacement", out var r) ? r : string.Empty;

        try
        {
            var options = RegexOptions.None;
            if (parameters.TryGet<bool>("IgnoreCase", out var ignoreCase) && ignoreCase)
                options |= RegexOptions.IgnoreCase;

            // 处理 RegexOptions 参数（如果提供）
            if (parameters.TryGet<string>("RegexOptions", out var regexOptionsStr) && !string.IsNullOrEmpty(regexOptionsStr))
            {
                var optionsParts = regexOptionsStr.Split(',');
                foreach (var part in optionsParts)
                {
                    var trimmedPart = part.Trim();
                    if (Enum.TryParse<RegexOptions>(trimmedPart, out var opt))
                    {
                        options |= opt;
                        Console.WriteLine($"[ReplaceProcessor] Applied RegexOption: {trimmedPart} -> {opt}");
                    }
                    else
                    {
                        Console.WriteLine($"[ReplaceProcessor] Failed to parse RegexOption: {trimmedPart}");
                    }
                }
            }

            // 处理转义序列
            pattern = UnescapeString(pattern);
            replacement = UnescapeString(replacement);

            Console.WriteLine($"[ReplaceProcessor] Pattern: {pattern}");
            Console.WriteLine($"[ReplaceProcessor] Input length: {input.Length}");
            Console.WriteLine($"[ReplaceProcessor] RegexOptions: {options}");

            // 检查模式是否匹配
            var matches = System.Text.RegularExpressions.Regex.Matches(input, pattern, options);
            Console.WriteLine($"[ReplaceProcessor] Matches found: {matches.Count}");

            var output = System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement ?? string.Empty, options);

            Console.WriteLine($"[ReplaceProcessor] Output length: {output.Length}");
            Console.WriteLine($"[ReplaceProcessor] Changed: {input.Length != output.Length}");

            return Task.FromResult(ProcessResult.Ok(output));
        }
        catch (ArgumentException ex)
        {
            return Task.FromResult(ProcessResult.Fail($"正则表达式错误: {ex.Message}", ex));
        }
    }

    /// <summary>
    /// 处理字符串中的转义序列
    /// </summary>
    private string UnescapeString(string? str)
    {
        if (string.IsNullOrEmpty(str))
            return str ?? string.Empty;

        // 注意：必须先处理 \\\\ 以避免影响其他转义序列
        return str
            .Replace("\\\\", "\x00")  // 临时占位符
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t")
            .Replace("\x00", "\\");   // 恢复单个反斜杠
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
                Name = "Replacement",
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
                Description = "移除所有数字",
                Parameters = new Dictionary<string, object> { { "Pattern", @"\d+" }, { "Replacement", "" } },
                Input = "abc123def456",
                ExpectedOutput = "abcdef"
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

