namespace Sard.Application.Validators.Group
{
    public class CreateGroupValidator : AbstractValidator<CreateGroupDto>
    {
        public CreateGroupValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم الجروب مطلوب")
                .MaximumLength(100);

            RuleFor(x => x.Description).MaximumLength(500);
        }
    }
}
