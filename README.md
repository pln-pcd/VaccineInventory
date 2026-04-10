# 💉 Vaccine Inventory Management System

A **CLI-based Inventory Management System** built in **C#** using **Object-Oriented Programming** principles. This project was created as a **Midterm Lab Exam** requirement. It runs entirely in the console with no database — all data is stored in-memory using `List<T>`.

---

## 🔐 Login Credentials

| Role  | Username | Password  |
|-------|----------|-----------|
| Admin | `admin`  | `admin123` |
| Staff | `staff`  | `staff123` |

> ⚠️ The system currently allows only the **admin** account to log in through the login gate in `Program.cs`. The staff account exists in the user list but the login gate is hardcoded to `admin`/`admin123`.

---

## 📁 Project Structure

```
VaccineInventory/
│
├── Models/
│   ├── Product.cs            — Vaccine product data model
│   ├── Category.cs           — Category data model
│   ├── Supplier.cs           — Supplier data model
│   ├── User.cs               — User/login data model
│   └── TransactionRecord.cs  — Transaction history data model
│
├── Services/
│   └── InventoryManager.cs   — All business logic and in-memory storage
│
├── Helpers/
│   └── DisplayHelper.cs      — Formatted console output utilities
│
└── Program.cs                — Entry point, login gate, and menu handlers
```

---

## 🧱 OOP Concepts Used

| Concept              | Where Applied                                                         |
|----------------------|-----------------------------------------------------------------------|
| Classes & Objects    | All 5 model classes + `InventoryManager` + `DisplayHelper`            |
| Constructors         | Every model class has a constructor for initialization                |
| Properties           | All fields exposed via get/set with backing fields                    |
| Encapsulation        | Private `_backing` fields with validated setters                      |
| Access Modifiers     | `private`, `public`, `static` used throughout                         |
| Methods              | Business logic methods in `InventoryManager`, display in `DisplayHelper` |
| Exception Handling   | `try-catch` in all handler methods; custom `DuplicateProductException` |
| Inheritance          | `DuplicateProductException` extends `Exception`                       |

---

## ⚙️ System Features

### 🏷️ Product Management (Options 1–5)
| # | Feature         | Description                                                                 |
|---|-----------------|-----------------------------------------------------------------------------|
| 1 | Add Product     | Add a new vaccine with name, category, supplier, quantity, price, expiry date, and minimum stock level. Detects duplicates and offers to restock instead. |
| 2 | View All        | View all products with optional filters (ALL / LOW / EXPIRED / OK / by Category) and sorting (Name / Qty / Price / Expiry). |
| 3 | Search Product  | Search by product name, category, supplier name, or product ID.             |
| 4 | Update Product  | Edit vaccine name, price, expiry date, and minimum stock level.             |
| 5 | Delete Product  | Remove a vaccine from inventory with confirmation prompt.                   |

### 📦 Stock Control (Options 6–8)
| # | Feature        | Description                                                                 |
|---|----------------|-----------------------------------------------------------------------------|
| 6 | Restock        | Add units to a product's stock. Warns if the product is expired or nearly expired. |
| 7 | Deduct Stock   | Remove units from stock. Prevents going below zero. Warns on zero-stock result. |
| 8 | Low Stock      | Displays all products currently below their minimum stock level.            |

### 🗂️ Category Management (Options 9–12)
| # | Feature         | Description                                              |
|---|-----------------|----------------------------------------------------------|
| 9 | Add Category    | Add a new vaccine category (e.g., COVID-19, Hepatitis B). |
| 10 | View Categories | List all categories with their IDs.                      |
| 11 | Update Category | Rename a category.                                       |
| 12 | Delete Category | Remove a category (blocked if assigned to any product).  |

### 🏢 Supplier Management (Options 13–16)
| # | Feature        | Description                                                              |
|---|----------------|--------------------------------------------------------------------------|
| 13 | Add Supplier   | Add a new supplier with name and contact number (digits only, 7–15 chars). |
| 14 | View Suppliers | List all suppliers with contact info.                                    |
| 15 | Update Supplier| Edit supplier name and contact number.                                   |
| 16 | Delete Supplier| Remove a supplier (blocked if assigned to any product).                  |

### 📊 Reports (Options 17–18)
| # | Feature              | Description                                                               |
|---|----------------------|---------------------------------------------------------------------------|
| 17 | Transaction History  | View a log of all ADD / RESTOCK / DEDUCT / UPDATE / DELETE actions with optional filter. |
| 18 | Total Inventory Value| Shows unit price × quantity for each product, plus a grand total.         |

---

## 🗃️ Data Models

### `Product`
Represents a single vaccine item in inventory.

| Field              | Type      | Description                          |
|--------------------|-----------|--------------------------------------|
| `ProductId`        | `int`     | Auto-incremented unique ID           |
| `VaccineName`      | `string`  | Name of the vaccine (max 40 chars)   |
| `CategoryId`       | `int`     | Foreign key to Category              |
| `SupplierId`       | `int`     | Foreign key to Supplier              |
| `Quantity`         | `int`     | Current stock (cannot be negative)   |
| `Price`            | `decimal` | Unit price in PHP                    |
| `ExpiryDate`       | `DateTime`| Expiry date of the vaccine batch     |
| `MinimumStockLevel`| `int`     | Threshold below which stock is "low" |

**Key Methods:**
- `IsLowStock()` — returns `true` if `Quantity < MinimumStockLevel`
- `IsExpired()` — returns `true` if `ExpiryDate` is before today
- `GetTotalValue()` — returns `Quantity × Price`

---

### `Category`
Represents a vaccine classification (e.g., COVID-19, Hepatitis B).

