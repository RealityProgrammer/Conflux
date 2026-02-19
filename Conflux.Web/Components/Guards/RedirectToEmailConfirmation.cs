using Conflux.Application.Abstracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Conflux.Web.Components.Guards;

public class RedirectToEmailConfirmation : ComponentBase {
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationState { get; set; } = null!;

    protected override async Task OnInitializedAsync() {
        var authState = await AuthenticationState;
        var user = authState.User;
        
        if (user.Identity is { IsAuthenticated: true }) {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            if (!await UserService.IsUserEmailConfirmed(userId)) {
                Navigation.NavigateTo(Navigation.GetUriWithQueryParameters("/auth/verify-email", new Dictionary<string, object?> {
                    ["UserId"] = userId,
                }), replace: true);
            }
        }
    }
}