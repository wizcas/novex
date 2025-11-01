# Analyze 页面故障排除指南

## 问题描述
Analyze 页面无法打开，但没有编译错误。

## 可能的原因和解决方案

### 1. 浏览器控制台错误
**检查步骤**:
- 打开浏览器开发者工具 (F12)
- 查看 Console 标签页
- 查看 Network 标签页，检查是否有 HTTP 错误

**常见错误**:
- 404 错误：页面路由不正确
- 500 错误：服务器端异常
- CORS 错误：跨域请求问题

### 2. 应用日志
**检查步骤**:
- 查看应用输出窗口
- 查看 Visual Studio 的 Debug 输出
- 查看事件查看器中的应用程序日志

**关键日志信息**:
- "加载数据时出错" - 数据加载失败
- "V2 规则引擎分析失败" - V2 引擎问题
- 任何异常堆栈跟踪

### 3. 数据库连接
**检查步骤**:
- 确保数据库已创建
- 确保有有效的 ChatLog 记录
- 确保 BookId 和 ChatLogId 参数有效

**测试 SQL**:
```sql
SELECT * FROM ChatLogs WHERE Id = @chatLogId;
SELECT * FROM Books WHERE Id = @bookId;
```

### 4. 依赖注入问题
**检查步骤**:
- 确保 `V2RuleEngineService` 已在 Program.cs 中注册
- 确保 `IProcessorRegistry` 已正确初始化
- 确保所有处理器已注册

**验证代码** (Program.cs):
```csharp
// 应该有这些行
builder.Services.AddNovexAnalyzerV2();
builder.Services.AddScoped<V2RuleEngineService>();

// 处理器应该已注册
var registry = app.Services.GetRequiredService<IProcessorRegistry>();
registry.Register("Text.Trim", typeof(...));
// ... 其他处理器
```

### 5. Analyze.razor 注入问题
**检查步骤**:
- 确保 `V2RuleEngineService` 已注入
- 确保所有其他服务已正确注入

**验证代码** (Analyze.razor):
```csharp
@inject V2RuleEngineService V2RuleEngine
@inject IChatLogService ChatLogService
@inject IAnalysisService AnalysisService
// ... 其他注入
```

### 6. 路由问题
**检查步骤**:
- 确保 URL 格式正确：`/chatlogs/{bookId}/{chatLogId}/analyze`
- 确保 bookId 和 chatLogId 都是有效的整数
- 确保路由参数与 @page 指令匹配

**示例 URL**:
```
https://localhost:5001/chatlogs/1/1/analyze
```

## 调试步骤

### 步骤 1: 启用详细日志
在 Program.cs 中添加日志配置：
```csharp
builder.Services.AddLogging(config =>
{
    config.SetMinimumLevel(LogLevel.Debug);
    config.AddConsole();
});
```

### 步骤 2: 添加异常处理
在 Analyze.razor 的 OnInitializedAsync 中添加 try-catch：
```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        await LoadData();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"初始化错误: {ex}");
        await MessageService.ErrorAsync($"页面初始化失败: {ex.Message}");
    }
}
```

### 步骤 3: 检查 V2RuleEngineService
确保服务能够正确初始化：
```csharp
// 在 V2RuleEngineService 构造函数中添加日志
public V2RuleEngineService(IProcessorRegistry registry)
{
    Console.WriteLine($"V2RuleEngineService 初始化，Registry: {registry != null}");
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    _engine = new RuleEngine(_registry);
}
```

## 常见问题解决

### 问题：页面加载时显示空白
**解决方案**:
1. 检查浏览器控制台是否有 JavaScript 错误
2. 检查应用日志是否有异常
3. 尝试刷新页面
4. 清除浏览器缓存

### 问题：数据加载失败
**解决方案**:
1. 检查数据库连接字符串
2. 确保数据库已创建
3. 确保有有效的测试数据
4. 检查数据库权限

### 问题：V2 规则引擎执行失败
**解决方案**:
1. 检查规则书 YAML 格式是否正确
2. 确保所有处理器已注册
3. 检查处理器参数是否正确
4. 查看详细的错误消息

## 测试清单

- [ ] 应用编译成功（0 个错误）
- [ ] 应用启动成功
- [ ] 可以导航到其他页面
- [ ] 数据库连接正常
- [ ] 有有效的测试数据
- [ ] V2RuleEngineService 已注册
- [ ] 所有处理器已注册
- [ ] 浏览器控制台没有错误
- [ ] 应用日志没有异常

## 联系支持

如果问题仍未解决，请收集以下信息：
1. 完整的错误消息和堆栈跟踪
2. 浏览器控制台输出
3. 应用日志输出
4. 使用的 URL
5. 数据库中的测试数据

