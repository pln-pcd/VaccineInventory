// ============================================================
// InventoryManager.cs
// The central service class for the Vaccine Inventory System.
// Handles all business logic and acts as the in-memory "database"
// using List<T> collections for all data storage.
//
// Responsibilities:
//   - Stores and manages all Products, Categories, Suppliers,
//     Users, and TransactionRecords in memory
//   - Enforces all business rules and input validation
//   - Exposes read-only views of internal lists to callers
//   - Records every stock action into the transaction history
//
// OOP Concepts Used:
//   - Encapsulation  : private lists and ID counters; read-only access via getters
//   - Methods        : all CRUD and business operations as public methods
//   - Exception handling: throws descriptive exceptions for invalid operations
//   - Custom Exception: DuplicateProductException (defined at bottom of file)
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VaccineInventory.Models;

namespace VaccineInventory.Services
{
    public class InventoryManager
    {
        // ===== In-memory storage lists =====
        // These are the only "tables" in the system — no database needed.
        // All data lives here for the duration of the program session.
        private List<Product> vaccineList = new List<Product>();
        private List<Category> categoryList = new List<Category>();
        private List<Supplier> supplierList = new List<Supplier>();
        private List<User> userList = new List<User>();
        private List<TransactionRecord> historyList = new List<TransactionRecord>();

        // Auto-increment counters — simulate database primary key sequences
        private int nextProductId = 1;
        private int nextCategoryId = 1;
        private int nextSupplierId = 1;
        private int nextTransactionId = 1;

        // ===== Constructor =====
        // Loads sample data on startup so the system is never completely empty
        // when the user first logs in.
        public InventoryManager()
        {
            LoadSampleData();
        }

        // ===== SAMPLE DATA =====
        // Pre-populates the system with default categories, suppliers, products,
        // and users. This makes the system immediately usable for testing.
        private void LoadSampleData()
        {
            // Default vaccine categories
            categoryList.Add(new Category(nextCategoryId++, "COVID-19"));
            categoryList.Add(new Category(nextCategoryId++, "Anti-Rabies"));
            categoryList.Add(new Category(nextCategoryId++, "Hepatitis B"));
            categoryList.Add(new Category(nextCategoryId++, "Tetanus"));
            categoryList.Add(new Category(nextCategoryId++, "Pneumonia"));

            // Default suppliers
            supplierList.Add(new Supplier(nextSupplierId++, "MedPharm Supplies", "09171234567"));
            supplierList.Add(new Supplier(nextSupplierId++, "VacciCare PH", "09281234567"));

            // Default vaccine products (categoryId, supplierId match above)
            vaccineList.Add(new Product(nextProductId++, "Sinovac", 1, 1, 100, 500.00m,
                new DateTime(2026, 12, 31), 20));
            vaccineList.Add(new Product(nextProductId++, "Verorab", 2, 2, 15, 1200.00m,
                new DateTime(2026, 6, 30), 10));
            vaccineList.Add(new Product(nextProductId++, "Engerix-B", 3, 1, 8, 850.00m,
                new DateTime(2027, 3, 15), 10));

            // Default system users
            // Note: passwords are stored as plain text (acceptable for this lab scope)
            userList.Add(new User(1, "admin", "admin123", "Admin"));
        }

        // ===== GETTERS =====
        // Return read-only wrappers of the internal lists.
        // Callers can read data but cannot directly add or remove items —
        // all mutations must go through the methods below.
        public IReadOnlyList<Category> GetCategories() => categoryList.AsReadOnly();
        public IReadOnlyList<Supplier> GetSuppliers() => supplierList.AsReadOnly();
        public IReadOnlyList<Product> GetVaccines() => vaccineList.AsReadOnly();
        public IReadOnlyList<TransactionRecord> GetHistory() => historyList.AsReadOnly();
        public IReadOnlyList<User> GetUsers() => userList.AsReadOnly();

        // ===== USER / LOGIN =====

        // Authenticates a login attempt.
        // Looks up the user by username (case-insensitive), then validates the password.
        // Returns the matching User object on success.
        // Throws an Exception with a specific message on failure.
        public User AuthenticateUser(string username, string password)
        {
            var user = userList.Find(u =>
                u.Username.ToLower() == username.ToLower());

            if (user == null)
                throw new Exception("Username not found.");

            if (!user.ValidatePassword(password))
                throw new Exception("Incorrect password.");

            return user;
        }

