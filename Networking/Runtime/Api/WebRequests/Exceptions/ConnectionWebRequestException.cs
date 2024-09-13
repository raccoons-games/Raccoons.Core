using Raccoons.Networking.Api.WebRequests;

namespace Raccoons.Networking.Runtime.Api.WebRequests.Exceptions
{
    public class ConnectionWebRequestException : WebRequestException
    {
        public ConnectionWebRequestException(IBaseWebResponse webResponse, string message = "") 
            : base(webResponse, $"Failed to perform web request: Connection Failed\n{webResponse.WebRequestBuilder.Method} {webResponse.WebRequestBuilder.Url}\n{message}")
        {
        }
    }
}
