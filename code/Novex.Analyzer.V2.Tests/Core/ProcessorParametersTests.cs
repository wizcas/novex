using Novex.Analyzer.V2.Core;

namespace Novex.Analyzer.V2.Tests.Core;

public class ProcessorParametersTests
{
    [Fact]
    public void Get_ReturnsValueWithCorrectType()
    {
        // Arrange
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "name", "John" },
            { "age", 30 }
        });

        // Act
        var name = parameters.Get<string>("name");
        var age = parameters.Get<int>("age");

        // Assert
        Assert.Equal("John", name);
        Assert.Equal(30, age);
    }

    [Fact]
    public void Get_ThrowsWhenParameterNotFound()
    {
        // Arrange
        var parameters = new ProcessorParameters(new Dictionary<string, object>());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parameters.Get<string>("nonexistent"));
    }

    [Fact]
    public void TryGet_ReturnsTrueWhenParameterExists()
    {
        // Arrange
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "name", "John" }
        });

        // Act
        var result = parameters.TryGet<string>("name", out var value);

        // Assert
        Assert.True(result);
        Assert.Equal("John", value);
    }

    [Fact]
    public void TryGet_ReturnsFalseWhenParameterNotExists()
    {
        // Arrange
        var parameters = new ProcessorParameters(new Dictionary<string, object>());

        // Act
        var result = parameters.TryGet<string>("nonexistent", out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void Has_ReturnsTrueWhenParameterExists()
    {
        // Arrange
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "name", "John" }
        });

        // Act
        var result = parameters.Has("name");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Has_ReturnsFalseWhenParameterNotExists()
    {
        // Arrange
        var parameters = new ProcessorParameters(new Dictionary<string, object>());

        // Act
        var result = parameters.Has("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNames_ReturnsAllParameterNames()
    {
        // Arrange
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "name", "John" },
            { "age", 30 }
        });

        // Act
        var names = parameters.GetNames();

        // Assert
        Assert.Equal(2, names.Count());
        Assert.Contains("name", names);
        Assert.Contains("age", names);
    }

    [Fact]
    public void Get_ConvertsStringToInt()
    {
        // Arrange
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "count", "42" }
        });

        // Act
        var count = parameters.Get<int>("count");

        // Assert
        Assert.Equal(42, count);
    }

    [Fact]
    public void Get_ConvertsBoolString()
    {
        // Arrange
        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "enabled", "true" }
        });

        // Act
        var enabled = parameters.Get<bool>("enabled");

        // Assert
        Assert.True(enabled);
    }
}

