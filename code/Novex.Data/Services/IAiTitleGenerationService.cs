using System.Collections.Generic;
using System.Threading.Tasks;

namespace Novex.Data.Services
{
  public interface IAiTitleGenerationService
  {
    Task<List<string>> GenerateTitlesAsync(string textContent, string promptTemplate);
  }
}