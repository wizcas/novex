using System.Text.RegularExpressions;
using Novex.Analyzer.V2.Attributes;
using Novex.Analyzer.V2.Core;

namespace Novex.Analyzer.V2.Processors.Extraction;

/// <summary>
/// 提取结构化数据处理器 - 从文本中提取结构化字段
/// 支持从标签内容或全文中提取，支持灵活的字段前缀配置
/// </summary>
[Processor("Extraction.ExtractStructuredData", Category = "Extraction", Description = "从文本中提取结构化数据")]
public class ExtractStructuredDataProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Extraction.ExtractStructuredData";
    public string DisplayName => "提取结构化数据";
    public string Description => "从文本中提取结构化数据（支持标签和全文提取）";

    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        if (string.IsNullOrEmpty(context.SourceContent))
            return Task.FromResult(ProcessResult.Ok(context.SourceContent));

        try
        {
            var input = context.SourceContent;
            var tagName = parameters.TryGet<string>("TagName", out var tag) ? tag : null;
            var fieldDefinitions = parameters.TryGet<string>("Fields", out var fields) ? fields : null;
            var separator = parameters.TryGet<string>("Separator", out var sep) ? sep : "/";

            if (string.IsNullOrEmpty(fieldDefinitions))
                return Task.FromResult(ProcessResult.Fail("Fields 参数必需，格式: 'FieldName1:Prefix1,FieldName2:Prefix2'"));

            // 提取内容范围
            var contentToProcess = input;
            if (!string.IsNullOrEmpty(tagName))
            {
                contentToProcess = ExtractTagContent(input, tagName);
                if (string.IsNullOrEmpty(contentToProcess))
                    return Task.FromResult(ProcessResult.Fail($"未找到标签 <{tagName}>"));
            }

            // 解析字段定义
            var fields_dict = ParseFieldDefinitions(fieldDefinitions);
            var extractedValues = new List<string>();

            // 提取每个字段
            foreach (var (fieldName, prefix) in fields_dict)
            {
                var value = ExtractField(contentToProcess, prefix);
                if (!string.IsNullOrEmpty(value))
                {
                    extractedValues.Add(value);
                }
            }

            // 组合结果
            var result = string.Join(separator, extractedValues);

            // 返回结果，由规则引擎决定如何保存
            return Task.FromResult(ProcessResult.Ok(result));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ProcessResult.Fail($"提取结构化数据失败: {ex.Message}", ex));
        }
    }

    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "TagName",
                Type = typeof(string),
                Required = false,
                Description = "要提取内容的标签名（如 'plot'），不指定则从全文提取"
            },
            new ParameterDefinition
            {
                Name = "Fields",
                Type = typeof(string),
                Required = true,
                Description = "字段定义，格式: 'FieldName1:Prefix1,FieldName2:Prefix2'"
            },
            new ParameterDefinition
            {
                Name = "Separator",
                Type = typeof(string),
                Required = false,
                Description = "字段分隔符，默认为 '/'"
            }
        };
    }

    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "从 plot 标签中提取章节和事件名，由规则引擎保存到 Title 字段",
                Parameters = new Dictionary<string, object>
                {
                    { "TagName", "plot" },
                    { "Fields", "Chapter:当前章节,Event:事件名" },
                    { "Separator", "/" }
                },
                Input = "<plot>\n当前章节: 初次接触\n事件名:虚假的小白兔(1/5)\n</plot>",
                ExpectedOutput = "初次接触/虚假的小白兔(1/5)"
            }
        };
    }

    /// <summary>
    /// 从标签中提取内容
    /// </summary>
    private string ExtractTagContent(string input, string tagName)
    {
        var pattern = $@"<{tagName}[^>]*>([\s\S]*?)</{tagName}>";
        var match = System.Text.RegularExpressions.Regex.Match(input, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    /// <summary>
    /// 提取字段值
    /// 支持格式: "前缀:" 或 "前缀: " 后的内容，直到行尾或下一个已知前缀
    /// </summary>
    private string ExtractField(string content, string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return string.Empty;

        // 构建正则表达式：匹配前缀后的内容
        // 支持 "前缀:" 或 "前缀: " 的格式
        // 使用 [^\n]* 而不是 .+? 来避免匹配空行
        var pattern = $@"{System.Text.RegularExpressions.Regex.Escape(prefix)}\s*:\s*([^\n]*)";
        var match = System.Text.RegularExpressions.Regex.Match(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

        if (match.Success)
        {
            var value = match.Groups[1].Value.Trim();
            // 如果值为空，返回空字符串
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            // 移除尾部的特殊字符（如逗号、分号等）
            value = System.Text.RegularExpressions.Regex.Replace(value, @"[,;]*$", "");
            return value;
        }

        return string.Empty;
    }

    /// <summary>
    /// 解析字段定义
    /// 格式: "FieldName1:Prefix1,FieldName2:Prefix2"
    /// </summary>
    private Dictionary<string, string> ParseFieldDefinitions(string fieldDefinitions)
    {
        var result = new Dictionary<string, string>();
        var fields = fieldDefinitions.Split(',');

        foreach (var field in fields)
        {
            var parts = field.Split(':');
            if (parts.Length == 2)
            {
                var fieldName = parts[0].Trim();
                var prefix = parts[1].Trim();
                result[fieldName] = prefix;
            }
        }

        return result;
    }
}

