# Ant Design Blazor 组件使用指南

本文档为 AI 提供 Ant Design Blazor 组件库的使用规则和 API 指导。

## 概述

Ant Design Blazor 是基于 Ant Design 设计规范的 Blazor 企业级 UI 组件库，提供开箱即用的中后台前端/设计解决方案。

**官方文档**: https://antblazor.com/zh-CN/components/overview

## 组件分类

### 1. 通用组件 (General)
- **Button** - 按钮
- **Icon** - 图标
- **Typography** - 排版

### 2. 布局组件 (Layout)
- **Divider** - 分割线
- **Flex** - 弹性布局
- **Grid** - 栅格
- **Layout** - 布局
- **Space** - 间距

### 3. 导航组件 (Navigation)
- **Affix** - 图钉
- **Breadcrumb** - 面包屑
- **Dropdown** - 下拉菜单
- **Menu** - 导航菜单
- **PageHeader** - 页头
- **Pagination** - 分页
- **Steps** - 步骤条

### 4. 数据录入组件 (Data Entry)
- **AutoComplete** - 自动完成
- **Cascader** - 级联选择
- **Checkbox** - 多选框
- **DatePicker** - 日期选择框
- **Form** - 表单
- **Input** - 输入框
- **InputNumber** - 数字输入框
- **Mentions** - 提及
- **Radio** - 单选框
- **Rate** - 评分
- **Select** - 选择器
- **Slider** - 滑动输入条
- **Switch** - 开关
- **TimePicker** - 时间选择框
- **Transfer** - 穿梭框
- **TreeSelect** - 树选择
- **Upload** - 上传

### 5. 数据展示组件 (Data Display)
- **Avatar** - 头像
- **Badge** - 徽标数
- **Calendar** - 日历
- **Card** - 卡片
- **Carousel** - 走马灯
- **Collapse** - 折叠面板
- **Comment** - 评论
- **Descriptions** - 描述列表
- **Empty** - 空状态
- **Image** - 图片
- **List** - 列表
- **Popover** - 气泡卡片
- **Segmented** - 分段控制器
- **Statistic** - 统计数值
- **Table** - 表格
- **Tabs** - 标签页
- **Tag** - 标签
- **Timeline** - 时间轴
- **Tooltip** - 文字提示
- **Tree** - 树形控件

### 6. 反馈组件 (Feedback)
- **Alert** - 警告提示
- **Drawer** - 抽屉
- **Message** - 全局提示
- **Modal** - 对话框
- **Notification** - 通知提醒框
- **Popconfirm** - 气泡确认框
- **Progress** - 进度条
- **Result** - 结果
- **Skeleton** - 骨架屏
- **Spin** - 加载中
- **Watermark** - 水印

### 7. 其他组件 (Other)
- **Anchor** - 锚点
- **BackTop** - 回到顶部

## 核心组件使用规范

### Button 按钮

#### 何时使用
- 标记一个（或封装一组）操作命令，响应用户点击行为，触发相应的业务逻辑
- 主按钮：用于主行动点，一个操作区域只能有一个主按钮
- 默认按钮：用于没有主次之分的一组行动点
- 虚线按钮：常用于添加操作
- 链接按钮：用于次要或外链的行动点

#### 按钮状态
- **危险 (Danger)**: 删除/移动/修改权限等危险操作，一般需要二次确认
- **幽灵 (Ghost)**: 用于背景色比较复杂的地方，常用在首页/产品页等展示场景
- **禁用 (Disabled)**: 行动点不可用的时候，一般需要文案解释
- **加载中 (Loading)**: 用于异步操作等待反馈的时候，也可以避免多次提交

#### 主要 API

```razor
<Button Type="@ButtonType.Primary" 
        Size="@ButtonSize.Default"
        Danger="false"
        Ghost="false"
        Loading="false"
        AutoLoading="false"
        Disabled="false"
        Block="false"
        Shape="null"
        Icon="string"
        HtmlType="button"
        OnClick="EventCallback<MouseEventArgs>">
    按钮文本
</Button>
```

