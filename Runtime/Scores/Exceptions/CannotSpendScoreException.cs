using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoons.Scores.Exceptions
{
    public class CannotSpendScoreException : ArgumentException
    {
        public CannotSpendScoreException(ScoreSpendingData data)
            : base(GenerateErrorMessage(data), "amount")
        {
            ScoreSpendingData = data;
        }

        public ScoreSpendingData ScoreSpendingData { get; }

        private static string GenerateErrorMessage(ScoreSpendingData data)
        {
            return $"[{data.ScoreBank.GetType().Name}][{data.ScoreBank.ScoreKey}] - cannot spend score amount: {data.TriedToSpend}";
        }
    }
}