| Field          | Type     | Description           |
|----------------|----------|-----------------------|
| `CategoryId`   | `int`    | Auto-incremented ID   |
| `CategoryName` | `string` | Name of the category  |

---

### `Supplier`
Represents a vaccine supplier or distributor.

| Field           | Type     | Description                         |
|-----------------|----------|-------------------------------------|
| `SupplierId`    | `int`    | Auto-incremented ID                 |
| `SupplierName`  | `string` | Name of the supplier (max 35 chars) |
| `ContactNumber` | `string` | Phone number (digits only, 7–15 chars, optional leading +) |

---

### `User`
Represents a system user for login authentication.

| Field      | Type     | Description                         |
|------------|----------|-------------------------------------|
| `UserId`   | `int`    | Unique user ID                      |
| `Username` | `string` | Login username                      |
| `Password` | `string` | Plain-text password (lab scope)     |
| `Role`     | `string` | `"Admin"` or `"Staff"`              |

**Key Method:**
- `ValidatePassword(input)` — compares input to stored password

---

### `TransactionRecord`
Logs every stock action for audit purposes.

| Field             | Type       | Description                                      |
|-------------------|------------|--------------------------------------------------|
| `TransactionId`   | `int`      | Auto-incremented ID                              |
| `ProductId`       | `int`      | ID of the affected product                       |
| `VaccineName`     | `string`   | Name stored at time of action (survives deletion)|
| `ActionType`      | `string`   | `ADD`, `RESTOCK`, `DEDUCT`, `UPDATE`, or `DELETE`|
| `QuantityChanged` | `int`      | Units added/removed (0 for UPDATE/DELETE)        |
| `Date`            | `DateTime` | Timestamp of the action                          |
| `Notes`           | `string`   | Human-readable description of the action         |

---

## 🔧 Services

### `InventoryManager`
The core service class. Holds all in-memory lists and implements all business logic.

**Storage:**
```csharp
private List<Product>           vaccineList
private List<Category>          categoryList
private List<Supplier>          supplierList
private List<User>              userList
private List<TransactionRecord> historyList
```

**Key Methods:**

| Method | Description |
|--------|-------------|
| `AuthenticateUser(username, password)` | Validates login credentials |
| `AddVaccine(...)` | Adds a new product; throws `DuplicateProductException` on duplicate name |
| `UpdateVaccine(...)` | Updates name, price, expiry, and min stock |
| `DeleteVaccine(id)` | Removes a product and logs the deletion |
| `RestockVaccine(id, qty)` | Adds quantity; capped at 100,000 per transaction |
| `DeductVaccine(id, qty)` | Subtracts quantity; prevents going below zero |
| `SearchVaccine(keyword)` | Searches name, category name, and supplier name |
| `GetLowStockVaccines()` | Returns all products below their minimum stock level |
| `ComputeTotalInventoryValue()` | Sums `Quantity × Price` across all products |
| `GetDashboardCounts()` | Returns totals for the login dashboard summary |
| `AddCategory / UpdateCategory / DeleteCategory` | Full CRUD for categories |
| `AddSupplier / UpdateSupplier / DeleteSupplier` | Full CRUD for suppliers |

**Custom Exception:**
```csharp
DuplicateProductException(int existingProductId)
```
Thrown when adding a product whose name already exists. Carries the existing product's ID so the UI can offer to redirect the user to restock instead.

---

## 🖥️ Display Helper

`DisplayHelper` is a `static` utility class for all formatted console output.

| Method | Description |
|--------|-------------|
| `ShowVaccineTable(vaccines, manager)` | Renders a formatted product table with status column |
| `ShowTransactionTable(records, filter)` | Renders transaction history, with optional action filter |
| `ShowMainMenu(username, role)` | Prints the 18-option main menu |
| `ShowFullDashboard(manager)` | Shows totals and alerts after login |
| `PrintSectionHeader(module, action)` | Section title with thick/thin borders |
| `PrintSuccess(message)` | Formatted success message |
| `PrintError(message)` | Formatted error message |
| `PressEnter()` | Pause and wait for user input |

---

## ✅ Validation Rules

| Field           | Rule                                                           |
|-----------------|----------------------------------------------------------------|
| Vaccine Name    | Non-empty, not purely numeric, max 40 characters              |
| Category Name   | Non-empty, max 30 characters                                   |
| Supplier Name   | Non-empty, max 35 characters                                   |
| Contact Number  | Digits only, 7–15 characters, optional leading `+`            |
| Quantity        | Non-negative integer; restock capped at 100,000               |
| Price           | Non-negative decimal                                           |
| Expiry Date     | Must be a future date when adding; update only rejects *newly set* past dates |
| Min Stock Level | At least 1                                                     |

---

## 💾 Sample Data (Preloaded on Start)

**Categories:** COVID-19, Anti-Rabies, Hepatitis B, Tetanus, Pneumonia

**Suppliers:** MedPharm Supplies (09171234567), VacciCare PH (09281234567)

**Products:**
| Vaccine    | Category   | Supplier          | Qty | Price     | Expiry     |
|------------|------------|-------------------|-----|-----------|------------|
| Sinovac    | COVID-19   | MedPharm Supplies | 100 | ₱500.00   | 12/31/2026 |
| Verorab    | Anti-Rabies| VacciCare PH      | 15  | ₱1,200.00 | 06/30/2026 |
| Engerix-B  | Hepatitis B| MedPharm Supplies | 8   | ₱850.00   | 03/15/2027 |

---

## 🚀 How to Run

1. Clone or download the project.
2. Open in **Visual Studio** or any C#-compatible IDE.
3. Build and run the project (`F5` or `dotnet run`).
4. Log in using the credentials above.
5. Navigate the menu by entering the option number.

> **Requirement:** .NET 6.0 or higher

---