**关键属性**:
- `Type`: ButtonType (Primary, Default, Dashed, Link, Text)
- `Size`: ButtonSize (Large, Default, Small)
- `Danger`: Boolean - 危险状态
- `Ghost`: Boolean - 幽灵按钮
- `Loading`: Boolean - 加载状态（需手动控制）
- `AutoLoading`: Boolean - 自动处理加载状态直到事件回调完成
- `Disabled`: Boolean - 禁用状态
- `Block`: Boolean - 适合父宽度
- `Shape`: ButtonShape (Circle, Round, null)
- `Icon`: String - 图标组件
- `HtmlType`: String - 原始 HTML 类型 (button, submit, reset)
- `OnClick`: EventCallback - 点击回调

**DownloadButton** 特有属性:
- `Url`: String - 文件下载地址
- `FileName`: String - 文件名

### Form 表单

#### 何时使用
- 用于创建一个实体或收集信息
- 需要对输入的数据类型进行校验时

#### 表单布局
- **Horizontal** (水平): 标签和表单控件水平排列
- **Vertical** (垂直): 标签和表单控件上下垂直排列
- **Inline** (行内): 表单项水平排列

#### 主要 API

```razor
<Form Model="@model"
      Layout="@FormLayout.Horizontal"
      LabelCol="new ColLayoutParam { Span = 8 }"
      WrapperCol="new ColLayoutParam { Span = 16 }"
      Size="@FormSize.Default"
      ValidateOnChange="false"
      ValidateMode="@FormValidateMode.Complex"
      OnFinish="HandleFinish"
      OnFinishFailed="HandleFinishFailed">
    
    <FormItem Label="用户名" Name="@nameof(model.Username)">
        <Input @bind-Value="@context.Username" />
    </FormItem>
    
    <FormItem>
        <Button Type="@ButtonType.Primary" HtmlType="submit">
            提交
        </Button>
    </FormItem>
</Form>
```

**Form 关键属性**:
- `Model`: TModel - 绑定的模型
- `Layout`: FormLayout (Horizontal, Vertical, Inline)
- `LabelCol`: ColLayoutParam - 标签布局
- `WrapperCol`: ColLayoutParam - 输入控件布局
- `Size`: FormSize - 表单内组件大小
- `RequiredMark`: FormRequiredMark (Required, Optional, None)
- `LabelAlign`: AntLabelAlignType (Left, Right)
- `ValidateOnChange`: Boolean - 组件值更改时启用验证
- `ValidateMode`: FormValidateMode (Default, Rules, Complex)
- `Name`: String - 表单处理程序名称（静态 SSR 必需）
- `Method`: HttpMethod - 提交表单的 HTTP 方法
- `Loading`: Boolean - 表单加载状态
- `OnFinish`: EventCallback<EditContext> - 验证通过时的回调
- `OnFinishFailed`: EventCallback<EditContext> - 验证失败时的回调
- `OnFieldChanged`: EventCallback<FieldChangedEventArgs>
- `Validator`: RenderFragment - 自定义验证器
- `Locale`: FormLocale - 本地化选项

**Form 方法**:
- `Reset()`: void - 重置所有值
- `Submit()`: void - 提交表单
- `Validate()`: Boolean - 执行验证
- `ValidationReset()`: void - 重置验证
- `SetValidationMessages(string field, string[] errorMessages)`: void

**FormItem 关键属性**:
- `Name`: String - 表单项名称/模型属性路径
- `Label`: String - 标签文本
- `LabelTemplate`: RenderFragment - 自定义标签内容
- `LabelCol`: ColLayoutParam - 标签布局
- `WrapperCol`: ColLayoutParam - 输入控件布局
- `NoStyle`: Boolean - 无样式，用作纯字段控制
- `Required`: Boolean - 标记为必填
- `Rules`: FormValidationRule[] - 验证规则
- `HasFeedback`: Boolean - 显示验证状态图标
- `ValidateStatus`: FormValidateStatus (Success, Warning, Error, Validating)
- `Help`: String - 提示信息
- `ToolTip`: String - 帮助提示信息

#### 表单验证

**两种验证方式**:

