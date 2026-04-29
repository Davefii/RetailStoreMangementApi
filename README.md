# 🛒 Retail Store Management API

A secure, production-ready RESTful API for managing retail store operations — built with **C# ASP.NET Core 8**, **ADO.NET**, and **Microsoft SQL Server**. The API features JWT authentication, role-based access control, refresh token rotation, BCrypt password hashing, and rate limiting.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Security Features](#security-features)
- [Roles & Permissions](#roles--permissions)
- [Getting Started](#getting-started)
- [Database Configuration](#database-configuration)
- [API Endpoints](#api-endpoints)
  - [Authentication](#authentication)
  - [Users](#users)
  - [Suppliers](#suppliers)
  - [Products](#products)
  - [Sales](#sales)
- [Authentication Flow](#authentication-flow)
- [Rate Limiting](#rate-limiting)
- [HTTP Status Codes](#http-status-codes)
- [Project Structure](#project-structure)

---

## Overview

The **Retail Store Management API** provides a complete backend solution for retail operations including inventory management, supplier tracking, sales recording, and user access management. All endpoints are secured with JWT Bearer tokens and enforced role-based authorization.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| Data Access | ADO.NET (raw SQL — no ORM) |
| Database | Microsoft SQL Server |
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt.Net |
| API Documentation | Swagger / OpenAPI |
| Rate Limiting | ASP.NET Core built-in Rate Limiter |

---

## Architecture

The project follows a **3-Layer Architecture** pattern:

```
RetailStoreMangementApi/
│
├── Controllers/              → API entry points (HTTP layer)
│   ├── Login.cs             → Auth endpoints (login, refresh, logout)
│   └── RetailStoreApiController.cs  → Business endpoints
│
├── BussinessLayer/           → Business logic & rules
│   ├── Users.cs
│   ├── Products.cs
│   ├── Sales.cs
│   ├── Supplier.cs
│   └── LoginHistory.cs
│
├── DataAccessLayer/          → Database queries via ADO.NET
│   ├── DataUsers.cs
│   ├── ProductData.cs
│   ├── SalesData.cs
│   ├── SuppliersData.cs
│   └── LoginHistoryData.cs
│
├── DataAccessSettings/       → Connection string configuration
│   └── DataAccessSettings.cs
│
├── DTO/Auth/                 → Data Transfer Objects
│   ├── TokenResponse.cs
│   ├── RefreshRequest.cs
│   └── LogoutRequest.cs
│
└── Program.cs                → App configuration & middleware pipeline
```

---

## Security Features

| Feature | Details |
|---|---|
| **JWT Authentication** | HS256-signed tokens, 30-minute expiry |
| **Refresh Tokens** | Cryptographically random (64 bytes), BCrypt-hashed, 7-day expiry |
| **Password Hashing** | BCrypt with salt — passwords never stored in plaintext |
| **Rate Limiting** | Max 5 requests/minute per IP on auth endpoints |
| **Role-Based Authorization** | Three-tier permission system enforced per endpoint |
| **Ownership Checks** | Non-admin users can only access their own data |
| **Security Logging** | Failed logins and forbidden access attempts are logged with IP |
| **Token Revocation** | Logout revokes the refresh token immediately |

---

## Roles & Permissions

| Role | Permission Value | Capabilities |
|---|---|---|
| **Admin** | `4` | Full access: manage users, suppliers, products, sales |
| **StaffOrCashier** | `2` | View/add suppliers, products, and sales |
| **Viewer** | `1` | Read-only access to suppliers, products, and sales |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server (local or remote instance)
- SQL Server Management Studio (optional)

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/RetailStoreMangementApi.git
cd RetailStoreMangementApi
```

### 2. Configure the Database

Open `DataAccessSettings/DataAccessSettings.cs` and update the connection string:

```csharp
public static string ConnictionString =
    "Server=YOUR_SERVER;Database=RetailDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False;TrustServerCertificate=True;";
```

### 3. Set Up the Database

Run the SQL scripts to create the `RetailDB` database and all required tables (Users, Products, Suppliers, Sales, LoginHistory).

### 4. Restore Dependencies & Run

```bash
dotnet restore
dotnet run
```

### 5. Open Swagger UI

Navigate to:

```
https://localhost:{PORT}/swagger
```

Click **Authorize** in the top-right corner and enter your JWT token:

```
Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## Database Configuration

The connection string is centralized in `DataAccessSettings/DataAccessSettings.cs`.

```csharp
public static string ConnictionString =
    "Server=LocalHost;Database=RetailDB;User Id=sa;Password=YOUR_PASSWORD;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;";
```

> ⚠️ **Important:** Before deploying to production, move credentials to environment variables or `appsettings.json` using `IConfiguration`. Never commit secrets to source control.

---

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `POST` | `/LoginUser` | Login with username & password | No |
| `POST` | `/refresh` | Get a new access token using refresh token | No |
| `POST` | `/logout` | Revoke refresh token and logout | No |

#### Login Request

```http
POST /LoginUser
Content-Type: application/json

{
  "userName": "admin",
  "password": "yourpassword"
}
```

#### Login Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "dGhpcyBpcyBhIHJhbmRvbSByZWZyZXNo..."
}
```

#### Refresh Token Request

```http
POST /refresh
Content-Type: application/json

{
  "userName": "admin",
  "refreshToken": "dGhpcyBpcyBhIHJhbmRvbSByZWZyZXNo..."
}
```

#### Logout Request

```http
POST /logout
Content-Type: application/json

{
  "userName": "admin",
  "refreshToken": "dGhpcyBpcyBhIHJhbmRvbSByZWZyZXNo..."
}
```

---

### Users

Base route: `/api/RetailStoreManagmentApi`

| Method | Endpoint | Description | Required Role |
|---|---|---|---|
| `GET` | `/ListUsers` | Get all users | Admin |
| `GET` | `/FindUserByID/{id}` | Get user by ID | Admin (or self) |
| `POST` | `/AddUser` | Create a new user | Admin |
| `PUT` | `/UpdateUser/{id}` | Update existing user | Admin |
| `DELETE` | `/DeleteUser/{id}` | Delete a user | Admin |
| `POST` | `/ChangePassword/{id}` | Change user password | Admin |

#### Add User Request Body

```json
{
  "id": 0,
  "userName": "john_staff",
  "password": "SecurePass123",
  "permition": 2,
  "isActive": true
}
```

> `permition` values: `1` = Viewer, `2` = StaffOrCashier, `4` = Admin

---

### Suppliers

| Method | Endpoint | Description | Required Role |
|---|---|---|---|
| `GET` | `/Listsuppliers` | Get all suppliers | Admin / Staff / Viewer |
| `GET` | `/FindsuppliersByID/{id}` | Get supplier by ID | Admin / Staff |
| `GET` | `/Findsuppliers/by-name/{name}` | Find supplier by name | Admin / Staff |
| `POST` | `/Addsuppliers` | Add new supplier | Admin |
| `PUT` | `/Updatesuppliers/{id}` | Update supplier | Admin |
| `DELETE` | `/Deletesuppliers/{id}` | Delete supplier | Admin |

#### Add Supplier Request Body

```json
{
  "id": 0,
  "supplier_Name": "ABC Electronics",
  "contact_Person": "John Doe",
  "phone_Number": "+213555123456",
  "address": "123 Main Street, Algiers",
  "email": "contact@abc-electronics.com",
  "status": true
}
```

---

### Products

| Method | Endpoint | Description | Required Role |
|---|---|---|---|
| `GET` | `/ListProducts` | Get all products | Admin / Staff / Viewer |
| `GET` | `/FindproductsByID/{id}` | Get product by ID | Admin / Staff |
| `GET` | `/Findproducts/by-name/{name}` | Find product by name | Admin / Staff |
| `POST` | `/Addproducts` | Add new product | Admin / Staff |
| `PUT` | `/Updateproducts/{id}` | Update product | Admin |
| `DELETE` | `/Deleteproducts/{id}` | Delete product | Admin / Staff |

#### Add Product Request Body

```json
{
  "id": 0,
  "name": "Laptop Dell XPS",
  "price": 1299.99,
  "quantity": 50,
  "supplier_ID": 3
}
```

---

### Sales

| Method | Endpoint | Description | Required Role |
|---|---|---|---|
| `GET` | `/Listsales` | Get all sales records | Admin / Staff / Viewer |
| `GET` | `/FindsalesByID/{id}` | Get sale by ID | Admin / Staff / Viewer |
| `POST` | `/AddSale` | Record a new sale | Admin / Staff |
| `GET` | `/sales/total` | Get total number of sales | All authenticated |

#### Add Sale Request Body

```json
{
  "id": 0,
  "product_ID": 5,
  "quantity": 2,
  "saleDate": "2026-04-29T10:00:00",
  "user_ID": 1,
  "totalPrice": 2599.98
}
```

---

## Authentication Flow

```
Client                          API
  │                              │
  │── POST /LoginUser ──────────>│
  │                              │  Verify credentials (BCrypt)
  │                              │  Generate JWT (30 min)
  │                              │  Generate Refresh Token (7 days)
  │<── { token, refreshToken } ──│
  │                              │
  │── GET /api/... ─────────────>│
  │   Authorization: Bearer JWT  │  Validate JWT
  │<── 200 OK ──────────────────│
  │                              │
  │  (JWT expires after 30 min)  │
  │                              │
  │── POST /refresh ────────────>│
  │   { userName, refreshToken } │  Verify BCrypt hash
  │                              │  Issue new JWT + new Refresh Token
  │<── { token, refreshToken } ──│
  │                              │
  │── POST /logout ─────────────>│
  │   { userName, refreshToken } │  Revoke refresh token
  │<── 200 OK ──────────────────│
```

---

## Rate Limiting

Auth endpoints (`/LoginUser`, `/refresh`) are protected against brute-force attacks:

- **Limit:** 5 requests per minute per IP address
- **Behavior:** Returns `429 Too Many Requests` with the message: `"Too many login attempts. Please try again later."`
- **Scope:** Applied per client IP address using a Fixed Window strategy

---

## HTTP Status Codes

| Code | Meaning |
|---|---|
| `200 OK` | Request succeeded |
| `201 Created` | Resource created successfully |
| `400 Bad Request` | Invalid input data |
| `401 Unauthorized` | Missing or invalid JWT token |
| `403 Forbidden` | Valid token but insufficient permissions |
| `404 Not Found` | Resource does not exist |
| `429 Too Many Requests` | Rate limit exceeded |

---

## Project Structure Summary

```
RetailStoreMangementApi-main/
├── Program.cs                          ← App entry point & middleware pipeline
├── Controllers/
│   ├── Login.cs                        ← JWT auth, refresh, logout
│   └── RetailStoreApiController.cs     ← All CRUD endpoints
├── BussinessLayer/
│   ├── Users.cs                        ← User logic + BCrypt
│   ├── Products.cs
│   ├── Sales.cs
│   ├── Supplier.cs
│   └── LoginHistory.cs
├── DataAccessLayer/
│   ├── DataUsers.cs                    ← ADO.NET SQL queries
│   ├── ProductData.cs
│   ├── SalesData.cs
│   ├── SuppliersData.cs
│   └── LoginHistoryData.cs
├── DataAccessSettings/
│   └── DataAccessSettings.cs           ← Connection string
└── DTO/Auth/
    ├── TokenResponse.cs
    ├── RefreshRequest.cs
    └── LogoutRequest.cs
```

---

## License

This project is intended for educational and demonstration purposes.

---

> Built with C# · ADO.NET · ASP.NET Core 8 · Microsoft SQL Server
