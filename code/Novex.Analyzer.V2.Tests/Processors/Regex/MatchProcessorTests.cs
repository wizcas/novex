using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Processors.Regex;

namespace Novex.Analyzer.V2.Tests.Processors.Regex;

public class MatchProcessorTests
{
    [Fact]
    public async Task ProcessAsync_MatchesPattern()
    {
        // Arrange
        var processor = new MatchProcessor();
        var context = new ProcessContext
        {
            SourceContent = "The year is 2025",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "Pattern", @"\d+" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("2025", result.Output);
    }

    [Fact]
    public async Task ProcessAsync_FailsWhenNoMatch()
    {
        // Arrange
        var processor = new MatchProcessor();
        var context = new ProcessContext
        {
            SourceContent = "No numbers here",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "Pattern", @"\d+" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ProcessAsync_ExtractsSpecificGroup()
    {
        // Arrange
        var processor = new MatchProcessor();
        var context = new ProcessContext
        {
            SourceContent = "Email: test@example.com",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "Pattern", @"(\w+)@(\w+\.\w+)" },
            { "GroupIndex", 1 }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("test", result.Output);
    }

    [Fact]
    public async Task ProcessAsync_IgnoresCaseWhenSpecified()
    {
        // Arrange
        var processor = new MatchProcessor();
        var context = new ProcessContext
        {
            SourceContent = "HELLO world",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "Pattern", "hello" },
            { "IgnoreCase", true }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("HELLO", result.Output);
    }

    [Fact]
    public async Task ProcessAsync_FailsWithInvalidPattern()
    {
        // Arrange
        var processor = new MatchProcessor();
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "Pattern", "[invalid(" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }
}

