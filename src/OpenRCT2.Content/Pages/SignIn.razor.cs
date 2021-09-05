using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using OpenRCT2.Content.Services;

namespace OpenRCT2.Content.Pages
{
    public partial class SignIn
    {
        [Inject]
        private OpenRCT2ApiService Api { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        private string emailInput;
        private string passwordInput;
        private string validateMessage;
        private bool wasValidated;

        private async Task OnSubmit()
        {
            if (await Api.SignInAsync(emailInput, passwordInput))
            {
                Navigation.NavigateTo("/");
            }
            else
            {
                validateMessage = "Invalid e-mail address, user name or password.";
                wasValidated = true;
                StateHasChanged();
            }
        }

        private void OnResetPasswordClick()
        {
        }
    }
}
