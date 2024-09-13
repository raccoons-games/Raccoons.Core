using Raccoons.Networking.Api.WebRequests;
using System;

namespace Raccoons.Networking.Runtime.Api.WebRequests.Exceptions
{
    public class BadWebRequestException : WebRequestException
    {
        public BadWebRequestException(IBaseWebResponse webResponse, string message = "") 
            : base(webResponse, $"Failed to perform web request: {webResponse.Code} ({WebRequestStatusCodes.GetStatusCodeName(webResponse.Code)})\n{webResponse.WebRequestBuilder.Method} {webResponse.WebRequestBuilder.Url}\n{message}")
        {
        }

        
    }
}
