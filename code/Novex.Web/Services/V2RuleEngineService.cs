using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Engine;
using Novex.Analyzer.V2.Registry;
using Novex.Data.Models;

namespace Novex.Web.Services;

/// <summary>
/// V2 规则引擎服务 - 将 Novex.Analyzer.V2 集成到 Web 应用
/// </summary>
public class V2RuleEngineService
{
    private readonly IProcessorRegistry _registry;
    private readonly RuleEngine _engine;

    public V2RuleEngineService(IProcessorRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _engine = new RuleEngine(_registry);
    }

    /// <summary>
    /// 使用 V2 规则书执行分析
    /// </summary>
    /// <param name="sourceContent">源内容</param>
    /// <param name="ruleBookYaml">规则书 YAML 内容</param>
    /// <returns>分析结果</returns>
    public async Task<ChatLogAnalysisResult> AnalyzeAsync(string sourceContent, string ruleBookYaml)
    {
        if (string.IsNullOrWhiteSpace(sourceContent))
            throw new ArgumentException("源内容不能为空", nameof(sourceContent));

        if (string.IsNullOrWhiteSpace(ruleBookYaml))
            throw new ArgumentException("规则书内容不能为空", nameof(ruleBookYaml));

        try
        {
            // 加载规则书
            var loader = new YamlRuleLoader();
            var ruleBook = loader.LoadFromYaml(ruleBookYaml);

            // 创建处理上下文
            var context = new ProcessContext
            {
                SourceContent = sourceContent,
                Fields = new Dictionary<string, string>(),
                Variables = new Dictionary<string, object>()
            };

            // 执行规则书
            var result = await _engine.ExecuteRuleBookAsync(ruleBook, context);

            // 检查执行是否成功
            if (!result.Success)
            {
                var errorMessages = string.Join("; ", result.Errors.Select(e => e.Message));
                throw new InvalidOperationException($"规则执行失败: {errorMessages}");
            }

            // 转换为 ChatLogAnalysisResult
            // 优先使用规则提取的字段，如果没有则使用默认提取方法
            var title = context.GetField("Title", null);
            var summary = context.GetField("Summary", null);

            return new ChatLogAnalysisResult
            {
                Title = !string.IsNullOrEmpty(title) ? title : ExtractTitle(sourceContent),
                Summary = !string.IsNullOrEmpty(summary) ? summary : ExtractSummary(sourceContent),
                MainBody = result.Output ?? sourceContent,
                CreatedAt = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"V2 规则引擎执行失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 从文件加载规则书并执行分析
    /// </summary>
    /// <param name="sourceContent">源内容</param>
    /// <param name="ruleBookFilePath">规则书文件路径</param>
    /// <returns>分析结果</returns>
    public async Task<ChatLogAnalysisResult> AnalyzeFromFileAsync(string sourceContent, string ruleBookFilePath)
    {
        if (string.IsNullOrWhiteSpace(sourceContent))
            throw new ArgumentException("源内容不能为空", nameof(sourceContent));

        if (string.IsNullOrWhiteSpace(ruleBookFilePath))
            throw new ArgumentException("规则书文件路径不能为空", nameof(ruleBookFilePath));

        if (!File.Exists(ruleBookFilePath))
            throw new FileNotFoundException($"规则书文件不存在: {ruleBookFilePath}");

        try
        {
            var ruleBookYaml = await File.ReadAllTextAsync(ruleBookFilePath);
            return await AnalyzeAsync(sourceContent, ruleBookYaml);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"从文件加载规则书失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 提取标题（取前50个字符）
    /// </summary>
    private string ExtractTitle(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        return content.Length > 50
            ? content.Substring(0, 50) + "..."
            : content;
    }

    /// <summary>
    /// 提取摘要（取前200个字符）
    /// </summary>
    private string ExtractSummary(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        return content.Length > 200
            ? content.Substring(0, 200) + "..."
            : content;
    }
}

