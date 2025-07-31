using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Raccoons.UI.Animations.CollectAnimationSettings.Assets
{
    [CreateAssetMenu(fileName = "BounceCollectAnimationSettings", menuName = "Raccoons/UI/Collect Animation/Bounce Settings")]
    public class BounceCollectAnimationSettingsAsset : CollectAnimationSettingsAsset
    {
        [Header("Bounce Settings")]
        [SerializeField] private float bounceScale = 1.2f;
        [SerializeField] private float bounceDuration = 0.5f;
        
        public override async UniTask AnimateItem(CollectAnimationItem item, Vector3 startPosition, Vector3 endPosition)
        {
            Sequence sequence = DOTween.Sequence();
            
            sequence.Join(item.GetTransform().DOMove(endPosition, animationDuration)
                .SetEase(positionCurve));
            
            sequence.Join(item.GetTransform().DOScale(endScale * bounceScale, bounceDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    item.GetTransform().DOScale(endScale, animationDuration - bounceDuration)
                        .SetEase(Ease.InQuad);
                }));
            
            sequence.Join(DOTween.To(() => startAlpha, item.SetAlpha, endAlpha, animationDuration)
                .SetEase(alphaCurve));
            
            await sequence.AsyncWaitForCompletion();
        }
    }
} 