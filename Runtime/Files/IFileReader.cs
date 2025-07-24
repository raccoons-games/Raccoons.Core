using Raccoons.Serialization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Files
{
    public interface IFileReader
    {
        string ReadAllText();
        Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default);
        byte[] ReadAllBytes();
        Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default);
        StreamReader CreateStreamReader();

    }
}