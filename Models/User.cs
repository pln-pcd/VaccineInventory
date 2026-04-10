// ============================================================
// User.cs
// Model class representing a system user.
// Used for login authentication at program startup.
//
// Default Accounts:
//   Username : admin    Password : admin123   Role : Admin
//   Username : staff    Password : staff123   Role : Staff
//
// OOP Concepts Used:
//   - Encapsulation : private backing fields with validated setters
//   - Properties    : Username, Password, Role enforce non-empty rules
//   - Constructor   : sets all fields at object creation
//   - Method        : ValidatePassword() for credential checking
//   - Override      : ToString() excludes password for security
// ============================================================

using System;

namespace VaccineInventory.Models
{
    public class User
    {
        // ===== Private backing fields (encapsulation) =====
        // Password is kept private and never exposed directly.
        // Other fields are also backed privately for consistency.
        private string _username;
        private string _password;
        private string _role;

        // ===== Properties =====

        // Auto-property — no validation needed for the integer ID
        public int UserId { get; set; }

        // Username must not be empty or whitespace; trimmed on assignment
        public string Username
        {
            get { return _username; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Username cannot be empty.");
                _username = value.Trim();
            }
        }

        // Password stored as plain text.
        // Note: plain-text passwords are acceptable for this CLI lab exam scope.
        // In a real production system, passwords should be hashed (e.g., BCrypt).
        public string Password
        {
            get { return _password; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Password cannot be empty.");
                _password = value;
            }
        }

        // Role identifies what the user can do (e.g., "Admin" or "Staff")
        public string Role
        {
            get { return _role; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Role cannot be empty.");
                _role = value.Trim();
            }
        }

        // ===== Constructor =====
        // Creates a User with all required fields.
        // All assignments go through the validated properties above.
        public User(int userId, string username, string password, string role)
        {
            UserId = userId;
            Username = username;
            Password = password;
            Role = role;
        }

        // ===== Methods =====

        // Validates a login attempt by comparing the input to the stored password.
        // Returns true if the passwords match, false otherwise.
        // Note: uses direct string equality — case-sensitive comparison.
        public bool ValidatePassword(string inputPassword)
        {
            return _password == inputPassword;
        }

        // ===== Override =====

        // Returns user info for display. Password is intentionally excluded
        // to avoid accidental exposure in logs or console output.
        // Example output: "[1] admin (Admin)"
        public override string ToString()
        {
            return $"[{UserId}] {Username} ({Role})";
        }
    }
}