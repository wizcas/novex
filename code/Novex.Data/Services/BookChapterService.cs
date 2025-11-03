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



  public async Task<int> GetNextOrderAsync(int bookId)
  {
    var maxOrder = await _context.BookChapters
        .Where(c => c.BookId == bookId)
        .MaxAsync(c => (int?)c.Order);

    return (maxOrder ?? 0) + 1;
  }
}