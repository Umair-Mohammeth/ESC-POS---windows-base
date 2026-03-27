# POSWinForms (ESC-POS Windows Base)

A modern, Windows-based Point of Sale (POS) application built with C# and Windows Forms.

## Overview

This project is a comprehensive POS solution that features a premium, modern, and dark-themed UI/UX design. It includes role-based access control and dedicated modules for various operations, ensuring a secure and efficient workflow for different types of users (e.g., Cashiers, Managers, Administrators).

## Features

- **Role-Based Access Control**: Secure login system with role-specific navigation and permissions.
- **Modern UI/UX**: Premium dark-themed aesthetics with consistent styling and an intuitive interface.
- **Core Modules**:
  - **Dashboard**: Central hub displaying key statistics and providing quick navigation.
  - **Cashier**: Streamlined interface for processing sales, handling transactions, and generating receipts.
  - **Inventory**: Tools for managing product catalogs, updating stock levels, and viewing item details.
  - **Manager**: Advanced controls for overseeing operations, managing staff, and viewing reports.
  - **Admin**: System administration, user account management, and overall software configuration.
- **Modular SQLite Database Architecture**: Utilizes separated databases for specialized data organization:
  - `users.db`: Manages user accounts, credentials, and role associations.
  - `products.db`: Stores inventory items, pricing, and stock details.
  - `sales.db`: Keeps records of all transactions and sales history.

## Tech Stack

- **Platform**: Windows Desktop (.NET Windows Forms)
- **Language**: C#
- **Database**: SQLite

## Default Credentials

When running the application for the first time, the following default users are created in the database:

- **Admin** (Role: Admin) — PIN: `1111`
- **Staff** (Role: Cashier) — PIN: `2222`
- **Manager** (Role: Manager) — PIN: `3333`
- **Inventory** (Role: Inventory) — PIN: `4444`

You can use these credentials to log in and test the respective role-based interfaces.

## Getting Started

1. Open the solution in **Visual Studio**.
2. Build the project. This will automatically restore any required NuGet packages (such as `System.Data.SQLite.Core`).
3. Ensure the SQLite database files (`users.db`, `products.db`, `sales.db`) are placed in the application's working directory or their designated paths.
4. Run the project in `Debug` or `Release` mode.