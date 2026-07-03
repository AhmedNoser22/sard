namespace Sard.Infrastructure.Services
{
    public class AiService(IOptions<GeminiSettings> options, HttpClient httpClient) : IAiService
    {
        private readonly GeminiSettings _settings = options.Value;

        public async Task<Result<string>> CorrectTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                return Result<string>.Failure("مفتاح Gemini API مش موجود في الإعدادات (Ai:ApiKey أو Gemini:ApiKey)");

            if (string.IsNullOrWhiteSpace(_settings.Model))
                return Result<string>.Failure("اسم الموديل (Gemini:Model) مش موجود في الإعدادات");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = $"أنت مصحح لغوي متخصص في اللغة العربية. صحح الأخطاء النحوية والإملائية في النص التالي فقط، دون تغيير الأسلوب أو المعنى. أرجع النص المصحح فقط بدون أي تعليق:\n\n{text}"
                            }
                        }
                    }
                }
            };

            HttpResponseMessage response;
            try
            {
                response = await httpClient.PostAsJsonAsync(url, body);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"تعذر الاتصال بـ Gemini: {ex.Message}");
            }

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var googleMessage = TryExtractGoogleError(raw) ?? raw;
                return Result<string>.Failure($"Gemini رجع خطأ ({(int)response.StatusCode}): {googleMessage}");
            }

            try
            {
                using var doc = JsonDocument.Parse(raw);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                    return Result<string>.Failure("Gemini رجع رد من غير نتيجة (يمكن الفلتر الأمني رفض النص)");

                var corrected = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return string.IsNullOrEmpty(corrected)
                    ? Result<string>.Failure("لم يتم الحصول على نتيجة")
                    : Result<string>.Success(corrected);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"شكل الرد من Gemini غير متوقع: {ex.Message}");
            }
        }

        private static string? TryExtractGoogleError(string raw)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("error", out var error) &&
                    error.TryGetProperty("message", out var message))
                {
                    return message.GetString();
                }
            }
            catch {  }
            return null;
        }
    }

    }

