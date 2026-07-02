namespace Sard.Application.Validators.Profile
{
    public class AddHighlightValidator : AbstractValidator<AddHighlightDto>
    {
        public AddHighlightValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("محتوى الاقتباس مطلوب")
                .MaximumLength(1000);

            RuleFor(x => x.NovelTitle).MaximumLength(200);
            RuleFor(x => x.NovelAuthor).MaximumLength(100);
        }
    }
}
