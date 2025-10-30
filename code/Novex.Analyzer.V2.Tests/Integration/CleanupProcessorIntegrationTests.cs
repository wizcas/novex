using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Engine;
using Novex.Analyzer.V2.Models;
using Novex.Analyzer.V2.Processors.Text;
using Novex.Analyzer.V2.Registry;

namespace Novex.Analyzer.V2.Tests.Integration;

/// <summary>
/// CleanupProcessor 集成测试
/// </summary>
public class CleanupProcessorIntegrationTests
{
    private readonly string _testDataDir = Path.Combine(
        AppContext.BaseDirectory, 
        "..", "..", "..", "..", "TestData");

    private ProcessorRegistry CreateRegistry()
    {
        var registry = new ProcessorRegistry();
        registry.Register("Text.Cleanup", typeof(CleanupProcessor));
        registry.Register("Text.Trim", typeof(TrimProcessor));
        return registry;
    }

    [Fact]
    public async Task ExecuteCleanupRule_WithTestData_ShouldRemoveCommentsAndThinkingBlocks()
    {
        // Arrange
        var sourceFile = Path.Combine(_testDataDir, "test.mainbody.source.md");
        var resultFile = Path.Combine(_testDataDir, "test.mainbody.result.md");
        
        if (!File.Exists(sourceFile) || !File.Exists(resultFile))
        {
            // 如果测试数据文件不存在，跳过此测试
            return;
        }
        
        var sourceContent = File.ReadAllText(sourceFile);
        var expectedContent = File.ReadAllText(resultFile);
        
        var registry = CreateRegistry();
        var engine = new Novex.Analyzer.V2.Engine.RuleEngine(registry);
        
        var rule = new ProcessRule
        {
            Id = "cleanup_mainbody",
            Name = "清理主体内容",
            Processor = "Text.Cleanup",
            Scope = ProcessorScope.Source,
            Priority = 1,
            Enabled = true,
            OnError = ErrorHandlingStrategy.Skip
        };
        
        var context = new ProcessContext
        {
            SourceContent = sourceContent,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        
        // Act
        var result = await engine.ExecuteRuleAsync(rule, context);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedContent.Trim(), result.Output.Trim());
    }

    [Fact]
    public async Task ExecuteCleanupRule_ShouldRemoveHtmlComments()
    {
        // Arrange
        var content = "Hello <!-- This is a comment --> World";
        var expected = "Hello  World";
        
        var registry = CreateRegistry();
        var engine = new Novex.Analyzer.V2.Engine.RuleEngine(registry);
        
        var rule = new ProcessRule
        {
            Id = "cleanup_comments",
            Name = "移除 HTML 注释",
            Processor = "Text.Cleanup",
            Scope = ProcessorScope.Source,
            Priority = 1,
            Enabled = true,
            OnError = ErrorHandlingStrategy.Skip
        };
        
        var context = new ProcessContext
        {
            SourceContent = content,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        
        // Act
        var result = await engine.ExecuteRuleAsync(rule, context);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal(expected, result.Output);
    }

    [Fact]
    public async Task ExecuteCleanupRule_ShouldRemoveThinkingBlocks()
    {
        // Arrange
        var content = "Start <think>This is thinking</think> End";
        var expected = "Start  End";
        
        var registry = CreateRegistry();
        var engine = new Novex.Analyzer.V2.Engine.RuleEngine(registry);
        
        var rule = new ProcessRule
        {
            Id = "cleanup_thinking",
            Name = "移除思考块",
            Processor = "Text.Cleanup",
            Scope = ProcessorScope.Source,
            Priority = 1,
            Enabled = true,
            OnError = ErrorHandlingStrategy.Skip
        };
        
        var context = new ProcessContext
        {
            SourceContent = content,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        
        // Act
        var result = await engine.ExecuteRuleAsync(rule, context);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal(expected, result.Output);
    }

    [Fact]
    public async Task ExecuteCleanupRule_ShouldRemoveExtraBlankLines()
    {
        // Arrange
        var content = "Line 1\n\n\nLine 2\n\n\nLine 3";
        var expected = "Line 1\n\nLine 2\n\nLine 3";
        
        var registry = CreateRegistry();
        var engine = new Novex.Analyzer.V2.Engine.RuleEngine(registry);
        
        var rule = new ProcessRule
        {
            Id = "cleanup_blanks",
            Name = "移除多余空行",
            Processor = "Text.Cleanup",
            Scope = ProcessorScope.Source,
            Priority = 1,
            Enabled = true,
            OnError = ErrorHandlingStrategy.Skip
        };
        
        var context = new ProcessContext
        {
            SourceContent = content,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        
        // Act
        var result = await engine.ExecuteRuleAsync(rule, context);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal(expected, result.Output);
    }
}

