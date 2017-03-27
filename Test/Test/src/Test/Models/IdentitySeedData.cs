using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class IdentitySeedData
    {
        private const string adminEmail = "rem@gmail.com";
        private const string adminPassword = "_Duy18101995";
        public static async void EnsurePopulated(IApplicationBuilder app)
        {
            UserManager<ApplicationUser> userManager = app.ApplicationServices
            .GetRequiredService<UserManager<ApplicationUser>>();
            ApplicationUser user = await userManager.FindByIdAsync(adminEmail);
            if (user == null)
            {
                user = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
                await userManager.CreateAsync(user, adminPassword);
            }
        }
    }
}
