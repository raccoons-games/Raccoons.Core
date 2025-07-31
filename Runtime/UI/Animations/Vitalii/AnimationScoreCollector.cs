using Raccoons.Identifiers.Guids;
using Raccoons.Scores;
using UnityEngine;
using Zenject;

namespace Raccoons.UI.Animations.Vitalii
{
    public class AnimationScoreCollector : MonoBehaviour
    {
        [SerializeField] private GuidAsset keyScore;
        private CollectAnimationSystem _collectAnimationSystem;
        private IScoreBank _scoreBank;

        [Inject]
        private void Construct(CollectAnimationSystem collectAnimationSystem, DiContainer container)
        {
            _collectAnimationSystem = collectAnimationSystem;
            _scoreBank = container.ResolveId<IScoreBank>(keyScore);
        }
    }
} 