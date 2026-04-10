// Category.cs
// model for vaccine categories like COVID-19, Hepatitis B, etc.

using System;

namespace VaccineInventory.Models
{
    public class Category
    {
        private string _categoryName;

        public int CategoryId { get; set; }

        // rejects blank or whitespace-only names at the model level
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

        public Category(int categoryId, string categoryName)
        {
            CategoryId = categoryId;
            CategoryName = categoryName;
        }

        public override string ToString()
        {
            return $"[{CategoryId}] {CategoryName}";
        }
    }
}