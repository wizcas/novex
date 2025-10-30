using Novex.Analyzer.V2.Processors.Text;
using Novex.Analyzer.V2.Registry;

namespace Novex.Analyzer.V2.Tests.Registry;

public class ProcessorRegistryTests
{
    [Fact]
    public void Register_RegistersProcessorType()
    {
        // Arrange
        var registry = new ProcessorRegistry();

        // Act
        registry.Register("Text.Trim", typeof(TrimProcessor));

        // Assert
        var processor = registry.Resolve("Text.Trim");
        Assert.NotNull(processor);
        Assert.IsType<TrimProcessor>(processor);
    }

    [Fact]
    public void Register_RegistersProcessorFactory()
    {
        // Arrange
        var registry = new ProcessorRegistry();
        var factory = () => new TrimProcessor();

        // Act
        registry.Register("Text.Trim", factory);

        // Assert
        var processor = registry.Resolve("Text.Trim");
        Assert.NotNull(processor);
        Assert.IsType<TrimProcessor>(processor);
    }

    [Fact]
    public void RegisterSingleton_RegistersSingletonProcessor()
    {
        // Arrange
        var registry = new ProcessorRegistry();
        var instance = new TrimProcessor();

        // Act
        registry.RegisterSingleton("Text.Trim", instance);

        // Assert
        var processor1 = registry.Resolve("Text.Trim");
        var processor2 = registry.Resolve("Text.Trim");
        Assert.Same(instance, processor1);
        Assert.Same(processor1, processor2);
    }

    [Fact]
    public void Resolve_ThrowsWhenProcessorNotFound()
    {
        // Arrange
        var registry = new ProcessorRegistry();

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => registry.Resolve("NonExistent"));
    }

    [Fact]
    public void TryResolve_ReturnsTrueWhenProcessorExists()
    {
        // Arrange
        var registry = new ProcessorRegistry();
        registry.Register("Text.Trim", typeof(TrimProcessor));

        // Act
        var result = registry.TryResolve("Text.Trim", out var processor);

        // Assert
        Assert.True(result);
        Assert.NotNull(processor);
    }

    [Fact]
    public void TryResolve_ReturnsFalseWhenProcessorNotExists()
    {
        // Arrange
        var registry = new ProcessorRegistry();

        // Act
        var result = registry.TryResolve("NonExistent", out var processor);

        // Assert
        Assert.False(result);
        Assert.Null(processor);
    }

    [Fact]
    public void GetRegisteredNames_ReturnsAllRegisteredNames()
    {
        // Arrange
        var registry = new ProcessorRegistry();
        registry.Register("Text.Trim", typeof(TrimProcessor));
        registry.Register("Text.Replace", typeof(ReplaceProcessor));

        // Act
        var names = registry.GetRegisteredNames();

        // Assert
        Assert.Equal(2, names.Count());
        Assert.Contains("Text.Trim", names);
        Assert.Contains("Text.Replace", names);
    }

    [Fact]
    public void Register_OverwritesPreviousRegistration()
    {
        // Arrange
        var registry = new ProcessorRegistry();
        registry.Register("Text.Trim", typeof(TrimProcessor));

        // Act
        registry.Register("Text.Trim", typeof(ReplaceProcessor));

        // Assert
        var processor = registry.Resolve("Text.Trim");
        Assert.IsType<ReplaceProcessor>(processor);
    }

    [Fact]
    public void GetMetadata_ReturnsProcessorMetadata()
    {
        // Arrange
        var registry = new ProcessorRegistry();
        registry.Register("Text.Trim", typeof(TrimProcessor));

        // Act
        var metadata = registry.GetMetadata("Text.Trim");

        // Assert
        Assert.NotNull(metadata);
        Assert.NotEmpty(metadata.DisplayName);
    }
}

