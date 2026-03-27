using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace POSWinForms.Data
{
    public class DatabaseContext
    {
        private static DatabaseContext _instance;
        public static DatabaseContext Instance => _instance ??= new DatabaseContext();

        public SQLiteConnection Connection { get; }

        private DatabaseContext()
        {
            // Ensure Data directory exists; database file stored there
            var dataDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!System.IO.Directory.Exists(dataDir)) System.IO.Directory.CreateDirectory(dataDir);

            var dbPath = System.IO.Path.Combine(dataDir, "pos.db");
            var cs = $"Data Source={dbPath};Version=3;";
            Connection = new SQLiteConnection(cs);
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            try
            {
                Connection.Open();
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        pin TEXT NOT NULL,
                        role TEXT NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS products (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        price REAL NOT NULL,
                        stock_quantity INTEGER NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS sales (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        product_name TEXT NOT NULL,
                        quantity INTEGER NOT NULL,
                        total_price REAL NOT NULL,
                        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                    );
                ";
                cmd.ExecuteNonQuery();
                // Granular Seeding: Ensure each required default user exists
                string[] seedList = 
                {
                    "INSERT INTO users (name, pin, role) SELECT 'Admin','1111','Admin' WHERE NOT EXISTS (SELECT 1 FROM users WHERE pin='1111')",
                    "INSERT INTO users (name, pin, role) SELECT 'Staff','2222','Cashier' WHERE NOT EXISTS (SELECT 1 FROM users WHERE pin='2222')",
                    "INSERT INTO users (name, pin, role) SELECT 'Manager','3333','Manager' WHERE NOT EXISTS (SELECT 1 FROM users WHERE pin='3333')",
                    "INSERT INTO users (name, pin, role) SELECT 'Inventory','4444','Inventory' WHERE NOT EXISTS (SELECT 1 FROM users WHERE pin='4444')"
                };
                foreach (var sql in seedList)
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }

                // Granular Seeding: Ensure some default products exist
                string[] productSeeds = 
                {
                    "INSERT INTO products (name, price, stock_quantity) SELECT 'Espresso Beans 500g', 18.50, 45 WHERE NOT EXISTS (SELECT 1 FROM products WHERE name='Espresso Beans 500g')",
                    "INSERT INTO products (name, price, stock_quantity) SELECT 'Whole Milk 1L', 2.10, 12 WHERE NOT EXISTS (SELECT 1 FROM products WHERE name='Whole Milk 1L')",
                    "INSERT INTO products (name, price, stock_quantity) SELECT 'Croissants (6pk)', 12.00, 4 WHERE NOT EXISTS (SELECT 1 FROM products WHERE name='Croissants (6pk)')"
                };
                foreach (var sql in productSeeds)
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
                Connection.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
