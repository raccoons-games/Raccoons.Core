namespace Raccoons.Scores
{
    public class ScoreAcquisitionData : BankScoreChangeData
    {
        public ScoreAcquisitionData(IScoreBank scoreBank, ScoreChangeData scoreChange, float acquired, float triedToAcquire) : base(scoreBank, scoreChange)
        {
            Acquired = acquired;
            TriedToAcquire = triedToAcquire;
        }

        public float Acquired { get; private set; }
        public float TriedToAcquire { get; private set; }
    }
}