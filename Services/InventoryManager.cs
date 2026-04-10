// InventoryManager.cs
// handles all business logic and in-memory storage for the system
// uses List<T> as the "database" since no actual db is needed

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VaccineInventory.Models;

namespace VaccineInventory.Services
{
    public class InventoryManager
    {
        // in-memory lists — all data is lost on exit (by design for this lab)
        private List<Product> vaccineList = new List<Product>();
        private List<Category> categoryList = new List<Category>();
        private List<Supplier> supplierList = new List<Supplier>();
        private List<User> userList = new List<User>();
        private List<TransactionRecord> historyList = new List<TransactionRecord>();

        // auto-increment counters; each new record gets the next available ID
        private int nextProductId = 1;
        private int nextCategoryId = 1;
        private int nextSupplierId = 1;
        private int nextTransactionId = 1;

        public InventoryManager()
        {
            LoadSampleData();
        }

        // preloads sample data so the system isn't empty on first run
        private void LoadSampleData()
        {
            categoryList.Add(new Category(nextCategoryId++, "COVID-19"));
            categoryList.Add(new Category(nextCategoryId++, "Anti-Rabies"));
            categoryList.Add(new Category(nextCategoryId++, "Hepatitis B"));
            categoryList.Add(new Category(nextCategoryId++, "Tetanus"));
            categoryList.Add(new Category(nextCategoryId++, "Pneumonia"));

            supplierList.Add(new Supplier(nextSupplierId++, "MedPharm Supplies", "09171234567"));
            supplierList.Add(new Supplier(nextSupplierId++, "VacciCare PH", "09281234567"));

            vaccineList.Add(new Product(nextProductId++, "Sinovac", 1, 1, 100, 500.00m,
                new DateTime(2026, 12, 31), 20));
            vaccineList.Add(new Product(nextProductId++, "Verorab", 2, 2, 15, 1200.00m,
                new DateTime(2026, 6, 30), 10));
            vaccineList.Add(new Product(nextProductId++, "Engerix-B", 3, 1, 8, 850.00m,
                new DateTime(2027, 3, 15), 10));

            // Note: passwords are plain text — acceptable for this lab scope
            userList.Add(new User(1, "admin", "admin123", "Admin"));
        }

        // read-only wrappers prevent external code from mutating the internal lists directly
        public IReadOnlyList<Category> GetCategories() => categoryList.AsReadOnly();
        public IReadOnlyList<Supplier> GetSuppliers() => supplierList.AsReadOnly();
        public IReadOnlyList<Product> GetVaccines() => vaccineList.AsReadOnly();
        public IReadOnlyList<TransactionRecord> GetHistory() => historyList.AsReadOnly();
        public IReadOnlyList<User> GetUsers() => userList.AsReadOnly();

        // LOGIN

        // case-insensitive username match; throws a descriptive exception on failure
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

        // SHARED VALIDATORS

        // digits only, 7–15 chars, optional leading '+' (covers local and international formats)
        private static void ValidateContactNumber(string contact)
        {
            if (!Regex.IsMatch(contact, @"^\+?[0-9]{7,15}$"))
                throw new Exception(
                    "Contact number must be 7-15 digits (digits only, optional leading +). " +
                    "No spaces or letters allowed.");
        }

        private static void ValidateNameLength(string name, string fieldLabel, int maxLen)
        {
            if (name.Length > maxLen)
                throw new Exception(
                    $"{fieldLabel} must not exceed {maxLen} characters " +
                    $"(you entered {name.Length}).");
        }

        // prevents nonsensical entries like "12345" as a category or vaccine name
        private static void ValidateNotPurelyNumeric(string name, string fieldLabel)
        {
            if (name.Length > 0 && name.All(char.IsDigit))
                throw new Exception($"{fieldLabel} cannot be a purely numeric value.");
        }

        // CATEGORY METHODS

        public void AddCategory(string name)
        {
            ValidateNameLength(name, "Category name", 30);

            // case-insensitive duplicate check
            bool alreadyExists = categoryList.Exists(c =>
                c.CategoryName.ToLower() == name.ToLower());

            if (alreadyExists)
                throw new Exception("Category already exists.");

            categoryList.Add(new Category(nextCategoryId++, name));
        }

        public void UpdateCategory(int categoryId, string newName)
        {
            var cat = categoryList.Find(c => c.CategoryId == categoryId);
            if (cat == null)
                throw new Exception("Category not found.");

            ValidateNameLength(newName, "Category name", 30);

            // allow renaming to the same name (case change); only block conflicts with other categories
            bool conflict = categoryList.Exists(c =>
                c.CategoryName.ToLower() == newName.ToLower() &&
                c.CategoryId != categoryId);
            if (conflict)
                throw new Exception("Another category already has that name.");

            cat.CategoryName = newName;
        }

