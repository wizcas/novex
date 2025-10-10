using System.ComponentModel.DataAnnotations;

namespace Novex.Data.Models;

public class Book
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // 导航属性
    public virtual ICollection<ChatLog> ChatLogs { get; set; } = new List<ChatLog>();
}
