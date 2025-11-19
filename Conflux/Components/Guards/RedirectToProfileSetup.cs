using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Conflux.Components.Guards;

public class RedirectToProfileSetup : ComponentBase {
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationState { get; set; }

    protected override async Task OnInitializedAsync() {
        var authState = await AuthenticationState;
        var user = authState.User;

        if (user.Identity is { IsAuthenticated: true }) {
            if (!await UserService.IsProfileSetup(user)) {
                Navigation.NavigateTo("/settings/profile");
            }
        }
    }
}