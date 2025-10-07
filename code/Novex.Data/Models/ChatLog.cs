using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

  [Required]
  public int BookId { get; set; }

  // 导航属性
  [ForeignKey("BookId")]
  public virtual Book Book { get; set; } = null!;
}