using System.ComponentModel.DataAnnotations;

namespace Novex.Data.Models;

public class ChatLog
{
  [Key]
  public int Id { get; set; }

  [Required]
  [MaxLength(100)]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Mes { get; set; } = string.Empty;

  [Required]
  public DateTime SendDate { get; set; }

  [Required]
  [MaxLength(200)]
  public string Preview { get; set; } = string.Empty;
}