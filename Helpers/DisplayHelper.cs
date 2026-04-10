// DisplayHelper.cs
// static helper class for all console output formatting
// keeps display logic separate from business logic

using System.Collections.Generic;
using VaccineInventory.Models;
using VaccineInventory.Services;

namespace VaccineInventory.Helpers
{
    public static class DisplayHelper
    {
        // reusable border strings to keep console output consistent
        private const string THICK = "  ================================================";
        private const string THIN = "  ------------------------------------------------";

        // renders the full vaccine table with a computed status column
        public static void ShowVaccineTable(IReadOnlyList<Product> vaccines, InventoryManager manager)
        {
            if (vaccines.Count == 0)
            {
                Console.WriteLine("\n  No products found.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("  " + new string('-', 117));
            Console.WriteLine($"  {"ID",-5} {"Vaccine Name",-22} {"Category",-16} {"Supplier",-20} {"Qty",-6} {"Price",-12} {"Expiry",-13} {"Min Stock",-10} Status");
            Console.WriteLine("  " + new string('-', 117));

            foreach (var v in vaccines)
            {
                // expired takes priority over low stock in the status display
                string status;
                if (v.IsExpired()) status = "EXPIRED";
                else if (v.IsLowStock()) status = "LOW STOCK";
                else status = "OK";

                Console.WriteLine(
                    $"  {v.ProductId,-5} {v.VaccineName,-22} {manager.GetCategoryName(v.CategoryId),-16}" +
                    $" {manager.GetSupplierName(v.SupplierId),-20} {v.Quantity,-6}" +
                    $" {"P" + v.Price.ToString("F2"),-12} {v.ExpiryDate.ToString("MM/dd/yyyy"),-13}" +
                    $" {v.MinimumStockLevel,-10} {status}");
            }

            Console.WriteLine("  " + new string('-', 117));
        }

        // renders the transaction log; pass "ALL" to skip filtering
        public static void ShowTransactionTable(IReadOnlyList<TransactionRecord> records,
                                                string filterAction = "ALL")
        {
            var filtered = filterAction == "ALL"
                ? records.ToList()
                : records.Where(t => t.ActionType == filterAction).ToList();

            if (filtered.Count == 0)
            {
                Console.WriteLine("\n  No transactions found.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("  " + new string('-', 102));
            Console.WriteLine($"  {"TxnID",-7} {"ProductID",-10} {"Vaccine",-22} {"Action",-10} {"Qty",-6} {"Date",-22} Notes");
            Console.WriteLine("  " + new string('-', 102));

            foreach (var t in filtered)
            {
                Console.WriteLine(
                    $"  {t.TransactionId,-7} {t.ProductId,-10} {t.VaccineName,-22}" +
                    $" {t.ActionType,-10} {t.QuantityChanged,-6}" +
                    $" {t.Date.ToString("MM/dd/yyyy hh:mm tt"),-22} {t.Notes}");
            }

            Console.WriteLine("  " + new string('-', 102));
        }

        public static void ShowMainMenu(string username, string role)
        {
            Console.WriteLine();
            Console.WriteLine(THICK);
            Console.WriteLine("    VACCINE INVENTORY MANAGEMENT SYSTEM");
            Console.WriteLine($"    Logged in as: {username}  ({role})");
            Console.WriteLine(THICK);
            Console.WriteLine();
            Console.WriteLine("    ------ PRODUCT MANAGEMENT ------");
            Console.WriteLine("     1  Add Product");
            Console.WriteLine("     2  View All Products");
            Console.WriteLine("     3  Search Product");
            Console.WriteLine("     4  Update Product");
            Console.WriteLine("     5  Delete Product");
            Console.WriteLine();
            Console.WriteLine("    ------ STOCK CONTROL ------");
            Console.WriteLine("     6  Restock Product");
            Console.WriteLine("     7  Deduct Stock");
            Console.WriteLine("     8  Show Low Stock Items");
            Console.WriteLine();
            Console.WriteLine("    ------ CATEGORY MANAGEMENT ------");
            Console.WriteLine("     9  Add Category");
            Console.WriteLine("    10  View Categories");
            Console.WriteLine("    11  Update Category");
            Console.WriteLine("    12  Delete Category");
            Console.WriteLine();
            Console.WriteLine("    ------ SUPPLIER MANAGEMENT ------");
            Console.WriteLine("    13  Add Supplier");
            Console.WriteLine("    14  View Suppliers");
            Console.WriteLine("    15  Update Supplier");
            Console.WriteLine("    16  Delete Supplier");
            Console.WriteLine();
            Console.WriteLine("    ------ REPORTS ------");
            Console.WriteLine("    17  View Transaction History");
            Console.WriteLine("    18  Compute Total Inventory Value");
            Console.WriteLine();
            Console.WriteLine("     0  Exit");
            Console.WriteLine();
            Console.WriteLine(THICK);
        }

        // shows key stats at a glance; alerts the user if action is needed
        public static void ShowFullDashboard(InventoryManager manager)
        {
            var (totalProducts, expired, lowStock, totalValue) = manager.GetDashboardCounts();

            Console.WriteLine();
            Console.WriteLine(THICK);
            Console.WriteLine("    DASHBOARD");
            Console.WriteLine(THIN);
            Console.WriteLine($"    Total Products        : {totalProducts}");
            Console.WriteLine($"    Low Stock Items       : {lowStock}");
            Console.WriteLine($"    Expired Vaccines      : {expired}");
            Console.WriteLine($"    Total Inventory Value : P{totalValue:F2}");
            Console.WriteLine(THIN);

            if (expired == 0 && lowStock == 0)
                Console.WriteLine("    All products are in good standing.");
            else
            {
                if (expired > 0) Console.WriteLine($"    ! {expired} product(s) have passed their expiry date.");
                if (lowStock > 0) Console.WriteLine($"    ! {lowStock} product(s) are at or below 5 units.");
                Console.WriteLine("      See options 2 or 8 for details.");
            }

            Console.WriteLine(THICK);
        }

        // used at the top of each handler to clearly identify the active module and action
        public static void PrintSectionHeader(string module, string action)
        {
            Console.WriteLine();
            Console.WriteLine(THICK);
            Console.WriteLine($"    {module}");
            Console.WriteLine(THIN);
            Console.WriteLine($"    {action}");
            Console.WriteLine(THICK);
            Console.WriteLine();
        }

        public static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine(THICK);
            Console.WriteLine($"    {title}");
            Console.WriteLine(THICK);
            Console.WriteLine();
        }

        public static void PrintSuccess(string message)
        {
            Console.WriteLine();
            Console.WriteLine(THIN);
            Console.WriteLine($"    Done. {message}");
            Console.WriteLine(THIN);
        }

        public static void PrintError(string message)
        {
            Console.WriteLine();
            Console.WriteLine($"    Error: {message}");
        }

        public static void PrintBack()
        {
            Console.WriteLine("\n  Returning to menu...");
        }

        public static void PrintLine()
        {
            Console.WriteLine(THIN);
        }

        // pauses the screen so the user can read output before returning to the menu
        public static void PressEnter()
        {
            Console.WriteLine();
            Console.Write("  Press ENTER to continue...");
            Console.ReadLine();
        }
    }
}