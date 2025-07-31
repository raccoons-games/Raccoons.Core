using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Raccoons.UI.Animations
{
    public class CollectAnimationSample : MonoBehaviour
    {
        [Header("Example Configuration")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private int coinAmount = 10;
        
        [Inject] private CollectAnimationSystem _animationSystem;
        
        [ContextMenu("1 Test Default Animation")]
        public async void TestDefaultAnimation()
        {
            await _animationSystem.Launch(coinAmount, startPoint.position, endPoint.position, AnimationType.Default);
        }
        
        [ContextMenu("2 Test Scatter Animation")]
        public async void TestScatterAnimation()
        {
            if (startPoint == null || endPoint == null) return;
            
            await _animationSystem.Launch(coinAmount, startPoint.position, endPoint.position, AnimationType.Scatter);
        }
        
        [ContextMenu("3 Test Custom Settings")]
        public async void TestCustomSettings()
        {
            if (startPoint == null || endPoint == null) return;
            
            var customSettings = ScriptableObject.CreateInstance<CollectAnimationSettings>();
            
            await _animationSystem.Launch(coinAmount, startPoint.position, endPoint.position, customSettings);
        }
        
        private void Update()
        {
            // Example: Press Space to trigger animation
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TestDefaultAnimation();
            }
        }
    }
} 