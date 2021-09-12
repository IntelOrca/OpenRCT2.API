namespace OpenRCT2.Api.Client.Models
{
    public class ContentModel
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string FileUrl { get; set; }
        public ContentVisibility ContentVisibility { get; set; }
    }
}
