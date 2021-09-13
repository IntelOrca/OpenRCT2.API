using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OpenRCT2.Api.Client;
using OpenRCT2.Api.Client.Models;
using OpenRCT2.Content.Models;
using OpenRCT2.Content.Services;

namespace OpenRCT2.Content.Pages
{
    public partial class ContentEdit
    {
        private object error;
        private ContentModel content;
        private readonly ContentEditFormModel contentEditForm = new ContentEditFormModel();

        [Inject]
        private OpenRCT2ApiService Api { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        [Parameter]
        public string Owner { get; set; }

        [Parameter]
        public string Name { get; set; }

        public string AuthorUrl => $"/{content.Owner}";
        public string ContentUrl => $"/{content.Owner}/{content.Name}";
        public string ImageUrl => content.ImageUrl;
        public string DownloadUrl => content.FileUrl;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                content = await Api.Client.Content.Get(Owner, Name);
                if (!content.CanEdit)
                {
                    Navigation.NavigateTo($"/{Owner}/{Name}");
                }

                contentEditForm.Name = content.Name;
                contentEditForm.Title = content.Title;
                contentEditForm.Description = content.Description;
                contentEditForm.Visibility = content.ContentVisibility;
                contentEditForm.SubmitButtonText = "Save";
                StateHasChanged();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        }

        private async Task OnValidate()
        {
            if (string.Equals(contentEditForm.Name, content.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                contentEditForm.NameIsValid = null;
                contentEditForm.NameValidationMessage = null;
            }
            else
            {
                var response = await Api.Client.Content.VerifyName(Owner, contentEditForm.Name);
                if (response.Valid)
                {
                    contentEditForm.NameIsValid = true;
                    contentEditForm.NameValidationMessage = null;
                }
                else
                {
                    contentEditForm.NameIsValid = false;
                    contentEditForm.NameValidationMessage = response.Message;
                }
            }
            StateHasChanged();
        }

        private async Task OnSubmit()
        {
            var request = contentEditForm.ToUploadContentRequest();
            try
            {
                var response = await Api.Client.Content.Update(Owner, Name, request);
                if (response.Valid)
                {
                    Navigation.NavigateTo($"/{response.Owner}/{response.Name}");
                }
                else
                {
                    contentEditForm.ValidationMessage = response.Message;
                }
            }
            catch (OpenRCT2ApiClientStatusCodeException<UploadContentResponse> ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    contentEditForm.ValidationMessage = "You must be logged in to upload content.";
                }
                else
                {
                    contentEditForm.ValidationMessage = ex.Content.Message;
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                contentEditForm.ValidationMessage = ex.Message;
                StateHasChanged();
            }
        }
    }
}
