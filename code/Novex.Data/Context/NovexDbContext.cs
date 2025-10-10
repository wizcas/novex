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

            // 外键关系
            entity.HasOne(e => e.Book)
                .WithMany(b => b.ChatLogs)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // 创建索引以优化查询性能
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.SendDate);
            entity.HasIndex(e => e.BookId);
        });
    }
}
