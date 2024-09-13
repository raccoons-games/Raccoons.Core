using Raccoons.Networking.Api.WebRequests;

namespace Raccoons.Networking.Runtime.Api.WebRequests.Exceptions
{
    public class ServerErrorWebRequestException : WebRequestException
    {
        public ServerErrorWebRequestException(IBaseWebResponse webResponse, string message = "") 
            : base(webResponse, $"Failed to perform web request: {webResponse.Code} ({WebRequestStatusCodes.GetStatusCodeName(webResponse.Code)})\n{webResponse.WebRequestBuilder.Method} {webResponse.WebRequestBuilder.Url}\n{message}")
        {
        }
    }
}
