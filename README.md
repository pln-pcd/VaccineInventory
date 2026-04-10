# Vaccine Inventory Management System

A CLI-based Inventory Management System built in C# using Object-Oriented Programming principles. Created as a Midterm Lab Exam requirement. Runs entirely in the console with no database — all data is stored in-memory using `List<T>`.

---

## Login Credentials

| Role  | Username | Password  |
|-------|----------|-----------|
| Admin | `admin`  | `admin123` |

---

## Project Structure

```
VaccineInventory/
│
├── Models/
│   ├── Product.cs
│   ├── Category.cs
│   ├── Supplier.cs
│   ├── User.cs
│   └── TransactionRecord.cs
│
├── Services/
│   └── InventoryManager.cs
│
├── Helpers/
│   └── DisplayHelper.cs
│
└── Program.cs
```

---

## OOP Concepts Used

| Concept | Where |
|---|---|
| Classes & Objects | All model classes, InventoryManager, DisplayHelper |
| Constructors | Every model class |
| Properties | All fields with get/set |
| Encapsulation | Private backing fields with validated setters |
| Access Modifiers | private, public, static |
| Methods | Business logic in InventoryManager, display in DisplayHelper |
| Exception Handling | try-catch in all handlers; custom DuplicateProductException |
| Inheritance | DuplicateProductException extends Exception |

---

## Features

### Product Management (1–5)
- **Add Product** – adds a new vaccine, detects duplicates and offers to restock instead
- **View All** – view products with filters (ALL / LOW / EXPIRED / OK / by Category) and sorting
- **Search** – search by name, category, supplier, or product ID
- **Update** – edit name, price, expiry date, min stock level
- **Delete** – remove a vaccine with confirmation

### Stock Control (6–8)
- **Restock** – add units to stock, warns if product is expired or near expiry
- **Deduct** – remove units, prevents going below zero
- **Low Stock** – shows all products below their minimum stock level

### Category Management (9–12)
- Full CRUD for vaccine categories
- Delete is blocked if the category is assigned to a product

### Supplier Management (13–16)
- Full CRUD for suppliers
- Delete is blocked if the supplier is assigned to a product

### Reports (17–18)
- **Transaction History** – full audit log with optional filter by action type
- **Total Inventory Value** – shows unit price x quantity per product and grand total

---

## Sample Data (Preloaded)

**Categories:** COVID-19, Anti-Rabies, Hepatitis B, Tetanus, Pneumonia

**Suppliers:** MedPharm Supplies, VacciCare PH

**Products:**
| Vaccine | Category | Supplier | Qty | Price | Expiry |
|---|---|---|---|---|---|
| Sinovac | COVID-19 | MedPharm Supplies | 100 | P500.00 | 12/31/2026 |
| Verorab | Anti-Rabies | VacciCare PH | 15 | P1,200.00 | 06/30/2026 |
| Engerix-B | Hepatitis B | MedPharm Supplies | 8 | P850.00 | 03/15/2027 |

---

## How to Run

1. Open in Visual Studio or any C# IDE
2. Build and run (`F5` or `dotnet run`)
3. Log in with the credentials above
4. Navigate using the option number

Requires .NET 6.0 or higher.