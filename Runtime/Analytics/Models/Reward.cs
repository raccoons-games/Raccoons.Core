using System.Collections.Generic;

namespace Raccoons.Analytics
{
    public class Reward
    {
        public Product ReceivedProduct { get; }

        public Reward(Product product)
        {
            ReceivedProduct = product;
        }

        internal Dictionary<string, object> ToParameters(string prefix = "reward")
        {
            var p = new Dictionary<string, object>();
            if (ReceivedProduct == null) return p;

            var currencies = ReceivedProduct.VirtualCurrencies;
            if (currencies != null && currencies.Count > 0)
            {
                for (int i = 0; i < currencies.Count; i++)
                {
                    string key = i == 0 ? prefix : $"{prefix}_{i}";
                    p[$"{key}_type"] = currencies[i].Type;
                    p[$"{key}_name"] = currencies[i].Name;
                    p[$"{key}_amount"] = currencies[i].Amount;
                }
            }

            if (ReceivedProduct.RealCurrency != null)
            {
                p[$"{prefix}_real_type"] = ReceivedProduct.RealCurrency.Type;
                p[$"{prefix}_real_amount"] = ReceivedProduct.RealCurrency.Amount;
            }

            var items = ReceivedProduct.Items;
            if (items != null && items.Count > 0)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    string key = $"{prefix}_item_{i}";
                    p[$"{key}_type"] = items[i].Type;
                    p[$"{key}_name"] = items[i].Name;
                    p[$"{key}_amount"] = items[i].Amount;
                }
            }

            return p;
        }
    }
}
