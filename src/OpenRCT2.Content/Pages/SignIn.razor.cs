using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using OpenRCT2.Api.Client;
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

        private ValidatedForm InputForm { get; } = new();
        private ValidatedValue<string> InputEmail { get; } = new();
        private ValidatedValue<string> InputPassword { get; } = new();

        private bool ShowPasswordResetMessage { get; set; }

        protected override void OnInitialized()
        {
            InputForm.AddChildren(InputEmail, InputPassword);

            if (Auth.IsSignedIn)
            {
                Navigation.NavigateTo("/");
            }
        }

        private void ClearValidation() => InputForm.ResetValidation();

        private async Task OnSubmit()
        {
            ClearValidation();
            try
            {
                if (await Api.SignInAsync(InputEmail.Value, InputPassword.Value))
                {
                    Navigation.NavigateTo("/");
                }
                else
                {
                    InputForm.Message = "Invalid e-mail address, user name or password.";
                    InputForm.IsValid = false;
                    StateHasChanged();
                }
            }
            catch
            {
                InputForm.Message = "Unable to sign in.";
                InputForm.IsValid = false;
                StateHasChanged();
            }
        }

        private async Task OnResetPasswordClick()
        {
            ClearValidation();
            try
            {
                await Api.Client.User.RequestRecovery(InputEmail.Value);
                ShowPasswordResetMessage = true;
            }
            catch (OpenRCT2ApiClientStatusCodeException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                InputForm.Message = "E-mail address or user name not found.";
                InputEmail.IsValid = false;
            }
            catch
            {
                InputForm.Message = "Unable to reset password.";
            }
            StateHasChanged();
        }
    }
}
