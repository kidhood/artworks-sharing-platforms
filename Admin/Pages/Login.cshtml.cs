using Admin.Pages.Resources;
using ArtHubService.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Admin.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAccountService accountService;

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public LoginModel(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        public IActionResult OnPost()
        {
            var account = accountService.GetAccountByUsernameAndPassword(Email, Password);
            if (account != null)
            {
                if (!account.Enabled)
                {
                    ViewData["ErrorMessage"] = MessageResource.AccountBanned;
                    return Page();
                }
                string role = account.Role.RoleName;
                // Configure JsonSerializerSettings to handle reference loops
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                var accountJson = JsonConvert.SerializeObject(account, settings);
                switch (role)
                {                    
                    case "Moderator":
                        HttpContext.Session.SetString("CREDENTIAL", accountJson);                        
                        return RedirectToPage("/ModHomePage");
                    case "Admin":
                        HttpContext.Session.SetString("CREDENTIAL", accountJson);
                        return RedirectToPage("/Moderator/ArtWorksManagement");
                    default:
                        ViewData["ErrorMessage"] = MessageResource.AccountNotHavePermission;
                        return Page();
                }
            }
            else
            {
                ViewData["ErrorMessage"] = MessageResource.InvalidUserNamePassword;
                return Page();
            }
        }
    }
}
