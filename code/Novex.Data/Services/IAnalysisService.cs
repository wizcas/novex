using Novex.Data.Models;

namespace Novex.Data.Services;

public interface IAnalysisService
{
  Task<ChatLogAnalysisResult?> GetAnalysisResultAsync(int chatLogId);
  Task<ChatLogAnalysisResult> AnalyzeChatLogAsync(int chatLogId);
  Task<ChatLogAnalysisResult> SaveAnalysisResultAsync(ChatLogAnalysisResult analysisResult);
  Task<bool> DeleteAnalysisResultAsync(int chatLogId);
}