using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Scores
{
    public interface IScoreBank : IScoreStorage
    {
        public event EventHandler<ScoreAcquisitionData> OnScoreAcquired;
        public event EventHandler<ScoreSpendingData> OnScoreSpent;
        public bool CanAcquire(float amount);
        public bool CanSpend(float amount);
        public ScoreAcquisitionData Acquire(float amount);
        public ScoreSpendingData Spend(float amount);

        public Task<bool> CanAcquireAsync(float amount, CancellationToken cancellationToken = default);
        public Task<bool> CanSpendAsync(float amount, CancellationToken cancellationToken = default);
        public Task<ScoreAcquisitionData> AcquireAsync(float amount, CancellationToken cancellationToken = default);
        public Task<ScoreSpendingData> SpendAsync(float amount, CancellationToken cancellationToken = default);
    }
}