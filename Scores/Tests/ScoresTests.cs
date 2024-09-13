using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Raccoons.Scores.Banks;
using Raccoons.Scores.Exceptions;
using Raccoons.Scores.Storages;
using Raccoons.Storage;
using UnityEngine;
using UnityEngine.TestTools;
using Random = UnityEngine.Random;

namespace Raccoons.Scores.Tests
{
    
    public class ScoresTests
    {
        public async Task TestStorage(Func<string, IStorageChannel, IScoreStorage> storageCreator)
        {
            IStorageChannel storage = new PlayerPrefsStorage("Raccoons/Scores/Tests");
            IScoreStorage scoreStorage = storageCreator("TestScore", storage);
            int randomScore = Random.Range(0, 100);
            scoreStorage.SetScore(randomScore);
            Assert.AreEqual(randomScore, scoreStorage.GetScore());
            randomScore = Random.Range(0, 100);
            await scoreStorage.SetScoreAsync(randomScore);
            Assert.AreEqual(randomScore, await scoreStorage.GetScoreAsync());
        }

        public void TestBank(Func<IScoreStorage, IScoreBank> bankCreator)
        {
            IStorage storage = new PlayerPrefsStorage("Raccoons/Scores/Tests");
            IScoreStorage scoreStorage = new DefaultScoreStorage("TestScore", storage);
            IScoreBank scoreBank = bankCreator(scoreStorage);
            scoreBank.OnScoreChanged += ScoreBank_OnScoreChanged;
            scoreBank.SetScore(10);
            Assert.IsTrue(scoreBank.CanSpend(9));
            Assert.IsFalse(scoreBank.CanSpend(11));
            scoreBank.Acquire(1);
            Assert.DoesNotThrow(() => scoreBank.Spend(11));
            Assert.Throws(typeof(CannotSpendScoreException), () => scoreBank.Spend(1));
        }

        private void ScoreBank_OnScoreChanged(object sender, ScoreChangeData e)
        {
            Debug.Log("ScoreChanged - "+e.NewScore);
        }

        [UnityTest]
        public IEnumerator TestDefaultStorage()
        {
            var task = TestStorage((key, storage) => new DefaultScoreStorage(key, storage));
            while (task.IsCompleted == false) yield return null;
        }

        [Test]
        public void TestDefaultBank()
        {
            TestBank(scoreStorage => new DefaultScoreBank(scoreStorage));
        }
    }
}