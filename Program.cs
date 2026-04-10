// Program.cs
// entry point for the Vaccine Inventory Management System
// handles login, menu routing, and all handler methods

using VaccineInventory.Services;
using VaccineInventory.Helpers;
using VaccineInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;

var ValidFilters = new HashSet<string> { "ALL", "ADD", "RESTOCK", "DEDUCT", "UPDATE", "DELETE" };

InventoryManager manager = new InventoryManager();

// INPUT HELPERS
// these local functions centralize input parsing and validation,
// keeping the handler methods clean and focused on flow logic

// reads a non-empty string that's not purely numeric
// returning "0" signals the caller to go back if allowBack is true
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

// reads any non-empty string (used for free-form text fields)
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

// reads and validates a contact number: digits only, 7-15 chars, optional leading '+'
// loops until valid input is entered rather than deferring to manager to throw
string ReadContactNumber(string prompt, bool allowBack = false)
{
    while (true)
    {
        Console.Write(prompt);
        string value = (Console.ReadLine() ?? string.Empty).Trim();
        if (allowBack && value == "0") return "0";
        if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^\+?[0-9]{7,15}$")) return value;
        Console.WriteLine("  Invalid input. Contact number must be 7-15 digits (digits only, optional leading +).");
    }
}

// reads a positive integer; returns 0 as a back signal if allowBack is true
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

// for prices where 0 is valid (e.g. donated/free vaccines)
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

// only accepts dates in the future to prevent adding already-expired vaccines
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

// accepts y/yes/n/no, case-insensitive
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

// shorthand used at the end of every handler loop to ask whether to repeat
bool AskDoAgain(string label) => ReadConfirm($"\n  {label} [Y/N]: ");

// these read and validate IDs against the live list so invalid IDs are caught immediately
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

// LOGIN GATE
// only the hardcoded admin account can log in; 3 failed attempts exits the program

Console.Clear();
Console.WriteLine();
Console.WriteLine("  ================================================");
Console.WriteLine("    VACCINE INVENTORY MANAGEMENT SYSTEM");
Console.WriteLine("  ================================================");
Console.WriteLine();
Console.WriteLine("  Please log in to continue.");
Console.WriteLine();

const string validUsername = "admin";
const string validPassword = "admin123";

User? currentUser = null;
int loginAttempts = 0;
const int maxAttempts = 3;

while (currentUser == null)
{
    Console.Write("  Username : ");
    string username = Console.ReadLine() ?? string.Empty;

    Console.Write("  Password : ");
    string password = Console.ReadLine() ?? string.Empty;

    if (username.Trim() == validUsername && password == validPassword)
    {
        // fetch the matching User object so we have role info for the menu header
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
            Console.WriteLine("  Too many failed attempts. Exiting.");
            Environment.Exit(0);
        }
        Console.WriteLine($"  {remaining} attempt(s) remaining.");
        Console.WriteLine();
    }
}

Console.WriteLine();
Console.WriteLine("  Loading inventory data...");
Console.WriteLine("  System ready!");
DisplayHelper.ShowFullDashboard(manager);
DisplayHelper.PressEnter();

// MAIN LOOP
// each menu option dispatches to a dedicated handler method below

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
        case "1": HandleAddVaccine(); break;
        case "2": HandleViewAllVaccines(); break;
        case "3": HandleSearchVaccine(); break;
        case "4": HandleUpdateVaccine(); break;
        case "5": HandleDeleteVaccine(); break;
        case "6": HandleRestockVaccine(); break;
        case "7": HandleDeductVaccine(); break;
        case "8": HandleLowStock(); break;
        case "9": HandleAddCategory(); break;
        case "10": HandleViewCategories(); break;
        case "11": HandleUpdateCategory(); break;
        case "12": HandleDeleteCategory(); break;
        case "13": HandleAddSupplier(); break;
        case "14": HandleViewSuppliers(); break;
        case "15": HandleUpdateSupplier(); break;
        case "16": HandleDeleteSupplier(); break;
        case "17": HandleViewTransactions(); break;
        case "18": HandleTotalValue(); break;
        case "0":
            // set flag to false so the loop exits cleanly after this iteration
            isRunning = false;
            Console.WriteLine();
            Console.WriteLine($"  Goodbye, {currentUser.Username}. Thank you!");
            Console.WriteLine();
            break;
        default:
            Console.WriteLine();
            Console.WriteLine("  Invalid choice. Please select a number from the menu.");
            DisplayHelper.PressEnter();
            break;
    }
}

