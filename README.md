# Novex - SillyTavern 聊天记录管理系统

> 一个用于管理和浏览 SillyTavern 聊天记录的 .NET Web 应用程序

## 🌟 功能特点

- **📁 智能导入** - 从 SillyTavern 导出的 JSONL 文件中导入聊天记录
- **🔍 高效筛选** - 支持按角色名称筛选聊天记录
- **📖 分页浏览** - 大量聊天记录的分页显示，提升浏览体验
- **🔎 详情查看** - 通过 ID 查询单条记录的完整内容
- **📊 导入统计** - 详细的导入过程统计信息和日志记录
- **🕒 多格式日期** - 支持多种日期时间格式解析
- **💾 本地存储** - 使用 SQLite 数据库，快速可靠的本地数据存储

## 🏗️ 技术架构

- **后端框架**: .NET 9.0
- **前端框架**: Blazor Server
- **数据库**: SQLite + Entity Framework Core
- **编程语言**: C#

## 📂 项目结构

```
novex/
├── code/                           # 源代码目录
│   ├── Novex.sln                  # 解决方案文件
│   ├── Novex.Data/                # 数据访问层
│   │   ├── Models/                # 数据模型
│   │   │   ├── ChatLog.cs         # 聊天记录实体
│   │   │   └── SillyTavernChatRecord.cs # SillyTavern JSON 模型
│   │   ├── Context/               # 数据库上下文
│   │   │   └── NovexDbContext.cs  # EF Core 数据库上下文
│   │   └── Services/              # 业务逻辑服务
│   │       └── ChatLogService.cs  # 聊天记录服务
│   ├── Novex.Web/                 # Blazor Server Web 项目
│   │   ├── Pages/                 # 页面组件
│   │   │   ├── Index.razor        # 首页
│   │   │   ├── Import.razor       # 导入页面
│   │   │   └── ChatLogs.razor     # 聊天记录列表
│   │   └── Shared/                # 共享组件
│   ├── TestData/                  # 测试数据
│   └── sample_chat.jsonl          # 示例 JSONL 文件
├── docs/                          # 文档目录
│   └── req-spec.md               # 需求规格说明
└── README.md                     # 项目说明文档
```

## 🚀 快速开始

### 环境要求

- .NET 9.0 SDK
- Visual Studio 2022 或 VS Code

### 运行步骤

1. **克隆项目**
   ```bash
   git clone <repository-url>
   cd novex
   ```

2. **恢复依赖**
   ```bash
   cd code
   dotnet restore
   ```

3. **运行应用**
   ```bash
   cd Novex.Web
   dotnet run
   ```

4. **访问应用**
    - 打开浏览器访问: `http://localhost:5124`

## 📋 使用说明

### 导入聊天记录

1. 访问 **导入聊天记录** 页面
2. 选择 SillyTavern 导出的 JSONL 文件
3. 点击 **开始导入** 按钮
4. 查看导入统计信息

### 浏览聊天记录

1. 访问 **聊天记录** 页面
2. 使用搜索框按角色名筛选
3. 通过分页浏览所有记录
4. 点击 **查看详情** 查看完整消息内容

### 支持的日期格式

系统支持以下日期时间格式：

- `October 4, 2025 7:45pm`
- `October 4, 2025 7:45 PM`
- `October 4, 2025 19:45`
- `2025-10-04 19:45:00`
- `10/4/2025 7:45PM`
- 其他常用格式...

## 📊 数据处理

### 导入过程

1. **文件读取** - 逐行读取 JSONL 文件
2. **数据筛选** - 只处理包含 `name` 字段的记录
3. **结构化处理** - 提取以下字段：
    - `name` - 角色名称
    - `mes` - 对话内容
    - `send_date` - 发送时间（支持多格式）
    - `preview` - 预览内容（角色名 + 时间 + 前30字符）
4. **时间排序** - 按发送时间升序排列
5. **数据库存储** - 保存到 SQLite 数据库

### 查询功能

- **全量查询** - 分页获取所有聊天记录（不含完整消息）
- **角色筛选** - 按角色名称筛选记录（不含完整消息）
- **详情查询** - 通过 ID 获取单条记录的完整信息

## 🔧 开发信息

### 主要依赖

- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.EntityFrameworkCore.Design
- Microsoft.AspNetCore.Components.Forms

### 数据库表结构

**ChatLogs 表**

- `Id` - 主键，自动递增
- `Name` - 角色名称 (VARCHAR(100), NOT NULL)
- `Mes` - 消息内容 (TEXT, NOT NULL)
- `SendDate` - 发送时间 (DATETIME, NOT NULL)
- `Preview` - 预览内容 (VARCHAR(200), NOT NULL)

**索引**

- `IX_ChatLogs_Name` - 角色名称索引
- `IX_ChatLogs_SendDate` - 发送时间索引

## 📝 更新日志

### v1.0.0 (2025-10-07)

- ✅ 实现基础的 JSONL 文件导入功能
- ✅ 支持按角色筛选和分页浏览
- ✅ 添加详细的导入统计信息显示
- ✅ 支持多种日期时间格式解析
- ✅ 集成控制台日志记录
- ✅ 实现完整的 Blazor Server 前端界面

## 🤝 贡献

欢迎提交 Issue 和 Pull Request 来改进这个项目！

## 📄 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

---

🌟 **如果这个项目对你有帮助，请给它一个 Star！**