using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace OpenRCT2.DB.Models
{
    [Table(TableNames.Content)]
    public class ContentItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [SecondaryIndex]
        public string OwnerId { get; set; }
        [SecondaryIndex]
        public string Name { get; set; }
        [SecondaryIndex]
        public int ContentType { get; set; }
        public string Image { get; set; }
        public string File { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
