using System;
using System.Net;

namespace OpenRCT2.Api.Client
{
    public class OpenRCT2ApiClientStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public object Content { get; }

        internal OpenRCT2ApiClientStatusCodeException(HttpStatusCode statusCode, object content = null)
            : base($"{(int)statusCode} ({statusCode}) status code returned.")
        {
            StatusCode = statusCode;
            Content = content;
        }
    }

    public class OpenRCT2ApiClientStatusCodeException<T> : OpenRCT2ApiClientStatusCodeException
    {
        public new T Content => (T)base.Content;

        internal OpenRCT2ApiClientStatusCodeException(HttpStatusCode statusCode, T content)
            : base(statusCode, content)
        {
        }
    }
}
