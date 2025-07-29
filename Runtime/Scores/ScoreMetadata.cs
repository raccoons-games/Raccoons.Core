using UnityEngine;

namespace Raccoons.Scores
{
    [System.Serializable]
    public class ScoreMetadata
    {
        public ScoreMetadata(string name, string description, Sprite smallIcon, Sprite bigIcon)
        {
            Name = name;
            Description = description;
            SmallIcon = smallIcon;
            BigIcon = bigIcon;
        }

        [field:SerializeField] public string Name { get; private set; }
        [field:SerializeField] public string Description { get; private set; }
        [field:SerializeField] public Sprite SmallIcon { get; private set; }
        [field:SerializeField] public Sprite BigIcon { get; private set; }
    }
}