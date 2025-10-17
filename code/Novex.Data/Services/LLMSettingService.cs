using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Novex.Data.Context;
using Novex.Data.Models;

namespace Novex.Data.Services
{
  public class LLMSettingService : ILLMSettingService
  {
    private readonly NovexDbContext _context;
    private readonly ILogger<LLMSettingService> _logger;

    public LLMSettingService(NovexDbContext context, ILogger<LLMSettingService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<LLMSetting?> GetFirstSettingAsync()
    {
      return await _context.LLMSettings.FirstOrDefaultAsync();
    }

    public async Task<LLMSetting> SaveSettingAsync(LLMSetting setting)
    {
      var existingSetting = await _context.LLMSettings.FirstOrDefaultAsync();
      if (existingSetting != null)
      {
        existingSetting.ApiUrl = setting.ApiUrl;
        existingSetting.ApiKey = setting.ApiKey;
        existingSetting.ModelName = setting.ModelName;
        _context.LLMSettings.Update(existingSetting);
      }
      else
      {
        _context.LLMSettings.Add(setting);
      }
      await _context.SaveChangesAsync();
      _logger.LogInformation("LLM settings saved.");
      return setting;
    }
  }
}