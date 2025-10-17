using System.ComponentModel.DataAnnotations;

namespace Novex.Data.Models
{
  /// <summary>
  /// Represents the settings for the Large Language Model (LLM) API.
  /// </summary>
  public class LLMSetting
  {
    [Key]
    public int Id { get; set; }

    public string? ApiUrl { get; set; }

    public string? ApiKey { get; set; }

    public string? ModelName { get; set; }
  }
}