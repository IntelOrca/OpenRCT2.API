using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Components;
using OpenRCT2.Api.Client;
using OpenRCT2.Api.Client.Models;
using OpenRCT2.Content.Services;

namespace OpenRCT2.Content.Pages
{
    public partial class SignUp
    {
        [Inject]
        private OpenRCT2ApiService Api { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        private string userInput;
        private string emailInput;
        private string passwordInput;
        private string passwordConfirmInput;

        private string validateMessageUser;
        private string validateMessageEmail;
        private string validateMessagePassword;
        private string validateMessagePasswordConfirm;

        private bool wasValidated;
        private bool wasSuccessful;
        private string failMessage;

        private async Task OnSubmit()
        {
            if (ValidateFields())
            {
                wasSuccessful = false;
                try
                {
                    await Api.Client.User.Create(userInput, emailInput, passwordInput);
                    wasSuccessful = true;
                }
                catch (OpenRCT2ApiClientStatusCodeException ex) when (ex.Content is DefaultErrorModel err)
                {
                    failMessage = err.Reason;
                }
                catch (Exception)
                {
                    failMessage = "Unable to create user account.";
                }
                StateHasChanged();
            }
            else
            {
                wasValidated = true;
                StateHasChanged();
            }
        }

        private bool ValidateFields()
        {
            var invalid = false;
            validateMessageUser = "";
            validateMessageEmail = "";
            validateMessagePassword = "";
            validateMessagePasswordConfirm = "";

            if ((userInput ?? "").Length < 3)
            {
                validateMessageUser = "Invalid user name.";
                invalid = true;
            }
            if ((emailInput ?? "").Count(c => c == '@') != 1)
            {
                validateMessageEmail = "Invalid email address.";
                invalid = true;
            }
            if ((passwordInput ?? "").Length < 6)
            {
                validateMessagePassword = "Password must be at least 6 characters.";
                invalid = true;
            }
            if (passwordInput != passwordConfirmInput)
            {
                validateMessagePasswordConfirm = "Did not match password.";
                invalid = true;
            }
            return !invalid;
        }
    }
}
