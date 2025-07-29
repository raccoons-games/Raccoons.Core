using UnityEngine.Networking;

namespace Raccoons.Networking.Api.WebRequests.UnityWebRequests
{
    public class TextUnityWebRequestResponse : BytesUnityWebRequestResponse, ITextResponse
    {
        public TextUnityWebRequestResponse(IWebRequestBuilder webRequestBuilder, UnityWebRequest request) 
            : base(webRequestBuilder, request)
        {
        }

        public string GetText()
        {
            return Request.downloadHandler.text;
        }
    }
}