        // ===== SHARED INPUT VALIDATORS =====
        // These private methods are reused across multiple Add/Update operations
        // to keep validation logic in one place and avoid code duplication.

        // Validates that a contact number contains only digits (and optional leading +),
        // and is between 7 and 15 characters long.
        // Example valid values: "09171234567", "+63917123456"
        private static void ValidateContactNumber(string contact)
        {
            if (!Regex.IsMatch(contact, @"^\+?[0-9]{7,15}$"))
                throw new Exception(
                    "Contact number must be 7–15 digits (digits only, optional leading +). " +
                    "No spaces or letters allowed.");
        }

        // Validates that a name field does not exceed a given character limit.
        // Prevents data from overflowing fixed-width table columns in the display.
        private static void ValidateNameLength(string name, string fieldLabel, int maxLen)
        {
            if (name.Length > maxLen)
                throw new Exception(
                    $"{fieldLabel} must not exceed {maxLen} characters " +
                    $"(you entered {name.Length}).");
        }

        // Rejects names that consist entirely of digits (e.g. "123", "456789").
        // Vaccine and category names should be descriptive text, not numbers.
        private static void ValidateNotPurelyNumeric(string name, string fieldLabel)
        {
            if (name.Length > 0 && name.All(char.IsDigit))
                throw new Exception($"{fieldLabel} cannot be a purely numeric value.");
        }

        // ===== CATEGORY METHODS =====

        // Adds a new category to the system.
        // Validates name length and checks for duplicate names (case-insensitive).
        public void AddCategory(string name)
        {
            ValidateNameLength(name, "Category name", 30);

            // Prevent duplicate category names
            bool alreadyExists = categoryList.Exists(c =>
                c.CategoryName.ToLower() == name.ToLower());

            if (alreadyExists)
                throw new Exception("Category already exists.");

            categoryList.Add(new Category(nextCategoryId++, name));
        }

        // Updates an existing category's name.
        // Validates the new name and checks it won't conflict with another category.
        public void UpdateCategory(int categoryId, string newName)
        {
            var cat = categoryList.Find(c => c.CategoryId == categoryId);
            if (cat == null)
                throw new Exception("Category not found.");

            ValidateNameLength(newName, "Category name", 30);

            // Make sure the new name doesn't match a different category
            bool conflict = categoryList.Exists(c =>
                c.CategoryName.ToLower() == newName.ToLower() &&
                c.CategoryId != categoryId);
            if (conflict)
                throw new Exception("Another category already has that name.");

            cat.CategoryName = newName;
        }

        // Deletes a category.
        // Blocked if any vaccine product is currently assigned to this category.
        public void DeleteCategory(int categoryId)
        {
            var cat = categoryList.Find(c => c.CategoryId == categoryId);
            if (cat == null)
                throw new Exception("Category not found.");

            // Safety check: do not delete a category that is in use
            bool inUse = vaccineList.Exists(v => v.CategoryId == categoryId);
            if (inUse)
                throw new Exception("Cannot delete category. It is assigned to existing products.");

            categoryList.Remove(cat);
        }

        // ===== SUPPLIER METHODS =====

        // Adds a new supplier with a name and contact number.
        // Validates name length, contact number format, and checks for duplicates.
        public void AddSupplier(string name, string contact)
        {
            ValidateNameLength(name, "Supplier name", 35);
            ValidateContactNumber(contact); // ensures digits only, 7–15 chars

            bool alreadyExists = supplierList.Exists(s =>
                s.SupplierName.ToLower() == name.ToLower());

            if (alreadyExists)
                throw new Exception("Supplier already exists.");

            supplierList.Add(new Supplier(nextSupplierId++, name, contact));
        }

        // Updates an existing supplier's name and contact number.
        // Validates new values and checks for name conflicts with other suppliers.
        public void UpdateSupplier(int supplierId, string newName, string newContact)
        {
            var sup = supplierList.Find(s => s.SupplierId == supplierId);
            if (sup == null)
                throw new Exception("Supplier not found.");

            ValidateNameLength(newName, "Supplier name", 35);
            ValidateContactNumber(newContact);

            // Make sure the new name doesn't match a different supplier
            bool conflict = supplierList.Exists(s =>
                s.SupplierName.ToLower() == newName.ToLower() &&
                s.SupplierId != supplierId);
            if (conflict)
                throw new Exception("Another supplier already has that name.");

            sup.SupplierName = newName;
            sup.ContactNumber = newContact;
        }

