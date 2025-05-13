using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Admin
{
    public class LogoutModel(SignInManager<AppUser> signInManager) : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                await signInManager.SignOutAsync();
            }
            return Redirect("/admin/login");

        }
    }
}
