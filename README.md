# 📚 LitraLand – Complete Library Management & Community System

LitraLand is a robust and fully-featured web platform designed for managing libraries and engaging reading communities. It provides tools for both administrators and regular users to interact with books, authors, and with each other in a clean, structured, and modern environment.

---

## 🚀 Project Highlights

* 🔐 Role-based access control (Admin (library staff) / Community user)
* 📖 Explore books by author, category, or keywords
* 🧾 Borrowing system with due dates and penalties
* ✍️ Reviews & ratings for books
* 🧑‍💼 Admin dashboard with full CRUD on:

  * Books
  * Authors
  * Categories
  * Publishers
  * subscribers
  * Users
* 🛠️ Clean layered architecture with separation of concerns
* 🧹 Form validation & anti-spam handling
* ☑️ External login support (Google / Facebook Auth)
* ⏱️ Performance-optimized for high user loads

---

## 🧠 Why LitraLand?

This is not just a CRUD system. LitraLand was built as a real-world project to demonstrate:

* ✅ Clean, scalable architecture in ASP.NET Core MVC
* ✅ Real separation between Core, Infrastructure, and Presentation
* ✅ Handling of real-world problems: validation, authorization, extensibility
* ✅ Ready-to-extend system for schools, communities, or commercial libraries

---

## 🏗️ Architecture Overview

```
LitraLand/
├── LitraLand.Web/           → MVC Presentation Layer (Controllers, Views, UI Logic)
├── LitraLand.Application/   → Business Logic Layer (Services, ViewModels, DTOs)
├── LitraLand.Domain/        → Core Layer (Entities, Interfaces, Business Rules)
├── LitraLand.Infrastructure/→ Data Access Layer (EF Core, DbContext, Migrations)
```

🧠 **Patterns & Technologies Used**:

* **Clean Architecture** (separated layers: Domain, Application, Infrastructure, Web)
* **Dependency Injection** (built-in .NET Core DI)
* **AutoMapper** (mapping between Entities & ViewModels)
* **FluentValidation** (for input validation)
* **Extension Methods** (to keep code clean and reusable)

---

## 🛠 Tech Stack

| Layer    | Tech                                               |
| -------- | -------------------------------------------------- |
| Backend  | ASP.NET Core MVC 7.0                               |
| Database | SQL Server                                         |
| ORM      | Entity Framework Core                              |
| Frontend | Razor Views, Bootstrap 5, jQuery                   |
| Auth     | ASP.NET Identity + Google/Facebook OAuth           |
| Others   | AutoMapper, FluentValidation, Layered Architecture |

---

## 🧪 Features in Detail

### 🔹 User

* View all available books
* Filter/search by keyword, author, or category
* Borrow/return books (with automatic tracking)
* Leave reviews, rate books
* Edit profile and track borrow history

### 🔸 Admin

* Full dashboard to manage:

  * Books, authors, categories, publishers
  * Users: lock/unlock, delete, assign roles
* Access logs and activity tracking
* Validate reviews, moderate content
* Export data (CSV/PDF support ready for plug-in)

---

## 🔐 Authentication & Access Flow

LitraLand distinguishes between two types of users, each with their own login portal and permissions:

### 🧑‍💼 Library Staff Login

* Accessible only via the dedicated **Library Staff Login Page**.
* Used exclusively by official library employees.
* No public registration allowed for staff accounts.
* After login, staff members access:

  * Book & user management dashboards.
  * Admin panels and advanced controls.

### 🌐 Community User Access

* Accessible via the **Community Login/Register Page**.
* Public users can:

  * **Register** for a new account.
  * **Login** using their community credentials.
* Once logged in, community users can:

  * Search and browse books.
  * Join discussions and comment.
  * Add books to personal lists.

> 🔑 Each user type is isolated with role-based access control to prevent unauthorized usage of staff features.

---

```
                    ┌──────────────────────┐
                    │     Login Page A     │
                    │   (Library Staff)    │
                    └─────────┬────────────┘
                              ↓
                    ┌──────────────────────┐
                    │   Admin Dashboard     │
                    └─────────▲────────────┘
              Roles: SuperAdmin, LibraryAdmin, Archive, Reception

┌────────────────┐                                 ┌────────────────────┐
│  Register Page │◄────────┐              ┌───────►│    Login Page B    │
│   (Community)  │         │              │        │   (Community)      │
└────────────────┘         │              │        └─────────┬──────────┘
                           └──────────────┴──────────────────┘
                                         ↓
                          ┌────────────────────────────┐
                          │     Community Area         │
                          └────────────▲───────────────┘
                 Roles: CommunityAdmin, CommunityMember
```

---

## 🗆 UI Demo Video

> [Watch on YouTube](https://www.youtube.com/watch?v=WQNnlcWqXuE&feature=youtu.be)

---

## ⚙️ Getting Started

### 1️⃣ Clone the repository

```bash
git clone https://github.com/M7-TROJAN/LitraLand.git
cd LitraLand
```

### 2️⃣ Configure the database

Open `appsettings.json` in `LitraLand.Web` and edit your local connection string:

```json
"DefaultConnection": "Server=.;Database=LitraLandDb;Trusted_Connection=True;"
```

### 3️⃣ Apply migrations

Open the Package Manager Console:

```powershell
Update-Database
```

Or use CLI:

```bash
dotnet ef database update
```

### 4️⃣ Run the application

```bash
dotnet run --project LitraLand.Web
```

---

## 🔐 Accounts

| Role   | Email                                       | Password    |
| ------ | ------------------------------------------- | ----------- |
| Admin  | [admin@litra.com](mailto:admin@litra.com)   | Admin\@123  |
| Reader | [reader@litra.com](mailto:reader@litra.com) | Reader\@123 |

⛑️ You can register new users and assign roles from the admin panel.

---

## 💡 What Makes This Stand Out?

* ✅ Real-world multi-role use case
* ✅ Clean, well-structured backend code
* ✅ Validations with FluentValidation (not just DataAnnotations)
* ✅ Extensible and ready for deployment
* ✅ Focus on maintainability and code readability
* ✅ Supports external logins (OAuth2)
* ✅ Optimized performance for high traffic scenarios

---

## 🧑‍💻 Author

**Mahmoud Mohamed Abd Elaziz**
- .NET Developer
- 📍 Cairo, Egypt
- 📬 [mahmoud.abdalaziz@outlook.com](mailto:mahmoud.abdalaziz@outlook.com)
- [🔗 LinkedIn](https://www.linkedin.com/in/mahmoud-mohamed-abd/) | [💻 GitHub](https://github.com/M7-TROJAN)

---

## 📝 License

Licensed under the MIT License – feel free to use, copy, and improve the code.

