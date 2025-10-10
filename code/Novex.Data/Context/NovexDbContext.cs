using Microsoft.EntityFrameworkCore;
using Novex.Data.Models;

namespace Novex.Data.Context;

public class NovexDbContext : DbContext
{
  public NovexDbContext(DbContextOptions<NovexDbContext> options) : base(options)
  {
  }

  public DbSet<ChatLog> ChatLogs { get; set; }
  public DbSet<Book> Books { get; set; }
  public DbSet<ChatLogAnalysisResult> ChatLogAnalysisResults { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Book>(entity => {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
      entity.Property(e => e.CreatedDate).IsRequired();

      // 创建唯一约束确保书目名称不重复
      entity.HasIndex(e => e.Name).IsUnique();
    });

    modelBuilder.Entity<ChatLog>(entity => {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
      entity.Property(e => e.Mes).IsRequired();
      entity.Property(e => e.SendDate).IsRequired();
      entity.Property(e => e.Preview).IsRequired().HasMaxLength(200);
      entity.Property(e => e.BookId).IsRequired();
      entity.Property(e => e.Index).IsRequired();

      // 外键关系
      entity.HasOne(e => e.Book)
          .WithMany(b => b.ChatLogs)
          .HasForeignKey(e => e.BookId)
          .OnDelete(DeleteBehavior.Cascade);

      // 一对一关系 - 分析结果
      entity.HasOne(e => e.AnalysisResult)
          .WithOne(ar => ar.ChatLog)
          .HasForeignKey<ChatLogAnalysisResult>(ar => ar.ChatLogId)
          .OnDelete(DeleteBehavior.Cascade);

      // 创建索引以优化查询性能
      entity.HasIndex(e => e.Name);
      entity.HasIndex(e => e.SendDate);
      entity.HasIndex(e => e.BookId);
    });

    modelBuilder.Entity<ChatLogAnalysisResult>(entity => {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.ChatLogId).IsRequired();
      entity.Property(e => e.Title).HasMaxLength(200);
      entity.Property(e => e.Summary);
      entity.Property(e => e.MainBody);
      entity.Property(e => e.CreatedAt).IsRequired();
      entity.Property(e => e.UpdatedAt);

      // 创建唯一约束确保每个ChatLog只有一个分析结果
      entity.HasIndex(e => e.ChatLogId).IsUnique();
    });
  }
}
