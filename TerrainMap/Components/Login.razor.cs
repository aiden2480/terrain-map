using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Components;

public partial class Login : ComponentBase
{
    [Inject]
    public required ITerrainAuthService TerrainAuthService { get; set; }

    [Inject]
    public required ILocalAuthService AuthService { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnLoginSuccess { get; set; }

    private MudForm form = default!;

    private MudNumericField<uint?> memberNumberField = default!;

    private MudTextField<string> passwordField = default!;

    private Branch selectedBranch = Branch.NSW;

    private uint? memberNumber;

    private string password = string.Empty;

    private string errorMessage = string.Empty;

    private bool isLoading = false;

    async Task OnKeyPress(KeyboardEventArgs args)
    {
        if (args.Key == "Enter"
            && memberNumberField.Value.HasValue
            && !string.IsNullOrWhiteSpace(password)
            && !isLoading)
        {
            await SignIn();
        }
    }

    async Task SignIn()
    {
        // Ensure form is valid before proceeding
        await form.Validate();

        if (!form.IsValid)
        {
            return;
        }

        // Disable button & display feedback while we validate
        isLoading = true;
        var loginApiResponse = await TerrainAuthService.AttemptLoginWithCredentials(selectedBranch, (uint)memberNumber!, password);

        // If successful, invoke callback. Otherwise display error
        if (loginApiResponse.Success)
        {
            await AuthService.UpdateFromLoginApiResponse(loginApiResponse);
            await OnLoginSuccess.InvokeAsync();
        }
        else
        {
            errorMessage = loginApiResponse.ErrorMessage!;
            isLoading = false;
        }
    }
}
