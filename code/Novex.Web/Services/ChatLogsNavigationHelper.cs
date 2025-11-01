using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.RegularExpressions;

namespace Novex.Web.Services;

/// <summary>
/// 辅助服务，用于管理 ChatLogs 相关页面的导航和 URL 构建
/// 自动处理 returnUrl 参数的传递，确保从 ChatLogs 进入的所有子页面都能正确返回
/// </summary>
public class ChatLogsNavigationHelper
{
  private readonly NavigationManager _navigationManager;

  public ChatLogsNavigationHelper(NavigationManager navigationManager)
  {
    _navigationManager = navigationManager;
  }

  /// <summary>
  /// 从当前 URL 获取 returnUrl 参数
  /// 如果当前页面没有 returnUrl，则尝试构建一个默认的返回 URL
  /// </summary>
  public string GetReturnUrl()
  {
    var uri = new Uri(_navigationManager.Uri);
    var query = QueryHelpers.ParseQuery(uri.Query);

    if (query.TryGetValue("returnUrl", out var returnUrl) && !string.IsNullOrEmpty(returnUrl))
    {
      return returnUrl.ToString();
    }

    // 如果没有 returnUrl 参数，尝试从当前 URL 提取
    return ExtractChatLogsBaseUrl(uri);
  }

  /// <summary>
  /// 导航到指定 URL，自动携带当前的 returnUrl 参数
  /// 这是核心方法，所有子页面间的导航都应该使用这个方法
  /// </summary>
  /// <param name="targetUrl">目标 URL（不包含 returnUrl 参数）</param>
  public void NavigateWithReturnUrl(string targetUrl)
  {
    var returnUrl = GetReturnUrl();
    var finalUrl = AppendReturnUrl(targetUrl, returnUrl);
    _navigationManager.NavigateTo(finalUrl);
  }

  /// <summary>
  /// 返回到 ChatLogs 页面
  /// </summary>
  public void NavigateBack()
  {
    var returnUrl = GetReturnUrl();
    _navigationManager.NavigateTo(returnUrl);
  }

  /// <summary>
  /// 为 ChatLogs 页面构建导航 URL，用于从 ChatLogs 进入子页面时调用
  /// </summary>
  /// <param name="targetUrl">目标 URL（不包含 returnUrl 参数）</param>
  /// <param name="chatLogsUrl">ChatLogs 页面的完整 URL（包含所有过滤参数）</param>
  public string BuildUrlWithReturn(string targetUrl, string chatLogsUrl)
  {
    return AppendReturnUrl(targetUrl, chatLogsUrl);
  }

  /// <summary>
  /// 构建面包屑链接 URL，自动携带当前的 returnUrl 参数
  /// 用于子页面的面包屑导航
  /// </summary>
  /// <param name="targetUrl">目标 URL（不包含 returnUrl 参数）</param>
  public string GetBreadcrumbUrl(string targetUrl)
  {
    var returnUrl = GetReturnUrl();
    return AppendReturnUrl(targetUrl, returnUrl);
  }

  /// <summary>
  /// 从当前 URL 中提取 bookId
  /// </summary>
  public int? ExtractBookId()
  {
    var uri = new Uri(_navigationManager.Uri);
    var match = Regex.Match(uri.AbsolutePath, @"/chatlogs/(\d+)");
    if (match.Success && int.TryParse(match.Groups[1].Value, out var bookId))
    {
      return bookId;
    }
    return null;
  }

  /// <summary>
  /// 从当前 URL 中提取 chatLogId
  /// </summary>
  public int? ExtractChatLogId()
  {
    var uri = new Uri(_navigationManager.Uri);
    var match = Regex.Match(uri.AbsolutePath, @"/chatlogs/\d+/(\d+)");
    if (match.Success && int.TryParse(match.Groups[1].Value, out var chatLogId))
    {
      return chatLogId;
    }
    return null;
  }

  /// <summary>
  /// 检查当前 URL 是否包含 returnUrl 参数
  /// </summary>
  public bool HasReturnUrl()
  {
    var uri = new Uri(_navigationManager.Uri);
    var query = QueryHelpers.ParseQuery(uri.Query);
    return query.TryGetValue("returnUrl", out var value) && !string.IsNullOrEmpty(value);
  }

  /// <summary>
  /// 将 returnUrl 参数附加到目标 URL
  /// </summary>
  private string AppendReturnUrl(string targetUrl, string returnUrl)
  {
    if (string.IsNullOrEmpty(returnUrl))
    {
      return targetUrl;
    }

    var separator = targetUrl.Contains('?') ? "&" : "?";
    return $"{targetUrl}{separator}returnUrl={Uri.EscapeDataString(returnUrl)}";
  }

  /// <summary>
  /// 从子页面 URL 中提取 ChatLogs 的基础 URL
  /// </summary>
  private string ExtractChatLogsBaseUrl(Uri uri)
  {
    // 从子页面 URL 中提取 bookId，构建基础 ChatLogs URL
    var match = Regex.Match(uri.AbsolutePath, @"/chatlogs/(\d+)");
    if (match.Success)
    {
      return $"/chatlogs/{match.Groups[1].Value}";
    }
    return "/books";
  }
}

