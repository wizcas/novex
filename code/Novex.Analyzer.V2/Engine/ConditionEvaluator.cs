namespace Novex.Analyzer.V2.Engine;

/// <summary>
/// 条件评估器
/// </summary>
public class ConditionEvaluator
{
    /// <summary>
    /// 评估条件
    /// </summary>
    public bool Evaluate(string? condition, ProcessContext context)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return true; // 没有条件时默认为真
        
        // 简单的条件评估（实际应该使用更复杂的表达式解析器）
        return EvaluateSimpleCondition(condition, context);
    }
    
    private bool EvaluateSimpleCondition(string condition, ProcessContext context)
    {
        // 支持的条件格式:
        // - "field:name" - 检查字段是否存在
        // - "field:name=value" - 检查字段值
        // - "field:name!=value" - 检查字段值不等于
        // - "field:name~pattern" - 检查字段值匹配正则
        
        if (condition.StartsWith("field:"))
        {
            var fieldCondition = condition.Substring(6);

            if (fieldCondition.Contains("!="))
            {
                var parts = fieldCondition.Split(new[] { "!=" }, StringSplitOptions.None);
                var fieldName = parts[0].Trim();
                var expectedValue = parts[1].Trim();
                var actualValue = context.GetField(fieldName);
                return actualValue != expectedValue;
            }
            else if (fieldCondition.Contains("="))
            {
                var parts = fieldCondition.Split('=', 2);
                var fieldName = parts[0].Trim();
                var expectedValue = parts[1].Trim();
                var actualValue = context.GetField(fieldName);
                return actualValue == expectedValue;
            }
            else if (fieldCondition.Contains("~"))
            {
                var parts = fieldCondition.Split('~', 2);
                var fieldName = parts[0].Trim();
                var pattern = parts[1].Trim();
                var actualValue = context.GetField(fieldName);
                try
                {
                    return System.Text.RegularExpressions.Regex.IsMatch(actualValue, pattern);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // 只检查字段是否存在
                var fieldValue = context.GetField(fieldCondition);
                return !string.IsNullOrEmpty(fieldValue);
            }
        }
        
        return false;
    }
}

