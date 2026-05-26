using System.Collections.Generic;

namespace Raccoons.Analytics
{
    public class Transaction
    {
        public string Name { get; }
        public string Currency { get; }
        public Product Received { get; private set; }
        public Product Spent { get; private set; }
        public string TransactionID { get; }
        public string ProductID { get; }

        public Transaction(
            string transactionName,
            string currency = null,
            Product received = null,
            Product spent = null,
            string transactionID = null,
            string productID = null)
        {
            Name = transactionName;
            Currency = currency;
            Received = received;
            Spent = spent;
            TransactionID = transactionID;
            ProductID = productID;
        }

        public void AddReceivedItem(string name, string type, float amount)
        {
            Received ??= new Product();
            Received.VirtualCurrencies ??= new List<VirtualCurrency>();
            Received.VirtualCurrencies.Add(new VirtualCurrency(name, type, (int)amount));
        }

        public void AddSpentItem(string name, string type, float amount)
        {
            Spent ??= new Product();
            Spent.VirtualCurrencies ??= new List<VirtualCurrency>();
            Spent.VirtualCurrencies.Add(new VirtualCurrency(name, type, (int)amount));
        }

        internal Dictionary<string, object> ToParameters()
        {
            var p = new Dictionary<string, object>
            {
                ["transaction_name"] = Name
            };

            if (TransactionID != null) p["transaction_id"] = TransactionID;
            if (ProductID != null) p["product_id"] = ProductID;
            if (Currency != null) p["currency"] = Currency;

            AddProductParams(p, Spent, "spent");
            AddProductParams(p, Received, "received");

            return p;
        }

        private static void AddProductParams(Dictionary<string, object> p, Product product, string prefix)
        {
            if (product == null) return;

            if (product.VirtualCurrencies?.Count > 0)
            {
                p[$"{prefix}_type"] = product.VirtualCurrencies[0].Type;
                p[$"{prefix}_name"] = product.VirtualCurrencies[0].Name;
                p[$"{prefix}_amount"] = product.VirtualCurrencies[0].Amount;
            }
            else if (product.RealCurrency != null)
            {
                p[$"{prefix}_type"] = "real";
                p[$"{prefix}_name"] = product.RealCurrency.Type;
                p[$"{prefix}_amount"] = product.RealCurrency.Amount;
            }
        }
    }
}
