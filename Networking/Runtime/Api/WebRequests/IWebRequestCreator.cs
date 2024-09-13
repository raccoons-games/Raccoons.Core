namespace Raccoons.Networking.Api.WebRequests
{
    public interface IWebRequestCreator
    {
        IWebRequestBuilder Delete(string url);
        IWebRequestBuilder Get(string url);
        IWebRequestBuilder Head(string url);
        IWebRequestBuilder Post(string url);
        IWebRequestBuilder Put(string url);
    }
}