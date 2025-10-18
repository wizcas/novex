using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Novex.Data.Services
{
  public class AiTitleGenerationService : IAiTitleGenerationService
  {
    private readonly ILLMSettingService _llmSettingService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AiTitleGenerationService> _logger;

    public AiTitleGenerationService(ILLMSettingService llmSettingService, IHttpClientFactory httpClientFactory, ILogger<AiTitleGenerationService> logger)
    {
      _llmSettingService = llmSettingService;
      _httpClientFactory = httpClientFactory;
      _logger = logger;
    }

    public async Task<List<string>> GenerateTitlesAsync(string textContent, string promptTemplate)
    {
      var llmSetting = await _llmSettingService.GetFirstSettingAsync();
      if (llmSetting == null || string.IsNullOrWhiteSpace(llmSetting.ApiUrl) || string.IsNullOrWhiteSpace(llmSetting.ApiKey) || string.IsNullOrWhiteSpace(llmSetting.ModelName))
      {
        throw new InvalidOperationException("请先在设置页面配置完整的 LLM API 信息。");
      }

      var httpClient = _httpClientFactory.CreateClient();
      var prompt = promptTemplate.Replace("{textContent}", textContent);

      object requestPayload;
      var isGoogleApi = llmSetting.ApiUrl.Contains("googleapis.com");

      if (isGoogleApi)
      {
        requestPayload = new {
          contents = new[] { new { parts = new[] { new { text = prompt } } } },
          generationConfig = new { temperature = 0.7, maxOutputTokens = 3000 }
        };
      }
      else
      {
        requestPayload = new {
          model = llmSetting.ModelName,
          messages = new[] { new { role = "user", content = prompt } },
          temperature = 0.7,
          max_tokens = 3000
        };
      }

      var jsonPayload = JsonSerializer.Serialize(requestPayload);
      var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

      string requestUrl;
      if (isGoogleApi)
      {
        requestUrl = $"{llmSetting.ApiUrl.TrimEnd('/')}/v1beta/{llmSetting.ModelName}:generateContent?key={llmSetting.ApiKey}";
      }
      else
      {
        requestUrl = $"{llmSetting.ApiUrl.TrimEnd('/')}/v1/chat/completions";
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", llmSetting.ApiKey);
      }

      var response = await httpClient.PostAsync(requestUrl, content);

      if (!response.IsSuccessStatusCode)
      {
        var error = await response.Content.ReadAsStringAsync();
        _logger.LogError("AI title generation failed. Status: {StatusCode}, Reason: {ReasonPhrase}, Body: {ErrorBody}", response.StatusCode, response.ReasonPhrase, error);
        throw new HttpRequestException($"AI 生成失败: {response.ReasonPhrase} - {error}");
      }

      var responseString = await response.Content.ReadAsStringAsync();
      _logger.LogInformation("AI Response: {Response}", responseString);
      string? generatedText;

      if (isGoogleApi)
      {
        var aiResponse = JsonSerializer.Deserialize<GoogleAiResponse>(responseString);
        generatedText = aiResponse?.GetContent();
      }
      else
      {
        var aiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
        generatedText = aiResponse?.GetContent();
      }

      if (string.IsNullOrWhiteSpace(generatedText))
      {
        return new List<string>();
      }

      return generatedText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(t => t.Trim().TrimStart('-', ' ', '1', '2', '3', '.').Trim())
                          .Where(t => !string.IsNullOrWhiteSpace(t))
                          .ToList();
    }

    #region AI Response DTOs
    private class OpenAiResponse
    {
      [JsonPropertyName("choices")]
      public List<Choice> Choices { get; set; } = new();
      public string? GetContent() => Choices?.FirstOrDefault()?.Message?.Content;
    }

    private class Choice
    {
      [JsonPropertyName("message")]
      public Message Message { get; set; } = new();
    }

    private class Message
    {
      [JsonPropertyName("content")]
      public string Content { get; set; } = string.Empty;
    }

    private class GoogleAiResponse
    {
      [JsonPropertyName("candidates")]
      public List<GoogleCandidate> Candidates { get; set; } = new();
      public string? GetContent() => Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
    }

    private class GoogleCandidate
    {
      [JsonPropertyName("content")]
      public GoogleContent Content { get; set; } = new();
    }

    private class GoogleContent
    {
      [JsonPropertyName("parts")]
      public List<GooglePart> Parts { get; set; } = new();
    }

    private class GooglePart
    {
      [JsonPropertyName("text")]
      public string Text { get; set; } = string.Empty;
    }
    #endregion
  }
}