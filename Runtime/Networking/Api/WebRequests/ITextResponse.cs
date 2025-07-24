namespace Raccoons.Networking.Api.WebRequests
{
    public interface ITextResponse
    {
        string GetText();
    }

    public interface IByteResponse
    {
        byte[] GetBytes();
    }
}
