using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using OpenRCT2.Api.Client.Models;
using OpenRCT2.Content.Services;

namespace OpenRCT2.Content.Pages
{
    public partial class Content
    {
        private object error;
        private ContentModel content;

        [Inject]
        private OpenRCT2ApiService Api { get; set; }

        [Parameter]
        public string Owner { get; set; }

        [Parameter]
        public string Name { get; set; }

        public string AuthorUrl => $"/{content.Owner}";
        public string ContentUrl => $"/{content.Owner}/{content.Name}";
        public string ImageUrl => content.ImageUrl;
        public string DownloadUrl => content.FileUrl;
        public string EditUrl => $"/{content.Owner}/{content.Name}/edit";

        protected override async Task OnInitializedAsync()
        {
            try
            {
                content = await Api.Client.Content.Get(Owner, Name);
            }
            catch (Exception ex)
            {
                error = ex;
            }
        }
    }
}