1. **Attribute 方式** - 使用数据注解特性:
```csharp
public class LoginModel
{
    [Required(ErrorMessage = "请输入用户名")]
    [StringLength(20, MinimumLength = 3)]
    public string Username { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
```

2. **Rules 方式** - 使用 FormValidationRule:
```razor
<FormItem Label="用户名">
    <Input @bind-Value="@context.Username" />
    <FormValidationRule Required="true" 
                        Message="请输入用户名"
                        Min="3" 
                        Max="20" />
</FormItem>
```

**FormValidationRule 属性**:
- `Required`: Boolean - 必填
- `Message`: String - 错误消息
- `Len`: Decimal - 验证长度
- `Min`: Decimal - 最小长度
- `Max`: Decimal - 最大长度
- `Pattern`: String - 正则表达式
- `Range`: (Double, Double) - 值范围
- `OneOf`: Object[] - 指定可选值
- `Enum`: Type - 枚举类型
- `Type`: FormFieldType - 字段类型
- `Validator`: Func<FormValidationContext, ValidationResult> - 自定义验证函数
- `Transform`: Func<Object, Object> - 验证前转换函数
- `ValidationAttribute`: ValidationAttribute - 使用指定特性验证

#### 静态 SSR 表单

.NET 8 静态服务端渲染支持，限制如下:
- 必须设置 Form 的 `Name` 属性
- 表单组件属性绑定必须用 `Model.Field`，不能用 `@context.Field`
- Model 实例需要标注 `[SupplyParameterFromForm]` 特性
- 支持的输入组件: Input, InputPassword, TextArea, Checkbox, Radio, Upload (InputFile)

```razor
<Form Model="@LoginModel" Name="login-form">
    <FormItem Label="用户名">
        <Input @bind-Value="@LoginModel.Username" />
    </FormItem>
</Form>

@code {
    [SupplyParameterFromForm(FormName = "login-form")]
    private LoginModel LoginModel { get; set; } = new();
}
```

## 组件使用最佳实践

### 1. 命名空间引用

在 `_Imports.razor` 中添加:
```razor
@using AntDesign
```

### 2. 组件参数绑定

使用 `@bind-Value` 进行双向绑定:
```razor
<Input @bind-Value="@model.Name" />
<DatePicker @bind-Value="@model.Date" />
<Select @bind-Value="@model.Category" />
```

### 3. 事件处理

```razor
<Button OnClick="HandleClick">点击</Button>

@code {
    private void HandleClick(MouseEventArgs e)
    {
        // 处理点击事件
    }
}
```

### 4. 异步操作

使用 `AutoLoading` 自动处理加载状态:
```razor
<Button Type="@ButtonType.Primary" 
        AutoLoading="true"
        OnClick="HandleSubmitAsync">
    提交
</Button>

@code {
    private async Task HandleSubmitAsync()
    {
        await Task.Delay(2000);
        // 异步操作
    }
}
```

### 5. 表单验证模式

- **FormValidateMode.Default**: 使用模型上的数据注解特性
- **FormValidateMode.Rules**: 使用 FormItem 的 Rules 属性
- **FormValidateMode.Complex**: 同时使用两种方式

### 6. 布局响应式

使用 Grid 系统进行响应式布局:
```razor
<Row Gutter="16">
    <Col Xs="24" Sm="12" Md="8" Lg="6">
        <FormItem Label="字段1">
            <Input />
        </FormItem>
    </Col>
</Row>
```

### 7. 自定义表单控件

继承 `AntInputComponentBase<T>`:
```csharp
@inherits AntInputComponentBase<MyType>

<div>
    <Input @bind-Value="@CurrentValue.Property1" />
    <Select @bind-Value="@CurrentValue.Property2" />
</div>
```

## 常见模式

### 搜索表单
```razor
<Form Layout="@FormLayout.Inline" Model="@searchModel">
    <FormItem>
        <Input @bind-Value="@context.Keyword" Placeholder="搜索关键词" />
    </FormItem>
    <FormItem>
        <Button Type="@ButtonType.Primary" HtmlType="submit">搜索</Button>
    </FormItem>
</Form>
```

