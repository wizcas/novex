using Novex.Analyzer;
using Novex.Data.Models;
using Xunit;

namespace Novex.Analyzer.Tests;

public class ChatLogAnalyzerTests
{
  private readonly ChatLogAnalyzer _analyzer;

  public ChatLogAnalyzerTests()
  {
    _analyzer = new ChatLogAnalyzer();
  }

  [Fact]
  public async Task AnalyzeAsync_WithValidMessageAndRule_ReturnsResult()
  {
    // Arrange
    var message = "这是一条测试消息，用来验证分析功能是否正常工作。";
    var rule = "general";

    // Act
    var result = await _analyzer.AnalyzeAsync(message, rule);

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Title);
    Assert.NotEmpty(result.Summary);
    Assert.NotEmpty(result.MainBody);
    Assert.True(result.CreatedAt > DateTime.MinValue);
  }

  [Fact]
  public async Task AnalyzeAsync_WithEmptyMessage_ThrowsArgumentException()
  {
    // Arrange
    var message = "";
    var rule = "general";

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => _analyzer.AnalyzeAsync(message, rule));
  }

  [Fact]
  public async Task AnalyzeAsync_WithNullMessage_ThrowsArgumentException()
  {
    // Arrange
    string message = null!;
    var rule = "general";

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => _analyzer.AnalyzeAsync(message, rule));
  }

  [Fact]
  public async Task AnalyzeAsync_WithEmptyRule_ThrowsArgumentException()
  {
    // Arrange
    var message = "测试消息";
    var rule = "";

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => _analyzer.AnalyzeAsync(message, rule));
  }

  [Fact]
  public async Task AnalyzeAsync_WithLengthRule_AnalyzesLength()
  {
    // Arrange
    var message = "短消息";
    var rule = "length";

    // Act
    var result = await _analyzer.AnalyzeAsync(message, rule);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("长度分析", result.MainBody);
    Assert.Contains("较短", result.MainBody);
  }

  [Fact]
  public async Task AnalyzeAsync_WithSentimentRulePositive_DetectsPositiveSentiment()
  {
    // Arrange
    var message = "我很开心，今天心情很好，感到非常快乐！";
    var rule = "sentiment";

    // Act
    var result = await _analyzer.AnalyzeAsync(message, rule);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("情感分析", result.MainBody);
    Assert.Contains("积极", result.MainBody);
  }

  [Fact]
  public async Task AnalyzeAsync_WithSentimentRuleNegative_DetectsNegativeSentiment()
  {
    // Arrange
    var message = "我很难过，今天心情很差，感到非常悲伤。";
    var rule = "sentiment";

    // Act
    var result = await _analyzer.AnalyzeAsync(message, rule);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("情感分析", result.MainBody);
    Assert.Contains("消极", result.MainBody);
  }

  [Fact]
  public async Task AnalyzeAsync_WithKeywordRule_AnalyzesKeywords()
  {
    // Arrange
    var message = "这是一个测试，我在测试这个功能是否正常工作。";
    var rule = "keyword";

    // Act
    var result = await _analyzer.AnalyzeAsync(message, rule);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("关键词分析", result.MainBody);
    Assert.Contains("常见词汇频次", result.MainBody);
  }

  [Fact]
  public async Task AnalyzeAsync_WithLongMessage_TruncatesTitle()
  {
    // Arrange - 确保消息超过50个字符
    var message = "这是一条非常长的消息，用来测试标题截断功能是否正常工作，标题应该会被截断并添加省略号以确保长度超过五十个字符的限制要求";
    var rule = "general";

    // Act
    var result = await _analyzer.AnalyzeAsync(message, rule);

    // Assert
    Assert.NotNull(result.Title);
    Assert.True(result.Title.Length <= 53); // 50 characters + "..."
    Assert.EndsWith("...", result.Title);
  }

  [Fact]
  public async Task AnalyzeAsync_WithShortMessage_DoesNotTruncateTitle()
  {
    // Arrange
    var message = "短消息";
    var rule = "general";

    // Act
    var result = await _analyzer.AnalyzeAsync(message, rule);

    // Assert
    Assert.NotNull(result.Title);
    Assert.Equal("短消息", result.Title);
    Assert.DoesNotContain("...", result.Title);
  }

  [Theory]
  [InlineData("length")]
  [InlineData("sentiment")]
  [InlineData("keyword")]
  [InlineData("general")]
  [InlineData("unknown")]
  public async Task AnalyzeAsync_WithDifferentRules_ReturnsValidResults(string rule)
  {
    // Arrange
    var message = "这是一条测试消息，包含一些常见的词汇和标点符号！";

    // Act
    var result = await _analyzer.AnalyzeAsync(message, rule);

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Title);
    Assert.NotEmpty(result.Summary);
    Assert.NotEmpty(result.MainBody);
    Assert.Contains(rule, result.Summary);
  }
}