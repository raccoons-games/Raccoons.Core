using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Files
{
    public interface IFileWriter
    {
        void WriteAllText(string text);
        Task WriteAllTextAsync(string text, CancellationToken cancellationToken = default);
        void WriteAllBytes(byte[] bytes);
        Task WriteAllBytesAsync(byte[] bytes, CancellationToken cancellationToken = default);
        StreamWriter CreateStreamWriter();
    }
}