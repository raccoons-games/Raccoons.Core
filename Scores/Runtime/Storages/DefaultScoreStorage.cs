using Raccoons.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Scores.Storages
{
    public class DefaultScoreStorage : IScoreStorage
    {
        public DefaultScoreStorage(string scoreKey, IStorageChannel storageChannel)
        {
            ScoreKey = scoreKey;
            StorageChannel = storageChannel;
        }

        public event EventHandler<ScoreChangeData> OnScoreChanged;
        public string ScoreKey { get; }
        public IStorageChannel StorageChannel { get; }
        public virtual float FallbackValue => 0;

        public float GetScore()
        {
            if (StorageChannel.Exists(ScoreKey))
            {
                return StorageChannel.GetFloat(ScoreKey);
            }
            else
            {
                return FallbackValue;
            }
        }

        public async Task<float> GetScoreAsync(CancellationToken cancellationToken = default)
        {
            if (await StorageChannel.ExistsAsync(ScoreKey))
            {
                return await StorageChannel.GetFloatAsync(ScoreKey, cancellationToken);
            }
            else
            {
                return FallbackValue;
            }
        }

        public ScoreChangeData SetScore(float newScore)
        {
            float oldScore = GetScore();
            StorageChannel.SetFloat(ScoreKey, newScore);
            return NotifyScoreChanged(newScore, oldScore);
        }

        private ScoreChangeData NotifyScoreChanged(float newScore, float oldScore)
        {
            ScoreChangeData scoreChangeData = new ScoreChangeData(this, oldScore, newScore);
            OnScoreChanged?.Invoke(this, scoreChangeData);
            return scoreChangeData;
        }

        public async Task<ScoreChangeData> SetScoreAsync(float newScore, CancellationToken cancellationToken = default)
        {
            float oldScore = await GetScoreAsync(cancellationToken);
            await StorageChannel.SetFloatAsync(ScoreKey, newScore, cancellationToken);
            return NotifyScoreChanged(newScore, oldScore);
        }
    }
}
