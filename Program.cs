// ============================================================
// Program.cs
// Entry point for the Vaccine Inventory Management System.
// Uses C# Top-Level Statements (no explicit Main method needed).
//
// Responsibilities:
//   1. Shared input helper functions (ReadName, ReadPositiveInt, etc.)
//   2. Login gate — validates credentials, limits to 3 attempts
//   3. Main program loop — displays menu, routes to handler methods
//   4. Handler methods — one per menu option (18 total)
//
// Login Credentials:
//   Username : admin    Password : admin123
//
// Menu Options:
//   [1–5]   Product Management  (Add, View, Search, Update, Delete)
//   [6–8]   Stock Control       (Restock, Deduct, Low Stock)
//   [9–12]  Category Management (Add, View, Update, Delete)
//   [13–16] Supplier Management (Add, View, Update, Delete)
//   [17–18] Reports             (Transaction History, Total Value)
//   [0]     Exit
// ============================================================

using VaccineInventory.Services;
using VaccineInventory.Helpers;
using VaccineInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;

// Valid values for filtering transaction history in option 17
var ValidFilters = new HashSet<string> { "ALL", "ADD", "RESTOCK", "DEDUCT", "UPDATE", "DELETE" };

// Create the InventoryManager — this loads sample data and starts all ID counters
InventoryManager manager = new InventoryManager();

// =====================================================
// SHARED INPUT HELPERS
// Helper functions used across all handler methods
// to consistently read and validate console input.
// =====================================================

// Reads a non-empty string that is NOT purely numeric.
// Used for vaccine names, category names, and supplier names.
// If allowBack=true, entering "0" returns "0" as a back-signal.
string ReadName(string prompt, bool allowBack = false)
{
    while (true)
    {
        Console.Write(prompt);
        string value = (Console.ReadLine() ?? string.Empty).Trim();
        if (allowBack && value == "0") return "0";
        if (string.IsNullOrWhiteSpace(value))
        {
            Console.WriteLine("  Invalid input. Name cannot be empty.");
            continue;
        }
        if (value.All(char.IsDigit))
        {
            Console.WriteLine("  Invalid input. Name cannot be numbers only.");
            continue;
        }
        return value;
    }
}

// Reads any non-empty, non-whitespace string.
// No numeric check — used for contact numbers, notes, and general text.
// If allowBack=true, entering "0" returns "0" as a back-signal.
string ReadText(string prompt, bool allowBack = false)
{
    while (true)
    {
        Console.Write(prompt);
        string value = (Console.ReadLine() ?? string.Empty).Trim();
        if (allowBack && value == "0") return "0";
        if (!string.IsNullOrWhiteSpace(value)) return value;
        Console.WriteLine("  Invalid input. Please try again.");
    }
}

// Reads a positive integer (value must be > 0).
// If allowBack=true, entering "0" returns 0 as a back-signal.
int ReadPositiveInt(string prompt, bool allowBack = false)
{
    while (true)
    {
        Console.Write(prompt);
        string raw = (Console.ReadLine() ?? string.Empty).Trim();
        if (allowBack && raw == "0") return 0;
        if (int.TryParse(raw, out int v) && v > 0) return v;
        Console.WriteLine("  Invalid input. Please enter a positive whole number.");
    }
}

// Reads a non-negative decimal (value must be >= 0).
// Used for prices where 0 is allowed (e.g. free vaccines).
decimal ReadNonNegativeDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string raw = (Console.ReadLine() ?? string.Empty).Trim();
        if (decimal.TryParse(raw, out decimal v) && v >= 0) return v;
        Console.WriteLine("  Invalid input. Please enter a valid number (0 or greater).");
    }
}

// Reads a strictly positive decimal (value must be > 0).
// Used for vaccine prices where free is not a valid entry.
decimal ReadPositiveDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string raw = (Console.ReadLine() ?? string.Empty).Trim();
        if (decimal.TryParse(raw, out decimal v) && v > 0) return v;
        Console.WriteLine("  Invalid input. Please enter a positive number.");
    }
}

// Reads a future date (must be strictly after today).
// Used for entering vaccine expiry dates when adding a new product.
DateTime ReadFutureDate(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string raw = (Console.ReadLine() ?? string.Empty).Trim();
        if (DateTime.TryParse(raw, out DateTime dt) && dt.Date > DateTime.Today) return dt;
        Console.WriteLine("  Invalid input. Please enter a future date (MM/DD/YYYY).");
    }
}

// Reads a Y/N confirmation from the user.
// Accepts: y, yes, n, no (case-insensitive).
// Returns true for yes, false for no.
bool ReadConfirm(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string raw = (Console.ReadLine() ?? string.Empty).Trim().ToLower();
        if (raw == "y" || raw == "yes") return true;
        if (raw == "n" || raw == "no") return false;
        Console.WriteLine("  Invalid input. Please enter Y or N.");
    }
}

// Shorthand helper that asks "Do again?" using ReadConfirm.
// Used at the end of each handler's loop to decide whether to repeat.
bool AskDoAgain(string label) => ReadConfirm($"\n  {label} [Y/N]: ");

// Reads a Category ID and validates it exists in the system.
// Returns 0 if the user types "0" (back-signal).
int ReadCategoryId(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string raw = (Console.ReadLine() ?? string.Empty).Trim();
        if (raw == "0") return 0;
        if (int.TryParse(raw, out int id) && id > 0 && manager.GetCategoryById(id) != null) return id;
        Console.WriteLine("  Invalid input. Please enter a valid Category ID from the list.");
    }
}

// Reads a Supplier ID and validates it exists in the system.
// Returns 0 if the user types "0" (back-signal).
int ReadSupplierId(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string raw = (Console.ReadLine() ?? string.Empty).Trim();
        if (raw == "0") return 0;
        if (int.TryParse(raw, out int id) && id > 0 && manager.GetSupplierById(id) != null) return id;
        Console.WriteLine("  Invalid input. Please enter a valid Supplier ID from the list.");
    }
}