        // deletion is blocked if any product still references this category
        public void DeleteCategory(int categoryId)
        {
            var cat = categoryList.Find(c => c.CategoryId == categoryId);
            if (cat == null)
                throw new Exception("Category not found.");

            bool inUse = vaccineList.Exists(v => v.CategoryId == categoryId);
            if (inUse)
                throw new Exception("Cannot delete category. It is assigned to existing products.");

            categoryList.Remove(cat);
        }

        // SUPPLIER METHODS

        public void AddSupplier(string name, string contact)
        {
            ValidateNameLength(name, "Supplier name", 35);
            ValidateContactNumber(contact);

            bool alreadyExists = supplierList.Exists(s =>
                s.SupplierName.ToLower() == name.ToLower());

            if (alreadyExists)
                throw new Exception("Supplier already exists.");

            supplierList.Add(new Supplier(nextSupplierId++, name, contact));
        }

        public void UpdateSupplier(int supplierId, string newName, string newContact)
        {
            var sup = supplierList.Find(s => s.SupplierId == supplierId);
            if (sup == null)
                throw new Exception("Supplier not found.");

            ValidateNameLength(newName, "Supplier name", 35);
            ValidateContactNumber(newContact);

            bool conflict = supplierList.Exists(s =>
                s.SupplierName.ToLower() == newName.ToLower() &&
                s.SupplierId != supplierId);
            if (conflict)
                throw new Exception("Another supplier already has that name.");

            sup.SupplierName = newName;
            sup.ContactNumber = newContact;
        }

        // deletion is blocked if any product still references this supplier
        public void DeleteSupplier(int supplierId)
        {
            var sup = supplierList.Find(s => s.SupplierId == supplierId);
            if (sup == null)
                throw new Exception("Supplier not found.");

            bool inUse = vaccineList.Exists(v => v.SupplierId == supplierId);
            if (inUse)
                throw new Exception("Cannot delete supplier. It is assigned to existing products.");

            supplierList.Remove(sup);
        }

        // PRODUCT METHODS

        // throws DuplicateProductException if the name already exists
        // so the UI can offer to restock instead of silently failing
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

            ValidateNameLength(name, "Vaccine name", 40);
            ValidateNotPurelyNumeric(name, "Vaccine name");

            var cat = categoryList.Find(c => c.CategoryId == categoryId);
            if (cat == null)
                throw new Exception("Category not found. Please enter a valid Category ID.");

            var sup = supplierList.Find(s => s.SupplierId == supplierId);
            if (sup == null)
                throw new Exception("Supplier not found. Please enter a valid Supplier ID.");

            // duplicate check before adding; the custom exception carries the existing ID
            var existingDup = vaccineList.Find(v => v.VaccineName.ToLower() == name.ToLower());
            if (existingDup != null)
                throw new DuplicateProductException(existingDup.ProductId);

            var newVaccine = new Product(nextProductId++, name, categoryId,
                                          supplierId, qty, price, expiry, minStock);

            vaccineList.Add(newVaccine);

            RecordTransaction(newVaccine.ProductId, name, "ADD", qty,
                $"New vaccine '{name}' added to inventory.");
        }

        // searches across name, category name, and supplier name
        public List<Product> SearchVaccine(string keyword)
        {
            string kw = keyword.ToLower();
            return vaccineList.FindAll(v =>
                v.VaccineName.ToLower().Contains(kw) ||
                GetCategoryName(v.CategoryId).ToLower().Contains(kw) ||
                GetSupplierName(v.SupplierId).ToLower().Contains(kw));
        }

        public List<Product> SearchVaccineByName(string keyword)
        {
            string kw = keyword.ToLower();
            return vaccineList.FindAll(v => v.VaccineName.ToLower().Contains(kw));
        }

        public List<Product> SearchVaccineByCategory(string keyword)
        {
            string kw = keyword.ToLower();
            return vaccineList.FindAll(v => GetCategoryName(v.CategoryId).ToLower().Contains(kw));
        }

        public List<Product> SearchVaccineBySupplier(string keyword)
        {
            string kw = keyword.ToLower();
            return vaccineList.FindAll(v => GetSupplierName(v.SupplierId).ToLower().Contains(kw));
        }

        public List<Product> SearchVaccineById(int productId)
        {
            return vaccineList.FindAll(v => v.ProductId == productId);
        }

        public Product FindVaccineByName(string name)
        {
            return vaccineList.Find(v => v.VaccineName.ToLower() == name.ToLower());
        }