        // Deletes a supplier.
        // Blocked if any vaccine product is currently assigned to this supplier.
        public void DeleteSupplier(int supplierId)
        {
            var sup = supplierList.Find(s => s.SupplierId == supplierId);
            if (sup == null)
                throw new Exception("Supplier not found.");

            // Safety check: do not delete a supplier that is in use
            bool inUse = vaccineList.Exists(v => v.SupplierId == supplierId);
            if (inUse)
                throw new Exception("Cannot delete supplier. It is assigned to existing products.");

            supplierList.Remove(sup);
        }

        // ===== PRODUCT METHODS =====

        // Adds a new vaccine product to the inventory.
        //
        // Validates all input fields before creating the product.
        // If the vaccine name already exists, throws DuplicateProductException
        // which carries the existing product's ID so the UI can offer to
        // redirect the user to restock that product instead.
        //
        // Also records an "ADD" transaction in the history.
        public void AddVaccine(string name, int categoryId, int supplierId,
                                int qty, decimal price, DateTime expiry, int minStock)
        {
            if (qty < 0)
                throw new Exception("Quantity cannot be negative.");

            if (qty > 100000)
                throw new Exception("Quantity seems unrealistically large. Please verify.");

            if (price < 0)
                throw new Exception("Price cannot be negative.");

            if (minStock < 1)
                throw new Exception("Minimum stock level must be at least 1.");

            if (expiry.Date <= DateTime.Today)
                throw new Exception("Expiry date must be a future date.");

            // Vaccine name must be descriptive text, not just digits, and within limit
            ValidateNameLength(name, "Vaccine name", 40);
            ValidateNotPurelyNumeric(name, "Vaccine name");

            // Verify that the given category and supplier IDs actually exist
            var cat = categoryList.Find(c => c.CategoryId == categoryId);
            if (cat == null)
                throw new Exception("Category not found. Please enter a valid Category ID.");

            var sup = supplierList.Find(s => s.SupplierId == supplierId);
            if (sup == null)
                throw new Exception("Supplier not found. Please enter a valid Supplier ID.");

            // Duplicate name check: throw a special exception so the UI layer
            // can offer to restock the existing product instead of just blocking the add
            var existingDup = vaccineList.Find(v => v.VaccineName.ToLower() == name.ToLower());
            if (existingDup != null)
                throw new DuplicateProductException(existingDup.ProductId);

            // All checks passed — create the product and add it
            var newVaccine = new Product(nextProductId++, name, categoryId,
                                          supplierId, qty, price, expiry, minStock);
            vaccineList.Add(newVaccine);

            // Log the add operation in the transaction history
            RecordTransaction(newVaccine.ProductId, name, "ADD", qty,
                $"New vaccine '{name}' added to inventory.");
        }

        // Searches vaccines across name, category name, and supplier name
        // using a single keyword (case-insensitive partial match).
        // Used by option 3 when the user picks the general search.
        public List<Product> SearchVaccine(string keyword)
        {
            string kw = keyword.ToLower();
            return vaccineList.FindAll(v =>
                v.VaccineName.ToLower().Contains(kw) ||
                GetCategoryName(v.CategoryId).ToLower().Contains(kw) ||
                GetSupplierName(v.SupplierId).ToLower().Contains(kw));
        }

        // Searches vaccines by vaccine name only (case-insensitive partial match)
        public List<Product> SearchVaccineByName(string keyword)
        {
            string kw = keyword.ToLower();
            return vaccineList.FindAll(v => v.VaccineName.ToLower().Contains(kw));
        }

        // Searches vaccines by their category name (case-insensitive partial match)
        public List<Product> SearchVaccineByCategory(string keyword)
        {
            string kw = keyword.ToLower();
            return vaccineList.FindAll(v => GetCategoryName(v.CategoryId).ToLower().Contains(kw));
        }

        // Searches vaccines by their supplier name (case-insensitive partial match)
        public List<Product> SearchVaccineBySupplier(string keyword)
        {
            string kw = keyword.ToLower();
            return vaccineList.FindAll(v => GetSupplierName(v.SupplierId).ToLower().Contains(kw));
        }

        // Searches vaccines by their exact integer product ID.
        // Returns a list (with 0 or 1 items) to be consistent with other search methods.
        public List<Product> SearchVaccineById(int productId)
        {
            return vaccineList.FindAll(v => v.ProductId == productId);
        }

