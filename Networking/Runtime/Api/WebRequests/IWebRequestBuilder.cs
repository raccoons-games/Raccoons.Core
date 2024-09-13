using System;
using System.Threading.Tasks;

namespace Raccoons.Networking.Api.WebRequests
{
    public interface IWebRequestBuilder : IDisposable
    {
        public string Method { get; }
        public string Url { get; }
        Task<IBaseWebResponse> Send();
        IWebRequestBuilder SetBody(string body);
        IWebRequestBuilder SetContentType(string contentType);
        IWebRequestBuilder SetHeader(string header, string value);
        IWebRequestBuilder ToFile(string file);
        IWebRequestBuilder UploadFile(string file);
    }
}