        // updates name, price, expiry, and min stock only
        // category and supplier are intentionally not updatable here (use delete + re-add)
        // if the product was already expired and the user skips the date field,
        // the update still proceeds so they can fix other fields without being blocked
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

            // only reject if a new past date is being actively set; leaving the date unchanged is allowed
            if (newExpiry.Date != vaccine.ExpiryDate.Date && newExpiry.Date <= DateTime.Today)
                throw new Exception("New expiry date must be a future date.");

            bool nameConflict = vaccineList.Exists(v =>
                v.VaccineName.ToLower() == newName.ToLower() &&
                v.ProductId != productId);
            if (nameConflict)
                throw new Exception("Another vaccine already has that name.");

            vaccine.VaccineName = newName;
            vaccine.Price = newPrice;
            vaccine.ExpiryDate = newExpiry;
            vaccine.MinimumStockLevel = newMinStock;

            RecordTransaction(productId, vaccine.VaccineName, "UPDATE", 0,
                $"Vaccine info updated for '{vaccine.VaccineName}'.");
        }

        public void DeleteVaccine(int productId)
        {
            var vaccine = vaccineList.Find(v => v.ProductId == productId);
            if (vaccine == null)
                throw new Exception("Vaccine not found.");

            string name = vaccine.VaccineName;

            // log before removing so the name is still captured in the transaction
            RecordTransaction(productId, name, "DELETE", 0,
                $"Vaccine '{name}' deleted from inventory.");

            vaccineList.Remove(vaccine);
        }

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

            RecordTransaction(productId, vaccine.VaccineName, "RESTOCK", addQty,
                $"Restocked {addQty} unit(s) of '{vaccine.VaccineName}'.");
        }

        public void DeductVaccine(int productId, int deductQty)
        {
            if (deductQty <= 0)
                throw new Exception("Deduct amount must be greater than zero.");

            var vaccine = vaccineList.Find(v => v.ProductId == productId);
            if (vaccine == null)
                throw new Exception("Vaccine not found.");

            // guard against going negative; the UI also checks this but defense in depth is good
            if (vaccine.Quantity - deductQty < 0)
                throw new Exception($"Not enough stock. Current quantity: {vaccine.Quantity}");

            vaccine.Quantity -= deductQty;

            RecordTransaction(productId, vaccine.VaccineName, "DEDUCT", deductQty,
                $"Deducted {deductQty} unit(s) of '{vaccine.VaccineName}'.");
        }

        // returns only products where quantity is below their individual minimum threshold
        public List<Product> GetLowStockVaccines()
        {
            return vaccineList.FindAll(v => v.IsLowStock());
        }

        public decimal ComputeTotalInventoryValue()
        {
            decimal total = 0;
            foreach (var vaccine in vaccineList)
                total += vaccine.GetTotalValue();
            return total;
        }

        // HELPER METHODS

        // returns "Unknown" as a safe fallback if the ID has been deleted or is invalid
        public string GetCategoryName(int id)
        {
            var cat = categoryList.Find(c => c.CategoryId == id);
            return cat != null ? cat.CategoryName : "Unknown";
        }

        public string GetSupplierName(int id)
        {
            var sup = supplierList.Find(s => s.SupplierId == id);
            return sup != null ? sup.SupplierName : "Unknown";
        }

        public Product GetVaccineById(int id)
        {
            return vaccineList.Find(v => v.ProductId == id);
        }

        public Category GetCategoryById(int id)
        {
            return categoryList.Find(c => c.CategoryId == id);
        }

        public Supplier GetSupplierById(int id)
        {
            return supplierList.Find(s => s.SupplierId == id);
        }

        // bundles the four dashboard figures into a single call to reduce UI coupling
        public (int TotalProducts, int Expired, int LowStock, decimal TotalValue) GetDashboardCounts()
        {
            int totalProducts = vaccineList.Count;
            int expired = vaccineList.Count(v => v.IsExpired());
            // expired items are excluded from the low-stock count to avoid double-alerting
            int lowStock = vaccineList.Count(v => v.IsLowStock() && !v.IsExpired());
            decimal totalValue = ComputeTotalInventoryValue();
            return (totalProducts, expired, lowStock, totalValue);
        }

        // central transaction logger; all stock-changing operations call this
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

    // thrown by AddVaccine when a product with the same name already exists
    // carries the existing product's ID so the UI can offer to restock instead
    public class DuplicateProductException : Exception
    {
        public int ExistingProductId { get; }

        public DuplicateProductException(int existingProductId)
            : base("Product already exists.")
        {
            ExistingProductId = existingProductId;
        }
    }
}