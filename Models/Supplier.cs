// Supplier.cs
// represents a vaccine supplier/distributor

using System;

namespace VaccineInventory.Models
{
    public class Supplier
    {
        private string _supplierName;
        private string _contactNumber;

        public int SupplierId { get; set; }

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

        // stores the raw value; digit-only and length checks are done in InventoryManager
        // so that validation messages stay consistent and centralized
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

        public Supplier(int supplierId, string supplierName, string contactNumber)
        {
            SupplierId = supplierId;
            SupplierName = supplierName;
            ContactNumber = contactNumber;
        }

        public override string ToString()
        {
            return $"[{SupplierId}] {SupplierName} - {ContactNumber}";
        }
    }
}