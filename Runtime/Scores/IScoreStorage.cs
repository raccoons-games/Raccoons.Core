using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.Scores
{

    public interface IScoreStorage
    {
        public string ScoreKey { get; }
        public event EventHandler<ScoreChangeData> OnScoreChanged;
        public float GetScore();
        public ScoreChangeData SetScore(float newScore);
        public Task<float> GetScoreAsync(CancellationToken cancellationToken = default);
        public Task<ScoreChangeData> SetScoreAsync(float newScore, CancellationToken cancellationToken = default);

    }
}