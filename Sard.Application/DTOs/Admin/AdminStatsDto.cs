namespace Sard.Application.DTOs.Admin
{
    public record AdminStatsDto(
    int TotalUsers,
    int TotalPosts,
    int TotalNovels,
    int TotalPurchases,
    decimal TotalRevenue
);
}
