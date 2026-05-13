namespace Raccoons.Analytics
{
    public class RealCurrency
    {
        public string Type { get; }
        public float Amount { get; }

        public RealCurrency(string type, float amount)
        {
            Type = type;
            Amount = amount;
        }
    }
}
