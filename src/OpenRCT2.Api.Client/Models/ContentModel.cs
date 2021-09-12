using System;

namespace OpenRCT2.Api.Client.Models
{
    public class ContentModel
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string FileUrl { get; set; }
        public ContentVisibility ContentVisibility { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public bool CanEdit { get; set; }
    }
}
