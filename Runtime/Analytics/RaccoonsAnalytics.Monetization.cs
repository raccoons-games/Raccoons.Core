using System.Collections.Generic;

namespace Raccoons.Analytics
{
    public static partial class RaccoonsAnalytics
    {
        public static void Economy(
            Transaction transaction,
            string placement = "General",
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["placement"] = placement;
            MergeParams(p, transaction.ToParameters());
            Schedule("economy", p);
        }

        public static void Economy(
            string transactionName,
            Product spent,
            Product received,
            string placement = "General",
            Dictionary<string, object> additionalData = null)
        {
            var transaction = new Transaction(transactionName, received: received, spent: spent);
            Economy(transaction, placement, additionalData);
        }

        public static void InAppPurchase(
            string purchaseName,
            Product spentProducts,
            Product receivedProducts,
            string productID,
            string transactionID,
            string placement,
            ReceiptStatus receiptStatus,
            bool isTestPurchase,
            Dictionary<string, object> additionalData = null)
        {
            var transaction = new Transaction(
                transactionName: purchaseName,
                received: receivedProducts,
                spent: spentProducts,
                transactionID: transactionID,
                productID: productID
            );
            InAppPurchase(transaction, placement, receiptStatus, isTestPurchase, additionalData);
        }

        public static void InAppPurchase(
            Transaction transaction,
            string placement,
            ReceiptStatus receiptStatus,
            bool isTestPurchase,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["placement"] = placement;
            p["receipt_status"] = receiptStatus.ToString();
            p["is_test_purchase"] = isTestPurchase;
            MergeParams(p, transaction.ToParameters());
            Schedule("in_app_purchase", p);
        }
    }
}
