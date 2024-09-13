namespace Raccoons.Scores
{
    public class ScoreChangeData
    {
        public ScoreChangeData(IScoreStorage scoreStorage, float oldScore, float newScore)
        {
            ScoreStorage = scoreStorage;
            OldScore = oldScore;
            NewScore = newScore;
        }

        public IScoreStorage ScoreStorage { get; private set; }
        public float OldScore { get; private set; }
        public float NewScore { get; private set; }
    }
}