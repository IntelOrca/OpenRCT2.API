using System.IO;

namespace OpenRCT2.Api.Client.Models
{
    public class UploadContentRequest
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ContentVisibility Visibility { get; set; }
        public string FileName { get; set; }
        public Stream File { get; set; }
        public string ImageFileName { get; set; }
        public Stream Image { get; set; }
    }
}
