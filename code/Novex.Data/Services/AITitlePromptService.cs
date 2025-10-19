using Microsoft.EntityFrameworkCore;
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

    public AITitlePromptService(NovexDbContext context)
    {
      _context = context;
    }

    public async Task<List<AITitlePrompt>> GetAllPromptsAsync()
    {
      // The global query filter in DbContext handles soft deletes automatically.
      return await _context.AITitlePrompts.OrderByDescending(p => p.UpdatedAt).ToListAsync();
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
        _context.AITitlePrompts.Update(prompt);
      }
      await _context.SaveChangesAsync();
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