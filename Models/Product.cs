// ============================================================
// Product.cs
// Model class representing a single vaccine product in the
// inventory system.
//
// OOP Concepts Used:
//   - Encapsulation : private backing fields with validated setters
//   - Properties    : public get/set with business rule enforcement
//   - Constructor   : initializes all fields at object creation
//   - Methods       : IsLowStock(), IsExpired(), GetTotalValue()
// ============================================================

using System;

namespace VaccineInventory.Models
{
    public class Product
    {
        // ===== Private backing fields (encapsulation) =====
        // These hold the actual data. Public properties below
        // control access and enforce validation rules.
        private string _vaccineName;
        private int _quantity;
        private decimal _price;
        private int _minimumStockLevel;

        // ===== Properties =====

        // Auto-property — no validation needed for a simple integer ID
        public int ProductId { get; set; }

        // Vaccine name must not be empty or whitespace; trimmed on assignment
        public string VaccineName
        {
            get { return _vaccineName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Vaccine name cannot be empty.");
                _vaccineName = value.Trim();
            }
        }

        // Foreign key linking this product to a Category record
        public int CategoryId { get; set; }

        // Foreign key linking this product to a Supplier record
        public int SupplierId { get; set; }

        // Quantity in stock — cannot go negative
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Quantity cannot be negative.");
                _quantity = value;
            }
        }

        // Unit price in Philippine Peso — cannot be negative
        public decimal Price
        {
            get { return _price; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Price cannot be negative.");
                _price = value;
            }
        }

        // Expiry date of this batch — used to detect expired vaccines
        public DateTime ExpiryDate { get; set; }

        // The stock level at or below which this product is flagged as "low"
        // Must be at least 1 so that zero-stock is always treated as low
        public int MinimumStockLevel
        {
            get { return _minimumStockLevel; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Minimum stock level must be at least 1.");
                _minimumStockLevel = value;
            }
        }

        // ===== Constructor =====
        // Initializes all product fields at once.
        // All assignments go through the validated properties above,
        // so invalid data is caught immediately at creation time.
        public Product(int productId, string vaccineName, int categoryId,
                       int supplierId, int quantity, decimal price,
                       DateTime expiryDate, int minimumStockLevel)
        {
            ProductId = productId;
            VaccineName = vaccineName;
            CategoryId = categoryId;
            SupplierId = supplierId;
            Quantity = quantity;
            Price = price;
            ExpiryDate = expiryDate;
            MinimumStockLevel = minimumStockLevel;
        }

        // ===== Methods =====

        // Returns true if current stock is strictly below the minimum stock level.
        // Used by the dashboard and option 8 (Low Stock view).
        public bool IsLowStock()
        {
            return Quantity < MinimumStockLevel;
        }

        // Returns true if the expiry date has already passed (before today).
        // Used to flag expired vaccines in the product table.
        public bool IsExpired()
        {
            return ExpiryDate.Date < DateTime.Today;
        }

        // Computes the total monetary value of this vaccine's stock.
        // Formula: Quantity × Price
        // Used in option 18 (Total Inventory Value report).
        public decimal GetTotalValue()
        {
            return Quantity * Price;
        }
    }
}