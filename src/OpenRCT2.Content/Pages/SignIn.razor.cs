using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using OpenRCT2.Content.Services;

namespace OpenRCT2.Content.Pages
{
    public partial class SignIn
    {
        [Inject]
        private AuthorisationService Auth { get; set; }

        [Inject]
        private OpenRCT2ApiService Api { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        private string emailInput;
        private string passwordInput;
        private string validateMessage;
        private bool wasValidated;

        protected override void OnInitialized()
        {
            if (Auth.IsSignedIn)
            {
                Navigation.NavigateTo("/");
            }
        }

        private async Task OnSubmit()
        {
            try
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
            catch
            {
                validateMessage = "Unable to sign in.";
                wasValidated = true;
                StateHasChanged();
            }
        }

        private void OnResetPasswordClick()
        {
        }
    }
}
