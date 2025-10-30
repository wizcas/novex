using HtmlAgilityPack;

namespace Novex.Analyzer.V2.Processors.Markup;

/// <summary>
/// 标记选择节点处理器 - 使用 XPath 选择 HTML/XML 节点
/// </summary>
[Processor("Markup.SelectNode", Category = "Markup", Description = "使用 XPath 选择 HTML/XML 节点")]
public class SelectNodeProcessor : IProcessor, IProcessorMetadata
{
    public string Name => "Markup.SelectNode";
    public string DisplayName => "选择节点";
    public string Description => "使用 XPath 选择 HTML/XML 节点";
    
    public Task<ProcessResult> ProcessAsync(ProcessContext context, ProcessorParameters parameters)
    {
        var input = GetInput(context, parameters);

        if (!parameters.TryGet<string>("XPath", out var xpath) || xpath == null)
            return Task.FromResult(ProcessResult.Fail("XPath 参数必需"));

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(input);

            var node = doc.DocumentNode.SelectSingleNode(xpath);
            if (node == null)
                return Task.FromResult(ProcessResult.Fail("未找到匹配的节点"));
            
            var output = parameters.TryGet<bool>("InnerHtml", out var innerHtml) && innerHtml
                ? node.InnerHtml
                : node.OuterHtml;
            
            return Task.FromResult(ProcessResult.Ok(output));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ProcessResult.Fail($"XPath 查询错误: {ex.Message}", ex));
        }
    }
    
    public IEnumerable<ParameterDefinition> GetParameters()
    {
        return new[]
        {
            new ParameterDefinition
            {
                Name = "XPath",
                Type = typeof(string),
                Required = true,
                Description = "XPath 表达式"
            },
            new ParameterDefinition
            {
                Name = "InnerHtml",
                Type = typeof(bool),
                Required = false,
                DefaultValue = false,
                Description = "是否返回 InnerHtml（否则返回 OuterHtml）"
            }
        };
    }
    
    public IEnumerable<ProcessorExample> GetExamples()
    {
        return new[]
        {
            new ProcessorExample
            {
                Description = "选择第一个段落",
                Parameters = new Dictionary<string, object> { { "XPath", "//p[1]" }, { "InnerHtml", true } },
                Input = "<div><p>First</p><p>Second</p></div>",
                ExpectedOutput = "First"
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

