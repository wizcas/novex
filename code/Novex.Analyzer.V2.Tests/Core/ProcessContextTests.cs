using Novex.Analyzer.V2.Core;

namespace Novex.Analyzer.V2.Tests.Core;

public class ProcessContextTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var context = new ProcessContext
        {
            SourceContent = "test content",
            Fields = new Dictionary<string, string> { { "field1", "value1" } },
            Variables = new Dictionary<string, object> { { "var1", "value1" } }
        };

        // Assert
        Assert.Equal("test content", context.SourceContent);
        Assert.Single(context.Fields);
        Assert.Single(context.Variables);
    }

    [Fact]
    public void SetField_AddsOrUpdatesField()
    {
        // Arrange
        var context = new ProcessContext
        {
            Fields = new Dictionary<string, string>()
        };

        // Act
        context.SetField("field1", "value1");
        context.SetField("field1", "value2");

        // Assert
        Assert.Equal("value2", context.GetField("field1"));
    }

    [Fact]
    public void GetField_ReturnsFieldValue()
    {
        // Arrange
        var context = new ProcessContext
        {
            Fields = new Dictionary<string, string> { { "field1", "value1" } }
        };

        // Act
        var value = context.GetField("field1");

        // Assert
        Assert.Equal("value1", value);
    }

    [Fact]
    public void GetField_ReturnsDefaultForNonExistentField()
    {
        // Arrange
        var context = new ProcessContext
        {
            Fields = new Dictionary<string, string>()
        };

        // Act
        var value = context.GetField("nonexistent");

        // Assert
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void SetVariable_AddsOrUpdatesVariable()
    {
        // Arrange
        var context = new ProcessContext
        {
            Variables = new Dictionary<string, object>()
        };

        // Act
        context.SetVariable("var1", "value1");
        context.SetVariable("var1", "value2");

        // Assert
        Assert.Equal("value2", context.GetVariable<string>("var1"));
    }

    [Fact]
    public void GetVariable_ReturnsVariableValue()
    {
        // Arrange
        var context = new ProcessContext
        {
            Variables = new Dictionary<string, object> { { "var1", 42 } }
        };

        // Act
        var value = context.GetVariable<int>("var1");

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetVariable_ReturnsNullForNonExistentVariable()
    {
        // Arrange
        var context = new ProcessContext
        {
            Variables = new Dictionary<string, object>()
        };

        // Act
        var value = context.GetVariable<string>("nonexistent");

        // Assert
        Assert.Null(value);
    }
}