// Reads a Product (Vaccine) ID and validates it exists in the system.
// Returns 0 if the user types "0" (back-signal).
int ReadVaccineId(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string raw = (Console.ReadLine() ?? string.Empty).Trim();
        if (raw == "0") return 0;
        if (int.TryParse(raw, out int id) && id > 0 && manager.GetVaccineById(id) != null) return id;
        Console.WriteLine("  Invalid input. Please enter a valid Product ID from the list.");
    }
}

// =====================================================
// LOGIN GATE
// Accepts only the admin account.
// Allows up to 3 attempts before force-exiting.
//
// Credentials:
//   Username : admin
//   Password : admin123
// =====================================================
Console.Clear();
Console.WriteLine();
Console.WriteLine("  ================================================");
Console.WriteLine("    VACCINE INVENTORY MANAGEMENT SYSTEM");
Console.WriteLine("  ================================================");
Console.WriteLine();
Console.WriteLine("  Please log in to continue.");
Console.WriteLine();

// Hardcoded credentials for the login gate
// (The same account also exists in InventoryManager's user list)
const string validUsername = "admin";
const string validPassword = "admin123";

User? currentUser = null;
int loginAttempts = 0;
const int maxAttempts = 3; // lock out after 3 failed attempts

while (currentUser == null)
{
    Console.Write("  Username : ");
    string username = Console.ReadLine() ?? string.Empty;

    Console.Write("  Password : ");
    string password = Console.ReadLine() ?? string.Empty;

    if (username.Trim() == validUsername && password == validPassword)
    {
        // Credentials match — look up the full User object from the manager
        currentUser = manager.GetUsers().First(u => u.Username == validUsername);
        Console.WriteLine();
        Console.WriteLine($"  Welcome, {currentUser.Username}  ({currentUser.Role})");
    }
    else
    {
        loginAttempts++;
        int remaining = maxAttempts - loginAttempts;
        Console.WriteLine();
        Console.WriteLine("  Invalid username or password.");
        if (loginAttempts >= maxAttempts)
        {
            // Too many failures — exit the program for security
            Console.WriteLine("  Too many failed attempts. Exiting.");
            Environment.Exit(0);
        }
        Console.WriteLine($"  {remaining} attempt(s) remaining.");
        Console.WriteLine();
    }
}

// Show post-login messages and the dashboard before entering the menu loop
Console.WriteLine();
Console.WriteLine("  Loading inventory data...");
Console.WriteLine("  System ready!");
DisplayHelper.ShowFullDashboard(manager); // shows total products, low stock, expired, total value
DisplayHelper.PressEnter();

// =====================================================
// MAIN PROGRAM LOOP
// Displays the menu and routes to the appropriate
// handler method based on the user's selection.
// Loop continues until the user enters "0" to exit.
// =====================================================
bool isRunning = true;

while (isRunning)
{
    Console.Clear();
    DisplayHelper.ShowMainMenu(currentUser.Username, currentUser.Role);

    Console.Write("\n  Select option: ");
    string input = (Console.ReadLine() ?? string.Empty).Trim();
    Console.Clear();

    switch (input)
    {
        // ── PRODUCT MANAGEMENT ──
        case "1": HandleAddVaccine(); break; // Add a new vaccine product
        case "2": HandleViewAllVaccines(); break; // View/filter/sort all products
        case "3": HandleSearchVaccine(); break; // Search by name/category/supplier/ID
        case "4": HandleUpdateVaccine(); break; // Edit vaccine name/price/expiry/min stock
        case "5": HandleDeleteVaccine(); break; // Remove a product from inventory

        // ── STOCK CONTROL ──
        case "6": HandleRestockVaccine(); break; // Add units to stock
        case "7": HandleDeductVaccine(); break; // Remove units from stock
        case "8": HandleLowStock(); break; // Show products below minimum stock level

        // ── CATEGORY MANAGEMENT ──
        case "9": HandleAddCategory(); break; // Create a new category
        case "10": HandleViewCategories(); break; // List all categories
        case "11": HandleUpdateCategory(); break; // Rename a category
        case "12": HandleDeleteCategory(); break; // Remove a category (if not in use)

        // ── SUPPLIER MANAGEMENT ──
        case "13": HandleAddSupplier(); break; // Create a new supplier
        case "14": HandleViewSuppliers(); break; // List all suppliers
        case "15": HandleUpdateSupplier(); break; // Update supplier name/contact
        case "16": HandleDeleteSupplier(); break; // Remove a supplier (if not in use)

        // ── REPORTS ──
        case "17": HandleViewTransactions(); break; // View/filter transaction audit log
        case "18": HandleTotalValue(); break; // Show total inventory value

        // ── EXIT ──
        case "0":
            isRunning = false;
            Console.WriteLine();
            Console.WriteLine($"  Goodbye, {currentUser.Username}. Thank you!");
            Console.WriteLine();
            break;

        default:
            // Any unrecognized input just shows an error and loops back
            Console.WriteLine();
            Console.WriteLine("  Invalid choice. Please select a number from the menu.");
            DisplayHelper.PressEnter();
            break;
    }
}

// =====================================================
// HANDLER METHODS
// Each method below corresponds to one menu option.
// All handlers follow the same pattern:
//   1. Print a section header
//   2. Collect and validate user input
//   3. Call the relevant InventoryManager method
//   4. Show success or catch and display errors
//   5. Ask if the user wants to do it again (loop)
// =====================================================

// ══════════════════════════════════════════════════
// PRODUCT MANAGEMENT
// ══════════════════════════════════════════════════

