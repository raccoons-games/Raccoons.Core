using UnityEngine;

namespace Raccoons.Scores
{
    public class ScoreAcquisitionSourceData
    {
        public ScoreAcquisitionSourceData(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; private set; }
    }
}