        // Finds a vaccine by its name (exact, case-insensitive match).
        // Used internally to check for duplicates before adding a new product.
        public Product FindVaccineByName(string name)
        {
            return vaccineList.Find(v => v.VaccineName.ToLower() == name.ToLower());
        }

        // Updates a vaccine's editable fields: name, price, expiry date, and min stock.
        //
        // Note on expiry date behavior:
        //   If the product was already expired and the user does NOT change the expiry date,
        //   the update is still allowed (keeping an old date is OK).
        //   However, if the user actively sets a NEW date that is in the past, it is rejected.
        //   This allows editing price/name/min stock on expired products without being blocked.
        //
        // Also records an "UPDATE" transaction in the history.
        public void UpdateVaccine(int productId, string newName, decimal newPrice,
                                   DateTime newExpiry, int newMinStock)
        {
            if (newPrice < 0)
                throw new Exception("Price cannot be negative.");

            if (newMinStock < 1)
                throw new Exception("Minimum stock level must be at least 1.");

            ValidateNameLength(newName, "Vaccine name", 40);
            ValidateNotPurelyNumeric(newName, "Vaccine name");

            var vaccine = vaccineList.Find(v => v.ProductId == productId);
            if (vaccine == null)
                throw new Exception("Vaccine not found.");

            // Only reject a past date if the user is actively changing it to a past value
            if (newExpiry.Date != vaccine.ExpiryDate.Date && newExpiry.Date <= DateTime.Today)
                throw new Exception("New expiry date must be a future date.");

            // Ensure no other vaccine already uses this name
            bool nameConflict = vaccineList.Exists(v =>
                v.VaccineName.ToLower() == newName.ToLower() &&
                v.ProductId != productId);
            if (nameConflict)
                throw new Exception("Another vaccine already has that name.");

            // Apply all changes
            vaccine.VaccineName = newName;
            vaccine.Price = newPrice;
            vaccine.ExpiryDate = newExpiry;
            vaccine.MinimumStockLevel = newMinStock;

            // Log the update in transaction history
            RecordTransaction(productId, vaccine.VaccineName, "UPDATE", 0,
                $"Vaccine info updated for '{vaccine.VaccineName}'.");
        }

        // Removes a vaccine from the inventory.
        // Logs a "DELETE" transaction before removing so the history is preserved.
        public void DeleteVaccine(int productId)
        {
            var vaccine = vaccineList.Find(v => v.ProductId == productId);
            if (vaccine == null)
                throw new Exception("Vaccine not found.");

            string name = vaccine.VaccineName;

            // Record deletion in history BEFORE removing (captures the name)
            RecordTransaction(productId, name, "DELETE", 0,
                $"Vaccine '{name}' deleted from inventory.");

            vaccineList.Remove(vaccine);
        }

        // Adds units to a vaccine's current stock quantity.
        // Restock amount must be positive and not exceed 100,000 per transaction.
        // Logs a "RESTOCK" transaction.
        public void RestockVaccine(int productId, int addQty)
        {
            if (addQty <= 0)
                throw new Exception("Restock amount must be greater than zero.");

            if (addQty > 100000)
                throw new Exception("Restock amount seems unrealistically large. Please verify.");

            var vaccine = vaccineList.Find(v => v.ProductId == productId);
            if (vaccine == null)
                throw new Exception("Vaccine not found.");

            vaccine.Quantity += addQty;

            // Log the restock in transaction history
            RecordTransaction(productId, vaccine.VaccineName, "RESTOCK", addQty,
                $"Restocked {addQty} unit(s) of '{vaccine.VaccineName}'.");
        }

        // Removes units from a vaccine's current stock quantity.
        // Prevents stock from going below zero.
        // Logs a "DEDUCT" transaction.
        public void DeductVaccine(int productId, int deductQty)
        {
            if (deductQty <= 0)
                throw new Exception("Deduct amount must be greater than zero.");

            var vaccine = vaccineList.Find(v => v.ProductId == productId);
            if (vaccine == null)
                throw new Exception("Vaccine not found.");

            if (vaccine.Quantity - deductQty < 0)
                throw new Exception($"Not enough stock. Current quantity: {vaccine.Quantity}");

            vaccine.Quantity -= deductQty;

            // Log the deduction in transaction history
            RecordTransaction(productId, vaccine.VaccineName, "DEDUCT", deductQty,
                $"Deducted {deductQty} unit(s) of '{vaccine.VaccineName}'.");
        }

