namespace Raccoons.Networking.Api.WebRequests.UnityWebRequests
{
    public class UnityWebRequestProvider : IWebRequestProvider
    {
        public IWebRequestCreator Provide()
        {
            return new UnityWebRequestBuilder();
        }
    }
}
