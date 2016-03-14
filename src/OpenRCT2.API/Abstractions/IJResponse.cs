namespace OpenRCT2.API.Abstractions
{
    public interface IJResponse
    {
        string status { get; set; }
        string message { get; set; }
    }
}
