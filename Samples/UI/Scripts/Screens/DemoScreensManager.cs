using Raccoons.UI.Screens;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.UI.Samples.Screens
{
    public class DemoScreensManager : BaseScreenManager, IDemoScreensManager
    {
        public async Task OpenGameScreen(float delay, CancellationToken cancellationToken = default)
        {
            DemoGameScreen screen = GetScreen<DemoGameScreen>();
            await screen.Init(delay, cancellationToken);
            screen?.Open();
        }

        public void OpenMainScreen()
        {
            DemoMainMenuScreen screen = GetScreen<DemoMainMenuScreen>();
            screen?.Open();
        }
    }
}