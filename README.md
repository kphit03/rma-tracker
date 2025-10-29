# RMA Tracker
_A .NET 8 web application for managing RMA (Return Merchandise Authorization) items, tracking statuses, and updating resolutions._

---

## 📸 Architecture Overview

Below is the system architecture and database design overview:

| Diagram | Description |
|----------|--------------|
| [Architecture Diagram] (Click to open in new tab)<img width="2161" height="737" alt="include" src="https://github.com/kphit03/rma-tracker/blob/36859e527046736359488f5068167152a2dd97e8/include.png" /> | High-level class structure |
| [Database Diagram] <img width="766" height="726" alt="image" src="https://github.com/kphit03/rma-tracker/blob/36859e527046736359488f5068167152a2dd97e8/Screenshot%202025-10-29%20171113.png" /> | ERD of the SQL Server database |

---

## 🧰 Tech Stack

- **Backend:** ASP.NET Core (.NET 8)
- **Database:** Microsoft SQL Server with SSMS
- **ORM:** Dapper / ADO.NET
- **Frontend:** Razor Pages / Bootstrap
- **IDE:** Visual Studio 2022
- **Language:** C#

---

## ⚙️ Installation Instructions

### Prerequisites
Make sure you have the following installed:

- [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)
  - With the **ASP.NET and web development** workload
- [.NET SDK 8.0+](https://dotnet.microsoft.com/en-us/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [SQL Server Management Studio (SSMS)](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

---

### 1️⃣ Clone the Repository

```bash
git clone https://github.com/kphit03/rma-tracker
cd rma-tracker
```

---
### 2️⃣ Set Up the Database

1. Open **SQL Server Management Studio (SSMS)** and connect to your SQL Server instance.
2. Create a new database:
   ```sql
   CREATE DATABASE RmaDb;
   GO
In SSMS, open the provided SQL script file:

📄 File: ./rmascript.sql
Make sure the database dropdown in SSMS (top toolbar) is set to RmaDb.

Execute the script:

USE RmaDb;
GO
-- Run script contents
After running, verify that the tables (rma_headers, rma_items, customers, products) and stored procedures were created successfully.


---

### 💡 Optional CLI Version (for completeness)

If someone prefers using the command line:
```bash
sqlcmd -S localhost -d master -E -i rmascript.sql
Use -U <username> -P <password> instead of -E if using SQL Authentication.
### 2️⃣ Set Up the Database

1. Open **SQL Server Management Studio (SSMS)**  
2. Create a new database:
   ```sql
   CREATE DATABASE RmaDb;
   ```

---

### 3️⃣ Configure the Connection String

Open `appsettings.json` and set your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YourDatabaseName;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  }
}
```

> 💡 If using SQL Authentication, replace `Trusted_Connection=True` with:
> ```
> User Id=your_username;Password=your_password;
> ```

---

### 4️⃣ Run the Application

In Visual Studio:
1. Set the startup project to your main web project (e.g. `YourProject.Web`)
2. Press **F5** or click **▶ Run**

Or use the CLI:
```bash
dotnet run --project YourProject.Web
```

---

## 🧮 Database Details

- Tables included:
  - `customers`
  - `products`
  - `rma_headers`
  - `rma_items`
- Stored procedures:
  - `sp_CloseRma`
  - `sp_CreateRma`
  - `sp_DeleteRma`
  - `sp_GetRmaDetail`
  - `sp_OpenRmasByAging`
  - `sp_UpdateRmaItemResolution`
  - `sp_UpdateRmaStatus`
  - `sp_RmaKpis`

Refer to **Database Diagram** for relationships.

---

## 🧱 Folder Structure

```
RmaTracker/
│
├── Models/
├── Views/
├── wwwroot/
├── appsettings.json
├── Program.cs
├── README.md
├── rmascript.sql
```

---
