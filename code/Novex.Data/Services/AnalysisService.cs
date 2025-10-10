using Microsoft.EntityFrameworkCore;
using Novex.Data.Context;
using Novex.Data.Models;

namespace Novex.Data.Services;

public class AnalysisService : IAnalysisService
{
  private readonly NovexDbContext _context;

  public AnalysisService(NovexDbContext context)
  {
    _context = context;
  }

  public async Task<ChatLogAnalysisResult?> GetAnalysisResultAsync(int chatLogId)
  {
    return await _context.ChatLogAnalysisResults
        .FirstOrDefaultAsync(ar => ar.ChatLogId == chatLogId);
  }

  public async Task<ChatLogAnalysisResult> AnalyzeChatLogAsync(int chatLogId)
  {
    var chatLog = await _context.ChatLogs
        .FirstOrDefaultAsync(cl => cl.Id == chatLogId);

    if (chatLog == null)
    {
      throw new ArgumentException($"ChatLog with ID {chatLogId} not found.");
    }

    // 检查是否已有分析结果
    var existingResult = await GetAnalysisResultAsync(chatLogId);
    if (existingResult != null)
    {
      return existingResult;
    }

    // 创建新的分析结果
    var analysisResult = new ChatLogAnalysisResult {
      ChatLogId = chatLogId,
      Title = GenerateTitle(chatLog.Mes),
      Summary = GenerateSummary(chatLog.Mes),
      MainBody = GenerateMainBody(chatLog.Mes),
      CreatedAt = DateTime.Now
    };

    _context.ChatLogAnalysisResults.Add(analysisResult);
    await _context.SaveChangesAsync();

    return analysisResult;
  }

  public async Task<ChatLogAnalysisResult> SaveAnalysisResultAsync(ChatLogAnalysisResult analysisResult)
  {
    var existing = await GetAnalysisResultAsync(analysisResult.ChatLogId);

    if (existing != null)
    {
      // 更新现有记录
      existing.Title = analysisResult.Title;
      existing.Summary = analysisResult.Summary;
      existing.MainBody = analysisResult.MainBody;
      existing.UpdatedAt = DateTime.Now;

      _context.Update(existing);
      await _context.SaveChangesAsync();
      return existing;
    }
    else
    {
      // 创建新记录
      analysisResult.CreatedAt = DateTime.Now;
      _context.ChatLogAnalysisResults.Add(analysisResult);
      await _context.SaveChangesAsync();
      return analysisResult;
    }
  }

  public async Task<bool> DeleteAnalysisResultAsync(int chatLogId)
  {
    var analysisResult = await GetAnalysisResultAsync(chatLogId);
    if (analysisResult != null)
    {
      _context.ChatLogAnalysisResults.Remove(analysisResult);
      await _context.SaveChangesAsync();
      return true;
    }
    return false;
  }

  private string GenerateTitle(string message)
  {
    if (string.IsNullOrWhiteSpace(message))
      return "未分析的对话";

    // 简单的标题生成逻辑 - 取前30个字符
    var title = message.Length > 30 ? message.Substring(0, 30) + "..." : message;
    return title.Replace("\n", " ").Replace("\r", " ").Trim();
  }

  private string GenerateSummary(string message)
  {
    if (string.IsNullOrWhiteSpace(message))
      return "暂无摘要";

    // 简单的摘要生成逻辑 - 取前100个字符作为摘要
    var lines = message.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    var summary = string.Join(" ", lines.Take(3));

    if (summary.Length > 100)
    {
      summary = summary.Substring(0, 100) + "...";
    }

    return summary.Trim();
  }

  private string GenerateMainBody(string message)
  {
    if (string.IsNullOrWhiteSpace(message))
      return "暂无主体内容";

    // 对于主体内容，我们进行格式化处理
    var formatted = message.Replace("\r\n", "\n").Replace("\r", "\n");

    // 简单的段落分割和整理
    var paragraphs = formatted.Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(p => p.Trim())
        .Where(p => !string.IsNullOrEmpty(p))
        .ToList();

    if (paragraphs.Count == 0)
      return "暂无主体内容";

    // 将段落重新组织，每个段落之间用双换行符分隔
    return string.Join("\n\n", paragraphs);
  }
}