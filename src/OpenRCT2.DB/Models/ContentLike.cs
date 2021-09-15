using System;
using Newtonsoft.Json;

namespace OpenRCT2.DB.Models
{
    [Table(TableNames.ContentLikes)]
    public class ContentLike
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [SecondaryIndex]
        public string ContentId { get; set; }
        [SecondaryIndex]
        public string UserId { get; set; }
        public DateTime When { get; set; }
    }
}
