namespace Raccoons.Scores
{
    public class BankScoreChangeData 
    {
        public BankScoreChangeData(IScoreBank scoreBank, ScoreChangeData scoreChange)
        {
            ScoreBank = scoreBank;
            ScoreChange = scoreChange;
        }

        public IScoreBank ScoreBank { get; private set; }
        public ScoreChangeData ScoreChange { get; private set; }


    }
}