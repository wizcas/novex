using Novex.Data.Models;

namespace Novex.Data.Services;

public interface IBookChapterService
{
  Task<List<BookChapter>> GetChaptersByBookIdAsync(int bookId);
  Task<BookChapter?> GetChapterByIdAsync(int id);
  Task<BookChapter> CreateChapterAsync(int bookId, string title, int order);
  Task<BookChapter> UpdateChapterAsync(BookChapter chapter);
  Task<bool> DeleteChapterAsync(int id);
  Task<(bool Success, List<BookChapter> UpdatedChapters)> ReorderChaptersAsync(int bookId, List<int> chapterIds);
  Task<int> GetNextOrderAsync(int bookId);
}