// ── [1] Add Product ─────────────────────────────────
// Collects all product details, shows a summary,
// then calls manager.AddVaccine().
// If the product already exists, catches DuplicateProductException
// and offers to restock the existing product instead.
void HandleAddVaccine()
{
    DisplayHelper.PrintSectionHeader("PRODUCT MANAGEMENT", "ADD PRODUCT");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        // Show available categories and suppliers so the user can pick valid IDs
        Console.WriteLine("  Categories:");
        foreach (var cat in manager.GetCategories())
            Console.WriteLine($"    {cat.CategoryId}  {cat.CategoryName}");
        Console.WriteLine();
        Console.WriteLine("  Suppliers:");
        foreach (var sup in manager.GetSuppliers())
            Console.WriteLine($"    {sup.SupplierId}  {sup.SupplierName}");
        Console.WriteLine();

        string name = ReadName("  Vaccine Name        : ", allowBack: true);
        if (name == "0") { DisplayHelper.PrintBack(); return; }

        int catId = ReadCategoryId("  Category ID         : ");
        if (catId == 0) { DisplayHelper.PrintBack(); return; }

        int supId = ReadSupplierId("  Supplier ID         : ");
        if (supId == 0) { DisplayHelper.PrintBack(); return; }

        // Quantity allows 0 (valid to add a product with zero initial stock)
        int qty;
        while (true)
        {
            Console.Write("  Quantity            : ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (raw == "0") { DisplayHelper.PrintBack(); return; }
            if (int.TryParse(raw, out qty) && qty >= 0) break;
            Console.WriteLine("  Invalid input. Please enter 0 or a positive whole number.");
        }

        decimal price = ReadPositiveDecimal("  Price (PHP)         : ");
        DateTime expiry = ReadFutureDate("  Expiry Date (MM/DD/YYYY): ");
        int minStock = ReadPositiveInt("  Min Stock Level     : ");

        // Warn if expiry is within 30 days — near-expiry products are unusual to add
        int daysToExpiry = (int)(expiry.Date - DateTime.Today).TotalDays;
        if (daysToExpiry <= 30)
        {
            Console.WriteLine($"\n  Warning: This vaccine expires in {daysToExpiry} day(s) ({expiry:MM/dd/yyyy}).");
            if (!ReadConfirm("  Proceed anyway? [Y/N]: "))
            {
                Console.WriteLine("  Cancelled.");
                Console.WriteLine();
                keepGoing = AskDoAgain("Add another product?");
                Console.WriteLine();
                continue;
            }
        }

        // Show summary so the user can review before confirming
        Console.WriteLine();
        Console.WriteLine("  Summary:");
        Console.WriteLine($"    Name       : {name}");
        Console.WriteLine($"    Category   : {manager.GetCategoryName(catId)}");
        Console.WriteLine($"    Supplier   : {manager.GetSupplierName(supId)}");
        Console.WriteLine($"    Quantity   : {qty}");
        Console.WriteLine($"    Price      : P{price:F2}");
        Console.WriteLine($"    Expiry     : {expiry:MM/dd/yyyy}");
        Console.WriteLine($"    Min Stock  : {minStock}");

        if (!ReadConfirm("\n  Confirm add product? [Y/N]: "))
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            try
            {
                manager.AddVaccine(name, catId, supId, qty, price, expiry, minStock);
                DisplayHelper.PrintSuccess("Product added successfully.");
            }
            catch (DuplicateProductException dupEx)
            {
                // Special case: vaccine with this name already exists
                // Offer to redirect the user to restock that product instead
                var dup = manager.GetVaccineById(dupEx.ExistingProductId);
                Console.WriteLine();
                Console.WriteLine($"  Product already exists: [{dup!.ProductId}] {dup.VaccineName}  (Qty: {dup.Quantity})");
                if (ReadConfirm("  Do you want to restock instead? [Y/N]: "))
                {
                    int restockQty = ReadPositiveInt($"  Quantity to add to '{dup.VaccineName}': ");
                    if (ReadConfirm($"\n  Restock '{dup.VaccineName}' by {restockQty} unit(s)? [Y/N]: "))
                    {
                        try
                        {
                            manager.RestockVaccine(dup.ProductId, restockQty);
                            DisplayHelper.PrintSuccess("Product restocked successfully.");
                        }
                        catch (Exception ex2) { DisplayHelper.PrintError(ex2.Message); }
                    }
                    else Console.WriteLine("  Restock cancelled.");
                }
                else Console.WriteLine("  Cancelled.");
            }
            catch (Exception ex) { DisplayHelper.PrintError(ex.Message); }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Add another product?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [2] View All Products ────────────────────────────
// Displays all products with optional filter (ALL/LOW/EXPIRED/OK/Category)
// and optional sort (NAME/QTY/PRICE/EXPIRY).
void HandleViewAllVaccines()
{
    DisplayHelper.PrintSectionHeader("PRODUCT MANAGEMENT", "VIEW ALL PRODUCTS");
    Console.WriteLine("  Enter 0 to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        // Show available filter options including dynamic category list
        Console.WriteLine("  Filter options:");
        Console.WriteLine("    ALL      — Show all products");
        Console.WriteLine("    LOW      — Low stock only");
        Console.WriteLine("    EXPIRED  — Expired only");
        Console.WriteLine("    OK       — Sufficient stock & not expired");

        var categories = manager.GetCategories();
        if (categories.Count > 0)
        {
            Console.WriteLine("\n  Or filter by Category ID:");
            foreach (var cat in categories)
                Console.WriteLine($"    {cat.CategoryId,-4} — {cat.CategoryName}");
        }

        // Validate the filter input — must be a keyword or a valid category ID
        string filterInput;
        while (true)
        {
            Console.Write("\n  Filter [ALL/LOW/EXPIRED/OK/CategoryID or 0]: ");
            filterInput = (Console.ReadLine() ?? string.Empty).Trim().ToUpper();
            if (filterInput == "0") { DisplayHelper.PrintBack(); return; }
            if (string.IsNullOrWhiteSpace(filterInput)) { filterInput = "ALL"; break; }
            if (filterInput == "ALL" || filterInput == "LOW" || filterInput == "EXPIRED" || filterInput == "OK") break;
            if (int.TryParse(filterInput, out int cid) && cid > 0 &&
                manager.GetCategories().Any(c => c.CategoryId == cid)) break;
            Console.WriteLine("  Invalid input. Please try again.");
        }

        var allVaccines = manager.GetVaccines();
        List<Product> filtered;
        string filterLabel;

        // Apply the selected filter
        if (int.TryParse(filterInput, out int catId))
        {
            // Filter by specific category ID
            var cat = manager.GetCategories().First(c => c.CategoryId == catId);
            filtered = allVaccines.Where(v => v.CategoryId == catId).ToList();
            filterLabel = $"Category: {cat.CategoryName}";
        }
        else
        {
            // Filter by keyword
            (filtered, filterLabel) = filterInput switch
            {
                "LOW" => (allVaccines.Where(v => v.IsLowStock() && !v.IsExpired()).ToList(), "Low Stock"),
                "EXPIRED" => (allVaccines.Where(v => v.IsExpired()).ToList(), "Expired"),
                "OK" => (allVaccines.Where(v => !v.IsLowStock() && !v.IsExpired()).ToList(), "Sufficient Stock / Not Expired"),
                _ => (allVaccines.ToList(), "All Products")
            };
        }

        // Optional sorting (only shown if there are results)
        if (filtered.Count > 0)
        {
            Console.Write("\n  Sort by [NAME/QTY/PRICE/EXPIRY — press Enter to skip]: ");
            string sortInput = (Console.ReadLine() ?? string.Empty).Trim().ToUpper();
            filtered = sortInput switch
            {
                "NAME" => filtered.OrderBy(v => v.VaccineName).ToList(),
                "QTY" => filtered.OrderBy(v => v.Quantity).ToList(),
                "PRICE" => filtered.OrderBy(v => v.Price).ToList(),
                "EXPIRY" => filtered.OrderBy(v => v.ExpiryDate).ToList(),
                _ => filtered
            };
            if (!string.IsNullOrWhiteSpace(sortInput) &&
                sortInput != "NAME" && sortInput != "QTY" && sortInput != "PRICE" && sortInput != "EXPIRY")
                Console.WriteLine("  Unrecognised sort key — displaying in default order.");
        }

        Console.WriteLine($"\n  Showing: [ {filterLabel} ]");
        DisplayHelper.ShowVaccineTable(filtered, manager);
        Console.WriteLine($"  Total shown: {filtered.Count} of {allVaccines.Count} product(s)");

        Console.WriteLine();
        keepGoing = AskDoAgain("Filter / view again?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [3] Search Product ───────────────────────────────
// Lets the user search by product name, category name, supplier name,
// or exact product ID. Results are displayed in a table.
void HandleSearchVaccine()
{
    DisplayHelper.PrintSectionHeader("PRODUCT MANAGEMENT", "SEARCH PRODUCT");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        Console.WriteLine("  Search by:");
        Console.WriteLine("    1  Product Name");
        Console.WriteLine("    2  Category");
        Console.WriteLine("    3  Supplier");
        Console.WriteLine("    4  Product ID");

        int searchType;
        while (true)
        {
            Console.Write("\n  Choose search type [1–4 or 0]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (raw == "0") { DisplayHelper.PrintBack(); return; }
            if (int.TryParse(raw, out searchType) && searchType >= 1 && searchType <= 4) break;
            Console.WriteLine("  Invalid input. Please enter 1, 2, 3, or 4.");
        }

        List<Product> results;
        string searchLabel;

        if (searchType == 4)
        {
            // Search by exact product ID
            int searchId;
            while (true)
            {
                Console.Write("  Enter Product ID: ");
                string raw = (Console.ReadLine() ?? string.Empty).Trim();
                if (raw == "0") { DisplayHelper.PrintBack(); return; }
                if (int.TryParse(raw, out searchId) && searchId > 0) break;
                Console.WriteLine("  Invalid input. Please enter a positive number.");
            }
            results = manager.SearchVaccineById(searchId);
            searchLabel = $"Product ID = {searchId}";
        }
        else
        {
            // Search by keyword (partial, case-insensitive match)
            string typeLabel = searchType switch { 1 => "Product Name", 2 => "Category", _ => "Supplier" };
            string keyword = ReadText($"  Enter {typeLabel} keyword: ", allowBack: true);
            if (keyword == "0") { DisplayHelper.PrintBack(); return; }

            string kw = keyword.ToLower();
            var all = manager.GetVaccines();
            results = searchType switch
            {
                1 => all.Where(v => v.VaccineName.ToLower().Contains(kw)).ToList(),
                2 => all.Where(v => manager.GetCategoryName(v.CategoryId).ToLower().Contains(kw)).ToList(),
                3 => all.Where(v => manager.GetSupplierName(v.SupplierId).ToLower().Contains(kw)).ToList(),
                _ => new List<Product>()
            };
            searchLabel = $"{typeLabel}: \"{keyword}\"";
        }

        Console.WriteLine($"\n  Search results for  {searchLabel}:");
        DisplayHelper.ShowVaccineTable(results, manager);
        Console.WriteLine($"  Found: {results.Count} result(s)");

        Console.WriteLine();
        keepGoing = AskDoAgain("Search again?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [4] Update Product ───────────────────────────────
// Allows editing vaccine name, price, expiry date, and minimum stock level.
// Leaving a field blank keeps the existing value.
// Expiry date: keeping an already-expired date is allowed;
// only actively setting a new past date is rejected.
void HandleUpdateVaccine()
{
    DisplayHelper.PrintSectionHeader("PRODUCT MANAGEMENT", "UPDATE PRODUCT");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");
    Console.WriteLine("  Leave any field blank to keep the current value.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        DisplayHelper.ShowVaccineTable(manager.GetVaccines(), manager);

        int id = ReadVaccineId("\n  Enter Product ID to update [0 = back]: ");
        if (id == 0) { DisplayHelper.PrintBack(); return; }

        var existing = manager.GetVaccineById(id)!;
        Console.WriteLine();
        Console.WriteLine($"  Updating: {existing.VaccineName}");

        // --- Name ---
        // Blank input = keep current; purely numeric names are rejected
        string newName;
        while (true)
        {
            Console.Write($"  New Name        [{existing.VaccineName}]: ");
            string raw = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw)) { newName = existing.VaccineName; break; }
            string trimmed = raw.Trim();
            if (trimmed.All(char.IsDigit)) { Console.WriteLine("  Invalid input. Name cannot be numbers only."); continue; }
            newName = trimmed; break;
        }

        // --- Price ---
        // Blank input = keep current; must be 0 or greater
        decimal newPrice;
        while (true)
        {
            Console.Write($"  New Price       [₱{existing.Price:F2}]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw)) { newPrice = existing.Price; break; }
            if (decimal.TryParse(raw, out newPrice) && newPrice >= 0) break;
            Console.WriteLine("  Invalid input. Please enter a valid price (0 or greater).");
        }

        // --- Expiry Date ---
        // Blank input = keep current (even if already expired).
        // New date must be a future date.
        DateTime newExpiry;
        while (true)
        {
            Console.Write($"  New Expiry Date [{existing.ExpiryDate:MM/dd/yyyy}]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw))
            {
                newExpiry = existing.ExpiryDate;
                // Inform the user if they're keeping an expired date (not blocking, just a note)
                if (existing.IsExpired())
                    Console.WriteLine($"  Note: '{existing.VaccineName}' is already expired. Consider setting a new date.");
                break;
            }
            if (DateTime.TryParse(raw, out newExpiry) && newExpiry.Date > DateTime.Today)
            {
                int daysLeft = (int)(newExpiry.Date - DateTime.Today).TotalDays;
                if (daysLeft <= 30)
                    Console.WriteLine($"  Note: New expiry is only {daysLeft} day(s) away ({newExpiry:MM/dd/yyyy}).");
                break;
            }
            Console.WriteLine("  Invalid input. Please enter a future date (MM/DD/YYYY).");
        }

        // --- Minimum Stock Level ---
        // Blank input = keep current; must be at least 1
        int newMin;
        while (true)
        {
            Console.Write($"  New Min Stock   [{existing.MinimumStockLevel}]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw)) { newMin = existing.MinimumStockLevel; break; }
            if (int.TryParse(raw, out newMin) && newMin >= 1) break;
            Console.WriteLine("  Invalid input. Minimum stock must be at least 1.");
        }

        if (!ReadConfirm($"\n  Confirm update for '{existing.VaccineName}'? [Y/N]: "))
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            try
            {
                manager.UpdateVaccine(id, newName, newPrice, newExpiry, newMin);
                DisplayHelper.PrintSuccess("Product updated successfully.");
            }
            catch (Exception ex) { DisplayHelper.PrintError(ex.Message); }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Update another product?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [5] Delete Product ───────────────────────────────
// Permanently removes a vaccine from inventory after confirmation.
// The deletion is logged in transaction history before removal.
void HandleDeleteVaccine()
{
    DisplayHelper.PrintSectionHeader("PRODUCT MANAGEMENT", "DELETE PRODUCT");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        DisplayHelper.ShowVaccineTable(manager.GetVaccines(), manager);

        int id = ReadVaccineId("\n  Enter Product ID to delete [0 = back]: ");
        if (id == 0) { DisplayHelper.PrintBack(); return; }

        var vaccine = manager.GetVaccineById(id)!;
        Console.WriteLine();
        Console.WriteLine($"  Delete: [{vaccine.ProductId}] {vaccine.VaccineName}");

        if (!ReadConfirm($"  Confirm deletion? [Y/N]: "))
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            try
            {
                manager.DeleteVaccine(id);
                DisplayHelper.PrintSuccess("Product deleted successfully.");
            }
            catch (Exception ex) { DisplayHelper.PrintError(ex.Message); }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Delete another product?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ══════════════════════════════════════════════════
// STOCK CONTROL
// ══════════════════════════════════════════════════

// ── [6] Restock Product ─────────────────────────────
// Adds units to a product's stock quantity.
// Warns if the product is expired or expiring within 30 days.
// Asks for confirmation before calling manager.RestockVaccine().
void HandleRestockVaccine()
{
    DisplayHelper.PrintSectionHeader("STOCK CONTROL", "RESTOCK PRODUCT");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        DisplayHelper.ShowVaccineTable(manager.GetVaccines(), manager);

        int id = ReadVaccineId("\n  Enter Product ID to restock [0 = back]: ");
        if (id == 0) { DisplayHelper.PrintBack(); return; }

        var vaccine = manager.GetVaccineById(id)!;
        bool proceed = true;

        if (vaccine.IsExpired())
        {
            // Warn the user — restocking an expired product is unusual
            Console.WriteLine($"\n  '{vaccine.VaccineName}' is EXPIRED  (expiry: {vaccine.ExpiryDate:MM/dd/yyyy}).");
            proceed = ReadConfirm("  Restocking an expired product is unusual. Proceed? [Y/N]: ");
        }
        else
        {
            // Inform if expiry is within 30 days (not blocking, just a heads-up)
            int daysLeft = (int)(vaccine.ExpiryDate.Date - DateTime.Today).TotalDays;
            if (daysLeft <= 30)
                Console.WriteLine($"\n  Note: '{vaccine.VaccineName}' expires in {daysLeft} day(s) ({vaccine.ExpiryDate:MM/dd/yyyy}).");
        }

        if (!proceed)
        {
            Console.WriteLine("  Restock cancelled.");
        }
        else
        {
            int qty = ReadPositiveInt("  Quantity to add        : ");
            if (!ReadConfirm($"\n  Restock '{vaccine.VaccineName}' by {qty} unit(s)? [Y/N]: "))
            {
                Console.WriteLine("  Restock cancelled.");
            }
            else
            {
                try
                {
                    manager.RestockVaccine(id, qty);
                    DisplayHelper.PrintSuccess("Product restocked successfully.");
                }
                catch (Exception ex) { DisplayHelper.PrintError(ex.Message); }
            }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Restock another product?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [7] Deduct Stock ─────────────────────────────────
// Removes units from a product's stock quantity.
// Prevents deducting more than current stock.
// Warns if the product is expired, or if the deduction brings stock to zero.
void HandleDeductVaccine()
{
    DisplayHelper.PrintSectionHeader("STOCK CONTROL", "DEDUCT STOCK");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        DisplayHelper.ShowVaccineTable(manager.GetVaccines(), manager);

        int id = ReadVaccineId("\n  Enter Product ID to deduct from [0 = back]: ");
        if (id == 0) { DisplayHelper.PrintBack(); return; }

        var vaccine = manager.GetVaccineById(id)!;
        bool proceed = true;

        if (vaccine.IsExpired())
        {
            // Warn the user — deducting from an expired product is unusual
            Console.WriteLine($"\n  '{vaccine.VaccineName}' is EXPIRED  (expiry: {vaccine.ExpiryDate:MM/dd/yyyy}).");
            proceed = ReadConfirm("  Deducting from an expired product is unusual. Proceed? [Y/N]: ");
        }

        if (!proceed)
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            // Keep asking for quantity until a valid deduction amount is entered
            int qty;
            while (true)
            {
                qty = ReadPositiveInt("  Quantity to deduct     : ");
                if (vaccine.Quantity - qty >= 0) break;
                Console.WriteLine($"  Cannot deduct {qty}. Current stock is only {vaccine.Quantity}.");
            }

            // Extra warning if this would bring stock to exactly zero
            if (vaccine.Quantity - qty == 0)
            {
                Console.WriteLine($"\n  This will bring '{vaccine.VaccineName}' to ZERO stock.");
                if (!ReadConfirm("  Proceed? [Y/N]: "))
                {
                    Console.WriteLine("  Cancelled.");
                    Console.WriteLine();
                    keepGoing = AskDoAgain("Deduct from another product?");
                    Console.WriteLine();
                    continue;
                }
            }

            if (!ReadConfirm($"\n  Deduct {qty} unit(s) from '{vaccine.VaccineName}'? [Y/N]: "))
            {
                Console.WriteLine("  Cancelled.");
            }
            else
            {
                try
                {
                    manager.DeductVaccine(id, qty);
                    // Post-deduction check: notify if now out of stock
                    var updated = manager.GetVaccineById(id);
                    if (updated != null && updated.Quantity == 0)
                        Console.WriteLine($"\n  '{updated.VaccineName}' is now OUT OF STOCK. Please restock soon.");
                    DisplayHelper.PrintSuccess("Stock deducted successfully.");
                }
                catch (Exception ex) { DisplayHelper.PrintError(ex.Message); }
            }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Deduct from another product?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [8] Low Stock Items ──────────────────────────────
// Displays all products currently below their minimum stock level.
// No user input required — just shows the table and waits for Enter.
void HandleLowStock()
{
    DisplayHelper.PrintSectionHeader("STOCK CONTROL", "LOW STOCK ITEMS");

    var lowStockVaccines = manager.GetLowStockVaccines();
    if (lowStockVaccines.Count == 0)
    {
        Console.WriteLine("\n  All products have sufficient stock. No alerts.");
    }
    else
    {
        Console.WriteLine($"\n  WARNING: {lowStockVaccines.Count} product(s) are below minimum stock level!\n");
        DisplayHelper.ShowVaccineTable(lowStockVaccines, manager);
        Console.WriteLine("\n  Please restock the products listed above as soon as possible.");
    }
    DisplayHelper.PressEnter();
}

// ══════════════════════════════════════════════════
// CATEGORY MANAGEMENT
// ══════════════════════════════════════════════════

// ── [9] Add Category ────────────────────────────────
// Creates a new vaccine category (e.g., COVID-19, Hepatitis B).
// Duplicates and purely-numeric names are rejected at the service layer.
void HandleAddCategory()
{
    DisplayHelper.PrintSectionHeader("CATEGORY MANAGEMENT", "ADD CATEGORY");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        string name = ReadName("  Category Name  : ", allowBack: true);
        if (name == "0") { DisplayHelper.PrintBack(); return; }

        if (!ReadConfirm($"\n  Add category '{name}'? [Y/N]: "))
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            try
            {
                manager.AddCategory(name);
                DisplayHelper.PrintSuccess("Category added successfully.");
            }
            catch (Exception ex) { DisplayHelper.PrintError(ex.Message); }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Add another category?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [10] View Categories ────────────────────────────
// Lists all categories with their IDs. No input required.
void HandleViewCategories()
{
    DisplayHelper.PrintSectionHeader("CATEGORY MANAGEMENT", "VIEW CATEGORIES");

    var cats = manager.GetCategories();
    if (cats.Count == 0)
    {
        Console.WriteLine("\n  No categories found.");
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine($"  {"ID",-6} Category Name");
        Console.WriteLine("  " + new string('-', 36));
        foreach (var cat in cats)
            Console.WriteLine($"  {cat.CategoryId,-6} {cat.CategoryName}");
        Console.WriteLine("  " + new string('-', 36));
        Console.WriteLine($"\n  Total: {cats.Count} category/ies");
    }
    DisplayHelper.PressEnter();
}

// ── [11] Update Category ────────────────────────────
// Renames an existing category. Blank input keeps the current name.
// Rejects names that conflict with another existing category.
void HandleUpdateCategory()
{
    DisplayHelper.PrintSectionHeader("CATEGORY MANAGEMENT", "UPDATE CATEGORY");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        var cats = manager.GetCategories();
        if (cats.Count == 0) { Console.WriteLine("\n  No categories available to update."); break; }

        Console.WriteLine($"  {"ID",-6} Category Name");
        Console.WriteLine("  " + new string('-', 36));
        foreach (var c in cats)
            Console.WriteLine($"  {c.CategoryId,-6} {c.CategoryName}");
        Console.WriteLine("  " + new string('-', 36));

        int id;
        while (true)
        {
            Console.Write("\n  Enter Category ID to update [0 = back]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (raw == "0") { DisplayHelper.PrintBack(); return; }
            if (int.TryParse(raw, out id) && manager.GetCategoryById(id) != null) break;
            Console.WriteLine("  Invalid input. Please enter a valid Category ID.");
        }

        var existing = manager.GetCategoryById(id)!;
        Console.WriteLine($"\n  Current Name: {existing.CategoryName}");

        // Blank input = keep current name; purely numeric names are rejected
        string newName;
        while (true)
        {
            Console.Write($"  New Name [{existing.CategoryName}]: ");
            string raw = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw)) { newName = existing.CategoryName; break; }
            string trimmed = raw.Trim();
            if (trimmed.All(char.IsDigit)) { Console.WriteLine("  Invalid input. Name cannot be numbers only."); continue; }
            newName = trimmed; break;
        }

        if (!ReadConfirm($"\n  Update category to '{newName}'? [Y/N]: "))
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            try
            {
                manager.UpdateCategory(id, newName);
                DisplayHelper.PrintSuccess("Category updated successfully.");
            }
            catch (Exception ex) { DisplayHelper.PrintError(ex.Message); }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Update another category?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [12] Delete Category ────────────────────────────
// Removes a category. Blocked if the category is assigned to any product.
void HandleDeleteCategory()
{
    DisplayHelper.PrintSectionHeader("CATEGORY MANAGEMENT", "DELETE CATEGORY");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    if (manager.GetCategories().Count == 0)
    {
        Console.WriteLine("  No categories to delete.");
        DisplayHelper.PressEnter();
        return;
    }

    bool keepGoing = true;
    while (keepGoing)
    {
        var cats = manager.GetCategories();
        if (cats.Count == 0) { Console.WriteLine("\n  No more categories to delete."); break; }

        Console.WriteLine($"  {"ID",-6} Category Name");
        Console.WriteLine("  " + new string('-', 36));
        foreach (var c in cats)
            Console.WriteLine($"  {c.CategoryId,-6} {c.CategoryName}");
        Console.WriteLine("  " + new string('-', 36));

        int id;
        while (true)
        {
            Console.Write("\n  Enter Category ID to delete [0 = back]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (raw == "0") { DisplayHelper.PrintBack(); return; }
            if (int.TryParse(raw, out id) && manager.GetCategoryById(id) != null) break;
            Console.WriteLine("  Invalid input. Please enter a valid Category ID.");
        }

        var found = manager.GetCategoryById(id)!;
        Console.WriteLine($"\n  You are about to delete: [{found.CategoryId}] {found.CategoryName}");

        if (!ReadConfirm($"  Confirm deletion? [Y/N]: "))
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            try
            {
                manager.DeleteCategory(id);
                DisplayHelper.PrintSuccess("Category deleted successfully.");
            }
            catch (Exception ex)
            {
                // If deletion fails (e.g., category in use), stop looping and return
                DisplayHelper.PrintError(ex.Message);
                DisplayHelper.PressEnter();
                return;
            }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Delete another category?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ══════════════════════════════════════════════════
// SUPPLIER MANAGEMENT
// ══════════════════════════════════════════════════

// ── [13] Add Supplier ───────────────────────────────
// Creates a new supplier. Contact number must be digits only, 7–15 chars.
// Format validation happens in manager.AddSupplier() via ValidateContactNumber().
void HandleAddSupplier()
{
    DisplayHelper.PrintSectionHeader("SUPPLIER MANAGEMENT", "ADD SUPPLIER");
    Console.WriteLine("  Enter 0 at any prompt to go back.");
    Console.WriteLine("  Contact number: digits only, 7–15 chars, optional leading +\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        string name = ReadName("  Supplier Name   : ", allowBack: true);
        if (name == "0") { DisplayHelper.PrintBack(); return; }

        string contact = ReadText("  Contact Number  : ");

        if (!ReadConfirm($"\n  Add supplier '{name}' ({contact})? [Y/N]: "))
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            try
            {
                manager.AddSupplier(name, contact);
                DisplayHelper.PrintSuccess("Supplier added successfully.");
            }
            catch (Exception ex) { DisplayHelper.PrintError(ex.Message); }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Add another supplier?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [14] View Suppliers ─────────────────────────────
// Lists all suppliers with their IDs and contact numbers. No input required.
void HandleViewSuppliers()
{
    DisplayHelper.PrintSectionHeader("SUPPLIER MANAGEMENT", "VIEW SUPPLIERS");

    var sups = manager.GetSuppliers();
    if (sups.Count == 0)
    {
        Console.WriteLine("\n  No suppliers found.");
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine($"  {"ID",-6} {"Supplier Name",-26} Contact");
        Console.WriteLine("  " + new string('-', 50));
        foreach (var s in sups)
            Console.WriteLine($"  {s.SupplierId,-6} {s.SupplierName,-26} {s.ContactNumber}");
        Console.WriteLine("  " + new string('-', 50));
        Console.WriteLine($"\n  Total: {sups.Count} supplier(s)");
    }
    DisplayHelper.PressEnter();
}

// ── [15] Update Supplier ────────────────────────────
// Edits a supplier's name and/or contact number.
// Blank input keeps the current value.
// Contact number format is validated in manager.UpdateSupplier().
void HandleUpdateSupplier()
{
    DisplayHelper.PrintSectionHeader("SUPPLIER MANAGEMENT", "UPDATE SUPPLIER");
    Console.WriteLine("  Enter 0 at any prompt to go back.");
    Console.WriteLine("  Contact number: digits only, 7–15 chars, optional leading +\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        var sups = manager.GetSuppliers();
        if (sups.Count == 0) { Console.WriteLine("\n  No suppliers available to update."); break; }

        Console.WriteLine($"  {"ID",-6} {"Supplier Name",-26} Contact");
        Console.WriteLine("  " + new string('-', 50));
        foreach (var s in sups)
            Console.WriteLine($"  {s.SupplierId,-6} {s.SupplierName,-26} {s.ContactNumber}");
        Console.WriteLine("  " + new string('-', 50));

        int id;
        while (true)
        {
            Console.Write("\n  Enter Supplier ID to update [0 = back]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (raw == "0") { DisplayHelper.PrintBack(); return; }
            if (int.TryParse(raw, out id) && manager.GetSupplierById(id) != null) break;
            Console.WriteLine("  Invalid input. Please enter a valid Supplier ID.");
        }

        var existing = manager.GetSupplierById(id)!;
        Console.WriteLine($"\n  Updating: {existing.SupplierName}");
        Console.WriteLine("  Leave any field blank to keep the current value.\n");

        // --- Supplier Name ---
        // Blank = keep current; purely numeric names rejected
        string newName;
        while (true)
        {
            Console.Write($"  New Name    [{existing.SupplierName}]: ");
            string raw = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw)) { newName = existing.SupplierName; break; }
            string trimmed = raw.Trim();
            if (trimmed.All(char.IsDigit)) { Console.WriteLine("  Invalid input. Name cannot be numbers only."); continue; }
            newName = trimmed; break;
        }

        // --- Contact Number ---
        // Blank = keep current; format validated downstream in manager.UpdateSupplier()
        string newContact;
        while (true)
        {
            Console.Write($"  New Contact [{existing.ContactNumber}]: ");
            string raw = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw)) { newContact = existing.ContactNumber; break; }
            if (!string.IsNullOrWhiteSpace(raw.Trim())) { newContact = raw.Trim(); break; }
            Console.WriteLine("  Invalid input. Please try again.");
        }

        if (!ReadConfirm($"\n  Update supplier to '{newName}' / '{newContact}'? [Y/N]: "))
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            try
            {
                manager.UpdateSupplier(id, newName, newContact);
                DisplayHelper.PrintSuccess("Supplier updated successfully.");
            }
            catch (Exception ex) { DisplayHelper.PrintError(ex.Message); }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Update another supplier?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [16] Delete Supplier ────────────────────────────
// Removes a supplier after confirmation.
// Blocked if the supplier is assigned to any product.
void HandleDeleteSupplier()
{
    DisplayHelper.PrintSectionHeader("SUPPLIER MANAGEMENT", "DELETE SUPPLIER");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    if (manager.GetSuppliers().Count == 0)
    {
        Console.WriteLine("  No suppliers to delete.");
        DisplayHelper.PressEnter();
        return;
    }

    bool keepGoing = true;
    while (keepGoing)
    {
        var sups = manager.GetSuppliers();
        if (sups.Count == 0) { Console.WriteLine("\n  No more suppliers to delete."); break; }

        Console.WriteLine($"  {"ID",-6} {"Supplier Name",-26} Contact");
        Console.WriteLine("  " + new string('-', 50));
        foreach (var s in sups)
            Console.WriteLine($"  {s.SupplierId,-6} {s.SupplierName,-26} {s.ContactNumber}");
        Console.WriteLine("  " + new string('-', 50));

        int id;
        while (true)
        {
            Console.Write("\n  Enter Supplier ID to delete [0 = back]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (raw == "0") { DisplayHelper.PrintBack(); return; }
            if (int.TryParse(raw, out id) && manager.GetSupplierById(id) != null) break;
            Console.WriteLine("  Invalid input. Please enter a valid Supplier ID.");
        }

        var found = manager.GetSupplierById(id)!;
        Console.WriteLine($"\n  You are about to delete: [{found.SupplierId}] {found.SupplierName}");

        if (!ReadConfirm($"  Confirm deletion? [Y/N]: "))
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            try
            {
                manager.DeleteSupplier(id);
                DisplayHelper.PrintSuccess("Supplier deleted successfully.");
            }
            catch (Exception ex)
            {
                // If deletion fails (e.g., supplier in use), stop looping and return
                DisplayHelper.PrintError(ex.Message);
                DisplayHelper.PressEnter();
                return;
            }
        }

        Console.WriteLine();
        keepGoing = AskDoAgain("Delete another supplier?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ══════════════════════════════════════════════════
// REPORTS
// ══════════════════════════════════════════════════

// ── [17] Transaction History ────────────────────────
// Displays the transaction audit log with optional filtering by action type.
// Valid filters: ADD, RESTOCK, DEDUCT, UPDATE, DELETE, ALL (or Enter for ALL).
void HandleViewTransactions()
{
    DisplayHelper.PrintSectionHeader("REPORTS", "TRANSACTION HISTORY");
    Console.WriteLine("  Enter 0 to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        Console.WriteLine("  Filter: ADD / RESTOCK / DEDUCT / UPDATE / DELETE / ALL");

        // Read and validate filter input
        string filter;
        while (true)
        {
            Console.Write("\n  Filter [action or Enter for ALL, 0 = back]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim().ToUpper();
            if (raw == "0") { DisplayHelper.PrintBack(); return; }
            if (string.IsNullOrWhiteSpace(raw)) { filter = "ALL"; break; } // Enter = show all
            if (ValidFilters.Contains(raw)) { filter = raw; break; }
            Console.WriteLine("  Invalid input. Use ADD, RESTOCK, DEDUCT, UPDATE, DELETE, or ALL.");
        }

        // Display the filtered table and count
        DisplayHelper.ShowTransactionTable(manager.GetHistory(), filter);

        int count = filter == "ALL"
            ? manager.GetHistory().Count
            : manager.GetHistory().Count(t => t.ActionType == filter);
        Console.WriteLine($"\n  Total transactions shown: {count}");

        Console.WriteLine();
        keepGoing = AskDoAgain("Filter again?");
        Console.WriteLine();
    }
    DisplayHelper.PressEnter();
}

// ── [18] Total Inventory Value ──────────────────────
// Shows each product's quantity, unit price, and total value,
// then displays the grand total across all products.
// Formula used: Quantity × Price per product.
void HandleTotalValue()
{
    DisplayHelper.PrintSectionHeader("REPORTS", "TOTAL INVENTORY VALUE");

    var vaccines = manager.GetVaccines();
    if (vaccines.Count == 0)
    {
        Console.WriteLine("\n  No products in inventory.");
        DisplayHelper.PressEnter();
        return;
    }

    // Print value breakdown table
    Console.WriteLine();
    Console.WriteLine($"  {"Vaccine Name",-26} {"Qty",-8} {"Unit Price",-14} Total Value");
    Console.WriteLine("  " + new string('-', 62));
    foreach (var v in vaccines)
        Console.WriteLine($"  {v.VaccineName,-26} {v.Quantity,-8} {"P" + v.Price.ToString("F2"),-14} P{v.GetTotalValue():F2}");
    Console.WriteLine("  " + new string('-', 62));

    // Grand total (sum of all products)
    Console.WriteLine();
    Console.WriteLine($"  Grand Total : P{manager.ComputeTotalInventoryValue():F2}");
    Console.WriteLine($"  Products    : {vaccines.Count} type(s)");
    DisplayHelper.PressEnter();
}