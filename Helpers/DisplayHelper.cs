// ============================================================
// DisplayHelper.cs
// Static helper class responsible for all formatted console
// output in the Vaccine Inventory Management System.
//
// This class separates presentation logic from business logic.
// All methods are static — no instance needed, just call directly.
//
// Methods:
//   ShowVaccineTable()     — Formatted product list table
//   ShowTransactionTable() — Filtered transaction history table
//   ShowMainMenu()         — 18-option main menu display
//   ShowFullDashboard()    — Login dashboard with summary counts
//   PrintSectionHeader()   — Module + action title block
//   PrintHeader()          — Simple title block
//   PrintSuccess()         — Styled success message
//   PrintError()           — Styled error message
//   PrintBack()            — "Returning to menu..." message
//   PrintLine()            — Thin separator line
//   PressEnter()           — Pause and wait for Enter key
// ============================================================

using System.Collections.Generic;
using VaccineInventory.Models;
using VaccineInventory.Services;

namespace VaccineInventory.Helpers
{
    public static class DisplayHelper
    {
        // Reusable border strings for consistent visual layout
        private const string THICK = "  ================================================";
        private const string THIN = "  ------------------------------------------------";

        // ── Product table ───────────────────────────────────────────────────────
        // Displays a formatted table of vaccine products.
        // Each row shows ID, name, category, supplier, quantity, price,
        // expiry date, minimum stock level, and status (OK / LOW STOCK / EXPIRED).
        //
        // Parameters:
        //   vaccines — the list of products to display
        //   manager  — used to look up category/supplier names by ID
        public static void ShowVaccineTable(IReadOnlyList<Product> vaccines, InventoryManager manager)
        {
            // Show a message instead of an empty table
            if (vaccines.Count == 0)
            {
                Console.WriteLine("\n  No products found.");
                return;
            }

            // Table header
            Console.WriteLine();
            Console.WriteLine("  " + new string('-', 117));
            Console.WriteLine($"  {"ID",-5} {"Vaccine Name",-22} {"Category",-16} {"Supplier",-20} {"Qty",-6} {"Price",-12} {"Expiry",-13} {"Min Stock",-10} Status");
            Console.WriteLine("  " + new string('-', 117));

            // Table rows — one per product
            foreach (var v in vaccines)
            {
                // Determine status label: check expired first, then low stock
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

        // ── Transaction table ───────────────────────────────────────────────────
        // Displays a formatted table of transaction records.
        // Optionally filtered by ActionType (ADD, RESTOCK, DEDUCT, UPDATE, DELETE).
        // Pass "ALL" to show every transaction without filtering.
        //
        // Parameters:
        //   records      — the full transaction history list
        //   filterAction — action type to filter by, or "ALL" to show everything
        public static void ShowTransactionTable(IReadOnlyList<TransactionRecord> records,
                                                string filterAction = "ALL")
        {
            // Apply filter: if "ALL", include everything; otherwise match ActionType
            var filtered = filterAction == "ALL"
                ? records.ToList()
                : records.Where(t => t.ActionType == filterAction).ToList();

            // Show a message if the filtered result is empty
            if (filtered.Count == 0)
            {
                Console.WriteLine("\n  No transactions found.");
                return;
            }

            // Table header
            Console.WriteLine();
            Console.WriteLine("  " + new string('-', 102));
            Console.WriteLine($"  {"TxnID",-7} {"ProductID",-10} {"Vaccine",-22} {"Action",-10} {"Qty",-6} {"Date",-22} Notes");
            Console.WriteLine("  " + new string('-', 102));

            // Table rows — one per transaction record
            foreach (var t in filtered)
            {
                Console.WriteLine(
                    $"  {t.TransactionId,-7} {t.ProductId,-10} {t.VaccineName,-22}" +
                    $" {t.ActionType,-10} {t.QuantityChanged,-6}" +
                    $" {t.Date.ToString("MM/dd/yyyy hh:mm tt"),-22} {t.Notes}");
            }

            Console.WriteLine("  " + new string('-', 102));
        }

        // ── Main menu ───────────────────────────────────────────────────────────
        // Prints the full 18-option main menu grouped into sections:
        //   Product Management, Stock Control, Category Management,
        //   Supplier Management, Reports
        //
        // Parameters:
        //   username — displayed in the header so the user knows who is logged in
        //   role     — shown alongside the username (e.g., Admin, Staff)
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

        // ── Dashboard ───────────────────────────────────────────────────────────
        // Displays a summary panel immediately after successful login.
        // Shows total products, low stock count, expired vaccine count,
        // total inventory value, and any active alerts.
        //
        // Parameters:
        //   manager — used to retrieve live counts from in-memory data
        public static void ShowFullDashboard(InventoryManager manager)
        {
            // Retrieve all dashboard figures in one call
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

            // Show a general OK message if no alerts, otherwise list specific warnings
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

        // ── Section header ──────────────────────────────────────────────────────
        // Prints a two-line header used at the top of each menu section.
        // Shows which module and which action is currently active.
        //
        // Example output:
        //   ================================================
        //     PRODUCT MANAGEMENT
        //   ------------------------------------------------
        //     ADD PRODUCT
        //   ================================================
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

        // ── Simple header ───────────────────────────────────────────────────────
        // Prints a single-line title block with thick borders.
        // Used for simpler headings that don't need a sub-action label.
        public static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine(THICK);
            Console.WriteLine($"    {title}");
            Console.WriteLine(THICK);
            Console.WriteLine();
        }

        // ── Success message ─────────────────────────────────────────────────────
        // Prints a styled success notification surrounded by thin borders.
        // Called after any successful add/update/delete/restock/deduct operation.
        //
        // Example output:
        //   ------------------------------------------------
        //     Done. Product added successfully.
        //   ------------------------------------------------
        public static void PrintSuccess(string message)
        {
            Console.WriteLine();
            Console.WriteLine(THIN);
            Console.WriteLine($"    Done. {message}");
            Console.WriteLine(THIN);
        }

        // ── Error message ───────────────────────────────────────────────────────
        // Prints a plain error message prefixed with "Error:".
        // Called inside catch blocks throughout Program.cs.
        public static void PrintError(string message)
        {
            Console.WriteLine();
            Console.WriteLine($"    Error: {message}");
        }

        // ── Back navigation ─────────────────────────────────────────────────────
        // Prints a short message to let the user know they are returning to the menu.
        // Called when the user enters "0" at a back-capable prompt.
        public static void PrintBack()
        {
            Console.WriteLine("\n  Returning to menu...");
        }

        // ── Separator line ──────────────────────────────────────────────────────
        // Prints a thin separator line. Used for visual grouping within sections.
        public static void PrintLine()
        {
            Console.WriteLine(THIN);
        }

        // ── Press Enter to continue ─────────────────────────────────────────────
        // Pauses execution and waits for the user to press Enter.
        // Prevents the screen from clearing before the user has read the output.
        public static void PressEnter()
        {
            Console.WriteLine();
            Console.Write("  Press ENTER to continue...");
            Console.ReadLine();
        }
    }
}