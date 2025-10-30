using Novex.Analyzer.V2.Engine;
using Novex.Analyzer.V2.Models;

namespace Novex.Analyzer.V2.Tests.Engine;

public class YamlRuleLoaderTests
{
    [Fact]
    public void LoadFromYaml_ParsesSimpleRuleBook()
    {
        // Arrange
        var yaml = @"
Version: 2.0
Description: Test Rule Book
Rules:
  - Id: rule1
    Name: Test Rule
    Processor: Text.Trim
    Scope: Source
    Priority: 1
    Enabled: true
";
        var loader = new YamlRuleLoader();

        // Act
        var ruleBook = loader.LoadFromYaml(yaml);

        // Assert
        Assert.NotNull(ruleBook);
        Assert.Single(ruleBook.Rules);
        Assert.Equal("rule1", ruleBook.Rules[0].Id);
        Assert.Equal("Text.Trim", ruleBook.Rules[0].Processor);
    }

    [Fact]
    public void LoadFromYaml_ParsesRuleWithParameters()
    {
        // Arrange
        var yaml = @"
Version: 2.0
Rules:
  - Id: rule1
    Processor: Text.Replace
    Scope: Source
    Priority: 1
    Enabled: true
    Parameters:
      OldValue: hello
      NewValue: world
";
        var loader = new YamlRuleLoader();

        // Act
        var ruleBook = loader.LoadFromYaml(yaml);

        // Assert
        Assert.NotNull(ruleBook);
        Assert.Single(ruleBook.Rules);
        Assert.NotEmpty(ruleBook.Rules[0].Parameters);
    }

    [Fact]
    public void LoadFromYaml_ThrowsOnEmptyYaml()
    {
        // Arrange
        var loader = new YamlRuleLoader();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => loader.LoadFromYaml(string.Empty));
    }

    [Fact]
    public void LoadFromYaml_ThrowsOnNullYaml()
    {
        // Arrange
        var loader = new YamlRuleLoader();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => loader.LoadFromYaml(null!));
    }

    [Fact]
    public void SaveToYaml_SerializesRuleBook()
    {
        // Arrange
        var ruleBook = new RuleBook
        {
            Description = "Test",
            Rules = new List<ProcessRule>
            {
                new ProcessRule
                {
                    Id = "rule1",
                    Name = "Test Rule",
                    Processor = "Text.Trim",
                    Scope = ProcessorScope.Source,
                    Priority = 1,
                    Enabled = true
                }
            }
        };
        var loader = new YamlRuleLoader();

        // Act
        var yaml = loader.SaveToYaml(ruleBook);

        // Assert
        Assert.NotNull(yaml);
        Assert.Contains("rule1", yaml);
        Assert.Contains("Text.Trim", yaml);
    }

    [Fact]
    public void SaveToYaml_ThenLoadFromYaml_RoundTrip()
    {
        // Arrange
        var originalRuleBook = new RuleBook
        {
            Description = "Test",
            Rules = new List<ProcessRule>
            {
                new ProcessRule
                {
                    Id = "rule1",
                    Name = "Test Rule",
                    Processor = "Text.Trim",
                    Scope = ProcessorScope.Source,
                    Priority = 1,
                    Enabled = true
                }
            }
        };
        var loader = new YamlRuleLoader();

        // Act
        var yaml = loader.SaveToYaml(originalRuleBook);
        var loadedRuleBook = loader.LoadFromYaml(yaml);

        // Assert
        Assert.NotNull(loadedRuleBook);
        Assert.Single(loadedRuleBook.Rules);
        Assert.Equal("rule1", loadedRuleBook.Rules[0].Id);
        Assert.Equal("Text.Trim", loadedRuleBook.Rules[0].Processor);
    }

    [Fact]
    public void LoadFromYaml_ParsesMultipleRules()
    {
        // Arrange
        var yaml = @"
Version: 2.0
Rules:
  - Id: rule1
    Processor: Text.Trim
    Scope: Source
    Priority: 1
    Enabled: true
  - Id: rule2
    Processor: Transform.ToUpper
    Scope: Source
    Priority: 2
    Enabled: true
";
        var loader = new YamlRuleLoader();

        // Act
        var ruleBook = loader.LoadFromYaml(yaml);

        // Assert
        Assert.NotNull(ruleBook);
        Assert.Equal(2, ruleBook.Rules.Count);
        Assert.Equal("rule1", ruleBook.Rules[0].Id);
        Assert.Equal("rule2", ruleBook.Rules[1].Id);
    }
}

