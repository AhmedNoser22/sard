namespace Sard.Application.DTOs.Payment
{
    public record PaymobWebhookDto(
    PaymobObj Obj
);

    public record PaymobObj(
        bool Success,
        string OrderId,
        int AmountCents,
        PaymobOrderData Order
    );

    public record PaymobOrderData(
        string MerchantOrderId
    );
}
