using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Raccoons.UI.Animations.CollectAnimationSettings.Assets
{
    [CreateAssetMenu(fileName = "SpiralCollectAnimationSettings", menuName = "Raccoons/UI/Collect Animation/Spiral Settings")]
    public class SpiralCollectAnimationSettingsAsset : CollectAnimationSettingsAsset
    {
        [Header("Spiral Settings")]
        [SerializeField] private float spiralRadius = 50f;
        [SerializeField] private int spiralTurns = 2;
        
        public override async UniTask AnimateItem(CollectAnimationItem item, Vector3 startPosition, Vector3 endPosition)
        {
            Sequence sequence = DOTween.Sequence();
            
            Vector3 direction = (endPosition - startPosition).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            
            for (var i = 0; i < spiralTurns; i++)
            {
                float angle = (i * 360f / spiralTurns) * Mathf.Deg2Rad;
                float radius = spiralRadius * (1f - (float)i / spiralTurns);
                
                Vector3 spiralPoint = startPosition + 
                    direction * (Vector3.Distance(startPosition, endPosition) * i / spiralTurns) +
                    perpendicular * (Mathf.Cos(angle) * radius) +
                    Vector3.up * (Mathf.Sin(angle) * radius);
                
                sequence.Append(item.GetTransform().DOMove(spiralPoint, animationDuration / spiralTurns)
                    .SetEase(Ease.InOutQuad));
            }
            
            sequence.Append(item.GetTransform().DOMove(endPosition, animationDuration / spiralTurns)
                .SetEase(positionCurve));
            
            sequence.Join(item.GetTransform().DOScale(endScale, animationDuration)
                .SetEase(scaleCurve));
            
            sequence.Join(DOTween.To(() => startAlpha, item.SetAlpha, endAlpha, animationDuration)
                .SetEase(alphaCurve));
            
            await sequence.AsyncWaitForCompletion();
        }
    }
} 