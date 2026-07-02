namespace Sard.Application.Validators.Profile
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("الاسم مطلوب")
                .MaximumLength(100);

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("البايو 500 حرف كحد أقصى");
        }
    }
}
