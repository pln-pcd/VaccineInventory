// ============================================================
// TransactionRecord.cs
// Model class that logs every stock action performed on a
// vaccine product. Acts as an audit trail for inventory changes.
//
// Supported ActionTypes:
//   ADD      — New vaccine added to inventory
//   RESTOCK  — Units added to existing stock
//   DEDUCT   — Units removed from stock
//   UPDATE   — Product info (name/price/expiry/min stock) changed
//   DELETE   — Product removed from inventory
//
// Design Note:
//   VaccineName is stored directly (not just the ProductId).
//   This ensures transaction history stays accurate even after
//   a product is renamed or deleted.
//
// OOP Concepts Used:
//   - Encapsulation : private backing fields with validated setters
//   - Properties    : ActionType normalized to uppercase; Notes defaults to ""
//   - Constructor   : initializes all fields at record creation
// ============================================================

using System;

namespace VaccineInventory.Models
{
    public class TransactionRecord
    {
        // ===== Private backing fields (encapsulation) =====
        private string _actionType;
        private string _notes;

        // ===== Properties =====

        // Auto-incremented unique ID for each transaction record
        public int TransactionId { get; set; }

        // The ID of the product affected by this transaction
        public int ProductId { get; set; }

        // Vaccine name snapshot — stored at time of action so history
        // remains accurate even if the product is later renamed or deleted
        public string VaccineName { get; set; }

        // The type of action performed. Always stored in UPPERCASE.
        // Valid values: ADD, RESTOCK, DEDUCT, UPDATE, DELETE
        public string ActionType
        {
            get { return _actionType; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Action type cannot be empty.");
                _actionType = value.Trim().ToUpper(); // normalize to uppercase for consistency
            }
        }

        // Number of units added or removed.
        // For UPDATE and DELETE actions this is stored as 0 (no quantity changed).
        public int QuantityChanged { get; set; }

        // Date and time the transaction was recorded
        public DateTime Date { get; set; }

        // Human-readable description of the action.
        // Defaults to empty string if null is passed — never null in practice.
        public string Notes
        {
            get { return _notes; }
            set { _notes = value ?? string.Empty; }
        }

        // ===== Constructor =====
        // Creates a complete transaction record.
        // Called internally by InventoryManager.RecordTransaction()
        // whenever a stock action is performed.
        public TransactionRecord(int transactionId, int productId, string vaccineName,
                                  string actionType, int quantityChanged,
                                  DateTime date, string notes)
        {
            TransactionId = transactionId;
            ProductId = productId;
            VaccineName = vaccineName;
            ActionType = actionType;
            QuantityChanged = quantityChanged;
            Date = date;
            Notes = notes;
        }
    }
}