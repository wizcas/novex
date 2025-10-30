using Novex.Analyzer.V2.Attributes;
using Novex.Analyzer.V2.Core;
using System.Text.RegularExpressions;

namespace Novex.Analyzer.V2.Processors.Text;

/// <summary>
/// 清理处理器 - 移除 HTML 注释、思考块等
/// </summary>
[Processor("Text.Cleanup")]
public class CleanupProcessor : IProcessor
{
    public string Name => "Text.Cleanup";

    public async Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        if (string.IsNullOrEmpty(context.SourceContent))
            return ProcessResult.Ok(context.SourceContent);

        var content = context.SourceContent;

        // 移除 HTML 注释
        content = RemoveHtmlComments(content);

        // 移除思考块 (<!-- ... -->)
        content = RemoveThinkingBlocks(content);

        // 移除多余的空行
        content = RemoveExtraBlankLines(content);

        // 修剪首尾空白
        content = content.Trim();

        return await Task.FromResult(ProcessResult.Ok(content));
    }

    /// <summary>
    /// 移除 HTML 注释
    /// </summary>
    private string RemoveHtmlComments(string content)
    {
        // 移除 <!-- ... --> 格式的注释
        return System.Text.RegularExpressions.Regex.Replace(content, @"<!--.*?-->", "", RegexOptions.Singleline);
    }

    /// <summary>
    /// 移除思考块
    /// </summary>
    private string RemoveThinkingBlocks(string content)
    {
        // 移除 <think>...</think> 格式的思考块
        content = System.Text.RegularExpressions.Regex.Replace(content, @"<think>.*?</think>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // 移除从文件开始到 </think> 的内容（处理没有开始标签的情况）
        // 只在没有 <think> 标签时才匹配
        if (!content.Contains("<think>") && content.Contains("</think>"))
        {
            content = System.Text.RegularExpressions.Regex.Replace(content, @"^.*?</think>", "", RegexOptions.Singleline);
        }

        // 移除 <!-- dialogue_antThinking: ... --> 格式的思考块
        content = System.Text.RegularExpressions.Regex.Replace(content, @"<!--\s*dialogue_antThinking:.*?-->", "", RegexOptions.Singleline);

        return content;
    }

    /// <summary>
    /// 移除多余的空行
    /// </summary>
    private string RemoveExtraBlankLines(string content)
    {
        // 将多个连续的空行替换为单个空行
        return System.Text.RegularExpressions.Regex.Replace(content, @"\n\s*\n\s*\n", "\n\n");
    }
}

