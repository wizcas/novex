using Novex.Analyzer.V2.Attributes;
using Novex.Analyzer.V2.Core;
using System.Text.RegularExpressions;

namespace Novex.Analyzer.V2.Processors.Text;

/// <summary>
/// 移除内容块处理器 - 根据配置的开始和结束标记移除内容块
/// </summary>
[Processor("Text.RemoveContentBlocks", Category = "Text", Description = "根据标记移除内容块")]
public class RemoveContentBlocksProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Text.RemoveContentBlocks";
    public string DisplayName => "移除内容块";
    public string Description => "根据配置的开始和结束标记移除内容块";

    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        if (string.IsNullOrEmpty(context.SourceContent))
            return Task.FromResult(ProcessResult.Ok(context.SourceContent));

        var content = context.SourceContent;

        // 支持多个块的移除（通过数组配置）
        // 尝试从参数字典中直接获取 Blocks，因为 TryGet 可能无法正确处理 IList
        var allParams = parameters.GetAll();
        if (allParams.TryGetValue("Blocks", out var blocksObj) && blocksObj is System.Collections.IList blocksList)
        {
            foreach (var blockItem in blocksList)
            {
                if (blockItem is Dictionary<string, object> blockDict)
                {
                    var start = blockDict.GetValueOrDefault("Start")?.ToString();
                    var end = blockDict.GetValueOrDefault("End")?.ToString();

                    if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
                    {
                        content = RemoveBlock(content, start, end);
                    }
                }
            }
        }

        // 支持单个块的移除（通过 Start 和 End 参数）
        if (parameters.TryGet<string>("Start", out var singleStart) &&
            parameters.TryGet<string>("End", out var singleEnd) &&
            !string.IsNullOrEmpty(singleStart) && !string.IsNullOrEmpty(singleEnd))
        {
            content = RemoveBlock(content, singleStart, singleEnd);
        }

        return Task.FromResult(ProcessResult.Ok(content));
    }

    private string RemoveBlock(string content, string startMarker, string endMarker)
    {
        // 转义特殊正则字符
        var escapedStart = System.Text.RegularExpressions.Regex.Escape(startMarker);
        var escapedEnd = System.Text.RegularExpressions.Regex.Escape(endMarker);

        // 创建模式：从开始标记到结束标记的所有内容（包括标记本身）
        var pattern = $"{escapedStart}.*?{escapedEnd}";

        // 移除所有匹配的块
        return System.Text.RegularExpressions.Regex.Replace(content, pattern, "", RegexOptions.Singleline);
    }

    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "Start",
                Type = typeof(string),
                Required = false,
                Description = "内容块开始标记"
            },
            new ParameterDefinition
            {
                Name = "End",
                Type = typeof(string),
                Required = false,
                Description = "内容块结束标记"
            },
            new ParameterDefinition
            {
                Name = "Blocks",
                Type = typeof(System.Collections.IList),
                Required = false,
                Description = "多个内容块配置（每个包含 Start 和 End）"
            }
        };
    }

    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "移除 HTML 注释风格的内容块",
                Parameters = new Dictionary<string, object>
                {
                    { "Start", "<!-- WEIBO_CONTENT_START -->" },
                    { "End", "<!-- WEIBO_CONTENT_END -->" }
                },
                Input = "开始\n<!-- WEIBO_CONTENT_START -->\n微博内容\n<!-- WEIBO_CONTENT_END -->\n结束",
                ExpectedOutput = "开始\n结束"
            },
            new ProcessorExample
            {
                Description = "移除方括号风格的内容块",
                Parameters = new Dictionary<string, object>
                {
                    { "Start", "[START]" },
                    { "End", "[END]" }
                },
                Input = "开始\n[START]\n内容\n[END]\n结束",
                ExpectedOutput = "开始\n结束"
            }
        };
    }
}

