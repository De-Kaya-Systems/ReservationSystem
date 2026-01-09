using DeKayaServer.BlazorApp.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace DeKayaServer.BlazorApp.Components.Common.UserContext;

public class UserContextComponent : ComponentBase, IDisposable
{
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;


    protected UserClaims CurrentUser { get; private set; } = CreateAnonymousUser();

    protected override void OnInitialized()
    {
        AuthStateProvider.AuthenticationStateChanged += HandleAuthStateChanged;
    }

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        CurrentUser = state.User.ToUserClaims() ?? CreateAnonymousUser();
    }
    private void HandleAuthStateChanged(Task<AuthenticationState> task)
    => _ = HandleAuthStateChangedAsync(task);

    private async Task HandleAuthStateChangedAsync(Task<AuthenticationState> task)
    {
        var state = await task;
        CurrentUser = state.User.ToUserClaims() ?? CreateAnonymousUser();
        await InvokeAsync(StateHasChanged);
    }
    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= HandleAuthStateChanged;
    }

    // Bu aslında gerekli değil fakat null kontrolü için kullanılıyor. Böylece CurrentUser her zaman bir değer taşır.Ve token olmayan bir durumda sistem kullanıcıyı login sayfasına yönlendiremezse hatayı yakalamak daha kolay olur.
    // EN: This is not really necessary, but it is used for null checking. Thus, CurrentUser always holds a value. And in case the system cannot redirect the user to the login page when there is no token, it is easier to catch the error.
    private static UserClaims CreateAnonymousUser() => new("", "Anonymous", "Anonymous User", "");

}
