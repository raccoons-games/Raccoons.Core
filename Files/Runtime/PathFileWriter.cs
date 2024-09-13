using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Files
{
    public class PathFileWriter : IFileWriter
    {
        private string _path;

        public PathFileWriter(string path)
        {
            _path = path;
        }

        public virtual string FilePath { get => _path; }

        public StreamWriter CreateStreamWriter()
        {
            return new StreamWriter(FilePath);
        }

        public void WriteAllBytes(byte[] bytes)
        {
            File.WriteAllBytes(FilePath, bytes);
        }

        public Task WriteAllBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            return File.WriteAllBytesAsync(FilePath, bytes, cancellationToken);
        }

        public void WriteAllText(string text)
        {
            File.WriteAllText(FilePath, text);
        }

        public Task WriteAllTextAsync(string text, CancellationToken cancellationToken = default)
        {
            return File.WriteAllTextAsync(FilePath, text, cancellationToken);
        }
    }
}