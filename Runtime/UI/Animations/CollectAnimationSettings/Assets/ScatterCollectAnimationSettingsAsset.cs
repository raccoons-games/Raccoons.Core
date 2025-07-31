using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Raccoons.UI.Animations.CollectAnimationSettings.Assets
{
    [CreateAssetMenu(fileName = "ScatterCollectAnimationSettings", menuName = "Raccoons/UI/Collect Animation/Scatter Settings")]
    public class ScatterCollectAnimationSettingsAsset : CollectAnimationSettingsAsset
    {
        [Header("Scatter Settings")]
        [SerializeField] private float scatterRadius = 100f;
        [SerializeField] private float scatterForce = 200f;
        [SerializeField] private float gravity = 500f;
        [SerializeField] private float scatterDuration = 0.3f;
        
        public override async UniTask AnimateItem(CollectAnimationItem item, Vector3 startPosition, Vector3 endPosition)
        {
            Sequence sequence = DOTween.Sequence();
            
            Vector3 scatterPos = startPosition + Random.insideUnitSphere * scatterRadius;

            Transform transform = item.GetTransform();
            sequence.Append(transform.DOMove(scatterPos, scatterDuration)
                .SetEase(Ease.OutQuad));
            
            sequence.Append(transform.DOMove(endPosition, animationDuration - scatterDuration)
                .SetEase(positionCurve));
            
            sequence.Join(transform.DOScale(endScale, animationDuration)
                .SetEase(scaleCurve));
            
            sequence.Join(DOTween.To(() => startAlpha, item.SetAlpha, endAlpha, animationDuration)
                .SetEase(alphaCurve));
            
            await sequence.AsyncWaitForCompletion();
        }
    }
} 