        // Returns all vaccines currently below their minimum stock level.
        // Used by option 8 (Show Low Stock Items) and the dashboard.
        public List<Product> GetLowStockVaccines()
        {
            return vaccineList.FindAll(v => v.IsLowStock());
        }

        // Computes the total monetary value of all vaccines in stock.
        // Formula: sum of (Quantity × Price) for each product.
        // Used by option 18 (Total Inventory Value report).
        public decimal ComputeTotalInventoryValue()
        {
            decimal total = 0;
            foreach (var vaccine in vaccineList)
                total += vaccine.GetTotalValue();
            return total;
        }

        // ===== HELPER METHODS =====

        // Returns the category name for a given CategoryId.
        // Falls back to "Unknown" if the category no longer exists.
        public string GetCategoryName(int id)
        {
            var cat = categoryList.Find(c => c.CategoryId == id);
            return cat != null ? cat.CategoryName : "Unknown";
        }

        // Returns the supplier name for a given SupplierId.
        // Falls back to "Unknown" if the supplier no longer exists.
        public string GetSupplierName(int id)
        {
            var sup = supplierList.Find(s => s.SupplierId == id);
            return sup != null ? sup.SupplierName : "Unknown";
        }

        // Finds and returns a vaccine by its ProductId.
        // Returns null if not found.
        public Product GetVaccineById(int id)
        {
            return vaccineList.Find(v => v.ProductId == id);
        }

        // Finds and returns a category by its CategoryId.
        // Returns null if not found.
        public Category GetCategoryById(int id)
        {
            return categoryList.Find(c => c.CategoryId == id);
        }

        // Finds and returns a supplier by its SupplierId.
        // Returns null if not found.
        public Supplier GetSupplierById(int id)
        {
            return supplierList.Find(s => s.SupplierId == id);
        }

        // Returns a summary tuple used by the post-login dashboard.
        // Calculates:
        //   TotalProducts — total number of vaccine records
        //   Expired       — vaccines past their expiry date
        //   LowStock      — vaccines below minimum stock (excluding expired ones)
        //   TotalValue    — grand total inventory value in PHP
        public (int TotalProducts, int Expired, int LowStock, decimal TotalValue) GetDashboardCounts()
        {
            int totalProducts = vaccineList.Count;
            int expired = vaccineList.Count(v => v.IsExpired());
            int lowStock = vaccineList.Count(v => v.IsLowStock() && !v.IsExpired());
            decimal totalValue = ComputeTotalInventoryValue();
            return (totalProducts, expired, lowStock, totalValue);
        }

        // Creates and stores a new TransactionRecord for any stock action.
        // Called internally after every successful Add, Restock, Deduct,
        // Update, or Delete operation.
        //
        // Parameters:
        //   productId   — ID of the affected product
        //   vaccineName — name at the time of action (preserved even after deletion)
        //   action      — one of: ADD, RESTOCK, DEDUCT, UPDATE, DELETE
        //   qty         — units changed (0 for UPDATE and DELETE)
        //   notes       — human-readable description of the action
        private void RecordTransaction(int productId, string vaccineName,
                                        string action, int qty, string notes)
        {
            var record = new TransactionRecord(
                nextTransactionId++,
                productId,
                vaccineName,
                action,
                qty,
                DateTime.Now,
                notes
            );
            historyList.Add(record);
        }
    }

    // ============================================================
    // DuplicateProductException
    // A custom exception thrown when AddVaccine detects that a
    // product with the same name already exists in the inventory.
    //
    // Carries the ExistingProductId so the UI layer (Program.cs)
    // can look up the product and offer to restock it instead of
    // just showing a generic "product already exists" error.
    //
    // Usage:
    //   throw new DuplicateProductException(existingProduct.ProductId);
    //   ...
    //   catch (DuplicateProductException ex) {
    //       var dup = manager.GetVaccineById(ex.ExistingProductId);
    //       // offer restock flow
    //   }
    // ============================================================
    public class DuplicateProductException : Exception
    {
        // Holds the ID of the product that already exists in inventory
        public int ExistingProductId { get; }

        public DuplicateProductException(int existingProductId)
            : base("Product already exists.")
        {
            ExistingProductId = existingProductId;
        }
    }
}