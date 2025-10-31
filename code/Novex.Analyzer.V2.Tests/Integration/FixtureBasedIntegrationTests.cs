using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Engine;
using Novex.Analyzer.V2.Models;
using Novex.Analyzer.V2.Processors.Text;
using Novex.Analyzer.V2.Processors.Regex;
using Novex.Analyzer.V2.Processors.Markup;
using Novex.Analyzer.V2.Registry;

namespace Novex.Analyzer.V2.Tests.Integration;

/// <summary>
/// 基于 Fixtures 的集成测试
/// 使用 Fixtures 目录中的 case*.input.md 和 case*.output.md 文件进行测试
/// 遵循 V1 规则原则：加载规则定义，处理输入文件，验证输出与预期一致
/// </summary>
public class FixtureBasedIntegrationTests
{
    private readonly string _fixturesDir = Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..", "Novex.Analyzer.V2.Tests", "Fixtures");

    private IProcessorRegistry CreateRegistry()
    {
        var registry = new ProcessorRegistry();
        // Text 处理器
        registry.Register("Text.Cleanup", typeof(CleanupProcessor));
        registry.Register("Text.Trim", typeof(TrimProcessor));
        registry.Register("Text.RemoveContentBlocks", typeof(RemoveContentBlocksProcessor));
        // Regex 处理器
        registry.Register("Regex.Replace", typeof(Novex.Analyzer.V2.Processors.Regex.ReplaceProcessor));
        registry.Register("Regex.Match", typeof(MatchProcessor));
        // Markup 处理器
        registry.Register("Markup.SelectNode", typeof(SelectNodeProcessor));
        registry.Register("Markup.ExtractText", typeof(ExtractTextProcessor));
        return registry;
    }

