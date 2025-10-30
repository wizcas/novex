using Novex.Analyzer.Processors;

namespace Novex.Analyzer.Tests;

public class FixUnclosedTagsProcessorTests
{
    private readonly FixUnclosedTagsProcessor _processor = new();

    [Fact]
    public async Task ProcessAsync_WithProperlyNestedTags_ReturnsSameString()
    {
        var input = "<root><a><b>some text</b></a></root>";
        var result = await _processor.ProcessAsync(input, new Dictionary<string, object>());
        Assert.Equal(input, result);
    }

    [Fact]
    public async Task ProcessAsync_WithSingleUnclosedTag_ClosesTagAtEnd()
    {
        var input = "<p>This is an unclosed paragraph.";
        var result = await _processor.ProcessAsync(input, new Dictionary<string, object>());
        Assert.Equal("<p>This is an unclosed paragraph.</p>", result);
    }

    [Fact]
    public async Task ProcessAsync_WithNestedUnclosedTags_ClosesTagsInCorrectOrder()
    {
        var input = "<div><p><span>text";
        var result = await _processor.ProcessAsync(input, new Dictionary<string, object>());
        Assert.Equal("<div><p><span>text</span></p></div>", result);
    }

    [Fact]
    public async Task ProcessAsync_WithMixedClosedAndUnclosedTags_ClosesOnlyUnclosed()
    {
        // Arrange
        var input = "<main><section>Content";

        // Act
        var result = await _processor.ProcessAsync(input, new Dictionary<string, object>());

        // Assert
        Assert.Equal("<main><section>Content</section></main>", result);
    }

