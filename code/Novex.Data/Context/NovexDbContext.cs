using Microsoft.EntityFrameworkCore;
using Novex.Data.Models;

namespace Novex.Data.Context;

public class NovexDbContext : DbContext
{
  public NovexDbContext(DbContextOptions<NovexDbContext> options) : base(options)
  {
  }

  public DbSet<ChatLog> ChatLogs { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<ChatLog>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
      entity.Property(e => e.Mes).IsRequired();
      entity.Property(e => e.SendDate).IsRequired();
      entity.Property(e => e.Preview).IsRequired().HasMaxLength(200);

      // 创建索引以优化查询性能
      entity.HasIndex(e => e.Name);
      entity.HasIndex(e => e.SendDate);
    });
  }
}