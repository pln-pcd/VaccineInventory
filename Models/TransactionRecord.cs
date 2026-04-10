// TransactionRecord.cs
// logs every stock action done on a vaccine (ADD, RESTOCK, DEDUCT, UPDATE, DELETE)
// vaccine name is stored directly so history stays accurate even after a product is deleted

using System;

namespace VaccineInventory.Models
{
    public class TransactionRecord
    {
        private string _actionType;
        private string _notes;

        public int TransactionId { get; set; }
        public int ProductId { get; set; }

        // snapshot of the name at time of transaction, not a live reference
        public string VaccineName { get; set; }

        // normalized to uppercase so filter comparisons are case-insensitive
        public string ActionType
        {
            get { return _actionType; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Action type cannot be empty.");
                _actionType = value.Trim().ToUpper();
            }
        }

        // 0 for UPDATE and DELETE since those actions don't change quantity
        public int QuantityChanged { get; set; }

        public DateTime Date { get; set; }

        // defaults to empty string rather than null to avoid null checks when displaying
        public string Notes
        {
            get { return _notes; }
            set { _notes = value ?? string.Empty; }
        }

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