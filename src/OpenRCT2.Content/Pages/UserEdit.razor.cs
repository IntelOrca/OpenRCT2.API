using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using OpenRCT2.Api.Client;
using OpenRCT2.Api.Client.Models;
using OpenRCT2.Content.Services;

namespace OpenRCT2.Content.Pages
{
    public partial class UserEdit
    {
        [Inject]
        private OpenRCT2ApiService Api { get; set; }

        [Inject]
        private AuthorisationService Auth { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        [Parameter]
        public string Name { get; set; }

        private UserModel User { get; set; }

        private object Error { get; set; }

        public string NameInput { get; set; }
        public string EmailCurrentInput { get; set; }
        public string EmailNewInput { get; set; }
        public UserAccountStatus StatusInput { get; set; }
        public string PasswordInput { get; set; }
        public string PasswordConfirmInput { get; set; }
        public string BioInput { get; set; }

        public bool? IsNameValid { get; set; }

        public bool WasValidated { get; set; }
        public string ValidateMessage { get; set; }
        public string ValidateMessageName { get; set; }
        public string ValidateMessageEmailCurrent { get; set; }
        public string ValidateMessageEmailNew { get; set; }
        public string ValidateMessagePassword { get; set; }

        public string ValidateMessagePasswordConfirm { get; set; }

        public bool HasAdminEditFeatures { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                User = await Api.Client.User.Get(Name);
                if (!User.CanEdit)
                {
                    Navigation.NavigateTo($"/{Name}");
                }

                NameInput = User.Name;
                StatusInput = User.Status;
                EmailCurrentInput = User.Email;
                BioInput = User.Bio;
                HasAdminEditFeatures = Auth.IsAdmin;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        }

        private async Task OnSubmit()
        {
            if (ValidateFields())
            {
                try
                {
                    var request = new UserEditRequest();
                    if (HasAdminEditFeatures)
                    {
                        request.Name = NameInput;
                        request.Status = StatusInput;
                        request.EmailCurrent = EmailCurrentInput;
                    }
                    request.EmailNew = EmailNewInput;
                    request.Password = PasswordInput;
                    request.Bio = BioInput;
                    await Api.Client.User.Edit(Name, request);
                    Navigation.NavigateTo($"/{Name}");
                }
                catch (OpenRCT2ApiClientStatusCodeException ex) when (ex.Content is DefaultErrorModel err)
                {
                    ValidateMessage = err.Reason;
                }
                catch (Exception)
                {
                    ValidateMessage = "Unable to edit user account.";
                }
                StateHasChanged();
            }
            else
            {
                WasValidated = true;
                StateHasChanged();
            }
        }

        private bool ValidateFields()
        {
            var invalid = false;
            ValidateMessageName = "";
            ValidateMessageEmailCurrent = "";
            ValidateMessageEmailNew = "";
            ValidateMessagePassword = "";
            ValidateMessagePasswordConfirm = "";

            if (HasAdminEditFeatures)
            {
                if (NameInput != User.Name)
                {
                    if ((NameInput ?? "").Length < 3)
                    {
                        ValidateMessageName = "Invalid user name.";
                        invalid = true;
                    }
                }
                if (EmailCurrentInput != User.Email)
                {
                    if ((EmailCurrentInput ?? "").Count(c => c == '@') != 1)
                    {
                        ValidateMessageEmailCurrent = "Invalid email address.";
                        invalid = true;
                    }
                }
            }
            if (!string.IsNullOrEmpty(EmailNewInput))
            {
                if ((EmailNewInput ?? "").Count(c => c == '@') != 1)
                {
                    ValidateMessageEmailNew = "Invalid email address.";
                    invalid = true;
                }
            }
            if (!string.IsNullOrEmpty(PasswordInput) || !string.IsNullOrEmpty(PasswordConfirmInput))
            {
                if ((PasswordInput ?? "").Length < 6)
                {
                    ValidateMessagePassword = "Password must be at least 6 characters.";
                    invalid = true;
                }
                if (PasswordInput != PasswordConfirmInput)
                {
                    ValidateMessagePasswordConfirm = "Did not match password.";
                    invalid = true;
                }
            }
            return !invalid;
        }
    }
}
