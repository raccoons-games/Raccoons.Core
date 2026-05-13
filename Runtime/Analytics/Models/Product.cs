using System.Collections.Generic;

namespace Raccoons.Analytics
{
    public class Product
    {
        public RealCurrency RealCurrency { get; set; }
        public List<VirtualCurrency> VirtualCurrencies { get; set; }
        public List<Item> Items { get; set; }
    }
}
