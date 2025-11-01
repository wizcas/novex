using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Novex.Data.Context;
using Novex.Data.Models;
using System.Globalization;
using System.Text.Json;

namespace Novex.Data.Services;

public interface IChatLogService
{
  Task<ImportResult> ImportChatLogsFromJsonlAsync(string filePath, int bookId);
  Task<List<ChatLogSummary>> GetChatLogsWithFiltersAsync(int bookId, string? nameFilter = null, int? startIndex = null, int? endIndex = null, int page = 1, int pageSize = 50);
  Task<ChatLog?> GetChatLogByIdAsync(int id);
  Task<int> GetTotalCountAsync(int bookId);
  Task<int> GetTotalCountByNameAsync(string name, int bookId);
  Task<int> GetTotalCountWithFiltersAsync(int bookId, string? nameFilter = null, int? startIndex = null, int? endIndex = null);
  Task<int?> GetPreviousChatLogIdAsync(int currentId);
  Task<int?> GetNextChatLogIdAsync(int currentId);
  Task<(int MinIndex, int MaxIndex)> GetIndexRangeAsync(int bookId);
  Task<List<string>> GetCharacterNamesAsync(int bookId);
  Task<int> GetPageNumberForChatLogAsync(int chatLogId, int pageSize = 50);
  Task<int?> GetFirstChatLogIdAsync(int bookId);
}
public class ImportResult
{
  public bool Success { get; set; }
  public int TotalLinesRead { get; set; }
  public int ValidRecordsFound { get; set; }
  public int ImportedRecords { get; set; }
  public string? ErrorMessage { get; set; }
}
public class ChatLogSummary
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public DateTime SendDate { get; set; }
  public string Preview { get; set; } = string.Empty;
  public int Index { get; set; }
  public string? Summary { get; set; }
}
public class ChatLogService : IChatLogService
{
  private readonly NovexDbContext _context;
  private readonly ILogger<ChatLogService> _logger;

  public ChatLogService(NovexDbContext context, ILogger<ChatLogService> logger)
  {
    _context = context;
    _logger = logger;
  }

  #region IChatLogService Members
  public async Task<ImportResult> ImportChatLogsFromJsonlAsync(string filePath, int bookId)
  {
    var result = new ImportResult();

    try
    {
      if (!File.Exists(filePath))
      {
        result.ErrorMessage = "文件不存在";
        _logger.LogError("导入失败: 文件不存在 - {FilePath}", filePath);
        return result;
      }

      var chatLogs = new List<ChatLog>();
      var lines = await File.ReadAllLinesAsync(filePath);
      result.TotalLinesRead = lines.Length;

      _logger.LogInformation("开始导入聊天记录，共读取 {TotalLines} 行", result.TotalLinesRead);

      var index = 0; // 从0开始的楼层序号
      foreach (var line in lines)
      {
        if (string.IsNullOrWhiteSpace(line))
          continue;

        try
        {
          SillyTavernChatRecord? record = JsonSerializer.Deserialize<SillyTavernChatRecord>(line);

          // 只处理包含 name 字段的条目
          if (record?.Name != null && !string.IsNullOrWhiteSpace(record.Name))
          {
            result.ValidRecordsFound++;

            DateTime? sendDate = ParseSendDate(record.SendDate);
            if (sendDate.HasValue)
            {
              var mes = record.Mes ?? string.Empty;
              var preview = GeneratePreview(record.Name, sendDate.Value, mes);

              var chatLog = new ChatLog {
                Name = record.Name,
                Mes = mes,
                SendDate = sendDate.Value,
                Preview = preview,
                BookId = bookId,
                Index = index
              };

              chatLogs.Add(chatLog);
            }
          }
        }
        catch (JsonException ex)
        {
          _logger.LogWarning("跳过无法解析的JSON行: {Line}, 错误: {Error}", line.Substring(0, Math.Min(line.Length, 100)), ex.Message);
        }

        index++; // 无论是否解析成功，Index都递增
      }

      // 按Index排序（保持源文件顺序）
      chatLogs = chatLogs.OrderBy(c => c.Index).ToList();
      result.ImportedRecords = chatLogs.Count;

      // 清空该书目的现有数据并插入新数据
      List<ChatLog> existingChatLogs = await _context.ChatLogs.Where(c => c.BookId == bookId).ToListAsync();
      _context.ChatLogs.RemoveRange(existingChatLogs);
      await _context.ChatLogs.AddRangeAsync(chatLogs);
      await _context.SaveChangesAsync();

      result.Success = true;
      _logger.LogInformation("导入完成: 读取 {TotalLines} 行，发现 {ValidRecords} 条有效记录，成功导入 {ImportedRecords} 条记录",
                             result.TotalLinesRead, result.ValidRecordsFound, result.ImportedRecords);

      return result;
    }
    catch (Exception ex)
    {
      result.ErrorMessage = ex.Message;
      _logger.LogError(ex, "导入聊天记录时发生错误");
      return result;
    }
  }

