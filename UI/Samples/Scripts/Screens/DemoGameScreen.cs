using Raccoons.UI.Screens;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Raccoons.UI.Samples.Screens
{
    public class DemoGameScreen : BaseScreen
    {
        [SerializeField]
        private Button _mainMenuButton;

        private IDemoScreensManager _demoScreensManager;

        [Inject]
        private void Construct(IDemoScreensManager demoScreensManager)
        {
            _demoScreensManager = demoScreensManager;
        }

        private void Awake()
        {
            _mainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
        }

        public async Task Init(float delay, CancellationToken cancellationToken = default)
        {
            await Task.Delay((int)(delay * 1000));
        }
        private void OnMainMenuButtonClick()
        {
            _demoScreensManager.OpenMainScreen();
            Close();
        }
    }
}
