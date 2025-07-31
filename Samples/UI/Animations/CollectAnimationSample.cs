using Raccoons.UI.Animations;
using Raccoons.UI.Animations.CollectAnimationSettings;
using Raccoons.UI.Animations.CollectAnimationSettings.Assets;
using UnityEngine;
using Zenject;

namespace Raccoons.Core.Samples.UI.Animations
{
    public class CollectAnimationSample : MonoBehaviour
    {
        [Header("Example Configuration")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private int coinAmount = 10;
        
        [Header("Animation Settings")]
        [SerializeField] private CollectAnimationSettingsAsset settingsAsset;
        [SerializeField] private KeyCode keyCode = KeyCode.Space;
        
        private CollectAnimationSystem _collectAnimationSystem;

        [Inject]
        private void Construct(CollectAnimationSystem collectAnimationSystem)
        {
            _collectAnimationSystem = collectAnimationSystem;
        }

        private async void Update()
        {
            if (Input.GetKeyDown(keyCode))
            {
                await _collectAnimationSystem.Emit(coinAmount, startPoint.position, endPoint.position, settingsAsset);
            }
        }
    }
} 