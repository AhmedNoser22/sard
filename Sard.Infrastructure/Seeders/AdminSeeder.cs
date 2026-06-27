//namespace Sard.Infrastructure.Seeders
//{
//    public class AdminSeeder
//    {
//        public static async Task SeedAsync(IServiceProvider serviceProvider)
//        {
//            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
//            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//            string[] roles = [AppRoles.Admin, AppRoles.User];

//            foreach (var role in roles)
//                if (!await roleManager.RoleExistsAsync(role))
//                    await roleManager.CreateAsync(new IdentityRole(role));

//            var adminEmail = "";
//            var admin = await userManager.FindByEmailAsync(adminEmail);

//            if (admin is null)
//            {
//                admin = new AppUser
//                {
//                    UserName = adminEmail,
//                    Email = adminEmail,
//                    DisplayName = "",
//                    EmailConfirmed = true,
//                    AgreeToTerms = true,
//                    CreatedAt = EgyptDateTime.Now
//                };

//                await userManager.CreateAsync(admin, "");
//                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
//            }
//        }
//    }
//}
