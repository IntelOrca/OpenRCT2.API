using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OpenRCT2.Api.Client;
using OpenRCT2.Api.Client.Models;
using OpenRCT2.Content.Services;

namespace OpenRCT2.Content.Pages
{
    public partial class Upload
    {
        [Inject]
        private AuthorisationService Auth { get; set; }

        [Inject]
        private OpenRCT2ApiService Api { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }

        private string ownerInput;
        private string nameInput;
        private string descriptionInput;
        private string visibilityInput;
        private IBrowserFile file;
        private IBrowserFile image;
        private string validatationMessage;

        protected override void OnInitialized()
        {
            if (!Auth.IsPower)
            {
                Navigation.NavigateTo("/");
            }

            ownerInput = "IntelOrca";
            visibilityInput = "public";
        }

        private async Task OnSubmit()
        {
            if (file == null || image == null)
            {
                validatationMessage = "You must upload a file and image.";
                StateHasChanged();
            }
            else
            {
                var maxAllowedSize = 8 * 1024 * 1024; // 8 MiB
                var maxAllowedImageSize = 4 * 1024 * 1024; // 4 MiB

                var request = new UploadContentRequest
                {
                    Owner = ownerInput,
                    Description = descriptionInput,
                    Visibility = Enum.Parse<ContentVisibility>(visibilityInput, true),
                    File = file.OpenReadStream(maxAllowedSize),
                    FileName = file.Name,
                    Image = image.OpenReadStream(maxAllowedImageSize),
                    ImageFileName = image.Name
                };
                try
                {
                    var response = await Api.Client.Content.Upload(request);
                    Navigation.NavigateTo($"/{response.Owner}/{response.Name}");
                }
                catch (OpenRCT2ApiClientStatusCodeException<UploadContentResponse> ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        validatationMessage = "You must be logged in to upload content.";
                    }
                    else
                    {
                        validatationMessage = ex.Content.Message;
                    }
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    validatationMessage = ex.Message;
                    StateHasChanged();
                }
            }
        }

        private void OnInputFileChange(InputFileChangeEventArgs e)
        {
            file = e.File;
        }

        private void OnInputImageChange(InputFileChangeEventArgs e)
        {
            image = e.File;
        }

        private void VisibilityChanged(ChangeEventArgs args)
        {
            visibilityInput = args.Value.ToString();
            Console.WriteLine(visibilityInput);
        }
    }
}
