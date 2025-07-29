using Raccoons.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Files
{
    public class PathFileReader : BasePathFileReader
    {
        public PathFileReader(string filePath) : base(filePath)
        {
        }

        public override StreamReader CreateStreamReader()
        {
            return new StreamReader(FilePath);
        }

        public override byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(FilePath);
        }

        public override Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default)
        {
            return File.ReadAllBytesAsync(FilePath);
        }

        public override string ReadAllText()
        {
            return File.ReadAllText(FilePath);
        }

        public override Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default)
        {
            return File.ReadAllTextAsync(FilePath, cancellationToken);
        }
    }
}