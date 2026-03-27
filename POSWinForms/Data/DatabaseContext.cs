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
                // Seed default data if empty
                cmd.CommandText = "SELECT COUNT(*) FROM users";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    cmd.CommandText = "INSERT INTO users (name, pin, role) VALUES ('Admin','1111','Admin'), ('Staff','2222','Cashier')";
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
