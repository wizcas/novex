using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Novex.Data.Models;

public class ChatLogAnalysisResult
{
  [Key]
  public int Id { get; set; }

  [Required]
  public int ChatLogId { get; set; }

  [MaxLength(200)]
  public string? Title { get; set; }

  public string? Summary { get; set; }

  public string? MainBody { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.Now;

  public DateTime? UpdatedAt { get; set; }

  // 导航属性 - 一对一关系
  [ForeignKey("ChatLogId")]
  public virtual ChatLog ChatLog { get; set; } = null!;
}