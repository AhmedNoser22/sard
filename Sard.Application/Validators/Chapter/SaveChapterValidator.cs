namespace Sard.Application.Validators.Chapter
{
    public class SaveChapterValidator : AbstractValidator<SaveChapterDto>
    {
        public SaveChapterValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان الفصل مطلوب")
                .MaximumLength(200);

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("محتوى الفصل مطلوب");

            RuleFor(x => x.Order)
                .GreaterThan(0).WithMessage("الترتيب يجب أن يكون أكبر من صفر");
        }
    }
}
