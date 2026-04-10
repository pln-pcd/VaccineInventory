// ============================================================
// Supplier.cs
// Model class representing a vaccine supplier or distributor.
//
// OOP Concepts Used:
//   - Encapsulation : private backing fields with validated setters
//   - Properties    : enforces non-empty rules for name and contact
//   - Constructor   : initializes all fields on object creation
//   - Override      : ToString() for readable display
// ============================================================

using System;

namespace VaccineInventory.Models
{
    public class Supplier
    {
        // ===== Private backing fields (encapsulation) =====
        // Direct field access is restricted to this class.
        // All reads and writes go through the public properties below.
        private string _supplierName;
        private string _contactNumber;

        // ===== Properties =====

        // Auto-property — no validation needed for the integer ID
        public int SupplierId { get; set; }

        // Supplier name must not be empty or whitespace.
        // Trimmed automatically on assignment.
        public string SupplierName
        {
            get { return _supplierName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Supplier name cannot be empty.");
                _supplierName = value.Trim();
            }
        }

        // Contact number must not be empty or whitespace.
        // Format validation (digits only, 7–15 chars) is handled
        // at the service layer (InventoryManager.ValidateContactNumber).
        public string ContactNumber
        {
            get { return _contactNumber; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Contact number cannot be empty.");
                _contactNumber = value.Trim();
            }
        }

        // ===== Constructor =====
        // Creates a Supplier with an ID, name, and contact number.
        // Both string assignments pass through their validated properties.
        public Supplier(int supplierId, string supplierName, string contactNumber)
        {
            SupplierId = supplierId;
            SupplierName = supplierName;
            ContactNumber = contactNumber;
        }

        // ===== Override =====

        // Returns a formatted string for easy display in lists or logs.
        // Example output: "[1] MedPharm Supplies - 09171234567"
        public override string ToString()
        {
            return $"[{SupplierId}] {SupplierName} - {ContactNumber}";
        }
    }
}