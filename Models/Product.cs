// Product.cs
// represents a single vaccine item in the inventory

using System;

namespace VaccineInventory.Models
{
    public class Product
    {
        // private backing fields for validated properties
        private string _vaccineName;
        private int _quantity;
        private decimal _price;
        private int _minimumStockLevel;

        public int ProductId { get; set; }

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

        public int CategoryId { get; set; }

        public int SupplierId { get; set; }

        // stock cant go negative; prevents data corruption from bad input
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

        public DateTime ExpiryDate { get; set; }

        // minimum must be at least 1 so that zero stock always triggers a low-stock alert
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

        // true if current stock is below the per-product threshold
        public bool IsLowStock()
        {
            return Quantity < MinimumStockLevel;
        }

        // compares against today's date only, ignoring time-of-day
        public bool IsExpired()
        {
            return ExpiryDate.Date < DateTime.Today;
        }

        // used for the total inventory value report
        public decimal GetTotalValue()
        {
            return Quantity * Price;
        }
    }
}