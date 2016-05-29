using System;

namespace OpenRCT2.API.OpenRCT2org
{
    public class OpenRCT2orgException : Exception
    {
        public ErrorCodes Error { get; }

        public OpenRCT2orgException(JResponse response) : this(response.error, response.errorMessage) { }

        public OpenRCT2orgException(ErrorCodes error, string message) : base(message)
        {
            Error = error;
        }
    }
}
