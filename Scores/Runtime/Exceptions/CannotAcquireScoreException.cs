using System;

namespace Raccoons.Scores.Exceptions
{
    public class CannotAcquireScoreException : ArgumentException
    {
        public CannotAcquireScoreException(ScoreAcquisitionData data)
            : base(GenerateErrorMessage(data), "amount")
        {
            ScoreSpendingData = data;
        }

        public ScoreAcquisitionData ScoreSpendingData { get; }

        private static string GenerateErrorMessage(ScoreAcquisitionData data)
        {
            return $"[{data.ScoreBank.GetType().Name}][{data.ScoreBank.ScoreKey}] - cannot acquire score amount: {data.TriedToAcquire}";
        }
    }
}
