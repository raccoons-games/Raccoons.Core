using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.Files
{
    public class TextAssetFileReader : IFileReader
    {
        private TextAsset _textAsset;

        public TextAssetFileReader(TextAsset textAsset)
        {
            _textAsset = textAsset;
        }

        public Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ReadAllBytes());
        }

        public byte[] ReadAllBytes()
        {
            return _textAsset.bytes;
        }

        public string ReadAllText()
        {
            return _textAsset.text;
        }

        public Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ReadAllText());
        }

        public StreamReader CreateStreamReader()
        {
            return new StreamReader(new MemoryStream(ReadAllBytes()));
        }
    }
}