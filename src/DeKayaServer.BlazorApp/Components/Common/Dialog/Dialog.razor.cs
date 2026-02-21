using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace DeKayaServer.BlazorApp.Components.Common.Dialog;

public partial class Dialog( IJSRuntime js ) : IAsyncDisposable
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string Subtitle { get; set; } = string.Empty;

    [Parameter] public EventCallback<DialogResultEnum> ParentMethod { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; } = null!;
    [Parameter] public RenderFragment? Footer { get; set; }

    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public DialogTypeEnum DialogType { get; set; }

    [Parameter] public bool ClickOutsideToClose { get; set; } = true;

    private IJSObjectReference? module;
    private ValueTask<IJSObjectReference>? moduleLoadingTask;
    private ElementReference dialogElementReference;

    private DotNetObjectReference<Dialog>? dotNetRef;

    protected override Task OnAfterRenderAsync( bool firstRender )
    {
        if ( firstRender )
            moduleLoadingTask = js.InvokeAsync<IJSObjectReference>( "import", "./Components/Common/Dialog/Dialog.razor.js" );

        return Task.CompletedTask;
    }

    private async Task<IJSObjectReference> EnsureModuleAsync()
    {
        if ( module is not null )
            return module;

        module = moduleLoadingTask is not null
            ? await moduleLoadingTask.Value
            : await js.InvokeAsync<IJSObjectReference>(
                "import",
                "./Components/Common/Dialog/Dialog.razor.js" );

        return module;
    }

    public async Task ShowAsync()
    {
        var mod = await EnsureModuleAsync();

        dotNetRef ??= DotNetObjectReference.Create( this );

        await mod.InvokeVoidAsync(
            "showDialog",
            dialogElementReference,
            dotNetRef,
            ClickOutsideToClose );
    }

    public async Task HideAsync()
    {
        var mod = await EnsureModuleAsync();
        await mod.InvokeVoidAsync( "hideDialog", dialogElementReference );
    }

    private async Task EscapeKeyHandlerAsync( KeyboardEventArgs args )
    {
        if ( args.Code == "Escape" )
        {
            await HideAsync();
            await ParentMethod.InvokeAsync( DialogResultEnum.Cancel );
        }
    }

    [JSInvokable]
    public async Task ClickOutsideTriggerAsync()
    {
        if ( !ClickOutsideToClose )
            return;

        await HideAsync();
        await ParentMethod.InvokeAsync( DialogResultEnum.Cancel );
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if ( module is not null )
        {
            try
            {
                await module.DisposeAsync();
            }
            catch ( JSDisconnectedException )
            { }
        }
    }
}

public enum DialogResultEnum
{
    Ok,
    Cancel,
    Close,
    XButton
}

public enum DialogTypeEnum
{
    Ok = 0,
    OkCancel = 1,
    Close = 2
}
