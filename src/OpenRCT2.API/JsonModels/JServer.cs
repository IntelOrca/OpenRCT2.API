using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRCT2.API.Models;

namespace OpenRCT2.API.JsonModels
{
    public class JServerAddressList
    {
        public string[] v4 { get; set; }
        public string[] v6 { get; set; }

        public static JServerAddressList FromServerAddressList(ServerAddressList serverAddressList)
        {
            return new JServerAddressList()
            {
                v4 = serverAddressList?.IPv4 ?? new string[0],
                v6 = serverAddressList?.IPv6 ?? new string[0]
            };
        }
    }

    public class JServer
    {
        public JServerAddressList ip { get; set; }
        public int port { get; set; }

        public string version { get; set; }
        public bool requiresPassword { get; set; }
        public int players { get; set; }
        public int maxPlayers { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public JServerProviderInfo provider { get; set; }

        public JGameInfo gameInfo { get; set; }

        public Server ToServer()
        {
            return new Server()
            {
                Addresses = new ServerAddressList()
                {
                    IPv4 = ip.v4,
                    IPv6 = ip.v6
                },
                Port = port,
                Version = version,
                RequiresPassword = requiresPassword,
                Players = players,
                MaxPlayers = maxPlayers,
                Name = name,
                Description = description,
                Provider = provider,
                GameInfo = gameInfo
            };
        }

        public static JServer FromServer(Server server)
        {
            return new JServer()
            {
                ip = JServerAddressList.FromServerAddressList(server.Addresses),
                port = server.Port,
                version = server.Version,
                requiresPassword = server.RequiresPassword,
                players = server.Players,
                maxPlayers = server.MaxPlayers,
                name = server.Name,
                description = server.Description,
                provider = server.Provider,
                gameInfo = server.GameInfo
            };
        }
    }
}
