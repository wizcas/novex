using Novex.Analyzer.V2.Core;

namespace Novex.Analyzer.V2.Tests.Core;

public class ProcessResultTests
{
    [Fact]
    public void Ok_CreatesSuccessfulResult()
    {
        // Act
        var result = ProcessResult.Ok("output");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("output", result.Output);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Fail_CreatesFailedResultWithMessage()
    {
        // Act
        var result = ProcessResult.Fail("error message");

        // Assert
        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Equal("error message", result.Errors[0].Message);
    }

    [Fact]
    public void Fail_CreatesFailedResultWithMessageAndException()
    {
        // Arrange
        var exception = new InvalidOperationException("test exception");

        // Act
        var result = ProcessResult.Fail("error message", exception);

        // Assert
        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Equal("error message", result.Errors[0].Message);
        Assert.NotNull(result.Errors[0].Exception);
        Assert.Equal("test exception", result.Errors[0].Exception?.Message);
    }

    [Fact]
    public void Ok_WithEmptyString()
    {
        // Act
        var result = ProcessResult.Ok(string.Empty);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(string.Empty, result.Output);
    }

    [Fact]
    public void Fail_WithEmptyMessage()
    {
        // Act
        var result = ProcessResult.Fail(string.Empty);

        // Assert
        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Equal(string.Empty, result.Errors[0].Message);
    }
}

