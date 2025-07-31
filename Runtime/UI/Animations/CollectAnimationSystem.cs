using System;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Raccoons.Factories;
using Raccoons.Factories.Pools;
using Raccoons.UI.Animations.CollectAnimationSettings;
using Object = UnityEngine.Object;

namespace Raccoons.UI.Animations
{
    public class CollectAnimationSystem : MonoBehaviour
    {
        [SerializeField] private CollectAnimationSettingsAsset defaultSettings;
        [SerializeField] private BaseFactory factory;
        private CollectAnimation _currentAnimation;

        [Header("Temp")]
        [SerializeField] private CollectAnimationItem itemPrefab;
        
        public bool IsAnimating => _currentAnimation?.IsPlaying ?? false;
        private void OnDestroy() => StopCurrentAnimation();

        public async UniTask<CollectAnimation> Emit(int count, Vector3 startPosition, Vector3 endPosition, CollectAnimationSettingsAsset settings = null)
        {
            if (count <= 0)
                return new CollectAnimation();
            
            if (_currentAnimation != null && _currentAnimation.IsPlaying)
            {
                StopCurrentAnimation();
            }
            
            _currentAnimation = new CollectAnimation();
            CollectAnimationSettingsAsset animationSettings = settings ?? defaultSettings;
            
            _currentAnimation.Initialize(count, startPosition, endPosition, animationSettings);
            CollectAnimation result =  _currentAnimation;
            
            for (var i = 0; i < count; i++)
            {
                var item = Object.Instantiate(itemPrefab).GetComponent<CollectAnimationItem>();
                //var item = factory.Create().GetComponent<CollectAnimationItem>();
                if (item == null)
                    continue;
                
                _currentAnimation.AddItem(item);
                item.SetActive(true);
                
                AnimateItem(item, i).Forget();
                
                if (animationSettings.DelayBetweenItems > 0 && i < count - 1)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(animationSettings.DelayBetweenItems));
                }
            }
            
            return result;
        }
        
        
        private async UniTaskVoid AnimateItem(CollectAnimationItem item, int itemIndex)
        {
            try
            {
                CollectAnimationSettingsAsset settings = _currentAnimation.Settings;
                Vector3 startPos = _currentAnimation.StartPosition;
                Vector3 endPos = _currentAnimation.EndPosition;
                
                item.SetPosition(startPos);
                item.SetLocalScale(settings.StartScale);
                item.SetAlpha(settings.StartAlpha);
                item.PlayParticle();
                
                await settings.AnimateItem(item, startPos, endPos);
                
                item.StopParticle();
                item.SetActive(false);
                item.Return();

                _currentAnimation.ItemCompleted();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error animating item {itemIndex}: {e.Message}");
                _currentAnimation.ItemCompleted();
            }
        }
        

        private void StopCurrentAnimation()
        {
            if (_currentAnimation == null)
                return;
            
            foreach (CollectAnimationItem item in _currentAnimation.Items
                         .Where(item => item != null && item.GetTransform() != null))
            {
                item.GetTransform().DOKill();
            }

            foreach (CollectAnimationItem item in _currentAnimation.Items)
            {
                if (item is PoolObject poolObject)
                {
                    poolObject.Return();
                }
            }
            
            _currentAnimation.Clear();
            _currentAnimation.Stop();
        }
    }
} 