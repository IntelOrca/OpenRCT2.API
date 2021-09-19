using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using OpenRCT2.Api.Client.Models;
using OpenRCT2.Content.Services;

namespace OpenRCT2.Content.Pages
{
    public partial class Author
    {
        private object error;
        private UserModel user;
        private ContentModel[] content;

        [Inject]
        private OpenRCT2ApiService Api { get; set; }

        [Parameter]
        public string Owner { get; set; }

        private string EditUrl => $"{Owner}/edit";

        protected override async Task OnInitializedAsync()
        {
            try
            {
                user = await Api.Client.User.Get(Owner);
                content = await Api.Client.Content.Get(Owner);
            }
            catch (Exception ex)
            {
                error = ex;
            }
        }
    }
}
