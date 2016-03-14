namespace OpenRCT2.API.JsonModels
{
    public class JServerInfo
    {
        public string name { get; set; }
        public bool requiresPassword { get; set; }
        public string version { get; set; }
        public int players { get; set; }
        public int maxPlayers { get; set; }
        public string description { get; set; }
        public bool dedicated { get; set; }
        public JServerProviderInfo provider { get; set; }
    }
}
