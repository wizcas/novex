using System.ComponentModel.DataAnnotations;

namespace Novex.Data.Models;

public class ChatLogAnalysisRuleBook
{
  public int Id { get; set; }

  [Required]
  [StringLength(200)]
  public string Name { get; set; } = string.Empty;

  [StringLength(1000)]
  public string Description { get; set; } = string.Empty;

  [Required]
  public string Content { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.Now;

  public DateTime UpdatedAt { get; set; } = DateTime.Now;
}