    private async Task<ProcessResult> ExecuteRulesFromFile(string sourceContent, string rulesFilePath)
    {
        var registry = CreateRegistry();
        var engine = new Novex.Analyzer.V2.Engine.RuleEngine(registry);

        // 加载规则文件
        var loader = new YamlRuleLoader();
        var ruleBook = loader.LoadFromFile(rulesFilePath);

        // 调试：打印规则
        Console.WriteLine($"\n=== Loaded {ruleBook.Rules.Count} rules ===");
        foreach (var rule in ruleBook.Rules.OrderBy(r => r.Priority))
        {
            Console.WriteLine($"  {rule.Priority:D3} - {rule.Id} (Enabled: {rule.Enabled})");
        }

        // 创建处理上下文
        var context = new ProcessContext
        {
            SourceContent = sourceContent,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        // 执行规则书
        return await engine.ExecuteRuleBookAsync(ruleBook, context);
    }

    /// <summary>
    /// 规范化换行符，将所有换行符转换为 LF（\n）用于比较
    /// </summary>
    private string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    /// <summary>
    /// 获取所有 fixture 文件对 (case*.input.md, case*.output.md)
    /// </summary>
    private IEnumerable<(string CaseNumber, string InputFile, string OutputFile)> GetFixturePairs()
    {
        var inputFiles = Directory.GetFiles(_fixturesDir, "case*.input.md")
            .OrderBy(f => f)
            .ToArray();

        foreach (var inputFile in inputFiles)
        {
            var caseNumber = Path.GetFileNameWithoutExtension(inputFile)
                .Replace("case", "")
                .Replace(".input", "");
            var outputFile = Path.Combine(_fixturesDir, $"case{caseNumber}.output.md");

            if (File.Exists(outputFile))
            {
                yield return (caseNumber, inputFile, outputFile);
            }
        }
    }

    /// <summary>
    /// 验证所有 fixture 文件对存在
    /// </summary>
    [Fact]
    public void FixtureFiles_ShouldExist()
    {
        // Arrange & Act
        var fixturePairs = GetFixturePairs().ToList();

        // Assert
        Assert.NotEmpty(fixturePairs);
        foreach (var (caseNumber, inputFile, outputFile) in fixturePairs)
        {
            Assert.True(File.Exists(inputFile), $"case{caseNumber}.input.md 应该存在");
            Assert.True(File.Exists(outputFile), $"case{caseNumber}.output.md 应该存在");
        }
    }

    /// <summary>
    /// 验证所有 fixture 文件不为空
    /// </summary>
    [Fact]
    public void FixtureFiles_ShouldNotBeEmpty()
    {
        // Arrange & Act
        var fixturePairs = GetFixturePairs().ToList();

        // Assert
        foreach (var (caseNumber, inputFile, outputFile) in fixturePairs)
        {
            var inputInfo = new FileInfo(inputFile);
            var outputInfo = new FileInfo(outputFile);
            Assert.True(inputInfo.Length > 0, $"case{caseNumber}.input.md 不应该为空");
            Assert.True(outputInfo.Length > 0, $"case{caseNumber}.output.md 不应该为空");
        }
    }

    /// <summary>
    /// 处理所有 fixture 输入文件，验证输出与预期完全一致
    /// 遵循 V1 规则原则：输入 -> 处理 -> 输出验证
    /// </summary>
    [Fact]
    public async Task AllFixtures_ShouldProduceExpectedOutput()
    {
        // Arrange
        var fixturePairs = GetFixturePairs().ToList();
        Assert.NotEmpty(fixturePairs);

        var rulesFile = Path.Combine(_fixturesDir, "integration-rules.yaml");
        if (!File.Exists(rulesFile))
        {
            return;
        }

        // Act & Assert
        foreach (var (caseNumber, inputFile, outputFile) in fixturePairs)
        {
            var sourceContent = File.ReadAllText(inputFile);
            var expectedOutput = File.ReadAllText(outputFile);

            var result = await ExecuteRulesFromFile(sourceContent, rulesFile);

            Assert.True(result.Success, $"Case {caseNumber}: 规则执行应该成功。错误: {string.Join("; ", result.Errors.Select(e => e.Message))}");
            Assert.Equal(expectedOutput.Trim(), result.Output.Trim());
        }
    }

    /// <summary>
    /// 调试：输出 Case2 处理结果到临时文件
    /// </summary>
    [Fact]
    public async Task Debug_OutputCase2ProcessedResult()
    {
        var inputFile = Path.Combine(_fixturesDir, "case2.input.md");
        var rulesFile = Path.Combine(_fixturesDir, "integration-rules.yaml");
        var outputFile = Path.Combine(_fixturesDir, "case2.processed.md");

        if (!File.Exists(inputFile) || !File.Exists(rulesFile))
        {
            return;
        }

        var sourceContent = File.ReadAllText(inputFile);
        var result = await ExecuteRulesFromFile(sourceContent, rulesFile);

        // 输出处理结果到临时文件
        File.WriteAllText(outputFile, result.Output);
        Console.WriteLine($"Processed output written to: {outputFile}");
        Console.WriteLine($"Output length: {result.Output.Length}");
    }

    /// <summary>
    /// 单独测试 Case 1: 处理输入文件，验证输出与预期一致
    /// </summary>
    [Fact]
    public async Task Case1_ShouldProduceExpectedOutput()
    {
        // Arrange
        var inputFile = Path.Combine(_fixturesDir, "case1.input.md");
        var outputFile = Path.Combine(_fixturesDir, "case1.output.md");
        var rulesFile = Path.Combine(_fixturesDir, "integration-rules.yaml");

        if (!File.Exists(inputFile) || !File.Exists(outputFile) || !File.Exists(rulesFile))
        {
            return;
        }

        var sourceContent = File.ReadAllText(inputFile);
        var expectedOutput = File.ReadAllText(outputFile);

        // Act
        var result = await ExecuteRulesFromFile(sourceContent, rulesFile);

        // 保存处理后的输出用于调试
        var processedFile = Path.Combine(_fixturesDir, "case1.processed.md");
        File.WriteAllText(processedFile, result.Output);

        // Assert
        Assert.True(result.Success, $"Case 1: 规则执行应该成功。错误: {string.Join("; ", result.Errors.Select(e => e.Message))}");
        // 忽略换行符差异进行比较
        var normalizedExpected = NormalizeLineEndings(expectedOutput.Trim());
        var normalizedActual = NormalizeLineEndings(result.Output.Trim());
        Assert.Equal(normalizedExpected, normalizedActual);
    }

    /// <summary>
    /// 单独测试 Case 2: 处理输入文件，验证输出与预期一致
    /// </summary>
    [Fact]
    public async Task Case2_ShouldProduceExpectedOutput()
    {
        // Arrange
        var inputFile = Path.Combine(_fixturesDir, "case2.input.md");
        var outputFile = Path.Combine(_fixturesDir, "case2.output.md");
        var rulesFile = Path.Combine(_fixturesDir, "integration-rules.yaml");

        if (!File.Exists(inputFile) || !File.Exists(outputFile) || !File.Exists(rulesFile))
        {
            return;
        }

        var sourceContent = File.ReadAllText(inputFile);
        var expectedOutput = File.ReadAllText(outputFile);

        // Act
        var result = await ExecuteRulesFromFile(sourceContent, rulesFile);

        // Assert
        Assert.True(result.Success, $"Case 2: 规则执行应该成功。错误: {string.Join("; ", result.Errors.Select(e => e.Message))}");
        // 忽略换行符差异进行比较
        var normalizedExpected = NormalizeLineEndings(expectedOutput.Trim());
        var normalizedActual = NormalizeLineEndings(result.Output.Trim());
        Assert.Equal(normalizedExpected, normalizedActual);
    }

    /// <summary>
    /// 单独测试 Case 3: 处理输入文件，验证输出与预期一致
    /// </summary>
    [Fact]
    public async Task Case3_ShouldProduceExpectedOutput()
    {
        // Arrange
        var inputFile = Path.Combine(_fixturesDir, "case3.input.md");
        var outputFile = Path.Combine(_fixturesDir, "case3.output.md");
        var rulesFile = Path.Combine(_fixturesDir, "integration-rules.yaml");

        if (!File.Exists(inputFile) || !File.Exists(outputFile) || !File.Exists(rulesFile))
        {
            return;
        }

        var sourceContent = File.ReadAllText(inputFile);
        var expectedOutput = File.ReadAllText(outputFile);

        // Act
        var result = await ExecuteRulesFromFile(sourceContent, rulesFile);

        // 保存处理后的输出用于调试
        var processedFile = Path.Combine(_fixturesDir, "case3.processed.md");
        File.WriteAllText(processedFile, result.Output);
        Console.WriteLine($"Output length: {result.Output.Length}");

        // Assert
        Assert.True(result.Success, $"Case 3: 规则执行应该成功。错误: {string.Join("; ", result.Errors.Select(e => e.Message))}");
        // 忽略换行符差异进行比较
        var normalizedExpected = NormalizeLineEndings(expectedOutput.Trim());
        var normalizedActual = NormalizeLineEndings(result.Output.Trim());
        Assert.Equal(normalizedExpected, normalizedActual);
    }
}

