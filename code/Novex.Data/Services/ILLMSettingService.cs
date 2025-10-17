using Novex.Data.Models;

namespace Novex.Data.Services
{
  public interface ILLMSettingService
  {
    Task<LLMSetting?> GetFirstSettingAsync();
    Task<LLMSetting> SaveSettingAsync(LLMSetting setting);
  }
}