using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Novex.Data.Services
{
  public interface IAiTitleGenerationService
  {
    Task<List<string>> GenerateTitlesAsync(string textContent, string promptTemplate = "", CancellationToken cancellationToken = default);
  }
}