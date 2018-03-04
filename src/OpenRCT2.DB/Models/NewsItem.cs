using System;
using Newtonsoft.Json;

namespace OpenRCT2.DB.Models
{
    [Table(TableNames.NewsItems)]
    public class NewsItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        [SecondaryIndex]
        public DateTime? Published { get; set; }
        public string Title { get; set; }
        public string AuthorId { get; set; }
        public string Html { get; set; }
    }

    public class NewsItemExtended : NewsItem
    {
        public string AuthorName { get; set; }
    }
}
