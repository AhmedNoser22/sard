namespace Sard.Application.Validators.Group
{
    public class SendMessageValidator : AbstractValidator<SendMessageDto>
    {
        public SendMessageValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("الرسالة لا يمكن أن تكون فارغة")
                .MaximumLength(2000);
        }
    }
}
