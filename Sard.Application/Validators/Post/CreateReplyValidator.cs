namespace Sard.Application.Validators.Post
{
    public class CreateReplyValidator : AbstractValidator<CreateReplyDto>
    {
        public CreateReplyValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("محتوى الرد مطلوب")
                .MaximumLength(1000);
        }
    }
}
