using Raccoons.Maths.Numbers;
using Raccoons.Scores;
using Raccoons.Scores.Banks;
using Raccoons.Scores.Storages;
using Raccoons.Storage;
using Raccoons.Storage.Memory;
using UnityEngine;

namespace Raccoons.Core.Samples.Scores.ScoreLogic
{
    public class ScoresLogicSample : MonoBehaviour
    {
        private IScoreStorage _scoreStorage;
        private IScoreBank _scoreBank;
        private AdvancedFloat _earningMultiplier;

        private void Start()
        {
            _earningMultiplier = new AdvancedFloat();
            _earningMultiplier.SetInitialValue(1);
            IStorageChannel storageChannel = new SingleDataMemoryStorageChannel();
            _scoreStorage = new DefaultScoreStorage("XP", storageChannel);
            _scoreBank = new MultipliedScoreBank(_scoreStorage, _earningMultiplier, null);
            _earningMultiplier.AddModificator(new FloatModificator(FloatModificatorOperation.Add, 0.5f, 0)); // +50%
            Debug.Log($"Initial Score: {_scoreStorage.GetScore()}");
            _scoreBank.Acquire(2);
            Debug.Log($"Score after acquiring 2 (with +50%): {_scoreStorage.GetScore()}");
            _scoreBank.Spend(1);
            Debug.Log($"Score after spending 1: {_scoreStorage.GetScore()}");
        }
    }
} 