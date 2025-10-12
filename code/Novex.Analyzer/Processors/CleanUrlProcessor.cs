using System.Text.Json;

namespace Novex.Analyzer.Processors;

/// <summary>
/// URL 清理处理器
/// </summary>
public class CleanUrlProcessor : ITransformationProcessor
{
  public Task<string> ProcessAsync(string input, Dictionary<string, object> parameters)
  {
    var result = input?.Trim() ?? "";

    if (string.IsNullOrEmpty(result))
      return Task.FromResult(result);

    // 获取要清理的参数列表
    if (parameters.TryGetValue("params_to_remove", out var paramsObj))
    {
      var paramsToRemove = new List<string>();

      if (paramsObj is JsonElement element && element.ValueKind == JsonValueKind.Array)
      {
        foreach (var item in element.EnumerateArray())
        {
          if (item.ValueKind == JsonValueKind.String)
          {
            paramsToRemove.Add(item.GetString() ?? "");
          }
        }
      }

      // 清理URL参数
      if (paramsToRemove.Any() && Uri.TryCreate(result, UriKind.Absolute, out var uri))
      {
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        foreach (var param in paramsToRemove)
        {
          query.Remove(param);
        }

        var uriBuilder = new UriBuilder(uri) {
          Query = query.ToString()
        };

        result = uriBuilder.ToString();
      }
    }

    return Task.FromResult(result);
  }
}