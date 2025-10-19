using System;
using System.ComponentModel.DataAnnotations;

namespace Novex.Data.Models
{
  /// <summary>
  /// Represents the prompt settings for AI title generation.
  /// </summary>
  public class AITitlePrompt
  {
    [Key]
    public int Id { get; set; }
    public string StylePrompt { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    override public string ToString()
    {
      return $"[{Id}] {StylePrompt} (UpdatedAt: {UpdatedAt}, DeletedAt: {DeletedAt})";
    }
  }
}