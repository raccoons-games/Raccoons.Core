using UnityEngine;

namespace Raccoons.Scores.Assets
{
    [CreateAssetMenu(fileName = "ScoreMetadata", menuName = "Raccoons/Scores/Metadata")]
    public class ScoreMetadataAsset : ScriptableObject
    {
        [field:SerializeField] public ScoreMetadata Metadata { get; private set; }
    }
}