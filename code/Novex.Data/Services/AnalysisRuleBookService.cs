using Microsoft.EntityFrameworkCore;
using Novex.Data.Context;
using Novex.Data.Models;

namespace Novex.Data.Services;

/// <summary>
/// 分析规则书服务 - 纯数据库操作
/// </summary>
public class AnalysisRuleBookService
{
  private readonly NovexDbContext _context;

  public AnalysisRuleBookService(NovexDbContext context)
  {
    _context = context;
  }

  /// <summary>
  /// 获取所有规则书
  /// </summary>
  public async Task<List<ChatLogAnalysisRuleBook>> GetAllRuleBooksAsync()
  {
    return await _context.ChatLogAnalysisRuleBooks
        .OrderBy(r => r.Name)
        .ToListAsync();
  }

  /// <summary>
  /// 根据ID获取规则书
  /// </summary>
  public async Task<ChatLogAnalysisRuleBook?> GetRuleBookByIdAsync(int id)
  {
    return await _context.ChatLogAnalysisRuleBooks
        .FirstOrDefaultAsync(r => r.Id == id);
  }

  /// <summary>
  /// 根据名称获取规则书
  /// </summary>
  public async Task<ChatLogAnalysisRuleBook?> GetRuleBookByNameAsync(string name)
  {
    return await _context.ChatLogAnalysisRuleBooks
        .FirstOrDefaultAsync(r => r.Name == name);
  }

  /// <summary>
  /// 创建新规则书
  /// </summary>
  public async Task<ChatLogAnalysisRuleBook> CreateRuleBookAsync(ChatLogAnalysisRuleBook ruleBook)
  {
    // 检查名称是否已存在
    var existing = await GetRuleBookByNameAsync(ruleBook.Name);
    if (existing != null)
    {
      throw new InvalidOperationException($"规则书名称 '{ruleBook.Name}' 已存在");
    }

    ruleBook.CreatedAt = DateTime.Now;
    ruleBook.UpdatedAt = DateTime.Now;

    _context.ChatLogAnalysisRuleBooks.Add(ruleBook);
    await _context.SaveChangesAsync();

    return ruleBook;
  }

  /// <summary>
  /// 更新规则书
  /// </summary>
  public async Task<ChatLogAnalysisRuleBook> UpdateRuleBookAsync(ChatLogAnalysisRuleBook ruleBook)
  {
    var existing = await GetRuleBookByIdAsync(ruleBook.Id);
    if (existing == null)
    {
      throw new InvalidOperationException($"规则书 ID {ruleBook.Id} 不存在");
    }

    // 检查名称是否与其他规则书冲突
    var nameConflict = await _context.ChatLogAnalysisRuleBooks
        .FirstOrDefaultAsync(r => r.Name == ruleBook.Name && r.Id != ruleBook.Id);
    if (nameConflict != null)
    {
      throw new InvalidOperationException($"规则书名称 '{ruleBook.Name}' 已被其他规则书使用");
    }

    existing.Name = ruleBook.Name;
    existing.Description = ruleBook.Description;
    existing.Content = ruleBook.Content;
    existing.UpdatedAt = DateTime.Now;

    await _context.SaveChangesAsync();

    return existing;
  }

  /// <summary>
  /// 删除规则书
  /// </summary>
  public async Task<bool> DeleteRuleBookAsync(int id)
  {
    var ruleBook = await GetRuleBookByIdAsync(id);
    if (ruleBook == null)
    {
      return false;
    }

    _context.ChatLogAnalysisRuleBooks.Remove(ruleBook);
    await _context.SaveChangesAsync();

    return true;
  }

  /// <summary>
  /// 搜索规则书
  /// </summary>
  public async Task<List<ChatLogAnalysisRuleBook>> SearchRuleBooksAsync(string searchTerm)
  {
    if (string.IsNullOrWhiteSpace(searchTerm))
    {
      return await GetAllRuleBooksAsync();
    }

    return await _context.ChatLogAnalysisRuleBooks
        .Where(r => r.Name.Contains(searchTerm) || r.Description.Contains(searchTerm))
        .OrderBy(r => r.Name)
        .ToListAsync();
  }

  /// <summary>
  /// 创建默认规则书
  /// </summary>
  public async Task<ChatLogAnalysisRuleBook> CreateDefaultRuleBookAsync()
  {
    var defaultContent = @"version: '1.0'
description: '默认分析规则 - 简单提取标题、摘要和正文'

extraction_rules:
- id: 'extract_title'
  name: '提取标题'
  matcher_type: 'Text'
  pattern: ''
  action: 'Extract'
  target: 'Title'
  priority: 10
  enabled: true

transformation_rules:
- id: 'generate_simple_title'
  name: '生成简单标题'
  source_field: 'source'
  target_field: 'title'
  transformation_type: 'Custom'
  parameters:
    max_length: 50
  priority: 100
  enabled: true

ai_generation_rule:
  enabled: false
";

    var defaultRuleBook = new ChatLogAnalysisRuleBook {
      Name = "默认规则",
      Description = "系统默认的分析规则，提供基本的标题、摘要和正文提取功能",
      Content = defaultContent
    };

    return await CreateRuleBookAsync(defaultRuleBook);
  }
}