using Raccoons.Networking.Api.WebRequests.UnityWebRequests;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Raccoons.Networking.Api.WebRequests.UnityWebRequests
{

    public class UnityWebRequestBuilder : BaseWebRequestCreator, IWebRequestBuilder
    {
        public UnityWebRequest UnityWebRequest { get; private set; }

        public string Method => UnityWebRequest.method;

        public string Url => UnityWebRequest.url;

        IWebRequestBuilder IWebRequestBuilder.SetHeader(string header, string value)
        {
            UnityWebRequest?.SetRequestHeader(header, value);
            return this;
        }

        public override IWebRequestBuilder Delete(string url)
        {
            UnityWebRequest = UnityWebRequest.Delete(url);
            return this;
        }

        public override IWebRequestBuilder Get(string url)
        {
            UnityWebRequest = UnityWebRequest.Get(url);
            return this;
        }

        public override IWebRequestBuilder Post(string url)
        {
            UnityWebRequest = UnityWebRequest.PostWwwForm(url,"");
            return this;
        }

        public override IWebRequestBuilder Put(string url)
        {
            UnityWebRequest = UnityWebRequest.Put(url, "");
            return this;
        }

        IWebRequestBuilder IWebRequestBuilder.SetBody(string body)
        {
            UnityWebRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            return this;
        }

        IWebRequestBuilder IWebRequestBuilder.UploadFile(string file)
        {
            UnityWebRequest.uploadHandler = new UploadHandlerFile(file);
            return this;
        }

        IWebRequestBuilder IWebRequestBuilder.SetContentType(string contentType)
        {
            (this as IWebRequestBuilder).SetHeader("Content-Type", contentType);
            return this;
        }

        IWebRequestBuilder IWebRequestBuilder.ToFile(string file)
        {
            UnityWebRequest.downloadHandler = new DownloadHandlerFile(file);
            return this;
        }

        async Task<IBaseWebResponse> IWebRequestBuilder.Send()
        {
            await UnityWebRequest.SendWebRequest();
            if (UnityWebRequest.method != UnityWebRequest.kHttpVerbHEAD)
            {
                return new TextUnityWebRequestResponse(this, UnityWebRequest);
            }
            else
            {
                return new UnityWebRequestResponse(this, UnityWebRequest);
            }
        }

        public override IWebRequestBuilder Head(string url)
        {
            UnityWebRequest = UnityWebRequest.Head(url);
            return this;
        }

        public void Dispose()
        {
            UnityWebRequest?.Dispose();
        }
    }
}
