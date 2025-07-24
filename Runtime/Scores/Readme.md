# Raccoons Scores

Framework is made to work with scores. Great solutions for scores, coins, etc.

### IScoreStorage

Interface that is responsible for accessing score value.

**Methods**

- float GetScore() - return score.
- ScoreChangeData SetScore(float newScore) - set score to newScore.

**Properties**

- string ScoreKey - unique score identifier.

**Events**

- EventHandler<ScoreChangeData> OnScoreChanged - notifies about score changes.

**Implementations**

- DefaultScoreStorage - implements accessing score as interaction with some external IStorageChannel.

### IScoreBank : IScoreStorage

Interface that implements score as something we can spend or earn.

**Methods**:

- bool CanAcquire(float amount) - answers can we acquire this amount of score
- bool CanSpend(float amount) - answers can we spend this amount of score
- ScoreAcquisitionData Acquire(float amount) - if can acquire, acquires the amount of score, otherwise throws CannotAcquireScoreException
- ScoreSpendingData Spend(float amount) - if can spend, spends the amount of score, otherwise throws CannotSpendScoreException

**Events**

- EventHandler<ScoreAcquisitionData> OnScoreAcquired - notifies when score is acquired
- EventHandler<ScoreSpendingData> OnScoreSpent - notifies when score is spent

**Implementations**

- DefaultScoreBank - simple implementation of IScoreBank, prevents score being negative.
- MultipliedScoreBank - adds acquisition and spending AdvancedFloat multipliers 

### Examples

Creating and usage of non-saving score with earning multiplier:

```C#
AdvancedFloat earningMultiplier = new AdvancedFloat();
earningMultiplier.SetInitialValue(1);
IStorageChannel storageChannel = new SingleDataMemoryStorage(); //Stores single data value in RAM.
IScoreStorage scoreStorage = new DefaultScoreStorage("XP", storageChannel);
IScoreBank scoreBank = new MultipliedScoreBank(scoreStorage, earningMultiplier, null);
earningMultiplier.AddModificator(new FloatModificator(FloatModificatorOperation.Add, 0.5f, 0)); //+50%
scoreBank.Acquire(2); //Adds 3 XP because of 50% modificator
```

