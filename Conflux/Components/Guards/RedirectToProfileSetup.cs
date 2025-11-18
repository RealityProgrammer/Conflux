using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Conflux.Components.Guards;

public class RedirectToProfileSetup : ComponentBase {
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    [Inject] private IAccountService AccountService { get; set; } = null!;

    protected override async Task OnInitializedAsync() {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity is { IsAuthenticated: true }) {
            if (!await AccountService.IsProfileSetup(user)) {
                Navigation.NavigateTo("/settings/profile");
            }
        }
    }
}