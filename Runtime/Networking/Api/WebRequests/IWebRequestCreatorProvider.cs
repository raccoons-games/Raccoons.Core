namespace Raccoons.Networking.Api.WebRequests
{
    public interface IWebRequestProvider
    {
        IWebRequestCreator Provide();
    }
}