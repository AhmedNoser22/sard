using Microsoft.Extensions.Caching.Memory;

namespace Sard.Infrastructure.Implementation.Auth
{
    public class AuthService(
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        IEmailService emailService,
        IGoogleAuthService googleAuthService,
        IMemoryCache cache) : IAuthService
    {
        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            if (await userManager.FindByEmailAsync(dto.Email) is not null)
                return Result<AuthResponseDto>.Failure("البريد الإلكتروني مستخدم بالفعل");

            var code = GenerateSixDigitCode();

            cache.Set($"reg:{dto.Email}", (dto, code), TimeSpan.FromMinutes(10));

            try
            {
                var body = EmailTemplates.GetCodeEmail(dto.DisplayName, code, "confirm");
                await emailService.SendEmailAsync(dto.Email, "تأكيد البريد الإلكتروني 💌", body);
            }
            catch (Exception ex)
            {
                cache.Remove($"reg:{dto.Email}");
                return Result<AuthResponseDto>.Failure($"فشل إرسال الإيميل: {ex.Message}");
            }

            return Result<AuthResponseDto>.Success(new AuthResponseDto(
                string.Empty,
                dto.DisplayName,
                dto.Email,
                string.Empty
                ));
        }

        public async Task<Result<AuthResponseDto>> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            if (!cache.TryGetValue($"reg:{dto.Email}", out (RegisterDto regDto, string code) cached))
                return Result<AuthResponseDto>.Failure("انتهت صلاحية الرمز أو البريد غير مسجل");

            if (cached.code != dto.Code)
                return Result<AuthResponseDto>.Failure("الرمز غير صحيح");

            if (await userManager.FindByEmailAsync(dto.Email) is not null)
                return Result<AuthResponseDto>.Failure("البريد الإلكتروني مستخدم بالفعل");

            var user = new AppUser
            {
                DisplayName = cached.regDto.DisplayName,
                Email = dto.Email,
                UserName = dto.Email,
                AgreeToTerms = cached.regDto.AgreeToTerms,
                EmailConfirmed = true,
                CreatedAt = EgyptDateTime.Now
            };

            var result = await userManager.CreateAsync(user, cached.regDto.Password);
            if (!result.Succeeded)
                return Result<AuthResponseDto>.Failure(result.Errors.First().Description);

            await userManager.AddToRoleAsync(user, AppRoles.User);
            cache.Remove($"reg:{dto.Email}");

            var roles = await userManager.GetRolesAsync(user);
            var token = tokenService.GenerateToken(user, roles);

            return Result<AuthResponseDto>.Success(new 
                AuthResponseDto(
                user.Id,           
                user.DisplayName,  
                user.Email!,       
                token              
                ));
        }

        public async Task<Result<string>> ResendCodeAsync(ResendCodeDto dto)
        {
            if (!cache.TryGetValue($"reg:{dto.Email}", out (RegisterDto regDto, string code) cached))
                return Result<string>.Failure("البريد غير موجود أو انتهت الجلسة، أعد التسجيل");

            var newCode = GenerateSixDigitCode();
            cache.Set($"reg:{dto.Email}", (cached.regDto, newCode), TimeSpan.FromMinutes(10));

            var body = EmailTemplates.GetCodeEmail(cached.regDto.DisplayName, newCode, "confirm");
            await emailService.SendEmailAsync(dto.Email, "تأكيد البريد الإلكتروني 💌", body);

            return Result<string>.Success("تم إرسال الرمز مرة أخرى ✅");
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, dto.Password))
                return Result<AuthResponseDto>.Failure("البريد الإلكتروني أو كلمة المرور غير صحيحة");

            if (!user.EmailConfirmed)
                return Result<AuthResponseDto>.Failure("يرجى تأكيد بريدك الإلكتروني أولاً");

            var isLocked = await userManager.IsLockedOutAsync(user);
            if (isLocked)
                return Result<AuthResponseDto>.Failure("تم قفل حسابك من قِبَل الإدارة. للاستفسار تواصل مع الدعم.");

            var roles = await userManager.GetRolesAsync(user);
            var token = tokenService.GenerateToken(user, roles);

            return Result<AuthResponseDto>.Success(new AuthResponseDto(
                user.Id, user.DisplayName, user.Email!, token));
        }

        public async Task<Result<AuthResponseDto>> GoogleLoginAsync(GoogleLoginDto dto)
        {
            var payload = await googleAuthService.VerifyAsync(dto.IdToken);
            if (payload is null)
                return Result<AuthResponseDto>.Failure("Google token غير صالح");

            var user = await userManager.FindByEmailAsync(payload.Email);

            if (user is null)
            {
                user = new AppUser
                {
                    DisplayName = payload.Name,
                    Email = payload.Email,
                    UserName = payload.Email,
                    EmailConfirmed = true,
                    AgreeToTerms = true,
                    CreatedAt = EgyptDateTime.Now
                };

                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return Result<AuthResponseDto>.Failure(result.Errors.First().Description);

                await userManager.AddToRoleAsync(user, AppRoles.User);
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = tokenService.GenerateToken(user, roles);

            return Result<AuthResponseDto>.Success(new
               AuthResponseDto(
               user.Id,
               user.DisplayName,
               user.Email!,
               token
               ));
        }

        public async Task<Result<string>> ForgotPasswordAsync(ResendCodeDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return Result<string>.Failure("المستخدم غير موجود");

            var code = GenerateSixDigitCode();
            cache.Set($"reset:{dto.Email}", code, TimeSpan.FromMinutes(10));

            var body = EmailTemplates.GetCodeEmail(user.DisplayName, code, "reset");
            await emailService.SendEmailAsync(user.Email!, "إعادة تعيين كلمة المرور 🔐", body);

            return Result<string>.Success("تم إرسال رمز إعادة التعيين ✅");
        }

        public async Task<Result<string>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return Result<string>.Failure("المستخدم غير موجود");

            if (!cache.TryGetValue($"reset:{dto.Email}", out string? savedCode) || savedCode != dto.Code)
                return Result<string>.Failure("الرمز غير صحيح أو منتهي الصلاحية");

            cache.Remove($"reset:{dto.Email}");

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);

            if (!result.Succeeded)
                return Result<string>.Failure(result.Errors.First().Description);

            return Result<string>.Success("تم إعادة تعيين كلمة المرور بنجاح ✅");
        }

        private static string GenerateSixDigitCode() =>
            Random.Shared.Next(100000, 999999).ToString();
    }
}
