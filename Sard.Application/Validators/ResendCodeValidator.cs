namespace Sard.Application.Validators
{
    public class ResendCodeValidator : AbstractValidator<ResendCodeDto>
    {
        public ResendCodeValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح");
        }
    }
}
