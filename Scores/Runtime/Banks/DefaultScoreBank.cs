using Raccoons.Scores.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Scores.Banks
{
    public class DefaultScoreBank : IScoreBank, IDisposable
    {
        public const double SCORE_ERROR = 0.000001;

        public DefaultScoreBank(IScoreStorage scoreStorage)
        {
            ScoreStorage = scoreStorage;
            scoreStorage.OnScoreChanged += NotifyScoreChanged;
        }

        public IScoreStorage ScoreStorage { get; }

        public string ScoreKey => ScoreStorage.ScoreKey;

        public event EventHandler<ScoreChangeData> OnScoreChanged;
        public event EventHandler<ScoreAcquisitionData> OnScoreAcquired;
        public event EventHandler<ScoreSpendingData> OnScoreSpent;


        public void Dispose() 
        {
            if (ScoreStorage != null)
            {
                ScoreStorage.OnScoreChanged -= NotifyScoreChanged;
            }
        }

        #region Acquisition
        public virtual ScoreAcquisitionData Acquire(float amount)
        {
            float score = GetScore();
            if (CanAcquireInternal(amount, score, out float acquisition))
            {
                ScoreChangeData changeData = ScoreStorage.SetScore(score + acquisition);
                return NotifyScoreAcquired(amount, acquisition, changeData);
            }
            throw GenerateFailedScoreAcquisition(amount, score, acquisition);
        }

        public virtual async Task<ScoreAcquisitionData> AcquireAsync(float amount, CancellationToken cancellationToken = default)
        {
            float score = await GetScoreAsync(cancellationToken);
            if (CanAcquireInternal(amount, score, out float acquisition))
            {
                ScoreChangeData changeData = await ScoreStorage.SetScoreAsync(score + acquisition, cancellationToken);
                return NotifyScoreAcquired(amount, acquisition, changeData);
            }
            throw GenerateFailedScoreAcquisition(amount, score, acquisition);
        }

        public virtual bool CanAcquire(float amount)
        {
            float score = GetScore();
            return CanAcquireInternal(amount, score, out float acquisition);
        }

        protected virtual bool CanAcquireInternal(float amount, float score, out float acquisition)
        {
            acquisition = CalculateAcquisition(score, amount);
            return true;
        }

        public async Task<bool> CanAcquireAsync(float amount, CancellationToken cancellationToken = default)
        {
            float score = await GetScoreAsync(cancellationToken);
            return CanAcquireInternal(amount, score, out float acquisition);
        }

        private ScoreAcquisitionData NotifyScoreAcquired(float amount, float newScoreAcquisition, ScoreChangeData changeData)
        {
            var acquisitionData = new ScoreAcquisitionData(this, changeData, newScoreAcquisition, amount);
            OnScoreAcquired?.Invoke(this, acquisitionData);
            return acquisitionData;
        }

        private CannotAcquireScoreException GenerateFailedScoreAcquisition(float amount, float score, float acquisition)
        {
            ScoreAcquisitionData fallbackSpendingScoreData = new ScoreAcquisitionData(this, new ScoreChangeData(ScoreStorage, score, score), acquisition, amount);
            return new CannotAcquireScoreException(fallbackSpendingScoreData);
        }

        public virtual float CalculateAcquisition(float score, float amount)
        {
            return amount;
        }

        #endregion

        #region Spending
        public virtual bool CanSpend(float amount)
        {
            float score = ScoreStorage.GetScore();
            return CanSpendInternal(amount, score, out float spending);
        }

        protected virtual bool CanSpendInternal(float amount, float score, out float spending)
        {
            spending = CalculateSpending(score, amount);
            return spending <= score + SCORE_ERROR;
        }

        protected virtual float CalculateSpending(float score, float amount)
        {
            return amount;
        }

        public virtual async Task<bool> CanSpendAsync(float amount, CancellationToken cancellationToken)
        {
            float score = await ScoreStorage.GetScoreAsync(cancellationToken);
            return CanSpendInternal(amount, score, out float spending);
        }

        public virtual ScoreSpendingData Spend(float amount)
        {
            float score = GetScore();
            if (CanSpendInternal(amount, score, out float spending))
            {
                ScoreChangeData changeData = SetScore(score - spending);
                return NotifyScoreSpent(amount, spending, changeData);
            }
            throw GenerateFailedScoreSpending(amount, score, spending);
        }

        private ScoreSpendingData NotifyScoreSpent(float amount, float spending, ScoreChangeData changeData)
        {
            ScoreSpendingData scoreSpendingData = new ScoreSpendingData(this, changeData, spending, amount);
            OnScoreSpent?.Invoke(this, scoreSpendingData);
            return scoreSpendingData;
        }

        private CannotSpendScoreException GenerateFailedScoreSpending(float amount, float score, float spending)
        {
            ScoreSpendingData fallbackSpendingScoreData = new ScoreSpendingData(this, new ScoreChangeData(ScoreStorage, score, score), spending, amount);
            return new CannotSpendScoreException(fallbackSpendingScoreData);
        }

        public virtual async Task<ScoreSpendingData> SpendAsync(float amount, CancellationToken cancellationToken = default)
        {
            float score = await GetScoreAsync(cancellationToken);
            if (CanSpendInternal(amount, score, out float spending))
            {
                ScoreChangeData changeData = await SetScoreAsync(score - spending, cancellationToken);
                return NotifyScoreSpent(amount, spending, changeData);
            }
            throw GenerateFailedScoreSpending(amount, score, spending);
        }

        #endregion

        #region Storage methods
        public float GetScore()
        {
            return ScoreStorage.GetScore();
        }

        public Task<float> GetScoreAsync(CancellationToken cancellationToken = default)
        {
            return ScoreStorage.GetScoreAsync();
        }

        public ScoreChangeData SetScore(float newScore)
        {
            return ScoreStorage.SetScore(newScore);
        }

        public Task<ScoreChangeData> SetScoreAsync(float newScore, CancellationToken cancellationToken = default)
        {
            return ScoreStorage.SetScoreAsync(newScore, cancellationToken);
        }

        public void NotifyScoreChanged(object sender, ScoreChangeData data)
        {
            OnScoreChanged?.Invoke(this, data);
        }

        #endregion
    }
}
