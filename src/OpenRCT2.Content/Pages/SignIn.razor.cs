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

        private string EmailInput { get; set; }
        private string PasswordInput { get; set; }
        private string ValidationMessage { get; set; }
        private string ValidationMessageEmail { get; set; }
        private string ValidationMessagePassword { get; set; }

        private bool? IsEmailValid { get; set; }
        private bool? IsPasswordValid { get; set; }

        private bool ShowPasswordResetMessage { get; set; }

        protected override void OnInitialized()
        {
            if (Auth.IsSignedIn)
            {
                Navigation.NavigateTo("/");
            }
        }

        private void ClearValidation()
        {
            ValidationMessage = null;
            ValidationMessageEmail = null;
            ValidationMessagePassword = null;
            IsEmailValid = null;
            IsPasswordValid = null;
        }

        private async Task OnSubmit()
        {
            ClearValidation();
            try
            {
                if (await Api.SignInAsync(EmailInput, PasswordInput))
                {
                    Navigation.NavigateTo("/");
                }
                else
                {
                    ValidationMessage = "Invalid e-mail address, user name or password.";
                    IsEmailValid = false;
                    IsPasswordValid = false;
                    StateHasChanged();
                }
            }
            catch
            {
                ValidationMessage = "Unable to sign in.";
                StateHasChanged();
            }
        }

        private async Task OnResetPasswordClick()
        {
            ClearValidation();
            try
            {
                await Api.Client.User.RequestRecovery(EmailInput);
                ShowPasswordResetMessage = true;
            }
            catch (OpenRCT2ApiClientStatusCodeException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                ValidationMessage = "E-mail address or user name not found.";
                IsEmailValid = false;
            }
            catch
            {
                ValidationMessage = "Unable to reset password.";
            }
            StateHasChanged();
        }
    }
}