    [Fact]
    public async Task ProcessAsync_WithSelfClosingTags_IgnoresThem()
    {
        var input = "<p>Here is a line break <br/> and an image <img src=\"test.jpg\"/>.";
        var result = await _processor.ProcessAsync(input, new Dictionary<string, object>());
        Assert.Equal("<p>Here is a line break <br/> and an image <img src=\"test.jpg\"/>.</p>", result);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyString_ReturnsEmptyString()
    {
        var input = "";
        var result = await _processor.ProcessAsync(input, new Dictionary<string, object>());
        Assert.Equal("", result);
    }

    [Fact]
    public async Task ProcessAsync_WithStringWithNoTags_ReturnsSameString()
    {
        var input = "This is just plain text without any tags.";
        var result = await _processor.ProcessAsync(input, new Dictionary<string, object>());
        Assert.Equal(input, result);
    }

    [Fact]
    public async Task ProcessAsync_WithMismatchedClosingTag_CorrectlyHandlesNesting()
    {
        // Arrange
        var input = "<a><b>text</c></a>"; // </c> is incorrect, <a> closes <b> implicitly

        // Act
        var result = await _processor.ProcessAsync(input, new Dictionary<string, object>());

        // Assert
        Assert.Equal(input, result);
    }

    [Fact]
    public async Task ProcessAsync_WithUnclosedTagBeforeClosedTag_CorrectlyNests()
    {
        var input = "<p>This is <span>first part.<div>Second part</div>";
        var result = await _processor.ProcessAsync(input, new Dictionary<string, object>());
        Assert.Equal("<p>This is <span>first part.<div>Second part</div></span></p>", result);
    }

    // --- Tests for OnlyForEndingTags = true ---

    [Fact]
    public async Task ProcessAsync_OnlyForEndingTags_WithOrphanedClosingTag_PrependsOpeningTag()
    {
        var input = "some text</think>";
        var parameters = new Dictionary<string, object> { { "OnlyForEndingTags", true } };
        var result = await _processor.ProcessAsync(input, parameters);
        Assert.Equal("<think>some text</think>", result);
    }

    [Fact]
    public async Task ProcessAsync_OnlyForEndingTags_WithMultipleOrphanedClosingTags_PrependsInCorrectOrder()
    {
        var input = "some text</b></a>";
        var parameters = new Dictionary<string, object> { { "OnlyForEndingTags", true } };
        var result = await _processor.ProcessAsync(input, parameters);
        Assert.Equal("<a><b>some text</b></a>", result);
    }

    [Fact]
    public async Task ProcessAsync_OnlyForEndingTags_WithUnclosedOpeningTag_DoesNothing()
    {
        var input = "<p>This should not be closed";
        var parameters = new Dictionary<string, object> { { "OnlyForEndingTags", true } };
        var result = await _processor.ProcessAsync(input, parameters);
        Assert.Equal(input, result);
    }

    [Fact]
    public async Task ProcessAsync_OnlyForEndingTags_WithMixedTags_PrependsForOrphanedClosingTag()
    {
        var input = "<p>some text</p> more text</section>";
        var parameters = new Dictionary<string, object> { { "OnlyForEndingTags", true } };
        var result = await _processor.ProcessAsync(input, parameters);
        Assert.Equal("<section><p>some text</p> more text</section>", result);
    }

    [Fact]
    public async Task ProcessAsync_OnlyForEndingTags_WithNoTags_ReturnsSameString()
    {
        var input = "Plain text.";
        var parameters = new Dictionary<string, object> { { "OnlyForEndingTags", true } };
        var result = await _processor.ProcessAsync(input, parameters);
        Assert.Equal(input, result);
    }
    [Fact]
    public async Task ProcessAsync_OnlyForEndingTags_WithUserProvidedText_PrependsCorrectly()
    {
        // Arrange
        var input = @"- 当前在什么情景中？时间：晚上七点后地点：林晨的车内，电影院附近人物位置：顾云坐在后排座椅上，陈晨在驾驶座。林晨去买奶茶了，随时可能回来。- 参考<possible_mistakes>，可能在创作时回避哪些问题，如何解决？1.  **回避冲突描写**：不能回避。根据用户的输入“一会电影院里有你哭的时候”，暗示了接下来会有更激烈的对话场景。我需要将车内的对话升级，并为电影院内的情节做铺垫。2.  **延缓角色的决策**：不能延缓。顾云已经主动开始对话，并且用言语和动作挑逗。我扮演的陈晨也给出了明确的回应，表示享受对话，并预告了接下来的行动。剧情需要立刻推进，不能犹豫。3.  **降级冲突**：不能降级。用户的输入明确指向了更刺激的场景，我不能用前戏或非插入式冲突来敷衍。车内的互动虽然受限于环境，但必须为电影院内的正式冲突做好铺垫，甚至可以在车内进行一些突破性的行为。4.  **女性不是冲突描写的重心**：必须以顾云为重心。她的反应、她的挑逗、她的身体变化，都是描写的核心。即使是我在主导，最终的落点也应该是她如何感受和回应这种刺激。- 极简要解读<interactive_input>，不做多余揣测，<interactive_input>中传递了怎样的需求？1.  **接受对话设定**：明确表示享受“对话”的快感，而不是拒绝或犹豫。2.  **主导权反转**：从被动挑逗转为主动掌控，用“一会电影院里有你哭的时候”来预告接下来的行动，暗示自己会采取更激烈、更具支配性的行为。3.  **场景转换预告**：将主要的对话场景从车内转移到了电影院，车内是前戏和铺垫。4.  **动作配合**：一边说着威胁性的话，一边“看着她”，显示出言行一致的侵略性。- 如何让情节、人物反应真实可信，而不是从抽象典例发展而来？顾云的情绪是追求刺激、不受束缚的“享乐主义者”，她对林晨只是“上班”，对我则是“玩具”。因此，当我表现出比她更强的掌控欲和更“强势”的玩法时，她不会害怕或退缩，反而会更加兴奋，觉得找到了同类，激发了她的好胜心和好胜心。她的反应应该是被激起了更大的兴趣，而不是被吓到。她会用更出格的言语和行动来回应我的“挑战”。- 传统文学创作中剧情应当怎样？但作为互动式小说，剧情又应该怎样才能符合互动式小क़的设定（基本符合主线或暗线）？传统文学可能会花更多笔墨在心理斗争、道德谴责上。但作为互动式小说，核心是“互动”和“体验”。剧情应该快速推进，聚焦于感官刺激和角色间的权力博弈。主线是“我”和顾云之间背着林晨的禁忌关系。暗线是顾云情绪的探索，她不仅仅是表面甜美，更有一种挑战规则、寻求极致刺激的内在驱动力。现在的剧情发展完全符合这个设定，将对话的刺激感不断升级。- 最后确认文字风格，回顾<fiction_style>以及<Writing_style>，该怎么创作？1.  **白描为主**：直接叙述动作和对话，避免华丽的修辞。例如，直接写“我看着她”，而不是“我那只不安分的手掌在她浑圆的臀部上游走”。2.  **聚焦表层互动**：重点写顾云的反应，她的动作，她说的话，而不是她的心理活动。3.  **强主观视角**：用“我”的口吻讲述，仿佛是回忆。对话直接呈现，不用“她说”。4.  **克制用词**：避免使用过于夸张的词汇，保持一种娓娓道来的讲述感，即使是在描述激烈的场景。5.  **冲突描写**：遵循<dialogue_guide>，注重动作、反应、感官刺激的交替。车内空间狭小，可以利用这个特点来设计动作，增加紧迫感和刺激感。重点要放在顾云的生理和心理反应上。</think>";
        var expected = "<think>" + input;
        var parameters = new Dictionary<string, object> { { "OnlyForEndingTags", true } };

        // Act
        var result = await _processor.ProcessAsync(input, parameters);

        // Assert
        Assert.Equal(expected, result);
    }
}
