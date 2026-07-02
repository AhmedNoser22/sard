namespace Sard.Application.Validators.Auth
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("الرمز مطلوب")
                .Length(6).WithMessage("الرمز يجب أن يكون 6 أرقام");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة")
                .MinimumLength(8).WithMessage("كلمة المرور 8 أحرف على الأقل")
                .Matches("[A-Z]").WithMessage("يجب أن تحتوي على حرف كبير")
                .Matches("[a-z]").WithMessage("يجب أن تحتوي على حرف صغير")
                .Matches("[0-9]").WithMessage("يجب أن تحتوي على رقم واحد على الأقل");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("كلمة المرور غير متطابقة");
        }
    }
}
