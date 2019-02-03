using Newtonsoft.Json;

namespace OpenRCT2.DB.Models
{
    [Table(TableNames.LegacyObjects)]
    public class LegacyRctObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public int NeDesignId { get; set; }
        [SecondaryIndex]
        public string Name { get; set; }
    }
}
