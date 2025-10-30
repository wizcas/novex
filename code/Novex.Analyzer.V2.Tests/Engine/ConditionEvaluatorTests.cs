using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Engine;

namespace Novex.Analyzer.V2.Tests.Engine;

public class ConditionEvaluatorTests
{
    [Fact]
    public void Evaluate_ReturnsTrueWhenConditionIsNull()
    {
        // Arrange
        var evaluator = new ConditionEvaluator();
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = evaluator.Evaluate(null, context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_ReturnsTrueWhenConditionIsEmpty()
    {
        // Arrange
        var evaluator = new ConditionEvaluator();
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = evaluator.Evaluate(string.Empty, context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_ChecksFieldExists()
    {
        // Arrange
        var evaluator = new ConditionEvaluator();
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string> { { "title", "hello" } },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = evaluator.Evaluate("field:title", context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_ChecksFieldNotExists()
    {
        // Arrange
        var evaluator = new ConditionEvaluator();
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = evaluator.Evaluate("field:title", context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Evaluate_ChecksFieldEqualsValue()
    {
        // Arrange
        var evaluator = new ConditionEvaluator();
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string> { { "status", "active" } },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = evaluator.Evaluate("field:status=active", context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_ChecksFieldNotEqualsValue()
    {
        // Arrange
        var evaluator = new ConditionEvaluator();
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string> { { "status", "active" } },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = evaluator.Evaluate("field:status!=inactive", context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_ChecksFieldMatchesPattern()
    {
        // Arrange
        var evaluator = new ConditionEvaluator();
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string> { { "email", "test@example.com" } },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = evaluator.Evaluate(@"field:email~\w+@\w+\.\w+", context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_ReturnsFalseForInvalidPattern()
    {
        // Arrange
        var evaluator = new ConditionEvaluator();
        var context = new ProcessContext
        {
            SourceContent = "test",
            Fields = new Dictionary<string, string> { { "email", "test@example.com" } },
            Variables = new Dictionary<string, object>()
        };

        // Act
        var result = evaluator.Evaluate(@"field:email~[invalid(", context);

        // Assert
        Assert.False(result);
    }
}

