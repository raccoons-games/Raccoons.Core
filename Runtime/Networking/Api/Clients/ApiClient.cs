using Raccoons.Networking.Api.Configs;
using Raccoons.Networking.Api.WebRequests;
using Raccoons.Serialization;
using System.Threading.Tasks;

namespace Raccoons.Networking.Api.Clients
{
    public class ApiClient<TConfig> : IWebRequestCreator
        where TConfig : IApiConfig
    {
        public ApiClient(TConfig apiConfig, ISerializer serializer, IWebRequestProvider webRequestProvider)
        {
            ApiConfig = apiConfig;
            Serializer = serializer;
            WebRequestProvider = webRequestProvider;
        }

        public TConfig ApiConfig { get; }
        public ISerializer Serializer { get; }
        public IWebRequestProvider WebRequestProvider { get; }

        public virtual IWebRequestCreator CreateWebRequest()
        {
            return WebRequestProvider.Provide();
        }

        public virtual IWebRequestBuilder Delete(string url)
        {
            return CreateWebRequest().Delete(url);
        }

        public virtual IWebRequestBuilder Get(string url)
        {
            return CreateWebRequest().Get(url);
        }

        public virtual IWebRequestBuilder Head(string url)
        {
            return CreateWebRequest().Head(url);
        }

        public virtual IWebRequestBuilder Post(string url)
        {
            return CreateWebRequest().Post(url);
        }

        public virtual IWebRequestBuilder Put(string url)
        {
            return CreateWebRequest().Put(url);
        }
       
        public async Task<TResult> Get<TResult>(string url)
        {
            var response = await Get(url).Send();
            using (response)
            {
                return Serializer.Deserialize<TResult>(((ITextResponse)response).GetText());
            }
        }

        public async Task<TResult> Put<TBody, TResult>(string url, TBody body)
        {
            var bodyStr = Serializer.Serialize(body);
            var response = await Put(url).SetBody(bodyStr).Send();
            using (response)
            {
                return Serializer.Deserialize<TResult>((response as ITextResponse).GetText());
            }
        }

        public async Task<TResult> Post<TBody, TResult>(string url, TBody body)
        {
            var bodyStr = Serializer.Serialize(body);
            var response = await Post(url).SetBody(bodyStr).Send();
            using (response)
            {
                return Serializer.Deserialize<TResult>((response as ITextResponse).GetText());
            }
        }

        public async Task<TResult> Delete<TResult>(string url)
        {
            var response = await Delete(url).Send();
            using (response)
            {
                return Serializer.Deserialize<TResult>((response as ITextResponse).GetText());
            }
        }

        public virtual string BuildUrl(string route, params object[] args)
        {
            return string.Format(ApiConfig.GetApiUrl() + route, args);
        }
    }
}