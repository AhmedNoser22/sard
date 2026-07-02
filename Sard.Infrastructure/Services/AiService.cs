namespace Sard.Infrastructure.Services
{
    public class AiService(IOptions<GeminiSettings> options, HttpClient httpClient) : IAiService
    {
        private readonly GeminiSettings _settings = options.Value;

        public async Task<Result<string>> CorrectTextAsync(string text)
        {
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

            var response = await httpClient.PostAsJsonAsync(url, body);

            if (!response.IsSuccessStatusCode)
                return Result<string>.Failure("فشل الاتصال بخدمة التصحيح");

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();

            var corrected = result
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return corrected is null
                ? Result<string>.Failure("لم يتم الحصول على نتيجة")
                : Result<string>.Success(corrected);
        }
    }
}
