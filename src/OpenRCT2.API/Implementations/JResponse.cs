using OpenRCT2.API.Abstractions;

namespace OpenRCT2.API.Implementations
{
    public class JResponse : IJResponse
    {
        public object status { get; set; }

        public string message { get; set; }

        public static JResponse OK()
        {
            return new JResponse()
            {
                status = JStatus.OK
            };
        }

        public static JResponse Error(string message)
        {
            return new JResponse()
            {
                status = JStatus.Error,
                message = message
            };
        }
    }
}
