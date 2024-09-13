using UnityEngine.Networking;

namespace Raccoons.Networking.Api.WebRequests.UnityWebRequests
{
    public class BytesUnityWebRequestResponse : UnityWebRequestResponse, IByteResponse
    {
        public BytesUnityWebRequestResponse(IWebRequestBuilder webRequestBuilder,
            UnityWebRequest request) : base(webRequestBuilder, request)
        {
        }

        public byte[] GetBytes()
        {
            return Request.downloadHandler.data;
        }
    }
}
