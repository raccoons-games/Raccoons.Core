using System;

namespace Raccoons.Networking.Api.WebRequests
{
    public interface IBaseWebResponse : IDisposable
    {
        public int Code { get; }
        public IWebRequestBuilder WebRequestBuilder { get; }
        public string GetHeader(string header);
    }
}
