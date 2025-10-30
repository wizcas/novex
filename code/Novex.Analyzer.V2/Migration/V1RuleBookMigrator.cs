using Novex.Analyzer.Models;
using Novex.Analyzer.V2.Models;
using System.Text.RegularExpressions;

namespace Novex.Analyzer.V2.Migration;

/// <summary>
/// V1 规则书迁移工具 - 将 V1 规则转换为 V2 规则
/// </summary>
public class V1RuleBookMigrator
{
    /// <summary>
    /// 迁移 V1 规则书到 V2
    /// </summary>
    public RuleBook MigrateRuleBook(AnalysisRuleBook v1RuleBook)
    {
        var v2RuleBook = new RuleBook
        {
            Version = "2.0",
            Description = v1RuleBook.Description,
            Rules = new List<ProcessRule>()
        };

        var priority = 1;

        // 迁移预处理规则
        foreach (var rule in v1RuleBook.PreparationRules)
        {
            var processRule = MigrateTransformationRule(rule, priority++);
            if (processRule != null)
                v2RuleBook.Rules.Add(processRule);
        }

        // 迁移提取规则
        foreach (var rule in v1RuleBook.ExtractionRules)
        {
            var processRule = MigrateExtractionRule(rule, priority++);
            if (processRule != null)
                v2RuleBook.Rules.Add(processRule);
        }

        // 迁移转换规则
        foreach (var rule in v1RuleBook.TransformationRules)
        {
            var processRule = MigrateTransformationRule(rule, priority++);
            if (processRule != null)
                v2RuleBook.Rules.Add(processRule);
        }

        return v2RuleBook;
    }

    /// <summary>
    /// 迁移提取规则
    /// </summary>
    private ProcessRule? MigrateExtractionRule(ExtractionRule v1Rule, int priority)
    {
        if (!v1Rule.Enabled)
            return null;

        var processorName = GetProcessorName(v1Rule.MatcherType, v1Rule.Action);
        if (string.IsNullOrEmpty(processorName))
            return null;

        var targetField = GetTargetField(v1Rule.Target, v1Rule.CustomTargetName);

        var rule = new ProcessRule
        {
            Id = v1Rule.Id,
            Name = v1Rule.Name,
            Processor = processorName,
            Scope = ProcessorScope.Field,
            SourceField = "Source",
            TargetField = targetField,
            Priority = priority,
            Enabled = v1Rule.Enabled,
            OnError = ErrorHandlingStrategy.Skip
        };

        // 设置处理器参数
        rule.Parameters = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(v1Rule.Pattern))
            rule.Parameters["pattern"] = v1Rule.Pattern;

        if (!string.IsNullOrEmpty(v1Rule.ReplacementValue))
            rule.Parameters["replacement"] = v1Rule.ReplacementValue;

        // 处理匹配选项
        if (v1Rule.Options != null)
        {
            if (v1Rule.Options.Multiline)
                rule.Parameters["multiline"] = true;
            if (v1Rule.Options.Singleline)
                rule.Parameters["singleline"] = true;
            if (v1Rule.Options.Global)
                rule.Parameters["global"] = true;
            if (v1Rule.Options.IgnoreCase)
                rule.Parameters["ignoreCase"] = true;
        }

        return rule;
    }

    /// <summary>
    /// 迁移转换规则
    /// </summary>
    private ProcessRule? MigrateTransformationRule(TransformationRule v1Rule, int priority)
    {
        if (!v1Rule.Enabled)
            return null;

        var processorName = GetTransformationProcessorName(v1Rule.TransformationType);
        if (string.IsNullOrEmpty(processorName))
            return null;

        var rule = new ProcessRule
        {
            Id = v1Rule.Id,
            Name = v1Rule.Name,
            Processor = processorName,
            Scope = ProcessorScope.Field,
            SourceField = v1Rule.SourceField,
            TargetField = v1Rule.TargetField,
            Priority = priority,
            Enabled = v1Rule.Enabled,
            OnError = ErrorHandlingStrategy.Skip,
            Parameters = new Dictionary<string, object>(v1Rule.Parameters)
        };

        return rule;
    }

    /// <summary>
    /// 根据匹配器类型和操作获取处理器名称
    /// </summary>
    private string? GetProcessorName(MatcherType matcherType, ActionType actionType)
    {
        return (matcherType, actionType) switch
        {
            (MatcherType.Regex, ActionType.Extract) => "Regex.Match",
            (MatcherType.Regex, ActionType.Remove) => "Regex.Replace",
            (MatcherType.Regex, ActionType.Replace) => "Regex.Replace",
            (MatcherType.Markup, ActionType.Extract) => "Markup.SelectNode",
            (MatcherType.Markup, ActionType.Remove) => "Markup.RemoveNode",
            (MatcherType.Text, ActionType.Extract) => "Text.Trim",
            (MatcherType.Text, ActionType.Remove) => "Text.Replace",
            (MatcherType.Text, ActionType.Replace) => "Text.Replace",
            (MatcherType.JsonPath, ActionType.Extract) => "Json.SelectToken",
            (MatcherType.XPath, ActionType.Extract) => "Markup.SelectNode",
            (MatcherType.CssSelector, ActionType.Extract) => "Markup.SelectNode",
            _ => null
        };
    }

    /// <summary>
    /// 根据转换类型获取处理器名称
    /// </summary>
    private string? GetTransformationProcessorName(TransformationType transformationType)
    {
        return transformationType switch
        {
            TransformationType.Format => "Text.Trim",
            TransformationType.Truncate => "Text.Truncate",
            TransformationType.CleanWhitespace => "Text.Trim",
            TransformationType.RemoveHtmlComments => "Regex.Replace",
            TransformationType.RemoveXmlTags => "Regex.Replace",
            _ => null
        };
    }

    /// <summary>
    /// 获取目标字段名称
    /// </summary>
    private string GetTargetField(TargetField targetField, string? customName)
    {
        return targetField switch
        {
            TargetField.Title => "Title",
            TargetField.Summary => "Summary",
            TargetField.MainBody => "MainBody",
            TargetField.Source => "Source",
            TargetField.Custom => customName ?? "Custom",
            _ => "Source"
        };
    }
}