### 模态框表单
```razor
<Modal Title="新建" Visible="@visible" OnOk="HandleOk">
    <Form @ref="form" Model="@model">
        <FormItem Label="名称">
            <Input @bind-Value="@context.Name" />
        </FormItem>
    </Form>
</Modal>

@code {
    private async Task HandleOk()
    {
        if (form.Validate())
        {
            await SubmitAsync();
        }
    }
}
```

### 动态表单
```razor
<Form Model="@dynamicModel">
    @foreach (var field in fields)
    {
        <FormItem Label="@field.Label" Name="@field.Name">
            @if (field.Type == "text")
            {
                <Input @bind-Value="@dynamicModel[field.Name]" />
            }
            else if (field.Type == "number")
            {
                <InputNumber @bind-Value="@dynamicModel[field.Name]" />
            }
        </FormItem>
    }
</Form>
```

## 注意事项

1. **性能优化**: 默认关闭 `ValidateOnChange`，仅在调用 `Validate()` 时验证
2. **表单提交**: 推荐使用 `<Button HtmlType="submit">` 触发原生提交逻辑
3. **验证器**: 可设置 `Validator="null"` 禁用验证以提高性能
4. **FormItem 嵌套**: 使用 `NoStyle` 避免样式冲突
5. **复杂控件**: FormItem 内只能有一个使用 `@bind-Value` 的组件
6. **本地化**: 通过 `Locale` 统一替换验证信息模板
7. **表单修改检测**: 使用 `IForm.IsModified` 判断表单是否被修改
8. **多表单联动**: 使用 `FormProvider` 在表单间处理数据

## 其他常用组件详解

### Table 表格

用于展示行列数据。

#### 基本用法
```razor
<Table TItem="User" DataSource="@users">
    <PropertyColumn Property="c => c.Name" />
    <PropertyColumn Property="c => c.Age" />
    <ActionColumn Title="操作">
        <Button Size="@ButtonSize.Small" OnClick="() => Edit(context)">编辑</Button>
    </ActionColumn>
</Table>
```

#### 关键特性
- 支持排序、筛选、分页
- 可展开行
- 树形数据展示
- 固定列和表头
- 可编辑单元格

### Modal 对话框

模态对话框。

#### 基本用法
```razor
<Modal Title="标题"
       Visible="@visible"
       OnOk="HandleOk"
       OnCancel="HandleCancel">
    <p>对话框内容</p>
</Modal>
```

#### 服务方式调用
```csharp
@inject ModalService ModalService

await ModalService.ConfirmAsync(new ConfirmOptions
{
    Title = "确认删除？",
    Content = "删除后无法恢复",
    OnOk = async (e) => { await DeleteAsync(); }
});
```

### Message 全局提示

全局展示操作反馈信息。

```csharp
@inject IMessageService MessageService

await MessageService.Success("操作成功");
await MessageService.Error("操作失败");
await MessageService.Warning("警告信息");
await MessageService.Info("提示信息");
```

### Select 选择器

下拉选择器。

```razor
<Select @bind-Value="@selectedValue"
        DataSource="@options"
        LabelName="@nameof(Option.Label)"
        ValueName="@nameof(Option.Value)"
        Placeholder="请选择"
        AllowClear="true"
        ShowSearch="true">
</Select>
```

#### 多选模式
```razor
<Select Mode="multiple" @bind-Values="@selectedValues">
    <SelectOptions>
        @foreach (var item in options)
        {
            <SelectOption TItemValue="string" TItem="string" Value="@item.Value" Label="@item.Label" />
        }
    </SelectOptions>
</Select>
```

### DatePicker 日期选择

日期选择器。

```razor
<DatePicker @bind-Value="@date"
            Format="yyyy-MM-dd"
            Placeholder="选择日期"
            AllowClear="true" />

<RangePicker @bind-Value="@dateRange"
             Format="yyyy-MM-dd" />
```

### Upload 上传

文件上传组件。

