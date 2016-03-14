using System;
using OpenRCT2.API.JsonModels;

namespace OpenRCT2.API.Models
{
    public class Server
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime LastHeartbeat { get; set; }

        public ServerAddressList Addresses { get; set; }
        public int Port { get; set; }

        public string Version { get; set; }
        public bool RequiresPassword { get; set; }
        public int Players { get; set; }
        public int MaxPlayers { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public JServerProviderInfo Provider { get; set; }

        public JGameInfo GameInfo { get; set; }
    }
}
