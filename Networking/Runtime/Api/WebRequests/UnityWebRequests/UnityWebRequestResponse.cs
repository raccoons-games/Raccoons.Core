using Raccoons.Networking.Runtime.Api.WebRequests;
using Raccoons.Networking.Runtime.Api.WebRequests.Exceptions;
using UnityEngine;
using UnityEngine.Networking;

namespace Raccoons.Networking.Api.WebRequests.UnityWebRequests
{
    public class UnityWebRequestResponse : IBaseWebResponse
    {
        public UnityWebRequestResponse(IWebRequestBuilder webRequestBuilder, UnityWebRequest request)
        {
            WebRequestBuilder = webRequestBuilder;
            Request = request;
            switch (Request.result)
            {
                case UnityWebRequest.Result.InProgress:
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log($"Web request succeded: {Code} ({WebRequestStatusCodes.GetStatusCodeName(Code)})\n{WebRequestBuilder.Method} {WebRequestBuilder.Url}");
                    break;
                case UnityWebRequest.Result.ConnectionError:
                    throw new ConnectionWebRequestException(this);
                case UnityWebRequest.Result.ProtocolError:
                    throw new BadWebRequestException(this);
                case UnityWebRequest.Result.DataProcessingError:
                    throw new ServerErrorWebRequestException(this);
            }
        }

        public IWebRequestBuilder WebRequestBuilder { get; }
        public UnityWebRequest Request { get; private set; }
        public int Code { get => (int)Request.responseCode; }

        public void Dispose()
        {
            Request?.Dispose();
        }

        public string GetHeader(string header)
        {
            return Request.GetResponseHeader(header);
        }
    }
}
