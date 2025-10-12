using Novex.Analyzer;
using Novex.Analyzer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Novex.Analyzer.Tests
{
  public class BlockRemovalIntegrationTests
  {
    private readonly ITestOutputHelper _output;

    public BlockRemovalIntegrationTests(ITestOutputHelper output)
    {
      _output = output;
    }

    [Fact]
    public async Task ChatLogAnalyzer_ShouldRemoveDiscussionBlocks_WhenUsingYamlRules()
    {
      // Arrange
      string testDataFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "TestData", "phoneDiscussions.jsonl");
      string rulesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Novex.Analyzer.Tests", "MockData", "dream-phone", "rules.yaml");

      if (!File.Exists(testDataFile))
      {
        _output.WriteLine("1.jsonl 文件不存在，跳过测试");
        return;
      }

      if (!File.Exists(rulesFile))
      {
        _output.WriteLine("rules.yaml 文件不存在，跳过测试");
        return;
      }

      string[] lines = File.ReadAllLines(testDataFile);
      if (lines.Length == 0)
      {
        _output.WriteLine("1.jsonl 文件为空，跳过测试");
        return;
      }

      // 解析第一行的JSON，获取mes内容
      JsonDocument jsonDoc = JsonDocument.Parse(lines[0]);
      string? originalContent = jsonDoc.RootElement.GetProperty("mes").GetString();

      if (originalContent == null)
      {
        _output.WriteLine("无法获取mes内容，跳过测试");
        return;
      }

      // 检查原始内容中是否包含目标块
      bool hasWeiboBlock = originalContent.Contains("【微博热议】") && originalContent.Contains("[由微博管理器自动生成]");
      bool hasForumBlock = originalContent.Contains("【论坛热议】") && originalContent.Contains("[由论坛管理器自动生成]");

      if (!hasWeiboBlock && !hasForumBlock)
      {
        // 如果测试数据中没有要删除的块，跳过测试
        return;
      }

      // Act - 使用完整的分析器
      var analyzer = new ChatLogAnalyzer();
      var rulesContent = File.ReadAllText(rulesFile);
      var analysisResult = await analyzer.AnalyzeAsync(originalContent, rulesContent);

      // Assert
      Assert.NotNull(analysisResult.MainBody);

      bool resultHasWeiboBlock = analysisResult.MainBody.Contains("【微博热议】") && analysisResult.MainBody.Contains("[由微博管理器自动生成]");
      bool resultHasForumBlock = analysisResult.MainBody.Contains("【论坛热议】") && analysisResult.MainBody.Contains("[由论坛管理器自动生成]");

      // 验证讨论块已被删除
      Assert.False(resultHasWeiboBlock, "微博热议块应该被删除");
      Assert.False(resultHasForumBlock, "论坛热议块应该被删除");

      // 验证内容确实被缩减了（删除了大量内容）
      Assert.True(analysisResult.MainBody.Length < originalContent.Length, "处理后的内容应该比原始内容短");
    }
  }
}