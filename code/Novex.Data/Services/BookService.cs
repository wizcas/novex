using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Novex.Data.Context;
using Novex.Data.Models;

namespace Novex.Data.Services;

public interface IBookService
{
  Task<List<Book>> GetAllBooksAsync();
  Task<List<Book>> SearchBooksAsync(string searchTerm);
  Task<Book?> GetBookByIdAsync(int id);
  Task<Book?> GetBookByNameAsync(string name);
  Task<Book> CreateBookAsync(string name);
  Task<bool> DeleteBookAsync(int id);
  Task<int> ClearBookChatLogsAsync(int bookId);
}

public class BookService : IBookService
{
  private readonly NovexDbContext _context;
  private readonly ILogger<BookService> _logger;

  public BookService(NovexDbContext context, ILogger<BookService> logger)
  {
    _context = context;
    _logger = logger;
  }

  public async Task<List<Book>> GetAllBooksAsync()
  {
    return await _context.Books
        .OrderBy(b => b.Name)
        .ToListAsync();
  }

  public async Task<List<Book>> SearchBooksAsync(string searchTerm)
  {
    if (string.IsNullOrWhiteSpace(searchTerm))
      return await GetAllBooksAsync();

    return await _context.Books
        .Where(b => b.Name.Contains(searchTerm))
        .OrderBy(b => b.Name)
        .ToListAsync();
  }

  public async Task<Book?> GetBookByIdAsync(int id)
  {
    return await _context.Books.FindAsync(id);
  }

  public async Task<Book?> GetBookByNameAsync(string name)
  {
    return await _context.Books
        .FirstOrDefaultAsync(b => b.Name == name);
  }

  public async Task<Book> CreateBookAsync(string name)
  {
    var existingBook = await GetBookByNameAsync(name);
    if (existingBook != null)
    {
      return existingBook;
    }

    var book = new Book
    {
      Name = name,
      CreatedDate = DateTime.UtcNow
    };

    _context.Books.Add(book);
    await _context.SaveChangesAsync();

    _logger.LogInformation("创建新书目: {BookName} (ID: {BookId})", book.Name, book.Id);
    return book;
  }

  public async Task<bool> DeleteBookAsync(int id)
  {
    var book = await _context.Books.FindAsync(id);
    if (book == null)
      return false;

    _context.Books.Remove(book);
    await _context.SaveChangesAsync();

    _logger.LogInformation("删除书目: {BookName} (ID: {BookId})", book.Name, book.Id);
    return true;
  }

  public async Task<int> ClearBookChatLogsAsync(int bookId)
  {
    var chatLogs = await _context.ChatLogs
        .Where(c => c.BookId == bookId)
        .ToListAsync();

    var count = chatLogs.Count;
    _context.ChatLogs.RemoveRange(chatLogs);
    await _context.SaveChangesAsync();

    _logger.LogInformation("清空书目 {BookId} 的聊天记录，共删除 {Count} 条记录", bookId, count);
    return count;
  }
}