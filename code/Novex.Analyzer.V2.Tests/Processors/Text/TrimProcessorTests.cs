using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Processors.Text;

namespace Novex.Analyzer.V2.Tests.Processors.Text;

public class TrimProcessorTests
{
    [Fact]
    public async Task ProcessAsync_TrimsLeadingAndTrailingWhitespace()
    {
        // Arrange
        var processor = new TrimProcessor();
        var context = new ProcessContext
        {
            SourceContent = "  hello world  ",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        var parameters = new ProcessorParameters(new Dictionary<string, object>());

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("hello world", result.Output);
    }

    [Fact]
    public async Task ProcessAsync_HandlesEmptyString()
    {
        // Arrange
        var processor = new TrimProcessor();
        var context = new ProcessContext
        {
            SourceContent = string.Empty,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        var parameters = new ProcessorParameters(new Dictionary<string, object>());

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(string.Empty, result.Output);
    }

    [Fact]
    public async Task ProcessAsync_HandlesOnlyWhitespace()
    {
        // Arrange
        var processor = new TrimProcessor();
        var context = new ProcessContext
        {
            SourceContent = "   ",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        var parameters = new ProcessorParameters(new Dictionary<string, object>());

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(string.Empty, result.Output);
    }

    [Fact]
    public async Task ProcessAsync_HandlesNoWhitespace()
    {
        // Arrange
        var processor = new TrimProcessor();
        var context = new ProcessContext
        {
            SourceContent = "hello",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        var parameters = new ProcessorParameters(new Dictionary<string, object>());

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("hello", result.Output);
    }

    [Fact]
    public async Task ProcessAsync_TrimsCustomCharacters()
    {
        // Arrange
        var processor = new TrimProcessor();
        var context = new ProcessContext
        {
            SourceContent = "***hello***",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "TrimChars", "*" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("hello", result.Output);
    }
}

