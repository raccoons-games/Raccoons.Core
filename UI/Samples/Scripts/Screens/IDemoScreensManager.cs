using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.UI.Samples.Screens
{
    public interface IDemoScreensManager
    {
        void OpenMainScreen();
        Task OpenGameScreen(float delay,CancellationToken cancellationToken = default);
    }
}
