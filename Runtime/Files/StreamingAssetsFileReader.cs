using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Raccoons.Files
{
    public class StreamingAssetsFileReader : BasePathFileReader
    {
        public StreamingAssetsFileReader(string filePath) : base(filePath)
        {
        }

        public override string FilePath => System.IO.Path.Combine(Application.streamingAssetsPath, base.FilePath);

        public override byte[] ReadAllBytes()
        {
            Task<byte[]> task = ReadAllBytesAsync();
            task.Wait();
            return task.Result;
        }

        public override Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<byte[]> tsc = new();
            RequestFile((handler) =>
            {
                tsc.SetResult(handler.data);
            });
            return tsc.Task;
        }

        public override string ReadAllText()
        {
            Task<string> task = ReadAllTextAsync();
            task.Wait();
            return task.Result;
        }

        public override Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<string> tsc = new();
            RequestFile((handler) =>
            {
                tsc.SetResult(handler.text); 
            });
            return tsc.Task;
        }

        private void RequestFile(System.Action<DownloadHandler> handling)
        {
            UnityWebRequest unityWebRequest = UnityWebRequest.Get($"file:///{FilePath}");
            var requestAsync = unityWebRequest.SendWebRequest();
            if (unityWebRequest.isDone)
            {
                handling?.Invoke(unityWebRequest.downloadHandler);
                requestAsync.webRequest.Dispose();
            }
            else
            {
                requestAsync.completed += asyncOp =>
                {
                    handling?.Invoke(requestAsync.webRequest.downloadHandler);
                    requestAsync.webRequest.Dispose();
                };
            }
        }

        public override StreamReader CreateStreamReader()
        {
            return new StreamReader(new MemoryStream(ReadAllBytes()));
        }
    }
}