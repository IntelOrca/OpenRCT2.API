namespace OpenRCT2.API.Models.Requests
{
    public class WriteNewsItemRequest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Html { get; set; }
        public bool? Published { get; set; }
    }
}