```razor
<Upload Name="files"
        Action="/api/upload"
        OnChange="HandleChange"
        BeforeUpload="BeforeUpload">
    <Button>
        <Icon Type="upload" />
        点击上传
    </Button>
</Upload>
```

### Drawer 抽屉

屏幕边缘滑出的浮层面板。

```razor
<Drawer Title="抽屉标题"
        Visible="@visible"
        Width="720"
        Placement="right"
        OnClose="() => visible = false">
    <p>抽屉内容</p>
</Drawer>
```

### Tabs 标签页

选项卡切换组件。

```razor
<Tabs DefaultActiveKey="1" OnChange="HandleTabChange">
    <TabPane Key="1" Tab="标签1">
        <p>内容1</p>
    </TabPane>
    <TabPane Key="2" Tab="标签2">
        <p>内容2</p>
    </TabPane>
</Tabs>
```

### Menu 导航菜单

为页面和功能提供导航的菜单列表。

```razor
<Menu Mode="@MenuMode.Inline"
      Theme="@MenuTheme.Dark"
      DefaultSelectedKeys="@(new[]{"1"})">
    <MenuItem Key="1" Icon="home">
        <a href="/">首页</a>
    </MenuItem>
    <SubMenu Key="sub1" Icon="setting" Title="设置">
        <MenuItem Key="2">选项1</MenuItem>
        <MenuItem Key="3">选项2</MenuItem>
    </SubMenu>
</Menu>
```

### Steps 步骤条

引导用户按照流程完成任务的导航条。

```razor
<Steps Current="@currentStep">
    <Step Title="步骤1" Description="这是描述" />
    <Step Title="步骤2" Description="这是描述" />
    <Step Title="步骤3" Description="这是描述" />
</Steps>
```

### Notification 通知提醒框

全局展示通知提醒信息。

```csharp
@inject NotificationService NotificationService

await NotificationService.Open(new NotificationConfig
{
    Message = "通知标题",
    Description = "这是通知的详细内容",
    NotificationType = NotificationType.Success
});
```

### Spin 加载中

用于页面和区块的加载中状态。

```razor
<Spin Spinning="@loading">
    <div>内容区域</div>
</Spin>

<!-- 自定义提示 -->
<Spin Spinning="@loading" Tip="加载中...">
    <div>内容区域</div>
</Spin>
```

### Pagination 分页

采用分页的形式分隔长列表。

```razor
<Pagination Total="@total"
            Current="@current"
            PageSize="@pageSize"
            ShowSizeChanger="true"
            ShowQuickJumper="true"
            OnChange="HandlePageChange"
            OnShowSizeChange="HandleSizeChange" />
```

## 布局组件详解

### Layout 布局

协助进行页面级整体布局。

```razor
<Layout>
    <Header>
        <div class="logo" />
        <Menu Theme="@MenuTheme.Dark" Mode="@MenuMode.Horizontal">
            <MenuItem Key="1">导航1</MenuItem>
        </Menu>
    </Header>
    <Layout>
        <Sider Width="200">
            <Menu Mode="@MenuMode.Inline">
                <MenuItem Key="1">菜单1</MenuItem>
            </Menu>
        </Sider>
        <Content Style="padding: 24px; min-height: 280px;">
            @Body
        </Content>
    </Layout>
    <Footer>Footer</Footer>
</Layout>
```

### Grid 栅格

24 栅格系统。

```razor
<Row Gutter="16">
    <Col Span="6">col-6</Col>
    <Col Span="6">col-6</Col>
    <Col Span="6">col-6</Col>
    <Col Span="6">col-6</Col>
</Row>

<!-- 响应式 -->
<Row>
    <Col Xs="24" Sm="12" Md="8" Lg="6" Xl="4">
        响应式列
    </Col>
</Row>
```

### Space 间距

设置组件之间的间距。

```razor
<Space Direction="horizontal" Size="@SpaceSize.Middle">
    <SpaceItem><Button>按钮1</Button></SpaceItem>
    <SpaceItem><Button>按钮2</Button></SpaceItem>
    <SpaceItem><Button>按钮3</Button></SpaceItem>
</Space>
```

## 数据录入组件补充

### Checkbox 多选框

