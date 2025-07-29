using Raccoons.Serialization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.Files.Instances
{
    public abstract class BaseFileAsset : ScriptableObject, IFileReader, IFileWriter
    {
        public virtual IFileReader Reader { get; private set; }
        public virtual IFileWriter Writer { get; private set; }

        protected virtual void OnEnable()
        {
            Reader = CreateReader();
            Writer = CreateWriter();
        }

        protected abstract IFileWriter CreateWriter();
        protected abstract IFileReader CreateReader();

        public string ReadAllText()
        {
            return Reader.ReadAllText();
        }

        public Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default)
        {
            return Reader.ReadAllTextAsync(cancellationToken);
        }

        public byte[] ReadAllBytes()
        {
            return Reader.ReadAllBytes();
        }

        public Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default)
        {
            return Reader.ReadAllBytesAsync(cancellationToken);
        }

        public StreamReader CreateStreamReader()
        {
            return Reader.CreateStreamReader();
        }

        public void WriteAllText(string text)
        {
            Writer.WriteAllText(text);
        }

        public Task WriteAllTextAsync(string text, CancellationToken cancellationToken = default)
        {
            return Writer.WriteAllTextAsync(text, cancellationToken);
        }

        public void WriteAllBytes(byte[] bytes)
        {
            Writer.WriteAllBytes(bytes);
        }

        public Task WriteAllBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            return Writer.WriteAllBytesAsync(bytes, cancellationToken);
        }

        public StreamWriter CreateStreamWriter()
        {
            return Writer.CreateStreamWriter();
        }
    }
}