using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Raccoons.Networking.Api.WebRequests.UnityWebRequests
{
    public static class UnityWebRequestExtensions
    {
        public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation reqOp)
        {
            TaskCompletionSource<UnityWebRequest.Result> tsc = new();
            if (reqOp.isDone)
            {
                tsc.TrySetResult(reqOp.webRequest.result);
            }
            else
            {
                reqOp.completed += asyncOp => tsc.TrySetResult(reqOp.webRequest.result);
            }
            return tsc.Task.GetAwaiter();
        }
    }
}