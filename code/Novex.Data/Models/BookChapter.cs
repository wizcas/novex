using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Novex.Data.Models;

public class BookChapter
{
  [Key]
  public int Id { get; set; }

  [Required]
  public int BookId { get; set; }

  [Required]
  [MaxLength(200)]
  public string Title { get; set; } = string.Empty;

  public string Content { get; set; } = string.Empty;

  [Required]
  public int Order { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.Now;

  public DateTime? UpdatedAt { get; set; }

  // 导航属性
  [ForeignKey("BookId")]
  public virtual Book Book { get; set; } = null!;
}