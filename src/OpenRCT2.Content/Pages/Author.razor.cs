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

        [Inject]
        private OpenRCT2ApiService Api { get; set; }

        [Parameter]
        public string Owner { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                user = await Api.Client.User.Get(Owner);
            }
            catch (Exception ex)
            {
                error = ex;
            }
        }
    }
}
