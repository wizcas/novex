namespace Novex.Analyzer.V2.Examples;

/// <summary>
/// 基础示例 - 展示如何使用 Novex.Analyzer V2
/// </summary>
public class BasicExample
{
    /// <summary>
    /// 示例 1: 使用单个处理器
    /// </summary>
    public static async Task Example1_SingleProcessor()
    {
        // 创建注册表并注册处理器
        var registry = new ProcessorRegistry();
        registry.Register("Text.Trim", typeof(Processors.Text.TrimProcessor));
        
        // 解析处理器
        var processor = registry.Resolve("Text.Trim");
        
        // 创建处理上下文
        var context = new ProcessContext
        {
            SourceContent = "  hello world  ",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        
        // 创建参数
        var parameters = new ProcessorParameters(new Dictionary<string, object>());
        
        // 执行处理器
        var result = await processor.ProcessAsync(context, parameters);
        
        Console.WriteLine($"输入: '  hello world  '");
        Console.WriteLine($"输出: '{result.Output}'");
        Console.WriteLine($"成功: {result.Success}");
    }
    
    /// <summary>
    /// 示例 2: 使用规则引擎
    /// </summary>
    public static async Task Example2_RuleEngine()
    {
        // 创建注册表并注册处理器
        var registry = new ProcessorRegistry();
        registry.Register("Text.Trim", typeof(Processors.Text.TrimProcessor));
        registry.Register("Transform.ToUpper", typeof(Processors.Transform.ToUpperProcessor));
        
        // 创建规则引擎
        var engine = new Engine.RuleEngine(registry);
        
        // 创建规则
        var rule = new ProcessRule
        {
            Id = "rule1",
            Name = "Trim and Upper",
            Processor = "Text.Trim",
            Scope = ProcessorScope.Source,
            Priority = 1,
            Enabled = true
        };
        
        // 创建处理上下文
        var context = new ProcessContext
        {
            SourceContent = "  hello world  ",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        
        // 执行规则
        var result = await engine.ExecuteRuleAsync(rule, context);
        
        Console.WriteLine($"规则执行结果: {result.Output}");
    }
    
    /// <summary>
    /// 示例 3: 从 YAML 加载规则
    /// </summary>
    public static async Task Example3_YamlRules()
    {
        var yaml = @"
version: 2.0
description: 示例规则书
rules:
  - id: rule1
    name: 修剪空白
    processor: Text.Trim
    scope: Source
    priority: 1
    enabled: true
";
        
        // 加载规则
        var loader = new Engine.YamlRuleLoader();
        var ruleBook = loader.LoadFromYaml(yaml);
        
        // 创建注册表
        var registry = new ProcessorRegistry();
        registry.Register("Text.Trim", typeof(Processors.Text.TrimProcessor));
        
        // 创建规则引擎
        var engine = new Engine.RuleEngine(registry);
        
        // 创建处理上下文
        var context = new ProcessContext
        {
            SourceContent = "  hello world  ",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        
        // 执行规则书
        var result = await engine.ExecuteRuleBookAsync(ruleBook, context);
        
        Console.WriteLine($"规则书执行结果: {result.Output}");
    }
}

