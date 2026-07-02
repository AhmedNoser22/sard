namespace Sard.Application.Validators.Novel
{
    public class CreateNovelValidator : AbstractValidator<CreateNovelDto>
    {
        public CreateNovelValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان الرواية مطلوب")
                .MaximumLength(200);

            RuleFor(x => x.Description).MaximumLength(1000);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("السعر لا يمكن أن يكون سالباً");
        }
    }
}
