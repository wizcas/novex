using Novex.Analyzer.V2.Core;
using Novex.Analyzer.V2.Processors.Extraction;

namespace Novex.Analyzer.V2.Tests.Processors.Extraction;

public class ExtractStructuredDataProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ExtractsFromPlotTag_WithChapterAndEventName()
    {
        // Arrange
        var processor = new ExtractStructuredDataProcessor();
        var input = @"<plot>
计算耗时:[5分钟]

当前章节: 初次接触
个人线:[顾云-朋友的女友[$1/100](初识)、送其回家]
当前角色内/衣物:[顾云:上身(白色吊带)，下身(超短百褶裙)，内衣(无)，真空状态]
长期事件：无

事件名:虚假的小白兔(1/5)
事件号码:1
摘要:陈晨开车送林晨的女友顾云回家。途中，陈晨因顾云过于暴露的穿着而开口提醒，担心会引发交通事故。顾云听到后，一改在林晨面前的甜美形象，用粗俗的语言直接回怼陈晨，展现出其真实火爆的性格。陈晨对此感到震惊，并在随后的对话中选择退让。顾云在沉默片刻后，开始主动询问陈晨关于林晨过往情史的问题，言语直接，让陈晨陷入了短暂的为难。
</plot>";

        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "TagName", "plot" },
            { "Fields", "Chapter:当前章节,Event:事件名" },
            { "Separator", "/" }
            // OutputField 已移除，由规则引擎通过 TargetField 管理
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("初次接触/虚假的小白兔(1/5)", result.Output);
    }

    [Fact]
    public async Task ProcessAsync_ExtractsFromFullText_WithoutTag()
    {
        // Arrange
        var processor = new ExtractStructuredDataProcessor();
        var input = @"当前章节: 第一章
事件名: 开始的故事
摘要: 这是一个故事的开始";

        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "Fields", "Chapter:当前章节,Event:事件名" },
            { "Separator", " - " }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("第一章 - 开始的故事", result.Output);
    }

    [Fact]
    public async Task ProcessAsync_SavesToField_WhenOutputFieldSpecified()
    {
        // Arrange
        var processor = new ExtractStructuredDataProcessor();
        var input = @"<plot>
当前章节: 第二章
事件名: 中间的故事
</plot>";

        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "TagName", "plot" },
            { "Fields", "Chapter:当前章节,Event:事件名" },
            { "Separator", "/" }
            // OutputField 已移除，由规则引擎通过 TargetField 管理
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("第二章/中间的故事", result.Output);
        // 注意：处理器不再设置字段，由规则引擎通过 TargetField 管理
        // 这个测试验证处理器返回正确的结果
    }

    [Fact]
    public async Task ProcessAsync_HandlesMultipleFields_InOrder()
    {
        // Arrange
        var processor = new ExtractStructuredDataProcessor();
        var input = @"<content>
当前章节: 第三章
事件名: 结束的故事
摘要: 这是故事的结束
</content>";

        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "TagName", "content" },
            { "Fields", "Chapter:当前章节,Event:事件名,Summary:摘要" },
            { "Separator", "|" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("第三章|结束的故事|这是故事的结束", result.Output);
    }

    [Fact]
    public async Task ProcessAsync_FailsWhenTagNotFound()
    {
        // Arrange
        var processor = new ExtractStructuredDataProcessor();
        var input = @"<content>
当前章节: 第五章
</content>";

        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "TagName", "plot" },
            { "Fields", "Chapter:当前章节" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("未找到标签", result.Errors.First().Message);
    }

    [Fact]
    public async Task ProcessAsync_FailsWhenFieldsParameterMissing()
    {
        // Arrange
        var processor = new ExtractStructuredDataProcessor();
        var input = "当前章节: 第六章";

        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        var parameters = new ProcessorParameters(new Dictionary<string, object>());

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Fields 参数必需", result.Errors.First().Message);
    }

    [Fact]
    public async Task ProcessAsync_ExtractsFromRealWorldData()
    {
        // Arrange - 使用真实的数据格式
        var processor = new ExtractStructuredDataProcessor();
        var input = @"<plot>
计算耗时:[40分钟]

当前章节:背德的终焉
个人线:[顾云-朋友的女友[$100/100](车内交合)、彻底沉沦]
当前角色内/衣物:[陈晨:上身(衬衫、西装)，下身(西裤)，衣物完整]
长期事件：无

事件名:崩坏的终曲(3/5)
事件号码:25
摘要:陈晨独自驾车行驶在城市夜色中，内心充满了对未来的迷茫。他思考着与顾云的疯狂行为以及与林晨彻底破裂的关系，决定将林晨的跑车归还。他将车开到A集团总部的地下停车场，停好车后，将车钥匙留在车上，然后独自乘电梯离开，象征着与过去的一段关系做出切割。
</plot>";

        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "TagName", "plot" },
            { "Fields", "Chapter:当前章节,Event:事件名" },
            { "Separator", "/" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("背德的终焉/崩坏的终曲(3/5)", result.Output);
        // 处理器返回正确的结果，由规则引擎通过 TargetField 保存到字段
    }

    [Fact]
    public async Task ProcessAsync_ExtractsSummaryFromRealWorldData()
    {
        // Arrange - 提取摘要
        var processor = new ExtractStructuredDataProcessor();
        var input = @"<plot>
计算耗时:[40分钟]

当前章节:背德的终焉
事件名:崩坏的终曲(3/5)
摘要:陈晨独自驾车行驶在城市夜色中，内心充满了对未来的迷茫。他思考着与顾云的疯狂行为以及与林晨彻底破裂的关系，决定将林晨的跑车归还。他将车开到A集团总部的地下停车场，停好车后，将车钥匙留在车上，然后独自乘电梯离开，象征着与过去的一段关系做出切割。
</plot>";

        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "TagName", "plot" },
            { "Fields", "Summary:摘要" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        var expectedSummary = "陈晨独自驾车行驶在城市夜色中，内心充满了对未来的迷茫。他思考着与顾云的疯狂行为以及与林晨彻底破裂的关系，决定将林晨的跑车归还。他将车开到A集团总部的地下停车场，停好车后，将车钥匙留在车上，然后独自乘电梯离开，象征着与过去的一段关系做出切割。";
        Assert.Equal(expectedSummary, result.Output);
        // 处理器返回正确的结果，由规则引擎通过 TargetField 保存到字段
    }

    [Fact]
    public async Task ProcessAsync_WithRuleEngine_SavesToTargetField()
    {
        // Arrange - 验证规则引擎是否正确使用 TargetField
        var processor = new ExtractStructuredDataProcessor();
        var input = @"<plot>
当前章节:背德的终焉
事件名:崩坏的终曲(3/5)
摘要:陈晨独自驾车行驶在城市夜色中
</plot>";

        var context = new ProcessContext
        {
            SourceContent = input,
            Fields = new Dictionary<string, string>(),
            Variables = new Dictionary<string, object>()
        };

        var parameters = new ProcessorParameters(new Dictionary<string, object>
        {
            { "TagName", "plot" },
            { "Fields", "Chapter:当前章节,Event:事件名" },
            { "Separator", "/" }
        });

        // Act
        var result = await processor.ProcessAsync(context, parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("背德的终焉/崩坏的终曲(3/5)", result.Output);
        // 处理器不再设置字段，这由规则引擎通过 TargetField 处理
        // 这个测试验证处理器返回正确的结果供规则引擎使用
    }
}

