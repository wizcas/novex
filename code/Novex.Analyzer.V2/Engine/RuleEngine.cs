using Microsoft.Extensions.Logging;

namespace Novex.Analyzer.V2.Engine;

/// <summary>
/// 规则引擎
/// </summary>
public class RuleEngine
{
    private readonly IProcessorRegistry _registry;
    private readonly RuleValidator _validator;
    private readonly ConditionEvaluator _conditionEvaluator;
    private readonly ILogger? _logger;
    
    public RuleEngine(IProcessorRegistry registry, ILogger? logger = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _validator = new RuleValidator(registry);
        _conditionEvaluator = new ConditionEvaluator();
        _logger = logger;
    }
    
    /// <summary>
    /// 执行规则
    /// </summary>
    public async Task<ProcessResult> ExecuteRuleAsync(ProcessRule rule, ProcessContext context)
    {
        // 验证规则
        var validationErrors = _validator.ValidateRule(rule);
        if (validationErrors.Any())
        {
            var errorMessage = string.Join("; ", validationErrors);
            return ProcessResult.Fail(errorMessage);
        }
        
        // 检查条件
        if (!_conditionEvaluator.Evaluate(rule.Condition, context))
        {
            _logger?.LogInformation($"规则 {rule.Id} 条件不满足，跳过执行");
            return ProcessResult.Ok(context.SourceContent);
        }
        
        // 解析处理器
        if (!_registry.TryResolve(rule.Processor, out var processor))
            return ProcessResult.Fail($"处理器未找到: {rule.Processor}");
        
        try
        {
            // 准备处理上下文
            var processorContext = new ProcessContext
            {
                SourceContent = context.SourceContent,
                Fields = new Dictionary<string, string>(context.Fields),
                Variables = new Dictionary<string, object>(context.Variables),
                Logger = context.Logger,
                CancellationToken = context.CancellationToken
            };
            
            // 执行处理器
            var processorParams = new ProcessorParameters(rule.Parameters ?? new Dictionary<string, object>());
            var result = await processor.ProcessAsync(processorContext, processorParams);

            if (!result.Success)
            {
                // 处理错误
                return HandleError(rule, result);
            }

            // 更新字段
            if (!string.IsNullOrWhiteSpace(rule.TargetField))
            {
                context.SetField(rule.TargetField, result.Output ?? string.Empty);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"执行规则 {rule.Id} 时出错: {ex.Message}");
            return ProcessResult.Fail($"规则执行错误: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 执行规则书
    /// </summary>
    public async Task<ProcessResult> ExecuteRuleBookAsync(RuleBook ruleBook, ProcessContext context)
    {
        // 验证规则书
        var validationErrors = _validator.ValidateRuleBook(ruleBook);
        if (validationErrors.Any())
        {
            var errorMessage = string.Join("; ", validationErrors);
            return ProcessResult.Fail(errorMessage);
        }
        
        // 按优先级排序规则
        var sortedRules = (ruleBook.Rules ?? new List<ProcessRule>())
            .Where(r => r.Enabled)
            .OrderBy(r => r.Priority)
            .ToList();
        
        foreach (var rule in sortedRules)
        {
            var result = await ExecuteRuleAsync(rule, context);
            if (!result.Success)
            {
                _logger?.LogError($"规则 {rule.Id} 执行失败: {result.Errors.FirstOrDefault()?.Message}");
            }
        }
        
        return ProcessResult.Ok(context.SourceContent);
    }
    
    private ProcessResult HandleError(ProcessRule rule, ProcessResult result)
    {
        return rule.OnError switch
        {
            ErrorHandlingStrategy.Throw => result,
            ErrorHandlingStrategy.Skip => ProcessResult.Ok(result.Output ?? string.Empty),
            ErrorHandlingStrategy.UseDefault => ProcessResult.Ok(string.Empty),
            _ => result
        };
    }
}

