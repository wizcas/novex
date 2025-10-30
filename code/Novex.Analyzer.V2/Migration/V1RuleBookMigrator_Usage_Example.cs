using Novex.Analyzer.Models;
using Novex.Analyzer.V2.Engine;
using Novex.Analyzer.V2.Migration;
using Novex.Analyzer.V2.Registry;

namespace Novex.Analyzer.V2.Examples;

/// <summary>
/// V1RuleBookMigrator 使用示例
/// 演示如何将 V1 规则书迁移到 V2
/// </summary>
public class V1RuleBookMigratorUsageExample
{
    /// <summary>
    /// 示例 1: 基本迁移 - 创建 V1 规则书并迁移到 V2
    /// </summary>
    public static void Example1_BasicMigration()
    {
        // 第一步：创建 V1 规则书
        var v1RuleBook = new AnalysisRuleBook
        {
            Version = "1.0",
            Description = "示例 V1 规则书",
            ExtractionRules = new List<ExtractionRule>
            {
                new ExtractionRule
                {
                    Id = "extract_title",
                    Name = "提取标题",
                    MatcherType = MatcherType.Regex,
                    Pattern = @"^(.+?)$",
                    Action = ActionType.Extract,
                    Target = TargetField.Title,
                    Enabled = true,
                    Options = new MatchOptions { Multiline = true }
                },
                new ExtractionRule
                {
                    Id = "remove_comments",
                    Name = "移除 HTML 注释",
                    MatcherType = MatcherType.Regex,
                    Pattern = @"<!--.*?-->",
                    Action = ActionType.Remove,
                    Target = TargetField.MainBody,
                    Enabled = true,
                    Options = new MatchOptions { Singleline = true }
                }
            },
            TransformationRules = new List<TransformationRule>
            {
                new TransformationRule
                {
                    Id = "cleanup_whitespace",
                    Name = "清理空白字符",
                    SourceField = "MainBody",
                    TargetField = "MainBody",
                    TransformationType = TransformationType.CleanWhitespace,
                    Enabled = true,
                    Parameters = new Dictionary<string, object>()
                }
            }
        };

        // 第二步：创建迁移工具并执行迁移
        var migrator = new V1RuleBookMigrator();
        var v2RuleBook = migrator.MigrateRuleBook(v1RuleBook);

        // 第三步：查看迁移结果
        Console.WriteLine($"V2 规则书版本: {v2RuleBook.Version}");
        Console.WriteLine($"V2 规则书描述: {v2RuleBook.Description}");
        Console.WriteLine($"V2 规则数量: {v2RuleBook.Rules.Count}");
        
        foreach (var rule in v2RuleBook.Rules)
        {
            Console.WriteLine($"  - {rule.Name} ({rule.Processor})");
        }
    }

    /// <summary>
    /// 示例 2: 迁移后执行规则
    /// </summary>
    public static async Task Example2_MigrateAndExecute()
    {
        // 创建 V1 规则书
        var v1RuleBook = new AnalysisRuleBook
        {
            Version = "1.0",
            Description = "清理规则",
            PreparationRules = new List<TransformationRule>
            {
                new TransformationRule
                {
                    Id = "cleanup",
                    Name = "清理内容",
                    SourceField = "Source",
                    TargetField = "Source",
                    TransformationType = TransformationType.RemoveHtmlComments,
                    Enabled = true
                }
            }
        };

        // 迁移规则
        var migrator = new V1RuleBookMigrator();
        var v2RuleBook = migrator.MigrateRuleBook(v1RuleBook);

        // 创建处理器注册表
        var registry = new ProcessorRegistry();
        registry.Register("Text.Cleanup", typeof(Novex.Analyzer.V2.Processors.Text.CleanupProcessor));

        // 创建规则引擎
        var engine = new Novex.Analyzer.V2.Engine.RuleEngine(registry);

        // 执行规则
        var context = new Novex.Analyzer.V2.Core.ProcessContext
        {
            SourceContent = "Hello <!-- comment --> World",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        var result = await engine.ExecuteRuleBookAsync(v2RuleBook, context);
        
        Console.WriteLine($"执行成功: {result.Success}");
        Console.WriteLine($"输出内容: {result.Output}");
    }

    /// <summary>
    /// 示例 3: 查看迁移映射
    /// </summary>
    public static void Example3_ViewMappings()
    {
        Console.WriteLine("=== V1 到 V2 处理器映射 ===\n");

        var mappings = new Dictionary<string, string>
        {
            // 提取规则映射
            ["Regex + Extract"] = "Regex.Match",
            ["Regex + Remove"] = "Regex.Replace",
            ["Regex + Replace"] = "Regex.Replace",
            ["Markup + Extract"] = "Markup.SelectNode",
            ["Markup + Remove"] = "Markup.RemoveNode",
            ["Text + Extract"] = "Text.Trim",
            ["Text + Remove"] = "Text.Replace",
            ["Text + Replace"] = "Text.Replace",
            ["JsonPath + Extract"] = "Json.SelectToken",
            ["XPath + Extract"] = "Markup.SelectNode",
            ["CssSelector + Extract"] = "Markup.SelectNode",
            
            // 转换规则映射
            ["Format"] = "Text.Trim",
            ["Truncate"] = "Text.Truncate",
            ["CleanWhitespace"] = "Text.Trim",
            ["RemoveHtmlComments"] = "Regex.Replace",
            ["RemoveXmlTags"] = "Regex.Replace"
        };

        foreach (var mapping in mappings)
        {
            Console.WriteLine($"{mapping.Key,-30} => {mapping.Value}");
        }
    }

    /// <summary>
    /// 示例 4: 处理迁移失败的规则
    /// </summary>
    public static void Example4_HandleFailedMigrations()
    {
        var v1RuleBook = new AnalysisRuleBook
        {
            Version = "1.0",
            ExtractionRules = new List<ExtractionRule>
            {
                new ExtractionRule
                {
                    Id = "rule1",
                    Name = "启用的规则",
                    MatcherType = MatcherType.Regex,
                    Pattern = @"\d+",
                    Action = ActionType.Extract,
                    Target = TargetField.Title,
                    Enabled = true
                },
                new ExtractionRule
                {
                    Id = "rule2",
                    Name = "禁用的规则",
                    MatcherType = MatcherType.Regex,
                    Pattern = @"\w+",
                    Action = ActionType.Extract,
                    Target = TargetField.Title,
                    Enabled = false  // 禁用的规则会被跳过
                }
            }
        };

        var migrator = new V1RuleBookMigrator();
        var v2RuleBook = migrator.MigrateRuleBook(v1RuleBook);

        Console.WriteLine($"V1 规则数: {v1RuleBook.ExtractionRules.Count}");
        Console.WriteLine($"V2 规则数: {v2RuleBook.Rules.Count}");
        Console.WriteLine("注意: 禁用的规则不会被迁移");
    }
}

