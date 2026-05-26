namespace Raccoons.Analytics
{
    public class Item
    {
        public string Name { get; }
        public string Type { get; }
        public int Amount { get; }

        public Item(string name, string type, int amount)
        {
            Name = name;
            Type = type;
            Amount = amount;
        }
    }
}
