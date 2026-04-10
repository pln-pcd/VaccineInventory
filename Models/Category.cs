// ============================================================
// Category.cs
// Model class representing a vaccine category.
// Examples: COVID-19, Anti-Rabies, Hepatitis B, Tetanus
//
// OOP Concepts Used:
//   - Encapsulation : private backing field with validated setter
//   - Properties    : public CategoryName enforces non-empty rule
//   - Constructor   : sets both fields on creation
//   - Override      : ToString() for readable display
// ============================================================

using System;

namespace VaccineInventory.Models
{
    public class Category
    {
        // ===== Private backing field (encapsulation) =====
        // Stores the actual category name value.
        // Direct field access is restricted; use the property instead.
        private string _categoryName;

        // ===== Properties =====

        // Auto-property — no special validation needed for the integer ID
        public int CategoryId { get; set; }

        // Category name must not be empty or whitespace.
        // Leading/trailing spaces are trimmed automatically.
        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Category name cannot be empty.");
                _categoryName = value.Trim();
            }
        }

        // ===== Constructor =====
        // Creates a Category with a specific ID and name.
        // The name assignment goes through the validated property above.
        public Category(int categoryId, string categoryName)
        {
            CategoryId = categoryId;
            CategoryName = categoryName;
        }

        // ===== Override =====

        // Returns a formatted string for easy display in lists or logs.
        // Example output: "[1] COVID-19"
        public override string ToString()
        {
            return $"[{CategoryId}] {CategoryName}";
        }
    }
}