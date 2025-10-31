using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Processors.Regex;

namespace Novex.Analyzer.V2.Tests.Processors.Regex;

public class ReplaceProcessorTests
{
    [Fact]
    public async Task ProcessAsync_RemovesForumContentBlock()
    {
        // Arrange
        var processor = new ReplaceProcessor();
        var input = @"Line 1
Line 2
<!-- FORUM_CONTENT_START -->
Forum content here
More forum content
<!-- FORUM_CONTENT_END -->
Line 3";
        
        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "Pattern", @"<!--\s*FORUM_CONTENT_START\s*-->[\s\S]*?<!--\s*FORUM_CONTENT_END\s*-->" },
            { "Replacement", "" },
            { "RegexOptions", "Singleline" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.DoesNotContain("Forum content", result.Output);
        Assert.Contains("Line 1", result.Output);
        Assert.Contains("Line 3", result.Output);
    }

    [Fact]
    public async Task ProcessAsync_MergesMultipleBlankLines()
    {
        // Arrange
        var processor = new ReplaceProcessor();
        var input = "Line 1\n\n\n\nLine 2";
        
        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };
        
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "Pattern", "\\n\\n+" },
            { "Replacement", "\\n\\n" },
            { "RegexOptions", "Singleline" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Line 1\n\nLine 2", result.Output);
    }
}

