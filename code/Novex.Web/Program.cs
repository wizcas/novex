using Novex.Data.Context;
using Novex.Data.Services;
using Syncfusion.Blazor;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 添加数据库上下文 (使用工厂方法)
builder.Services.AddDbContext<NovexDbContext>(options =>
                                                  NovexDbContextFactory.ConfigureDbContext(options, builder.Configuration));

// 添加 AntDesign 服务
builder.Services.AddAntDesign();

// 添加 Syncfusion 服务
builder.Services.AddSyncfusionBlazor();

// 添加服务
builder.Services.AddScoped<IChatLogService, ChatLogService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<AnalysisRuleBookService>();
builder.Services.AddScoped<IBookChapterService, BookChapterService>();

WebApplication app = builder.Build();

// 确保数据库创建
using (IServiceScope scope = app.Services.CreateScope())
{
    NovexDbContext context = scope.ServiceProvider.GetRequiredService<NovexDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
