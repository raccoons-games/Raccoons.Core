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
    public class DemoMainMenuScreen : BaseScreen
    {
        [SerializeField]
        private Button _playButton;
        [Header("PopUps")]
        [SerializeField]
        private GameObject _waitPopUp;

        private IDemoScreensManager _demoScreensManager;

        [Inject]
        private void Construct(IDemoScreensManager demoScreensManager)
        {
            _demoScreensManager = demoScreensManager;
        }

        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayButtonClick);
        }

        public override async Task Open(CancellationToken cancellationToken = default)
        {
            _waitPopUp.gameObject.SetActive(false);
           await base.Open();
        }

        private async void OnPlayButtonClick()
        {
            _waitPopUp.gameObject.SetActive(true);
            await _demoScreensManager.OpenGameScreen(2);
            _waitPopUp.gameObject.SetActive(false);
            await Close();
        }
    }
}
