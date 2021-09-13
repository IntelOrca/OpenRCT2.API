using Microsoft.AspNetCore.Components.Forms;
using OpenRCT2.Api.Client.Models;

namespace OpenRCT2.Content.Models
{
    public class ContentEditFormModel
    {
        public string[] AvailableOwners { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ContentVisibility Visibility { get; set; }
        public IBrowserFile File { get; set; }
        public IBrowserFile Image { get; set; }
        public string SubmitButtonText { get; set; }

        public bool? NameIsValid { get; set; }
        public string NameValidationMessage { get; set; }
        public string ValidationMessage { get; set; }

        public UploadContentRequest ToUploadContentRequest()
        {
            return new UploadContentRequest
            {
                Owner = Owner,
                Name = Name,
                Title = Title,
                Description = Description,
                Visibility = Visibility,
                File = File?.OpenReadStream(Constants.MaxAllowedFileSize),
                FileName = File?.Name,
                Image = Image?.OpenReadStream(Constants.MaxAllowedImageSize),
                ImageFileName = Image?.Name
            };
        }
    }
}
