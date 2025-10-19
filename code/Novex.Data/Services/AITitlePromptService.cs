using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Novex.Data.Context;
using Novex.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Novex.Data.Services
{
  public class AITitlePromptService : IAITitlePromptService
  {
    private readonly NovexDbContext _context;
    private readonly ILogger<AITitlePromptService> _logger;

    public AITitlePromptService(NovexDbContext context, ILogger<AITitlePromptService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<List<AITitlePrompt>> GetAllPromptsAsync()
    {
      // The global query filter in DbContext handles soft deletes automatically.
      return await _context.AITitlePrompts.AsNoTracking().OrderByDescending(p => p.UpdatedAt).ToListAsync();
    }

    public async Task<AITitlePrompt> SavePromptAsync(AITitlePrompt prompt)
    {
      prompt.UpdatedAt = DateTime.UtcNow;
      if (prompt.Id == 0)
      {
        _context.AITitlePrompts.Add(prompt);
      }
      else
      {
        var existing = await _context.AITitlePrompts.FindAsync(prompt.Id);
        if (existing != null)
        {
          existing.StylePrompt = prompt.StylePrompt;
          existing.UpdatedAt = prompt.UpdatedAt;
        }
        else
        {
          _context.AITitlePrompts.Update(prompt);
        }
      }
      await _context.SaveChangesAsync();
      _logger.LogInformation("Saved AI title prompt {PromptId}", prompt.Id);
      return prompt;
    }

    public async Task<bool> DeletePromptAsync(int id)
    {
      var prompt = await _context.AITitlePrompts.FindAsync(id);
      if (prompt == null)
      {
        return false;
      }

      // Soft delete
      prompt.DeletedAt = DateTime.UtcNow;
      await _context.SaveChangesAsync();
      return true;
    }
  }
}