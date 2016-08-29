namespace OpenRCT2.API.Abstractions
{
    public interface IJResponse
    {
        object status { get; set; }
        string message { get; set; }
    }
}
