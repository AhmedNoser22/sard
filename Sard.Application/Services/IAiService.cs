namespace Sard.Application.Services
{
    public interface IAiService
    {
        Task<Result<string>> CorrectTextAsync(string text);
    }
}
