using UnityEngine;

namespace Raccoons.UI.Animations.CollectAnimationSettings
{
    [System.Serializable]
    public class CollectAnimationSettings
    {
        [Header("Animation Timing")]
        public float animationDuration = 1f;
        public float delayBetweenItems = 0.05f;
        
        [Header("Animation Curves")]
        public AnimationCurve positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Visual Settings")]
        public Vector3 startScale = Vector3.one;
        public Vector3 endScale = Vector3.zero;
        public float startAlpha = 1f;
        public float endAlpha = 0f;
        
        
        [Header("Scatter Settings (for Scatter type)")]
        public float scatterRadius = 100f;
        public float scatterForce = 200f;
        public float gravity = 500f;
    }
} 