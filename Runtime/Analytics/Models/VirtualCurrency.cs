namespace Raccoons.Analytics
{
    public class VirtualCurrency
    {
        public string Name { get; }
        public string Type { get; }
        public int Amount { get; }

        public VirtualCurrency(string name, string type, int amount)
        {
            Name = name;
            Type = type;
            Amount = amount;
        }
    }
}
