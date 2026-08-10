[ApiController]
[Route("api/[controller]")]
public class AIHomeController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AIHomeController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("analyze-novel")]
    [AllowAnonymous]
    public async Task<IActionResult> AnalyzeNovel([FromBody] NovelAnalysisRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.NovelTitle) || string.IsNullOrWhiteSpace(request.NovelDescription))
        {
            return BadRequest(new { message = "اكتب عنوان الرواية ووصفها أولاً." });
        }

        var geminiKey = _configuration["AIModelHome:ApiKey"];
        if (string.IsNullOrWhiteSpace(geminiKey))
        {
            return StatusCode(500, new { message = "GeminiSettings:ApiKey غير مُعرّف على السيرفر." });
        }

        var prompt = $@"
أنت ناقد أدبي عربي متخصص.
قيّم الفكرة التالية وأرجع JSON فقط بدون أي نص إضافي أو backticks.

عنوان الرواية: {request.NovelTitle}
وصف الرواية: {request.NovelDescription}

الشكل المطلوب حرفياً:
{{
  ""genre"": ""نوع الرواية باختصار"",
  ""score"": 75,
  ""strengths"": [""نقطة 1"", ""نقطة 2"", ""نقطة 3""],
  ""weaknesses"": [""نقطة 1"", ""نقطة 2"", ""نقطة 3""],
  ""suggestions"": [""اقتراح 1"", ""اقتراح 2"", ""اقتراح 3""],
  ""verdict"": ""رأي نهائي جملة أو اتنين بس""
}}
";

        var geminiRequestBody = new
        {
            contents = new[]
            {
                    new
                    {
                        parts = new[] { new { text = prompt } }
                    }
                }
        };

        var httpClient = _httpClientFactory.CreateClient();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={geminiKey}";

        var content = new StringContent(
            JsonSerializer.Serialize(geminiRequestBody),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "فشل الاتصال بخدمة Gemini.", detail = ex.Message });
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, new { message = "حدث خطأ", detail = responseBody });
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0)
            {
                return StatusCode(502, new { message = "Gemini لم يرجع أي رد." });
            }

            var parts = candidates[0].GetProperty("content").GetProperty("parts");
            var rawText = string.Concat(
                parts.EnumerateArray().Select(p => p.TryGetProperty("text", out var t) ? t.GetString() : ""));

            rawText = rawText.Replace("```json", "").Replace("```", "").Trim();

            if (string.IsNullOrWhiteSpace(rawText))
            {
                return StatusCode(502, new { message = "الرد وصل لكن بدون نص." });
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<AnalysisResultDto>(rawText, options);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "تعذّر قراءة رد Gemini.", detail = ex.Message });
        }
    }
}