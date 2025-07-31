using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Raccoons.UI.Animations.CollectAnimationSettings
{
    public abstract class CollectAnimationSettingsAsset : ScriptableObject
    {
        [Header("Base Animation Settings")]
        [SerializeField] protected float animationDuration = 1f;
        [SerializeField] protected float delayBetweenItems = 0.05f;
        
        [Header("Animation Curves")]
        [SerializeField] protected AnimationCurve positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] protected AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] protected AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Visual Settings")]
        [SerializeField] protected Vector3 startScale = Vector3.one;
        [SerializeField] protected Vector3 endScale = Vector3.zero;
        [SerializeField] protected float startAlpha = 1f;
        [SerializeField] protected float endAlpha = 0f;
        
        public float AnimationDuration => animationDuration;
        public float DelayBetweenItems => delayBetweenItems;
        public AnimationCurve PositionCurve => positionCurve;
        public AnimationCurve ScaleCurve => scaleCurve;
        public AnimationCurve AlphaCurve => alphaCurve;
        public Vector3 StartScale => startScale;
        public Vector3 EndScale => endScale;
        public float StartAlpha => startAlpha;
        public float EndAlpha => endAlpha;
        
        public abstract UniTask AnimateItem(CollectAnimationItem item, Vector3 startPosition, Vector3 endPosition);
        
        protected virtual Sequence CreateBaseSequence(CollectAnimationItem item, Vector3 startPosition, Vector3 endPosition)
        {
            Sequence sequence = DOTween.Sequence();
            
            sequence.Join(item.GetTransform().DOMove(endPosition, animationDuration)
                .SetEase(positionCurve));
            
            sequence.Join(item.GetTransform().DOScale(endScale, animationDuration)
                .SetEase(scaleCurve));
            
            sequence.Join(DOTween.To(() => startAlpha, (alpha) => item.SetAlpha(alpha), endAlpha, animationDuration)
                .SetEase(alphaCurve));
            
            return sequence;
        }
    }
} 