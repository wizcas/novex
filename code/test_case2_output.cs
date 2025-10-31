using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Engine;
using Novex.Analyzer.V2.Processors.Text;
using Novex.Analyzer.V2.Processors.Regex;
using Novex.Analyzer.V2.Processors.Markup;
using Novex.Analyzer.V2.Registry;
using System.IO;

var fixturesDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Novex.Analyzer.V2.Tests", "Fixtures");
var inputFile = Path.Combine(fixturesDir, "case2.input.md");
var outputFile = Path.Combine(fixturesDir, "case2.output.md");
var rulesFile = Path.Combine(fixturesDir, "integration-rules.yaml");

// 加载输入和预期输出
var input = File.ReadAllText(inputFile);
var expected = File.ReadAllText(outputFile);

// 创建注册表
var registry = new ProcessorRegistry();
registry.Register("Text.Cleanup", typeof(CleanupProcessor));
registry.Register("Text.Trim", typeof(TrimProcessor));
registry.Register("Regex.Replace", typeof(Novex.Analyzer.V2.Processors.Regex.ReplaceProcessor));
registry.Register("Markup.SelectNode", typeof(SelectNodeProcessor));
registry.Register("Markup.ExtractText", typeof(ExtractTextProcessor));

// 创建引擎
var engine = new Novex.Analyzer.V2.Engine.RuleEngine(registry);

// 加载规则
var loader = new YamlRuleLoader();
var ruleBook = loader.LoadFromFile(rulesFile);

// 创建上下文
var context = new ProcessContext
{
    SourceContent = input,
    Fields = new Dictionary<string, string>(),
    Variables = new Dictionary<string, object>()
};

// 执行规则
var result = await engine.ExecuteRuleBookAsync(ruleBook, context);

Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Input length: {input.Length}");
Console.WriteLine($"Output length: {result.Output.Length}");
Console.WriteLine($"Expected length: {expected.Length}");
Console.WriteLine($"Output == Expected: {result.Output.Trim() == expected.Trim()}");

// 找到第一个不同的位置
var actualTrimmed = result.Output.Trim();
var expectedTrimmed = expected.Trim();

for (int i = 0; i < Math.Min(actualTrimmed.Length, expectedTrimmed.Length); i++)
{
    if (actualTrimmed[i] != expectedTrimmed[i])
    {
        Console.WriteLine($"\nFirst difference at position {i}:");
        Console.WriteLine($"Expected char: '{expectedTrimmed[i]}' (code: {(int)expectedTrimmed[i]})");
        Console.WriteLine($"Actual char: '{actualTrimmed[i]}' (code: {(int)actualTrimmed[i]})");
        
        // 显示上下文
        var start = Math.Max(0, i - 50);
        var end = Math.Min(actualTrimmed.Length, i + 50);
        Console.WriteLine($"\nExpected context: ...{expectedTrimmed.Substring(start, end - start)}...");
        Console.WriteLine($"Actual context:   ...{actualTrimmed.Substring(start, end - start)}...");
        break;
    }
}

if (actualTrimmed.Length != expectedTrimmed.Length)
{
    Console.WriteLine($"\nLength difference: Expected {expectedTrimmed.Length}, got {actualTrimmed.Length}");
}

