namespace OpenRCT2.API.JsonModels
{
    public class ServerGameInfo
    {
        /// <summary>
        /// May be an int or an object containing x and y.
        /// </summary>
        public object MapSize { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Guests { get; set; }
        public int ParkValue { get; set; }
        public int Cash { get; set; }
    }
}
