using Sard.Domain.Enums;
using Sard.Domain.Helpers;

namespace Sard.Domain.Entities
{
    public class Purchase
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public PurchaseType Type { get; set; }
        public string PaymobTransactionId { get; set; }
        public DateTime PaidAt { get; set; } = EgyptDateTime.Now;

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int NovelId { get; set; }
        public Novel Novel { get; set; }
    }
}
