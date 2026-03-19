using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Raccoons.Infrastructure
{
    public class SceneEntryPoint : MonoBehaviour
    {
        private List<IGlobalService> _globalServices;
        private List<ISceneService> _sceneServices;

        private CancellationTokenSource _cancellationTokenSource;

        [Inject]
        private void Construct(List<IGlobalService> globalServices, List<ISceneService> sceneServices)
        {
            _sceneServices = sceneServices;
            _globalServices = globalServices;
        }


        private async void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                if (!ProjectEntryPoint.GlobalServicesInitialized)
                {
                    await InitializeServicesAsync(_globalServices, cancellationToken);
                    ProjectEntryPoint.MarkGlobalServicesInitialized();
                }

                await InitializeServicesAsync(_sceneServices, cancellationToken);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task InitializeServicesAsync(IEnumerable<IService> services, CancellationToken cancellationToken)
        {
            var sorted = new List<IService>(services);
            sorted.Sort((a, b) => a.InitOrder.CompareTo(b.InitOrder));

            var pendingTasks = new List<Task>();

            foreach (var service in sorted)
            {
                var task = service.InitializeAsync(cancellationToken);
                if (service.Awaitable)
                    await task;
                else
                    pendingTasks.Add(task);
            }

            await Task.WhenAll(pendingTasks);
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}
