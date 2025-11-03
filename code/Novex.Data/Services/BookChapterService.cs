using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Novex.Data.Context;
using Novex.Data.Models;

namespace Novex.Data.Services;

public class BookChapterService : IBookChapterService
{
  private readonly NovexDbContext _context;
  private readonly ILogger<BookChapterService> _logger;

  public BookChapterService(NovexDbContext context, ILogger<BookChapterService> logger)
  {
    _context = context;
    _logger = logger;
  }

  public async Task<List<BookChapter>> GetChaptersByBookIdAsync(int bookId)
  {
    return await _context.BookChapters
        .Where(c => c.BookId == bookId)
        .OrderBy(c => c.Order)
        .ToListAsync();
  }

  public async Task<BookChapter?> GetChapterByIdAsync(int id)
  {
    return await _context.BookChapters.FindAsync(id);
  }

  public async Task<BookChapter> CreateChapterAsync(int bookId, string title, int order)
  {
    var chapter = new BookChapter {
      BookId = bookId,
      Title = title,
      Content = string.Empty,
      Order = order,
      CreatedAt = DateTime.Now
    };

    _context.BookChapters.Add(chapter);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Created chapter {ChapterId} for book {BookId}", chapter.Id, bookId);
    return chapter;
  }

  public async Task<BookChapter> UpdateChapterAsync(BookChapter chapter)
  {
    chapter.UpdatedAt = DateTime.Now;
    _context.BookChapters.Update(chapter);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Updated chapter {ChapterId}", chapter.Id);
    return chapter;
  }

  public async Task<bool> DeleteChapterAsync(int id)
  {
    var chapter = await _context.BookChapters.FindAsync(id);
    if (chapter == null)
    {
      return false;
    }

    _context.BookChapters.Remove(chapter);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Deleted chapter {ChapterId}", id);
    return true;
  }



  public async Task<(bool Success, List<BookChapter> UpdatedChapters)> ReorderChaptersAsync(int bookId, List<int> chapterIds)
  {
    try
    {
      // 验证所有章节都属于同一本书
      var existingCount = await _context.BookChapters
          .Where(c => c.BookId == bookId && chapterIds.Contains(c.Id))
          .CountAsync();

      if (existingCount != chapterIds.Count)
      {
        _logger.LogWarning("Reorder failed: not all chapters belong to book {BookId}", bookId);
        return (false, new List<BookChapter>());
      }

      // 开启数据库事务
      using (var transaction = await _context.Database.BeginTransactionAsync())
      {
        try
        {
          var now = DateTime.Now;

          // 第一步：将所有相关章节的 Order 设置为负数（临时值）
          // 这避免了唯一索引冲突
          var placeholders = string.Join(",", chapterIds.Select((_, i) => $"@id{i}"));
          var sql1 = $"UPDATE BookChapters SET \"Order\" = -\"Order\", UpdatedAt = @now WHERE BookId = @bookId AND Id IN ({placeholders})";

          var parameters1 = new List<Microsoft.Data.Sqlite.SqliteParameter>();
          parameters1.Add(new Microsoft.Data.Sqlite.SqliteParameter("@now", now));
          parameters1.Add(new Microsoft.Data.Sqlite.SqliteParameter("@bookId", bookId));

          for (int i = 0; i < chapterIds.Count; i++)
          {
            parameters1.Add(new Microsoft.Data.Sqlite.SqliteParameter($"@id{i}", chapterIds[i]));
          }

          #pragma warning disable EF1002
          await _context.Database.ExecuteSqlRawAsync(sql1, parameters1.ToArray());
          #pragma warning restore EF1002

          // 第二步：按照新的顺序设置正确的 Order 值
          // 使用 CASE 语句在单个 SQL 查询中完成所有更新
          var caseWhenClauses = new List<string>();
          var caseParameters = new List<Microsoft.Data.Sqlite.SqliteParameter>();
          caseParameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("@now", now));
          caseParameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("@bookId", bookId));

          for (int i = 0; i < chapterIds.Count; i++)
          {
            var paramName = $"@id{i}";
            var orderParamName = $"@order{i}";
            caseWhenClauses.Add($"WHEN {paramName} THEN {orderParamName}");
            caseParameters.Add(new Microsoft.Data.Sqlite.SqliteParameter(paramName, chapterIds[i]));
            caseParameters.Add(new Microsoft.Data.Sqlite.SqliteParameter(orderParamName, i + 1));
          }

          var caseStatement = string.Join(" ", caseWhenClauses);
          var placeholders2 = string.Join(",", chapterIds.Select((_, i) => $"@id{i}"));
          var sql2 = $"UPDATE BookChapters SET \"Order\" = CASE Id {caseStatement} END, UpdatedAt = @now WHERE BookId = @bookId AND Id IN ({placeholders2})";

          #pragma warning disable EF1002
          await _context.Database.ExecuteSqlRawAsync(sql2, caseParameters.ToArray());
          #pragma warning restore EF1002

          // 提交事务
          await transaction.CommitAsync();

          // 清除 EF Core 的变更跟踪，确保后续操作使用最新的数据库状态
          _context.ChangeTracker.Clear();

          // 重新加载更新后的章节数据
          var updatedChapters = await _context.BookChapters
              .Where(c => c.BookId == bookId && chapterIds.Contains(c.Id))
              .OrderBy(c => c.Order)
              .ToListAsync();

          _logger.LogInformation("Successfully reordered {ChapterCount} chapters for book {BookId}", chapterIds.Count, bookId);
          return (true, updatedChapters);
        }
        catch (Exception ex)
        {
          // 异常时自动回滚
          await transaction.RollbackAsync();
          _logger.LogError(ex, "Error during chapter reordering for book {BookId}", bookId);
          throw;
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to reorder chapters for book {BookId}", bookId);
      return (false, new List<BookChapter>());
    }
  }

  public async Task<int> GetNextOrderAsync(int bookId)
  {
    var maxOrder = await _context.BookChapters
        .Where(c => c.BookId == bookId)
        .MaxAsync(c => (int?)c.Order);

    return (maxOrder ?? 0) + 1;
  }
}