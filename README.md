# 📚 LitraLand – Complete Library Management & Community System

LitraLand is a robust and fully-featured web platform designed for managing libraries and engaging reading communities. It provides tools for both administrators and regular users to interact with books, authors, and with each other in a clean, structured, and modern environment.

---

## 🚀 Project Highlights

- 🔐 Role-based access control (Admin / Reader)
- 📖 Explore books by author, category, or keywords
- 🧾 Borrowing system with due dates and penalties
- ✍️ Reviews & ratings for books
- 🧑‍💼 Admin dashboard with full CRUD on:
  - Books
  - Authors
  - Categories
  - Publishers
  - Users
- 🛠️ Clean layered architecture with separation of concerns
- 🧹 Form validation & anti-spam handling
- 🌐 Localization-ready (Arabic + English support)

---

## 🧠 Why LitraLand?

This is not just a CRUD system. LitraLand was built as a real-world project to demonstrate:
- ✅ Clean, scalable architecture in ASP.NET Core MVC
- ✅ Real separation between Core, Infrastructure, and Presentation
- ✅ Handling of real-world problems: validation, authorization, extensibility
- ✅ Ready-to-extend system for schools, communities, or commercial libraries

---

## 🏗️ Architecture Overview

```

LitraLand/
├── LitraLand.Web/           → MVC presentation layer (views, controllers)
├── LitraLand.Core/          → Entities, ViewModels, interfaces
├── LitraLand.Services/      → Business logic (implementations of I...Service)
├── LitraLand.Infrastructure/→ EF Core, Repositories, DbContext
└── Shared/                  → Extensions, Helpers, Custom logic

````

🧠 Patterns Used:
- Repository Pattern + Unit of Work
- AutoMapper for ViewModel mapping
- FluentValidation for model validation
- Extension methods for cleaner code
- Dependency Injection throughout

---

## 🛠 Tech Stack

| Layer       | Tech |
|-------------|------|
| Backend     | ASP.NET Core MVC 7.0 |
| Database    | SQL Server |
| ORM         | Entity Framework Core |
| Frontend    | Razor Views, Bootstrap 5, jQuery |
| Auth        | ASP.NET Identity |
| Others      | AutoMapper, FluentValidation, Layered Architecture |

---

## 🧪 Features in Detail

### 🔹 User
- View all available books
- Filter/search by keyword, author, or category
- Borrow/return books (with automatic tracking)
- Leave reviews, rate books
- Edit profile and track borrow history

### 🔸 Admin
- Full dashboard to manage:
  - Books, authors, categories, publishers
  - Users: lock/unlock, delete, assign roles
- Access logs and activity tracking
- Validate reviews, moderate content
- Export data (CSV/PDF support ready for plug-in)

---

## 🖼 UI Sample (Optional — Replace with screenshots)

> Add screenshots like:
> ![Homepage Screenshot](screenshots/homepage.png)
> ![Admin Panel](screenshots/admin-dashboard.png)

---

## ⚙️ Getting Started

### 1️⃣ Clone the repository

```bash
git clone https://github.com/M7-TROJAN/LitraLand.git
cd LitraLand
````

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

🛑 You can register new users and assign roles from the admin panel.

---

## 💡 What Makes This Stand Out?

* ✅ Real-world multi-role use case
* ✅ Clean, well-structured backend code
* ✅ Validations with FluentValidation (not just DataAnnotations)
* ✅ Extensible and ready for deployment
* ✅ Focus on maintainability and code readability

---

## 🧑‍💻 Author

**Mahmoud Mohamed Abd Elaziz**
.NET Developer
📍 Cairo, Egypt
📬 [mahmoud.abdalaziz@outlook.com](mailto:mahmoud.abdalaziz@outlook.com)
[🔗 LinkedIn](https://www.linkedin.com/in/mahmoud-mohamed-abd/) | [💻 GitHub](https://github.com/M7-TROJAN)

---

## 📝 License

Licensed under the MIT License – feel free to use, copy, and improve the code.
