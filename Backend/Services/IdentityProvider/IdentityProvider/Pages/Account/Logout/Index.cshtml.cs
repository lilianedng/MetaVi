using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityProvider.Pages.Logout;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel {
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly SignInManager<ApplicationUser> _signinManager;

    [BindProperty]
    public string LogoutId { get; set; }

    public Index (IIdentityServerInteractionService interaction, IEventService events, SignInManager<ApplicationUser> signinManager) {
        _interaction = interaction;
        _events = events;
        _signinManager = signinManager;
    }

    public async Task<IActionResult> OnGet (string logoutId) {
        LogoutId = logoutId;

        return await OnPost();
    }

    public async Task<IActionResult> OnPost () {
        if (User?.Identity.IsAuthenticated == true) {
            // if there's no current logout context, we need to create one
            // this captures necessary info from the current logged in user
            // this can still return null if there is no context needed
            LogoutId ??= await _interaction.CreateLogoutContextAsync();

            // delete local authentication cookie
            await HttpContext.SignOutAsync();
            await _signinManager.SignOutAsync();

            // raise the logout event
            await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));

            // see if we need to trigger federated logout
            var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

            // if it's a local login we can ignore this workflow
            if (idp != null && idp != Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider) {
                // we need to see if the provider supports external logout
                if (await HttpContext.GetSchemeSupportsSignOutAsync(idp)) {
                    // build a return URL so the upstream provider will redirect back
                    // to us after the user has logged out. this allows us to then
                    // complete our single sign-out processing.
                    string url = Url.Page("/Account/Logout/Loggedout", new { logoutId = LogoutId });

                    // this triggers a redirect to the external provider for sign-out
                    return SignOut(new AuthenticationProperties { RedirectUri = url }, idp);
                }
            }
        }

        return RedirectToPage("/Account/Logout/LoggedOut", new { logoutId = LogoutId });
    }
}