// User.cs
// model for system users, used for login

using System;

namespace VaccineInventory.Models
{
    public class User
    {
        private string _username;
        private string _password;
        private string _role;

        public int UserId { get; set; }

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

        // stored as plain text — acceptable for this lab scope, not for production
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

        public User(int userId, string username, string password, string role)
        {
            UserId = userId;
            Username = username;
            Password = password;
            Role = role;
        }

        // direct equality check; would use hashing in a real application
        public bool ValidatePassword(string inputPassword)
        {
            return _password == inputPassword;
        }

        // password intentionally excluded to avoid accidental exposure in logs or UI
        public override string ToString()
        {
            return $"[{UserId}] {Username} ({Role})";
        }
    }
}