// HANDLER METHODS
// each handler owns one menu action: it collects input, calls the manager, and shows results
// all handlers loop with AskDoAgain() so users can perform multiple operations without
// returning to the menu every time

// PRODUCT MANAGEMENT

void HandleAddVaccine()
{
    DisplayHelper.PrintSectionHeader("PRODUCT MANAGEMENT", "ADD PRODUCT");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        // show current categories and suppliers so the user knows valid IDs
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

        // 0 is a valid initial quantity (product registered before stock arrives)
        // but "0" also means "go back" in other prompts — handled with a dedicated block here
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

        // warn early if the vaccine is already close to expiring
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

        // show a summary before committing
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
                // vaccine name already exists — offer to restock the existing record instead
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

void HandleViewAllVaccines()
{
    DisplayHelper.PrintSectionHeader("PRODUCT MANAGEMENT", "VIEW ALL PRODUCTS");
    Console.WriteLine("  Enter 0 to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        Console.WriteLine("  Filter options:");
        Console.WriteLine("    ALL      - Show all products");
        Console.WriteLine("    LOW      - Low stock only");
        Console.WriteLine("    EXPIRED  - Expired only");
        Console.WriteLine("    OK       - Sufficient stock & not expired");

        var categories = manager.GetCategories();
        if (categories.Count > 0)
        {
            Console.WriteLine("\n  Or filter by Category ID:");
            foreach (var cat in categories)
                Console.WriteLine($"    {cat.CategoryId,-4} - {cat.CategoryName}");
        }

        string filterInput;
        while (true)
        {
            Console.Write("\n  Filter [ALL/LOW/EXPIRED/OK/CategoryID or 0]: ");
            filterInput = (Console.ReadLine() ?? string.Empty).Trim().ToUpper();
            if (filterInput == "0") { DisplayHelper.PrintBack(); return; }
            if (string.IsNullOrWhiteSpace(filterInput)) { filterInput = "ALL"; break; }
            if (filterInput == "ALL" || filterInput == "LOW" || filterInput == "EXPIRED" || filterInput == "OK") break;
            // also accept a numeric category ID as a filter
            if (int.TryParse(filterInput, out int cid) && cid > 0 &&
                manager.GetCategories().Any(c => c.CategoryId == cid)) break;
            Console.WriteLine("  Invalid input. Please try again.");
        }

        var allVaccines = manager.GetVaccines();
        List<Product> filtered;
        string filterLabel;

        if (int.TryParse(filterInput, out int catId))
        {
            var cat = manager.GetCategories().First(c => c.CategoryId == catId);
            filtered = allVaccines.Where(v => v.CategoryId == catId).ToList();
            filterLabel = $"Category: {cat.CategoryName}";
        }
        else
        {
            (filtered, filterLabel) = filterInput switch
            {
                "LOW" => (allVaccines.Where(v => v.IsLowStock() && !v.IsExpired()).ToList(), "Low Stock"),
                "EXPIRED" => (allVaccines.Where(v => v.IsExpired()).ToList(), "Expired"),
                "OK" => (allVaccines.Where(v => !v.IsLowStock() && !v.IsExpired()).ToList(), "Sufficient Stock / Not Expired"),
                _ => (allVaccines.ToList(), "All Products")
            };
        }

        // optional sort — skipping input leaves the filtered order unchanged
        if (filtered.Count > 0)
        {
            Console.Write("\n  Sort by [NAME/QTY/PRICE/EXPIRY - press Enter to skip]: ");
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
                Console.WriteLine("  Unrecognised sort key - displaying in default order.");
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
            Console.Write("\n  Choose search type [1-4 or 0]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (raw == "0") { DisplayHelper.PrintBack(); return; }
            if (int.TryParse(raw, out searchType) && searchType >= 1 && searchType <= 4) break;
            Console.WriteLine("  Invalid input. Please enter 1, 2, 3, or 4.");
        }

        List<Product> results;
        string searchLabel;

        if (searchType == 4)
        {
            // ID search — exact match only
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
            // keyword search — partial, case-insensitive match
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

// leaving a field blank keeps the current value, so users don't have to retype everything
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

        decimal newPrice;
        while (true)
        {
            Console.Write($"  New Price       [P{existing.Price:F2}]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw)) { newPrice = existing.Price; break; }
            if (decimal.TryParse(raw, out newPrice) && newPrice >= 0) break;
            Console.WriteLine("  Invalid input. Please enter a valid price (0 or greater).");
        }

        // blank = keep the current date even if it's already expired
        // entering a new date requires it to be in the future
        DateTime newExpiry;
        while (true)
        {
            Console.Write($"  New Expiry Date [{existing.ExpiryDate:MM/dd/yyyy}]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw))
            {
                newExpiry = existing.ExpiryDate;
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

        // blank input keeps the current min stock; must be >= 1 if a new value is entered
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
        // show the product name so the user can confirm they picked the right one
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

// STOCK CONTROL

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

        // warn if restocking an expired product — unusual but not blocked
        if (vaccine.IsExpired())
        {
            Console.WriteLine($"\n  '{vaccine.VaccineName}' is EXPIRED  (expiry: {vaccine.ExpiryDate:MM/dd/yyyy}).");
            proceed = ReadConfirm("  Restocking an expired product is unusual. Proceed? [Y/N]: ");
        }
        else
        {
            // inform if nearing expiry so the user can decide how much to order
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

        // deducting from an expired vaccine is unusual — flag it but allow if confirmed
        if (vaccine.IsExpired())
        {
            Console.WriteLine($"\n  '{vaccine.VaccineName}' is EXPIRED  (expiry: {vaccine.ExpiryDate:MM/dd/yyyy}).");
            proceed = ReadConfirm("  Deducting from an expired product is unusual. Proceed? [Y/N]: ");
        }

        if (!proceed)
        {
            Console.WriteLine("  Cancelled.");
        }
        else
        {
            // loop until the user enters a quantity that won't go below zero
            int qty;
            while (true)
            {
                qty = ReadPositiveInt("  Quantity to deduct     : ");
                if (vaccine.Quantity - qty >= 0) break;
                Console.WriteLine($"  Cannot deduct {qty}. Current stock is only {vaccine.Quantity}.");
            }

            // extra confirmation if the deduction will zero out stock
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
                    // re-fetch to get the updated quantity for the out-of-stock notice
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

void HandleLowStock()
{
    DisplayHelper.PrintSectionHeader("STOCK CONTROL", "LOW STOCK ITEMS");

    // includes expired products since they also fall below their minimum threshold
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

// CATEGORY MANAGEMENT

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
        // simple read-only list; no actions available here
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

void HandleUpdateCategory()
{
    DisplayHelper.PrintSectionHeader("CATEGORY MANAGEMENT", "UPDATE CATEGORY");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    if (manager.GetCategories().Count == 0)
    {
        Console.WriteLine("  No categories to update.");
        DisplayHelper.PressEnter();
        return;
    }

    bool keepGoing = true;
    while (keepGoing)
    {
        // re-fetch each iteration in case a previous update changed the list
        var cats = manager.GetCategories();
        if (cats.Count == 0) { Console.WriteLine("\n  No categories left."); break; }

        Console.WriteLine($"  {"ID",-6} Category Name");
        Console.WriteLine("  " + new string('-', 36));
        foreach (var cat in cats)
            Console.WriteLine($"  {cat.CategoryId,-6} {cat.CategoryName}");
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
        string newName = ReadName($"  New Name [{existing.CategoryName}]: ", allowBack: true);
        if (newName == "0") { DisplayHelper.PrintBack(); return; }

        if (!ReadConfirm($"\n  Rename '{existing.CategoryName}' to '{newName}'? [Y/N]: "))
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
        foreach (var cat in cats)
            Console.WriteLine($"  {cat.CategoryId,-6} {cat.CategoryName}");
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

        if (!ReadConfirm("  Confirm deletion? [Y/N]: "))
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
                // referential integrity error from manager — exit the loop so user sees the message
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

// SUPPLIER MANAGEMENT

void HandleAddSupplier()
{
    DisplayHelper.PrintSectionHeader("SUPPLIER MANAGEMENT", "ADD SUPPLIER");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        string name = ReadName("  Supplier Name   : ", allowBack: true);
        if (name == "0") { DisplayHelper.PrintBack(); return; }

        // ReadContactNumber enforces the digit-only format before passing to manager
        string contact = ReadContactNumber("  Contact Number  : ", allowBack: true);
        if (contact == "0") { DisplayHelper.PrintBack(); return; }

        if (!ReadConfirm($"\n  Add supplier '{name}'? [Y/N]: "))
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
        // simple read-only list; no actions available here
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

void HandleUpdateSupplier()
{
    DisplayHelper.PrintSectionHeader("SUPPLIER MANAGEMENT", "UPDATE SUPPLIER");
    Console.WriteLine("  Enter 0 at any prompt to go back.\n");

    if (manager.GetSuppliers().Count == 0)
    {
        Console.WriteLine("  No suppliers available to update.");
        DisplayHelper.PressEnter();
        return;
    }

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

        string newContact;
        while (true)
        {
            Console.Write($"  New Contact [{existing.ContactNumber}]: ");
            string raw = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw)) { newContact = existing.ContactNumber; break; }
            string trimmed = raw.Trim();
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\+?[0-9]{7,15}$")) { newContact = trimmed; break; }
            Console.WriteLine("  Invalid input. Contact number must be 7-15 digits (digits only, optional leading +).");
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
                // referential integrity error — exit loop so user sees the message clearly
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

// REPORTS

void HandleViewTransactions()
{
    DisplayHelper.PrintSectionHeader("REPORTS", "TRANSACTION HISTORY");
    Console.WriteLine("  Enter 0 to go back.\n");

    bool keepGoing = true;
    while (keepGoing)
    {
        Console.WriteLine("  Filter: ADD / RESTOCK / DEDUCT / UPDATE / DELETE / ALL");

        string filter;
        while (true)
        {
            Console.Write("\n  Filter [action or Enter for ALL, 0 = back]: ");
            string raw = (Console.ReadLine() ?? string.Empty).Trim().ToUpper();
            if (raw == "0") { DisplayHelper.PrintBack(); return; }
            if (string.IsNullOrWhiteSpace(raw)) { filter = "ALL"; break; }
            if (ValidFilters.Contains(raw)) { filter = raw; break; }
            Console.WriteLine("  Invalid input. Use ADD, RESTOCK, DEDUCT, UPDATE, DELETE, or ALL.");
        }

        DisplayHelper.ShowTransactionTable(manager.GetHistory(), filter);

        // recount after display so the total matches what's actually shown
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

    // per-product breakdown followed by a grand total
    Console.WriteLine();
    Console.WriteLine($"  {"Vaccine Name",-26} {"Qty",-8} {"Unit Price",-14} Total Value");
    Console.WriteLine("  " + new string('-', 62));
    foreach (var v in vaccines)
        Console.WriteLine($"  {v.VaccineName,-26} {v.Quantity,-8} {"P" + v.Price.ToString("F2"),-14} P{v.GetTotalValue():F2}");
    Console.WriteLine("  " + new string('-', 62));

    Console.WriteLine();
    Console.WriteLine($"  Grand Total : P{manager.ComputeTotalInventoryValue():F2}");
    Console.WriteLine($"  Products    : {vaccines.Count} type(s)");
    DisplayHelper.PressEnter();
}