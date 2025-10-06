using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Novex.Data.Context;
using Novex.Data.Models;

namespace Novex.Data.Services;

public interface IChatLogService
{
  Task<ImportResult> ImportChatLogsFromJsonlAsync(string filePath);
  Task<List<ChatLogSummary>> GetChatLogsByNameAsync(string name, int page = 1, int pageSize = 20);
  Task<List<ChatLogSummary>> GetAllChatLogsAsync(int page = 1, int pageSize = 20);
  Task<ChatLog?> GetChatLogByIdAsync(int id);
  Task<int> GetTotalCountAsync();
  Task<int> GetTotalCountByNameAsync(string name);
  Task<int?> GetPreviousChatLogIdAsync(int currentId);
  Task<int?> GetNextChatLogIdAsync(int currentId);
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

  public async Task<ImportResult> ImportChatLogsFromJsonlAsync(string filePath)
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

      foreach (var line in lines)
      {
        if (string.IsNullOrWhiteSpace(line))
          continue;

        try
        {
          var record = JsonSerializer.Deserialize<SillyTavernChatRecord>(line);

          // 只处理包含 name 字段的条目
          if (record?.Name != null && !string.IsNullOrWhiteSpace(record.Name))
          {
            result.ValidRecordsFound++;

            var sendDate = ParseSendDate(record.SendDate);
            if (sendDate.HasValue)
            {
              var mes = record.Mes ?? string.Empty;
              var preview = GeneratePreview(record.Name, sendDate.Value, mes);

              var chatLog = new ChatLog
              {
                Name = record.Name,
                Mes = mes,
                SendDate = sendDate.Value,
                Preview = preview
              };

              chatLogs.Add(chatLog);
            }
          }
        }
        catch (JsonException ex)
        {
          _logger.LogWarning("跳过无法解析的JSON行: {Line}, 错误: {Error}", line.Substring(0, Math.Min(line.Length, 100)), ex.Message);
          continue;
        }
      }

      // 按发送时间排序
      chatLogs = chatLogs.OrderBy(c => c.SendDate).ToList();
      result.ImportedRecords = chatLogs.Count;

      // 清空现有数据并插入新数据
      _context.ChatLogs.RemoveRange(_context.ChatLogs);
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

  public async Task<List<ChatLogSummary>> GetChatLogsByNameAsync(string name, int page = 1, int pageSize = 20)
  {
    var skip = (page - 1) * pageSize;

    return await _context.ChatLogs
        .Where(c => c.Name == name)
        .OrderBy(c => c.SendDate)
        .Skip(skip)
        .Take(pageSize)
        .Select(c => new ChatLogSummary
        {
          Id = c.Id,
          Name = c.Name,
          SendDate = c.SendDate,
          Preview = c.Preview
        })
        .ToListAsync();
  }

  public async Task<List<ChatLogSummary>> GetAllChatLogsAsync(int page = 1, int pageSize = 20)
  {
    var skip = (page - 1) * pageSize;

    return await _context.ChatLogs
        .OrderBy(c => c.SendDate)
        .Skip(skip)
        .Take(pageSize)
        .Select(c => new ChatLogSummary
        {
          Id = c.Id,
          Name = c.Name,
          SendDate = c.SendDate,
          Preview = c.Preview
        })
        .ToListAsync();
  }

  public async Task<ChatLog?> GetChatLogByIdAsync(int id)
  {
    return await _context.ChatLogs.FindAsync(id);
  }

  public async Task<int> GetTotalCountAsync()
  {
    return await _context.ChatLogs.CountAsync();
  }

  public async Task<int> GetTotalCountByNameAsync(string name)
  {
    return await _context.ChatLogs.CountAsync(c => c.Name == name);
  }

  public async Task<int?> GetPreviousChatLogIdAsync(int currentId)
  {
    var currentLog = await _context.ChatLogs.FindAsync(currentId);
    if (currentLog == null) return null;

    var previousLog = await _context.ChatLogs
        .Where(c => c.SendDate < currentLog.SendDate || (c.SendDate == currentLog.SendDate && c.Id < currentId))
        .OrderByDescending(c => c.SendDate)
        .ThenByDescending(c => c.Id)
        .FirstOrDefaultAsync();

    return previousLog?.Id;
  }

  public async Task<int?> GetNextChatLogIdAsync(int currentId)
  {
    var currentLog = await _context.ChatLogs.FindAsync(currentId);
    if (currentLog == null) return null;

    var nextLog = await _context.ChatLogs
        .Where(c => c.SendDate > currentLog.SendDate || (c.SendDate == currentLog.SendDate && c.Id > currentId))
        .OrderBy(c => c.SendDate)
        .ThenBy(c => c.Id)
        .FirstOrDefaultAsync();

    return nextLog?.Id;
  }

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
      var formats = new[]
      {
        "MMMM d, yyyy h:mmtt",     // October 4, 2025 7:45PM
        "MMMM d, yyyy h:mm tt",    // October 4, 2025 7:45 PM  
        "MMMM d, yyyy HH:mm",      // October 4, 2025 19:45
        "yyyy-MM-dd HH:mm:ss",     // 2025-10-04 19:45:00
        "yyyy-MM-dd HH:mm",        // 2025-10-04 19:45
        "MM/dd/yyyy h:mmtt",       // 10/4/2025 7:45PM
        "MM/dd/yyyy h:mm tt",      // 10/4/2025 7:45 PM
        "MM/dd/yyyy HH:mm"         // 10/4/2025 19:45
      };

      foreach (var format in formats)
      {
        if (DateTime.TryParseExact(sendDateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
          // 假设输入时间是 UTC+8 时区，转换为 UTC 时间
          var utcDate = parsedDate.AddHours(-8);
          return utcDate;
        }
      }

      // 如果上面的格式都不匹配，尝试通用解析
      if (DateTime.TryParse(sendDateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var generalParsedDate))
      {
        // 假设输入时间是 UTC+8 时区，转换为 UTC 时间
        var utcDate = generalParsedDate.AddHours(-8);
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
}