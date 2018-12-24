using System;
using Newtonsoft.Json;
using OpenRCT2.API.JsonModels;

namespace OpenRCT2.API.Models
{
    public class Server
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public string Token { get; set; }
        [JsonIgnore]
        public DateTime LastHeartbeat { get; set; }

        [JsonProperty("ip")]
        public ServerAddressList Addresses { get; set; }
        public int Port { get; set; }

        public string Version { get; set; }
        public bool RequiresPassword { get; set; }
        public int Players { get; set; }
        public int MaxPlayers { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public ServerProviderInfo Provider { get; set; }

        public ServerGameInfo GameInfo { get; set; }
    }
}
