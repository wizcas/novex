namespace Novex.Analyzer.V2.Engine;

/// <summary>
/// 规则验证器
/// </summary>
public class RuleValidator
{
    private readonly IProcessorRegistry _registry;
    
    public RuleValidator(IProcessorRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }
    
    /// <summary>
    /// 验证规则
    /// </summary>
    public IEnumerable<string> ValidateRule(ProcessRule rule)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(rule.Id))
            errors.Add("规则 ID 不能为空");

        if (string.IsNullOrWhiteSpace(rule.Name))
            errors.Add("规则名称不能为空");

        if (string.IsNullOrWhiteSpace(rule.Processor))
            errors.Add("处理器名称不能为空");
        // 注意：不在这里检查处理器是否存在，而是在执行时处理

        if (rule.Scope == ProcessorScope.Field && string.IsNullOrWhiteSpace(rule.SourceField))
            errors.Add("字段作用域需要指定 SourceField");

        return errors;
    }
    
    /// <summary>
    /// 验证规则书
    /// </summary>
    public IEnumerable<string> ValidateRuleBook(RuleBook ruleBook)
    {
        var errors = new List<string>();
        
        if (ruleBook.Rules == null || ruleBook.Rules.Count == 0)
            errors.Add("规则书中没有规则");
        
        var ruleIds = new HashSet<string>();
        foreach (var rule in ruleBook.Rules ?? new List<ProcessRule>())
        {
            var ruleErrors = ValidateRule(rule);
            errors.AddRange(ruleErrors);
            
            if (!string.IsNullOrWhiteSpace(rule.Id))
            {
                if (ruleIds.Contains(rule.Id))
                    errors.Add($"重复的规则 ID: {rule.Id}");
                else
                    ruleIds.Add(rule.Id);
            }
        }
        
        return errors;
    }
}