```razor
<!-- 单个复选框 -->
<Checkbox @bind-Checked="@checked">选项</Checkbox>

<!-- 复选框组 -->
<CheckboxGroup @bind-Value="@checkedValues" Options="@options" />
```

### Radio 单选框

```razor
<!-- 单选按钮 -->
<Radio @bind-Checked="@selected" Value="1">选项1</Radio>

<!-- 单选组 -->
<RadioGroup @bind-Value="@value">
    <Radio Value="1">选项1</Radio>
    <Radio Value="2">选项2</Radio>
</RadioGroup>
```

### Switch 开关

```razor
<Switch @bind-Checked="@enabled"
        CheckedChildren="开"
        UnCheckedChildren="关" />
```

### Slider 滑动输入条

```razor
<Slider @bind-Value="@value"
        Min="0"
        Max="100"
        Step="1"
        Marks="@marks" />
```

### Rate 评分

```razor
<Rate @bind-Value="@rating"
      AllowHalf="true"
      Count="5" />
```

### Cascader 级联选择

```razor
<Cascader @bind-Value="@selectedValue"
          Options="@cascaderOptions"
          Placeholder="请选择"
          ChangeOnSelect="true" />
```

### TreeSelect 树选择

```razor
<TreeSelect @bind-Value="@selectedValue"
            DataSource="@treeData"
            Placeholder="请选择"
            TreeCheckable="true"
            ShowSearch="true" />
```

### Transfer 穿梭框

```razor
<Transfer DataSource="@dataSource"
          @bind-TargetKeys="@targetKeys"
          Render="item => item.Title"
          ShowSearch="true" />
```

## 数据展示组件补充

### Card 卡片

```razor
<Card Title="卡片标题"
      Extra="@extraTemplate"
      Bordered="true"
      Hoverable="true">
    <Body>卡片内容</Body>
    <Actions>
        <CardAction>操作1</CardAction>
        <CardAction>操作2</CardAction>
    </Actions>
</Card>
```

### Descriptions 描述列表

```razor
<Descriptions Title="用户信息" Bordered="true">
    <DescriptionsItem Title="姓名">张三</DescriptionsItem>
    <DescriptionsItem Title="年龄">28</DescriptionsItem>
    <DescriptionsItem Title="地址" Span="2">
        浙江省杭州市西湖区
    </DescriptionsItem>
</Descriptions>
```

### Timeline 时间轴

```razor
<Timeline Mode="@TimelineMode.Left">
    <TimelineItem Color="green">创建服务 2015-09-01</TimelineItem>
    <TimelineItem Color="blue">解决问题 2015-09-02</TimelineItem>
    <TimelineItem Color="red">网络问题 2015-09-03</TimelineItem>
</Timeline>
```

### Badge 徽标数

```razor
<Badge Count="5">
    <Avatar Shape="square" Icon="user" />
</Badge>

<Badge Dot="true">
    <Icon Type="notification" />
</Badge>
```

### Avatar 头像

```razor
<Avatar Size="64" Icon="user" />
<Avatar Size="64" Src="https://example.com/avatar.jpg" />
<Avatar Size="64">USER</Avatar>

<AvatarGroup>
    <Avatar>A</Avatar>
    <Avatar>B</Avatar>
    <Avatar>C</Avatar>
</AvatarGroup>
```

### Collapse 折叠面板

```razor
<Collapse DefaultActiveKey="@(new[]{"1"})"
          Accordion="false"
          Bordered="true">
    <Panel Header="面板1" Key="1">
        <p>面板1内容</p>
    </Panel>
    <Panel Header="面板2" Key="2">
        <p>面板2内容</p>
    </Panel>
</Collapse>
```

### Carousel 走马灯

```razor
<Carousel Autoplay="true" Dots="true">
    <CarouselSlick>
        <div><h3>1</h3></div>
    </CarouselSlick>
    <CarouselSlick>
        <div><h3>2</h3></div>
    </CarouselSlick>
</Carousel>
```

### Tree 树形控件

