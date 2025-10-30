using HtmlAgilityPack;

namespace Novex.Analyzer.V2.Processors.Markup;

/// <summary>
/// 标记提取文本处理器 - 从 HTML/XML 中提取纯文本
/// </summary>
[Processor("Markup.ExtractText", Category = "Markup", Description = "从 HTML/XML 中提取纯文本")]
public class ExtractTextProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Markup.ExtractText";
    public string DisplayName => "提取文本";
    public string Description => "从 HTML/XML 中提取纯文本";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);
        
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(input);
            
            var text = doc.DocumentNode.InnerText;
            
            // 清理多余空白
            if (parameters.TryGet<bool>("CleanWhitespace", out var clean) && clean)
            {
                text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
            }
            
            return Task.FromResult(ProcessResult.Ok(text));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ProcessResult.Fail($"HTML 解析错误: {ex.Message}", ex));
        }
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "CleanWhitespace",
                Type = typeof(bool),
                Required = false,
                DefaultValue = true,
                Description = "是否清理多余空白"
            }
        };
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "从 HTML 中提取文本",
                Parameters = new Dictionary<string, object> { { "CleanWhitespace", true } },
                Input = "<p>Hello <b>World</b></p>",
                ExpectedOutput = "Hello World"
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

