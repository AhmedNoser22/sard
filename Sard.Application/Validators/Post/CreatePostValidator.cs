namespace Sard.Application.Validators.Post
{
    public class CreatePostValidator : AbstractValidator<CreatePostDto>
    {
        public CreatePostValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("محتوى البوست مطلوب")
                .MaximumLength(2000).WithMessage("الحد الأقصى 2000 حرف");
        }
    }
}
