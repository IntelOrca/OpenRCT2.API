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

        private ValidatedForm InputForm { get; } = new();
        private ValidatedValue<string> InputName { get; } = new();
        private UserAccountStatus InputStatus { get; set; }
        private string InputSuspensionReason { get; set; }
        private ValidatedValue<string> InputEmailCurrent { get; } = new();
        private ValidatedValue<string> InputEmailNew { get; } = new();
        private ValidatedValue<string> InputPassword { get; } = new();
        private ValidatedValue<string> InputPasswordConfirm { get; } = new();
        private string InputBio { get; set; }

        public bool HasAdminEditFeatures { get; set; }

        private bool IsGeneratingSecret { get; set; }

        protected override async Task OnInitializedAsync()
        {
            InputForm.AddChildren(InputName, InputEmailCurrent, InputEmailNew, InputPassword, InputPasswordConfirm);

            if (!Auth.IsPower)
            {
                Navigation.NavigateTo($"/{Name}");
            }

            try
            {
                User = await Api.Client.User.Get(Name);
                if (!User.CanEdit)
                {
                    Navigation.NavigateTo($"/{Name}");
                }

                InputName.Value = User.Name;
                InputStatus = User.Status;
                InputSuspensionReason = User.SuspensionReason;
                InputEmailCurrent.Value = User.Email;
                InputEmailNew.Value = User.EmailPending;
                InputBio = User.Bio;
                HasAdminEditFeatures = Auth.IsAdmin;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        }

        private async Task OnGenerateSecretKey()
        {
            if (IsGeneratingSecret)
                return;

            try
            {
                IsGeneratingSecret = true;
                StateHasChanged();
                User.SecretKey = await Api.Client.User.GenerateSecretKey(User.Name);
            }
            catch
            {
            }
            finally
            {
                IsGeneratingSecret = false;
            }
            StateHasChanged();
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
                        request.Name = InputName.Value;
                        request.Status = InputStatus;
                        request.EmailCurrent = InputEmailCurrent.Value;
                        request.SuspensionReason = InputSuspensionReason;
                    }
                    request.EmailNew = InputEmailNew.Value;
                    request.Password = InputPassword.Value;
                    request.Bio = InputBio;
                    await Api.Client.User.Edit(Name, request);
                    Navigation.NavigateTo($"/{Name}");
                }
                catch (OpenRCT2ApiClientStatusCodeException ex) when (ex.Content is DefaultErrorModel err)
                {
                    InputForm.Message = err.Reason ?? "Unable to edit user account.";
                    InputForm.IsValid = false;
                }
                catch (Exception)
                {
                    InputForm.Message = "Unable to edit user account.";
                    InputForm.IsValid = false;
                }
                StateHasChanged();
            }
            else
            {
                StateHasChanged();
            }
        }

        private bool ValidateFields()
        {
            InputForm.ResetValidation();

            if (HasAdminEditFeatures)
            {
                if (InputName.Value != User.Name)
                {
                    Validation.ValidateName(InputName);
                }
                if (InputEmailCurrent.Value != User.Email)
                {
                    Validation.ValidateEmail(InputEmailCurrent);
                }
            }
            if (!string.IsNullOrEmpty(InputEmailNew.Value))
            {
                Validation.ValidateEmail(InputEmailNew);
            }
            if (!string.IsNullOrEmpty(InputPassword.Value) || !string.IsNullOrEmpty(InputPasswordConfirm.Value))
            {
                Validation.ValidatePassword(InputPassword, InputPasswordConfirm);
            }

            return InputForm.AreAllChildrenValid;
        }
    }
}
