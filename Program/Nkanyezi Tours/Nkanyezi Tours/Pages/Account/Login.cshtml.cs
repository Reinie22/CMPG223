using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nkanyezi.Services;

namespace Nkanyezi_Tours.Pages.Account;

/// <summary>
/// The login screen.
///
/// AllowAnonymous is required: without it the page that lets you log in would
/// itself demand that you are logged in, and the site would redirect forever.
/// </summary>
[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly AuthService _auth;

    public LoginModel(AuthService auth)
    {
        _auth = auth;
    }

    /// <summary>
    /// BindProperty means the form fields are copied into this object when the
    /// form is posted. The validation attributes produce the messages next to
    /// each box, before anything reaches the database.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "That does not look like an email address.")]
        [Display(Name = "Email address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Keep me signed in")]
        public bool RememberMe { get; set; }
    }

    public void OnGet()
    {
        // Nothing to do. The form starts empty.
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        // The boxes were not filled in properly, so do not bother the database.
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var (result, user) = await _auth.LoginAsync(
                Input.Email, Input.Password, Input.RememberMe);

            switch (result)
            {
                case AuthService.LoginResult.Success:
                    // Only follow returnUrl when it points inside this site.
                    // Without that check, a link could send somebody who has
                    // just logged in off to another website entirely.
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return LocalRedirect(returnUrl);

                    return RedirectToPage("/Index");

                case AuthService.LoginResult.AccountDeactivated:
                    ErrorMessage = "This account has been deactivated. "
                                 + "Please contact the system administrator.";
                    return Page();

                default:
                    ErrorMessage = "Email address or password is incorrect.";
                    return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Could not reach the database. Check that SQL Server is "
                         + "running and that the schema and procedures have been "
                         + "created. Details: " + ex.Message;
            return Page();
        }
    }
}
