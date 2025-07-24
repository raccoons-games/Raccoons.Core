using Raccoons.Maths.Numbers;

namespace Raccoons.Scores.Banks
{
    public class MultipliedScoreBank : DefaultScoreBank
    {
        public AdvancedFloat AcquisitionMultiplier { get; set; }
        public AdvancedFloat SpendingMultiplier { get; set; }
        public MultipliedScoreBank(IScoreStorage scoreStorage, AdvancedFloat acquisitionMultiplier = null, AdvancedFloat spendingMultiplier = null) : base(scoreStorage)
        {
            if (acquisitionMultiplier == null)
            {
                acquisitionMultiplier = new AdvancedFloat();
                acquisitionMultiplier.SetInitialValue(1);
            }
            AcquisitionMultiplier = acquisitionMultiplier;

            if (spendingMultiplier == null)
            {
                spendingMultiplier = new AdvancedFloat();
                spendingMultiplier.SetInitialValue(1);
            }
            SpendingMultiplier = spendingMultiplier;
        }

        public override float CalculateAcquisition(float score, float amount)
        {
            return base.CalculateAcquisition(score, amount) * AcquisitionMultiplier.Value;
        }

        protected override float CalculateSpending(float score, float amount)
        {
            return base.CalculateSpending(score, amount) * SpendingMultiplier.Value;
        }
    }
}
