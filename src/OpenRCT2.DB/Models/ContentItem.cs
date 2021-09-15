using System;
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
        public string Name { get; set; }
        [SecondaryIndex]
        public string NormalisedName { get; set; }
        [SecondaryIndex]
        public int ContentType { get; set; }
        public string ImageKey { get; set; }
        public string FileKey { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [SecondaryIndex]
        public ContentVisibility Visibility { get; set; }
        [SecondaryIndex]
        public int LikeCount { get; set; }
    }

    public class ContentItemExtended : ContentItem
    {
        public string Owner { get; set; }
        public bool HasLiked { get; set; }
    }
}
