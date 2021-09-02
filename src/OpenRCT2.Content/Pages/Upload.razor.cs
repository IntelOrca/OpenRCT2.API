using Microsoft.AspNetCore.Components;

namespace OpenRCT2.Content.Pages
{
    public partial class Upload
    {
        [Inject]
        private NavigationManager Navigation { get; set; }

        private string nameInput;
        private string descriptionInput;

        private void OnSubmit()
        {
            Navigation.NavigateTo("/");
        }
    }
}