  public async Task<ChatLog?> GetChatLogByIdAsync(int id)
  {
    return await _context.ChatLogs.FindAsync(id);
  }

  public async Task<int> GetTotalCountAsync(int bookId)
  {
    return await _context.ChatLogs.CountAsync(c => c.BookId == bookId);
  }

  public async Task<int> GetTotalCountByNameAsync(string name, int bookId)
  {
    return await _context.ChatLogs.CountAsync(c => c.Name == name && c.BookId == bookId);
  }

  public async Task<int?> GetPreviousChatLogIdAsync(int currentId)
  {
    ChatLog? currentLog = await _context.ChatLogs.FindAsync(currentId);
    if (currentLog == null) return null;

    ChatLog? previousLog = await _context.ChatLogs
        .Where(c => c.BookId == currentLog.BookId && c.Index < currentLog.Index)
        .OrderByDescending(c => c.Index)
        .FirstOrDefaultAsync();

    return previousLog?.Id;
  }

  public async Task<int?> GetNextChatLogIdAsync(int currentId)
  {
    ChatLog? currentLog = await _context.ChatLogs.FindAsync(currentId);
    if (currentLog == null) return null;

    ChatLog? nextLog = await _context.ChatLogs
        .Where(c => c.BookId == currentLog.BookId && c.Index > currentLog.Index)
        .OrderBy(c => c.Index)
        .FirstOrDefaultAsync();

    return nextLog?.Id;
  }


  public async Task<(int MinIndex, int MaxIndex)> GetIndexRangeAsync(int bookId)
  {
    List<int> logs = await _context.ChatLogs
        .Where(c => c.BookId == bookId)
        .Select(c => c.Index)
        .ToListAsync();

    if (!logs.Any())
      return (0, 0);

    return (logs.Min(), logs.Max());
  }

  public async Task<List<ChatLogSummary>> GetChatLogsWithFiltersAsync(int bookId,
                                                                      string? nameFilter = null,
                                                                      int? startIndex = null,
                                                                      int? endIndex = null,
                                                                      int page = 1,
                                                                      int pageSize = 50)
  {
    IQueryable<ChatLog> query = _context.ChatLogs.Where(c => c.BookId == bookId);

    // 应用角色名筛选
    if (!string.IsNullOrWhiteSpace(nameFilter))
    {
      query = query.Where(c => c.Name == nameFilter);
    }

    // 应用楼层范围筛选
    if (startIndex.HasValue || endIndex.HasValue)
    {
      // 获取实际的范围边界
      (var minIdx, var maxIdx) = await GetIndexRangeAsync(bookId);

      var actualStartIndex = startIndex ?? minIdx;
      var actualEndIndex = endIndex ?? maxIdx;

      query = query.Where(c => c.Index >= actualStartIndex && c.Index <= actualEndIndex);
    }

    var skip = (page - 1) * pageSize;

    return await query
        .Include(c => c.AnalysisResult)
        .OrderBy(c => c.Index)
        .Skip(skip)
        .Take(pageSize)
        .Select(c => new ChatLogSummary {
          Id = c.Id,
          Name = c.Name,
          SendDate = c.SendDate,
          Preview = c.Preview,
          Index = c.Index,
          Summary = c.AnalysisResult != null ? c.AnalysisResult.Summary : null
        })
        .ToListAsync();
  }

