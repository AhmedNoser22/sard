namespace Sard.Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {

        public RegisterValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("الاسم الكامل مطلوب")
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح")
                .Must(email =>
                {
                    var domain = email.Split('@').LastOrDefault();
                    return AllowedDomains.Contains(domain);
                }).WithMessage("يجب أن يكون البريد من مزود معروف (Gmail, Yahoo, Outlook...)");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة")
                .MinimumLength(8).WithMessage("كلمة المرور 8 أحرف على الأقل")
                .Matches("[A-Z]").WithMessage("يجب أن تحتوي على حرف كبير")
                .Matches("[a-z]").WithMessage("يجب أن تحتوي على حرف صغير")
                .Matches("[0-9]").WithMessage("يجب أن تحتوي على رقم واحد على الأقل")
                .Matches("[^a-zA-Z0-9]").WithMessage("يجب أن تحتوي على رمز خاص");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("كلمة المرور غير متطابقة");

            RuleFor(x => x.AgreeToTerms)
                .Equal(true).WithMessage("يجب الموافقة على الشروط والأحكام");
        }
        private static readonly string[] AllowedDomains =
            [
            "gmail.com", "yahoo.com", "outlook.com", "hotmail.com",
            "icloud.com", "live.com", "protonmail.com", "mail.com"
            ];
    }
}
