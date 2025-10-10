using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Novex.Data.Context;

public class NovexDbContextFactory : IDesignTimeDbContextFactory<NovexDbContext>
{
    #region IDesignTimeDbContextFactory<NovexDbContext> Members
    // 设计时工厂方法 (用于 EF 迁移工具)
    public NovexDbContext CreateDbContext(string[] args)
    {
        // 构建配置，指向 Web 项目的 appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Novex.Web"))
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", true)
            .Build();

        return CreateDbContext(configuration);
    }
    #endregion

    // 运行时工厂方法 (用于应用程序)
    public static NovexDbContext CreateDbContext(IConfiguration configuration)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NovexDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=novex.db"; // fallback

        optionsBuilder.UseSqlite(connectionString);

        return new NovexDbContext(optionsBuilder.Options);
    }

    // 运行时配置选项方法 (用于依赖注入)
    public static void ConfigureDbContext(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=novex.db"; // fallback
        options.UseSqlite(connectionString);
    }
}
