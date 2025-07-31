using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Raccoons.UI.Animations.CollectAnimationSettings.Assets
{
    [CreateAssetMenu(fileName = "DefaultCollectAnimationSettings", menuName = "Raccoons/UI/Collect Animation/Default Settings")]
    public class DefaultCollectAnimationSettingsAsset : CollectAnimationSettingsAsset
    {
        public override async UniTask AnimateItem(CollectAnimationItem item, Vector3 startPosition, Vector3 endPosition)
        {
            await CreateBaseSequence(item, startPosition, endPosition)
                .AsyncWaitForCompletion();
        }
    }
} 