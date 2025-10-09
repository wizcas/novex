<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

- [x] Verify that the copilot-instructions.md file in the .github directory is created. ✅ Created

- [ ] Clarify Project Requirements
<!-- Ask for project type, language, and frameworks if not specified. Skip if already provided. -->

- [ ] Scaffold the Project
<!--
Ensure that the previous step has been marked as completed.
Call project setup tool with projectType parameter.
Run scaffolding command to create project files and folders.
Use '.' as the working directory.
If no appropriate projectType is available, search documentation using available tools.
Otherwise, create the project structure manually using available file creation tools.
-->

- [ ] Customize the Project
<!--
Verify that all previous steps have been completed successfully and you have marked the step as completed.
Develop a plan to modify codebase according to user requirements.
Apply modifications using appropriate tools and user-provided references.
Skip this step for "Hello World" projects.
-->

- [ ] Install Required Extensions
<!-- ONLY install extensions provided mentioned in the get_project_setup_info. Skip this step otherwise and mark as completed. -->

- [ ] Compile the Project
<!--
Verify that all previous steps have been completed.
Install any missing dependencies.
Run diagnostics and resolve any issues.
Check for markdown files in project folder for relevant instructions on how to do this.
-->

- [ ] Create and Run Task
<!--
Verify that all previous steps have been completed.
Check https://code.visualstudio.com/docs/debugtest/tasks to determine if the project needs a task. If so, use the create_and_run_task to create and launch a task based on package.json, README.md, and project structure.
Skip this step otherwise.
 -->

- [ ] Launch the Project
<!--
Verify that all previous steps have been completed.
Prompt user for debug mode, launch only if confirmed.
 -->

- [ ] Ensure Documentation is Complete
<!--
Verify that all previous steps have been completed.
Verify that README.md and the copilot-instructions.md file in the .github directory exists and contains current project information.
Clean up the copilot-instructions.md file in the .github directory by removing all HTML comments.
 -->

---

# 必须严格遵守的规范

- 代码注释使用英文
- 变量、函数命名使用英文
- 所有文档都用中文作为主要语言
- 代码风格遵循 C# 规范
- 不允许删除 docs 目录下的任何文件
- 当需要生成临时数据、文件时，必须放在项目根目录下的 temp 目录中

## 主题系统规范

**✅ 主题切换功能**

- 应用支持亮色、暗色和跟随系统三种主题模式
- 主题切换按钮位于导航菜单底部
- 所有组件自动适应主题变化
- 初次加载页面时，根据系统主题自动设置应用主题
- 用户选择主题后，将其作为个人偏好保存在用户的本地存储中。之后每次加载页面时，优先使用用户的偏好设置。

**❌ 禁止硬编码颜色**

- 不允许在样式中使用 `#ffffff`、`#000000` 等硬编码颜色
- 不允许使用 `background: white` 等硬编码背景色

# 开发规范相关

## 环境

- 使用 Powershell 作为主要的命令行工具

## Powershell 下的常用命令

- 使用 `rg` 代替 `grep` 进行代码或输出搜索, 若没有 rg, 则使用 `Select-String`.
- 使用 `ls` 代替 `dir` 列出目录内容
- 使用 `Remove-Item` 代替 `rm` 删除文件或目录

## TailwindCSS 代码规范

- 优先使用`/opacity数字`来设置透明度，例如 `bg-black/50`
- 尽量避免使用 `bg-opacity-数字` 这种写法

## FluentUI 代码规范

- 确保不允许 `<FluentCard>` 组件嵌套
- 确保不使用 `<FluentLabel>` 组件来显示标题或正文文本，改用标准 HTML 标签如 `<h1>`, `<p>` 等，然后通过 Tailwind CSS 类来设置样式
- 确保不使用 `style` 属性来设置样式，改用 Tailwind CSS 类

## 开发环境下 dotnet 调试命令

- 使用 `--project` 参数来指定项目目录，因为解决方案目录下有多个项目。
- 优先通过 `dotnet watch run` 来启动项目，以启用热重载功能。
- 如果 `dotnet build` 或 `dotnet run` 时发现 Novex.Web 是通过 `dotnet watch run` 运行的，则跳过。

## LucidIcon 相关 (暂时停用)

- 当使用到 wwwroot/icons 目录下缺少的图标 SVG 时，使用 `Novex.Web` 根目录下的 `manage-icons.ps1` 脚本，来从 lucid npm 中提取所需图标。
