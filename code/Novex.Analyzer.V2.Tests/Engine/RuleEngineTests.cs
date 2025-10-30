using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Engine;
using Novex.Analyzer.V2.Models;
using Novex.Analyzer.V2.Processors.Text;
using Novex.Analyzer.V2.Registry;

namespace Novex.Analyzer.V2.Tests.Engine;

public class RuleEngineTests
{
    private ProcessorRegistry CreateRegistry()
    {
        var registry = new ProcessorRegistry();
        registry.Register("Text.Trim", typeof(TrimProcessor));
        return registry;
    }

    [Fact]
    public async Task ExecuteRuleAsync_ExecutesSingleRule()
    {
        // Arrange
        var registry = CreateRegistry();
        var engine = new RuleEngine(registry);
        var rule = new ProcessRule
        {
            Id = "rule1",
            Name = "Test Rule",
            Processor = "Text.Trim",
            Scope = ProcessorScope.Source,
            Priority = 1,
            Enabled = true,
            OnError = ErrorHandlingStrategy.Skip
        };
        var context = new ProcessContext
        {
            SourceContent = "  hello  ",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = await engine.ExecuteRuleAsync(rule, context);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("hello", result.Output);
    }

    [Fact]
    public async Task ExecuteRuleAsync_SkipsDisabledRule()
    {
        // Arrange
        var registry = CreateRegistry();
        var engine = new RuleEngine(registry);
        var rule = new ProcessRule
        {
            Id = "rule1",
            Name = "Test Rule",
            Processor = "Text.Trim",
            Scope = ProcessorScope.Source,
            Priority = 1,
            Enabled = false,
            OnError = ErrorHandlingStrategy.Skip
        };
        var context = new ProcessContext
        {
            SourceContent = "  hello  ",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = await engine.ExecuteRuleAsync(rule, context);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("  hello  ", result.Output);
    }

    [Fact]
    public async Task ExecuteRuleAsync_UpdatesTargetField()
    {
        // Arrange
        var registry = CreateRegistry();
        var engine = new RuleEngine(registry);
        var rule = new ProcessRule
        {
            Id = "rule1",
            Name = "Test Rule",
            Processor = "Text.Trim",
            Scope = ProcessorScope.Field,
            SourceField = "title",
            TargetField = "title",
            Priority = 1,
            Enabled = true,
            OnError = ErrorHandlingStrategy.Skip
        };
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string> { { "title", "  hello  " } },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = await engine.ExecuteRuleAsync(rule, context);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("hello", context.GetField("title"));
    }

    [Fact]
    public async Task ExecuteRuleBookAsync_ExecutesMultipleRules()
    {
        // Arrange
        var registry = CreateRegistry();
        var engine = new RuleEngine(registry);
        var ruleBook = new RuleBook
        {
            Rules = new List<ProcessRule>
            {
                new ProcessRule
                {
                    Id = "rule1",
                    Name = "Test Rule",
                    Processor = "Text.Trim",
                    Scope = ProcessorScope.Source,
                    Priority = 1,
                    Enabled = true,
                    OnError = ErrorHandlingStrategy.Skip
                }
            }
        };
        var context = new ProcessContext
        {
            SourceContent = "  hello  ",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = await engine.ExecuteRuleBookAsync(ruleBook, context);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("hello", result.Output);
    }

    [Fact]
    public async Task ExecuteRuleAsync_HandlesErrorWithSkipStrategy()
    {
        // Arrange
        var registry = CreateRegistry();
        var engine = new RuleEngine(registry);
        var rule = new ProcessRule
        {
            Id = "rule1",
            Name = "Test Rule",
            Processor = "NonExistent",
            Scope = ProcessorScope.Source,
            Priority = 1,
            Enabled = true,
            OnError = ErrorHandlingStrategy.Skip
        };
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = await engine.ExecuteRuleAsync(rule, context);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ExecuteRuleAsync_HandlesErrorWithThrowStrategy()
    {
        // Arrange
        var registry = CreateRegistry();
        var engine = new RuleEngine(registry);
        var rule = new ProcessRule
        {
            Id = "rule1",
            Name = "Test Rule",
            Processor = "NonExistent",
            Scope = ProcessorScope.Source,
            Priority = 1,
            Enabled = true,
            OnError = ErrorHandlingStrategy.Throw
        };
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => engine.ExecuteRuleAsync(rule, context));
    }
}

