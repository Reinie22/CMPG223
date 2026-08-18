using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nkanyezi.Services;

namespace Nkanyezi_Tours.Pages.Account;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    private readonly AuthService _auth;

    public LogoutModel(AuthService auth)
    {
        _auth = auth;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await _auth.LogoutAsync();
        return RedirectToPage("/Account/Login");
    }
}
