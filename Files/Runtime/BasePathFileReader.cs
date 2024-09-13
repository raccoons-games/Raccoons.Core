using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.Files
{
    [CreateAssetMenu(fileName = "PathFileReader", menuName = "Raccoons/Files/PathFileReader")]
    public abstract class BasePathFileReader: IFileReader
    {
        private string _path;

        public virtual string FilePath { get => _path; }
        public BasePathFileReader(string filePath)
        {
            _path = filePath;
        }

        public abstract byte[] ReadAllBytes();
        public abstract Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default);
        public abstract string ReadAllText();
        public abstract Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default);

        public abstract StreamReader CreateStreamReader();
    }
}