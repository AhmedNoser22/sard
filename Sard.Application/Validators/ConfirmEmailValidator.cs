namespace Sard.Application.Validators
{
    public class ConfirmEmailValidator : AbstractValidator<ConfirmEmailDto>
    {
        public ConfirmEmailValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("الرمز مطلوب")
                .Length(6).WithMessage("الرمز يجب أن يكون 6 أرقام");
        }
    }
}
