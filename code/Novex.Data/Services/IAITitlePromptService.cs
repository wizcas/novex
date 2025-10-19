using Novex.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Novex.Data.Services
{
  public interface IAITitlePromptService
  {
    Task<List<AITitlePrompt>> GetAllPromptsAsync();
    Task<AITitlePrompt> SavePromptAsync(AITitlePrompt prompt);
    Task<bool> DeletePromptAsync(int id);
  }
}