  public async Task<int> GetTotalCountWithFiltersAsync(int bookId, string? nameFilter = null, int? startIndex = null, int? endIndex = null)
  {
    IQueryable<ChatLog> query = _context.ChatLogs.Where(c => c.BookId == bookId);

    // 应用角色名筛选
    if (!string.IsNullOrWhiteSpace(nameFilter))
    {
      query = query.Where(c => c.Name == nameFilter);
    }

    // 应用楼层范围筛选
    if (startIndex.HasValue || endIndex.HasValue)
    {
      // 获取实际的范围边界
      (var minIdx, var maxIdx) = await GetIndexRangeAsync(bookId);

      var actualStartIndex = startIndex ?? minIdx;
      var actualEndIndex = endIndex ?? maxIdx;

      query = query.Where(c => c.Index >= actualStartIndex && c.Index <= actualEndIndex);
    }

    return await query.CountAsync();
  }
  #endregion

  private static string GeneratePreview(string name, DateTime sendDate, string mes)
  {
    var mesPreview = mes.Length > 30 ? mes[..30] : mes;
    return $"{name} {sendDate:yyyy-MM-dd HH:mm} {mesPreview}";
  }

  private DateTime? ParseSendDate(string? sendDateString)
  {
    if (string.IsNullOrWhiteSpace(sendDateString))
      return null;

    try
    {
      // 尝试解析 "October 4, 2025 7:45pm" 格式
      // 首先处理am/pm格式
      var formats = new[] {
                "MMMM d, yyyy h:mmtt", // October 4, 2025 7:45PM
                "MMMM d, yyyy h:mm tt", // October 4, 2025 7:45 PM
                "MMMM d, yyyy HH:mm", // October 4, 2025 19:45
                "yyyy-MM-dd HH:mm:ss", // 2025-10-04 19:45:00
                "yyyy-MM-dd HH:mm", // 2025-10-04 19:45
                "MM/dd/yyyy h:mmtt", // 10/4/2025 7:45PM
                "MM/dd/yyyy h:mm tt", // 10/4/2025 7:45 PM
                "MM/dd/yyyy HH:mm" // 10/4/2025 19:45
            };

      foreach (var format in formats)
      {
        if (DateTime.TryParseExact(sendDateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
          // 假设输入时间是 UTC+8 时区，转换为 UTC 时间
          DateTime utcDate = parsedDate.AddHours(-8);
          return utcDate;
        }
      }

      // 如果上面的格式都不匹配，尝试通用解析
      if (DateTime.TryParse(sendDateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime generalParsedDate))
      {
        // 假设输入时间是 UTC+8 时区，转换为 UTC 时间
        DateTime utcDate = generalParsedDate.AddHours(-8);
        return utcDate;
      }

      _logger.LogWarning("无法解析日期格式: {DateString}", sendDateString);
      return null;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "解析日期时发生错误: {DateString}", sendDateString);
      return null;
    }
  }

  public async Task<List<string>> GetCharacterNamesAsync(int bookId)
  {
    return await _context.ChatLogs
        .Where(c => c.BookId == bookId)
        .Select(c => c.Name)
        .Distinct()
        .OrderBy(n => n)
        .ToListAsync();
  }

  public async Task<int> GetPageNumberForChatLogAsync(int chatLogId, int pageSize = 50)
  {
    var chatLog = await _context.ChatLogs.FindAsync(chatLogId);
    if (chatLog == null) return 1;

    // 计算在该书目中，有多少条记录的Index小于当前记录
    var countBefore = await _context.ChatLogs
        .CountAsync(c => c.BookId == chatLog.BookId && c.Index < chatLog.Index);

    // 计算页码（从1开始）
    return (countBefore / pageSize) + 1;
  }

  public async Task<int?> GetFirstChatLogIdAsync(int bookId)
  {
    var firstLog = await _context.ChatLogs
        .Where(c => c.BookId == bookId)
        .OrderBy(c => c.Index)
        .FirstOrDefaultAsync();

    return firstLog?.Id;
  }
}
