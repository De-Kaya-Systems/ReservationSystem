using DeKayaServer.BlazorApp.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public class UserContextComponent : ComponentBase
{
    [Inject] protected AuthenticationStateProvider? AuthStateProvider { get; set; }

    protected UserClaims? CurrentUser { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthStateProvider!.GetAuthenticationStateAsync();
        CurrentUser = state.User.ToUserClaims();
    }
}