```razor
<Tree DataSource="@treeData"
      Checkable="true"
      DefaultExpandAll="true"
      OnCheck="HandleCheck"
      OnSelect="HandleSelect">
    <TitleTemplate>
        <span>@context.DataItem.Title</span>
    </TitleTemplate>
</Tree>
```

## 反馈组件补充

### Alert 警告提示

```razor
<Alert Type="@AlertType.Success"
       Message="成功提示"
       Description="详细描述"
       Closable="true"
       ShowIcon="true" />
```

### Progress 进度条

```razor
<Progress Percent="@percent"
          Status="@ProgressStatus.Active"
          ShowInfo="true" />

<Progress Type="@ProgressType.Circle"
          Percent="@percent" />
```

### Result 结果

```razor
<Result Status="success"
        Title="操作成功"
        SubTitle="订单号: 2017182818828182881">
    <Extra>
        <Button Type="@ButtonType.Primary">返回首页</Button>
    </Extra>
</Result>
```

### Skeleton 骨架屏

```razor
<Skeleton Active="true"
          Loading="@loading"
          Paragraph="new SkeletonParagraphProps { Rows = 4 }">
    <div>实际内容</div>
</Skeleton>
```

### Popconfirm 气泡确认框

```razor
<Popconfirm Title="确定删除吗？"
            OnConfirm="HandleConfirm"
            OnCancel="HandleCancel"
            OkText="确定"
            CancelText="取消">
    <Button Danger="true">删除</Button>
</Popconfirm>
```

## 高级使用技巧

### 1. 表单动态验证

```csharp
private FormValidationRule[] GetDynamicRules()
{
    var rules = new List<FormValidationRule>();

    if (requireValidation)
    {
        rules.Add(new FormValidationRule
        {
            Required = true,
            Message = "此字段必填"
        });
    }

    return rules.ToArray();
}
```

### 2. 表格远程数据加载

```csharp
private async Task<TableData<User>> LoadData(QueryModel queryModel)
{
    var result = await HttpClient.GetFromJsonAsync<PageResult<User>>(
        $"/api/users?page={queryModel.PageIndex}&size={queryModel.PageSize}");

    return new TableData<User>
    {
        Items = result.Items,
        Total = result.Total
    };
}
```

### 3. 级联选择器数据结构

```csharp
public class CascaderOption
{
    public string Value { get; set; }
    public string Label { get; set; }
    public CascaderOption[] Children { get; set; }
}
```

### 4. 自定义主题

在 `wwwroot/index.html` 或 `_Host.cshtml` 中:
```html
<link href="_content/AntDesign/css/ant-design-blazor.css" rel="stylesheet" />
```

### 5. 国际化配置

```csharp
// Program.cs
builder.Services.AddAntDesign();
builder.Services.Configure<LocaleOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("zh-CN");
});
```

## 性能优化建议

1. **表单验证**: 默认关闭 `ValidateOnChange`，仅在提交时验证
2. **表格虚拟滚动**: 大数据量时使用虚拟滚动
3. **懒加载**: 树形控件、级联选择器使用懒加载
4. **防抖**: 搜索输入使用防抖处理
5. **分页**: 大列表使用分页而非一次性加载

## 常见问题

### Q: 如何在表单中使用自定义组件？
A: 继承 `AntInputComponentBase<T>` 并使用 `CurrentValue` 属性。

### Q: 如何实现表单的条件验证？
A: 使用动态 Rules 或自定义 Validator 函数。

### Q: Modal 中的表单如何提交？
A: 通过 `@ref` 获取 Form 引用，在 Modal 的 OnOk 中调用 `form.Submit()`。

### Q: 如何实现表格的行选择？
A: 使用 `RowSelection` 属性配置选择行为。

### Q: 如何自定义表单验证消息？
A: 通过 `FormValidationRule.Message` 或 `Form.Locale` 设置。

## 参考资源

- 官方文档: https://antblazor.com/zh-CN/components/overview
- GitHub: https://github.com/ant-design-blazor/ant-design-blazor
- Ant Design 设计规范: https://ant.design/
- Blazor 官方文档: https://docs.microsoft.com/aspnet/core/blazor/

