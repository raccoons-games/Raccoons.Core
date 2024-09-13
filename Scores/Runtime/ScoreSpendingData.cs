namespace Raccoons.Scores
{
    public class ScoreSpendingData : BankScoreChangeData
    {
        public ScoreSpendingData(IScoreBank scoreBank, ScoreChangeData scoreChange, float spent, float triedToSpend) : base(scoreBank, scoreChange)
        {
            Spent = spent;
            TriedToSpend = triedToSpend;
        }

        public float Spent { get; private set; }
        public float TriedToSpend { get; private set; }

    }
}