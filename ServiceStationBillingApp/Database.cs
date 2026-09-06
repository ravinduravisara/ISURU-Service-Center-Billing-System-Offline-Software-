using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public static class Database
    {
        private static readonly string DbPath = Environment.GetEnvironmentVariable("BILLING_DB_PATH")
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "billing.db");
        public static string ConnectionString => $"Data Source={DbPath}";

        public static string GetDbPath() => DbPath;

        public static void Ensure()
        {
            if (!File.Exists(DbPath))
            {
                using (File.Create(DbPath)) { }
            }

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            string sql = @"
                CREATE TABLE IF NOT EXISTS customers (
                    id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    name        TEXT NOT NULL,
                    phone       TEXT
                );

                CREATE TABLE IF NOT EXISTS vehicles (
                    id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    customer_id  INTEGER NOT NULL,
                    vehicle_no   TEXT NOT NULL,
                    vehicle_type TEXT,
                    FOREIGN KEY (customer_id) REFERENCES customers(id)
                );

                CREATE TABLE IF NOT EXISTS invoices (
                    id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    bill_no         TEXT NOT NULL,
                    customer_id     INTEGER NOT NULL,
                    vehicle_id      INTEGER NOT NULL,
                    date_time       TEXT NOT NULL,
                    total_amount    REAL NOT NULL,
                    payment_method  TEXT,
                    payment_status  TEXT,
                    FOREIGN KEY (customer_id) REFERENCES customers(id),
                    FOREIGN KEY (vehicle_id) REFERENCES vehicles(id)
                );

                CREATE TABLE IF NOT EXISTS invoice_items (
                    id            INTEGER PRIMARY KEY AUTOINCREMENT,
                    invoice_id    INTEGER NOT NULL,
                    service       TEXT,
                    description   TEXT,
                    qty           REAL,
                    rate          REAL,
                    amount        REAL,
                    item_id       TEXT,
                    buying_price  REAL NOT NULL DEFAULT 0,
                    FOREIGN KEY (invoice_id) REFERENCES invoices(id)
                );

                CREATE TABLE IF NOT EXISTS inventory (
                    id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    item_id     TEXT,
                    item_name   TEXT NOT NULL,
                    brand       TEXT,
                    quantity    REAL NOT NULL DEFAULT 0,
                    unit_price  REAL NOT NULL DEFAULT 0,
                    updated_at  TEXT NOT NULL,
                    supplier_code TEXT,
                    payment_method TEXT
                );

                CREATE TABLE IF NOT EXISTS suppliers (
                    id             INTEGER PRIMARY KEY AUTOINCREMENT,
                    supplier_code  TEXT UNIQUE,
                    name           TEXT NOT NULL,
                    contact        TEXT,
                    cash_total     REAL NOT NULL DEFAULT 0,
                    credit_total   REAL NOT NULL DEFAULT 0,
                    invoice        TEXT
                );

                -- Track per-stock purchase movements to support accurate supplier cash/credit totals
                CREATE TABLE IF NOT EXISTS stock_movements (
                    id             INTEGER PRIMARY KEY AUTOINCREMENT,
                    item_id        TEXT,
                    supplier_code  TEXT,
                    quantity       REAL NOT NULL DEFAULT 0,
                    unit_price     REAL NOT NULL DEFAULT 0,
                    payment_method TEXT,
                    moved_at       TEXT NOT NULL
                );

                -- Supply invoices: one supplier can have multiple invoices
                CREATE TABLE IF NOT EXISTS supply_invoices (
                    id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    invoice_no      TEXT NOT NULL,
                    supplier_code   TEXT NOT NULL,
                    item_id         TEXT NOT NULL,
                    buying_price    REAL NOT NULL DEFAULT 0,
                    quantity        REAL NOT NULL DEFAULT 0,
                    payment_status  TEXT NOT NULL DEFAULT 'Cash',
                    created_at      TEXT NOT NULL,
                    cheque_no       TEXT,
                    branch_code     TEXT,
                    bank_code       TEXT,
                    value_date      TEXT,
                    settled_at      TEXT
                );

                CREATE TABLE IF NOT EXISTS services (
                    id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    category    TEXT NOT NULL DEFAULT 'Others',
                    name        TEXT NOT NULL,
                    price       REAL NOT NULL DEFAULT 0,
                    description TEXT,
                    UNIQUE(category, name)
                );
            ";

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            // Seed default services with proper categories so dropdown filtering works
            SeedDefaultServices(conn);

            // Ensure optional/migrated columns and indexes exist
            try
            {
                using var addCol = conn.CreateCommand();
                addCol.CommandText = "ALTER TABLE invoices ADD COLUMN payment_settled_at TEXT";
                addCol.ExecuteNonQuery();
            }
            catch { }

            foreach (var column in new[] { "subtotal_amount REAL NOT NULL DEFAULT 0", "discount_amount REAL NOT NULL DEFAULT 0" })
            {
                try
                {
                    using var addInvoiceColumn = conn.CreateCommand();
                    addInvoiceColumn.CommandText = $"ALTER TABLE invoices ADD COLUMN {column}";
                    addInvoiceColumn.ExecuteNonQuery();
                }
                catch { }
            }

            try
            {
                // Ensure 'invoice' and 'cheque_total' columns exist
                bool hasInvoice = false;
                bool hasChequeTotal = false;
                using (var check = conn.CreateCommand())
                {
                    check.CommandText = "PRAGMA table_info(suppliers);";
                    using var reader = check.ExecuteReader();
                    while (reader.Read())
                    {
                        var colName = reader.GetString(1);
                        if (string.Equals(colName, "invoice", StringComparison.OrdinalIgnoreCase)) hasInvoice = true;
                        if (string.Equals(colName, "cheque_total", StringComparison.OrdinalIgnoreCase)) hasChequeTotal = true;
                    }
                }
                if (!hasInvoice)
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = "ALTER TABLE suppliers ADD COLUMN invoice TEXT";
                    alter.ExecuteNonQuery();
                }
                if (!hasChequeTotal)
                {
                    using var alterCheque = conn.CreateCommand();
                    alterCheque.CommandText = "ALTER TABLE suppliers ADD COLUMN cheque_total REAL NOT NULL DEFAULT 0";
                    alterCheque.ExecuteNonQuery();
                }
            }
            catch { }

            try
            {
                // Add missing inventory columns if needed
                bool hasItemId = false;
                bool hasBrand = false;
                bool hasSupplierCode = false;
                bool hasPaymentMethod = false;
                using (var check = conn.CreateCommand())
                {
                    check.CommandText = "PRAGMA table_info(inventory);";
                    using var reader = check.ExecuteReader();
                    while (reader.Read())
                    {
                        var colName = reader.GetString(1);
                        if (string.Equals(colName, "item_id", StringComparison.OrdinalIgnoreCase)) hasItemId = true;
                        if (string.Equals(colName, "brand", StringComparison.OrdinalIgnoreCase)) hasBrand = true;
                        if (string.Equals(colName, "supplier_code", StringComparison.OrdinalIgnoreCase)) hasSupplierCode = true;
                        if (string.Equals(colName, "payment_method", StringComparison.OrdinalIgnoreCase)) hasPaymentMethod = true;
                    }
                }
                if (!hasItemId)
                {
                    using var alter1 = conn.CreateCommand();
                    alter1.CommandText = "ALTER TABLE inventory ADD COLUMN item_id TEXT";
                    alter1.ExecuteNonQuery();
                }
                if (!hasBrand)
                {
                    using var alter2 = conn.CreateCommand();
                    alter2.CommandText = "ALTER TABLE inventory ADD COLUMN brand TEXT";
                    alter2.ExecuteNonQuery();
                }
                if (!hasSupplierCode)
                {
                    using var alter3 = conn.CreateCommand();
                    alter3.CommandText = "ALTER TABLE inventory ADD COLUMN supplier_code TEXT";
                    alter3.ExecuteNonQuery();
                }
                if (!hasPaymentMethod)
                {
                    using var alter4 = conn.CreateCommand();
                    alter4.CommandText = "ALTER TABLE inventory ADD COLUMN payment_method TEXT";
                    alter4.ExecuteNonQuery();
                }
                // Ensure unique index on item_id for fast lookups
                using var idx = conn.CreateCommand();
                idx.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS ux_inventory_item_id ON inventory(item_id)";
                idx.ExecuteNonQuery();
            }
            catch { }

            try
            {
                // Add missing columns to invoice_items if needed
                bool iiHasItemId = false;
                bool iiHasBuyingPrice = false;
                using (var check2 = conn.CreateCommand())
                {
                    check2.CommandText = "PRAGMA table_info(invoice_items);";
                    using var reader2 = check2.ExecuteReader();
                    while (reader2.Read())
                    {
                        var colName = reader2.GetString(1);
                        if (string.Equals(colName, "item_id", StringComparison.OrdinalIgnoreCase)) iiHasItemId = true;
                        if (string.Equals(colName, "buying_price", StringComparison.OrdinalIgnoreCase)) iiHasBuyingPrice = true;
                    }
                }
                if (!iiHasItemId)
                {
                    using var alter3 = conn.CreateCommand();
                    alter3.CommandText = "ALTER TABLE invoice_items ADD COLUMN item_id TEXT";
                    alter3.ExecuteNonQuery();
                }
                if (!iiHasBuyingPrice)
                {
                    using var alter4 = conn.CreateCommand();
                    alter4.CommandText = "ALTER TABLE invoice_items ADD COLUMN buying_price REAL NOT NULL DEFAULT 0";
                    alter4.ExecuteNonQuery();
                    // Backfill existing invoice_items from current inventory buying prices
                    using var backfill = conn.CreateCommand();
                    backfill.CommandText = @"UPDATE invoice_items SET buying_price = COALESCE(
                        (SELECT inv.buying_price FROM inventory inv WHERE inv.item_id = invoice_items.item_id), 0)
                        WHERE TRIM(COALESCE(item_id,'')) <> ''";
                    backfill.ExecuteNonQuery();
                }
            }
            catch { }

            try
            {
                // Ensure indexes for stock_movements
                using var idx1 = conn.CreateCommand();
                idx1.CommandText = "CREATE INDEX IF NOT EXISTS ix_stock_movements_supplier ON stock_movements(supplier_code)";
                idx1.ExecuteNonQuery();

                using var idx2 = conn.CreateCommand();
                idx2.CommandText = "CREATE INDEX IF NOT EXISTS ix_stock_movements_item ON stock_movements(item_id)";
                idx2.ExecuteNonQuery();
            }
            catch { }

            try
            {
                // Add buying_price column to inventory
                bool hasBuyingPrice = false;
                using (var check = conn.CreateCommand())
                {
                    check.CommandText = "PRAGMA table_info(inventory);";
                    using var reader = check.ExecuteReader();
                    while (reader.Read())
                    {
                        var colName = reader.GetString(1);
                        if (string.Equals(colName, "buying_price", StringComparison.OrdinalIgnoreCase)) hasBuyingPrice = true;
                    }
                }
                if (!hasBuyingPrice)
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = "ALTER TABLE inventory ADD COLUMN buying_price REAL NOT NULL DEFAULT 0";
                    alter.ExecuteNonQuery();
                }
            }
            catch { }

            try
            {
                // Add buying_price column to stock_movements
                bool smHasBuyingPrice = false;
                using (var check = conn.CreateCommand())
                {
                    check.CommandText = "PRAGMA table_info(stock_movements);";
                    using var reader = check.ExecuteReader();
                    while (reader.Read())
                    {
                        var colName = reader.GetString(1);
                        if (string.Equals(colName, "buying_price", StringComparison.OrdinalIgnoreCase)) smHasBuyingPrice = true;
                    }
                }
                if (!smHasBuyingPrice)
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = "ALTER TABLE stock_movements ADD COLUMN buying_price REAL NOT NULL DEFAULT 0";
                    alter.ExecuteNonQuery();
                }
            }
            catch { }

            try
            {
                // Add settled_at column to stock_movements for credit/cheque settlement tracking
                bool hasSettledAt = false;
                using (var check = conn.CreateCommand())
                {
                    check.CommandText = "PRAGMA table_info(stock_movements);";
                    using var reader = check.ExecuteReader();
                    while (reader.Read())
                    {
                        var colName = reader.GetString(1);
                        if (string.Equals(colName, "settled_at", StringComparison.OrdinalIgnoreCase)) hasSettledAt = true;
                    }
                }
                if (!hasSettledAt)
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = "ALTER TABLE stock_movements ADD COLUMN settled_at TEXT";
                    alter.ExecuteNonQuery();
                }
            }
            catch { }

            // Add cheque detail columns to supply_invoices if missing
            try
            {
                bool hasChequeNo = false, hasBranchCode = false, hasBankCode = false, hasValueDate = false;
                using (var check = conn.CreateCommand())
                {
                    check.CommandText = "PRAGMA table_info(supply_invoices);";
                    using var reader = check.ExecuteReader();
                    while (reader.Read())
                    {
                        var colName = reader.GetString(1);
                        if (string.Equals(colName, "cheque_no", StringComparison.OrdinalIgnoreCase)) hasChequeNo = true;
                        if (string.Equals(colName, "branch_code", StringComparison.OrdinalIgnoreCase)) hasBranchCode = true;
                        if (string.Equals(colName, "bank_code", StringComparison.OrdinalIgnoreCase)) hasBankCode = true;
                        if (string.Equals(colName, "value_date", StringComparison.OrdinalIgnoreCase)) hasValueDate = true;
                    }
                }
                if (!hasChequeNo) { using var a = conn.CreateCommand(); a.CommandText = "ALTER TABLE supply_invoices ADD COLUMN cheque_no TEXT"; a.ExecuteNonQuery(); }
                if (!hasBranchCode) { using var a = conn.CreateCommand(); a.CommandText = "ALTER TABLE supply_invoices ADD COLUMN branch_code TEXT"; a.ExecuteNonQuery(); }
                if (!hasBankCode) { using var a = conn.CreateCommand(); a.CommandText = "ALTER TABLE supply_invoices ADD COLUMN bank_code TEXT"; a.ExecuteNonQuery(); }
                if (!hasValueDate) { using var a = conn.CreateCommand(); a.CommandText = "ALTER TABLE supply_invoices ADD COLUMN value_date TEXT"; a.ExecuteNonQuery(); }

                // Also add settled_at
                bool hasSettledAtSI = false;
                using (var check2 = conn.CreateCommand())
                {
                    check2.CommandText = "PRAGMA table_info(supply_invoices);";
                    using var r2 = check2.ExecuteReader();
                    while (r2.Read())
                    {
                        if (string.Equals(r2.GetString(1), "settled_at", StringComparison.OrdinalIgnoreCase)) hasSettledAtSI = true;
                    }
                }
                if (!hasSettledAtSI) { using var a = conn.CreateCommand(); a.CommandText = "ALTER TABLE supply_invoices ADD COLUMN settled_at TEXT"; a.ExecuteNonQuery(); }
            }
            catch { }
        }

        public static SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static void PurgeSuppliersData()
        {
            using var conn = OpenConnection();
            using var tx = conn.BeginTransaction();

            using (var clearMovements = conn.CreateCommand())
            {
                clearMovements.Transaction = tx;
                clearMovements.CommandText = @"DELETE FROM stock_movements WHERE TRIM(COALESCE(supplier_code,'')) <> ''";
                clearMovements.ExecuteNonQuery();
            }

            using (var clearInventory = conn.CreateCommand())
            {
                clearInventory.Transaction = tx;
                clearInventory.CommandText = @"UPDATE inventory SET supplier_code = NULL";
                clearInventory.ExecuteNonQuery();
            }

            using (var clearSuppliers = conn.CreateCommand())
            {
                clearSuppliers.Transaction = tx;
                clearSuppliers.CommandText = @"DELETE FROM suppliers";
                clearSuppliers.ExecuteNonQuery();
            }

            tx.Commit();
        }

        public static void PurgeAllData()
        {
            using var conn = OpenConnection();
            using var tx = conn.BeginTransaction();

            // Delete in dependency order to avoid FK issues
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;

                cmd.CommandText = @"DELETE FROM invoice_items";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"DELETE FROM invoices";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"DELETE FROM vehicles";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"DELETE FROM customers";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"DELETE FROM stock_movements";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"DELETE FROM suppliers";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"DELETE FROM inventory";
                cmd.ExecuteNonQuery();
            }

            // Reset AUTOINCREMENT counters
            try
            {
                using var reset = conn.CreateCommand();
                reset.Transaction = tx;
                reset.CommandText = @"DELETE FROM sqlite_sequence";
                reset.ExecuteNonQuery();
            }
            catch { /* sqlite_sequence may not exist; ignore */ }

            tx.Commit();
        }

        // ========== APP SETTINGS ==========

        public static void EnsureSettingsTable()
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS app_settings (
                    key   TEXT PRIMARY KEY,
                    value TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();

            // Seed default credentials if not present
            SeedDefaultSetting(conn, "admin_username", "Isuru@6800");
            SeedDefaultSetting(conn, "admin_password", "Isuru@service");
            SeedDefaultSetting(conn, "user_username", "User@123");
            SeedDefaultSetting(conn, "user_password", "User@service");
        }

        private static void SeedDefaultSetting(SqliteConnection conn, string key, string value)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO app_settings (key, value) VALUES ($k, $v)";
            cmd.Parameters.AddWithValue("$k", key);
            cmd.Parameters.AddWithValue("$v", value);
            cmd.ExecuteNonQuery();
        }

        public static string GetSetting(string key, string defaultValue = "")
        {
            try
            {
                EnsureSettingsTable();
                using var conn = OpenConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT value FROM app_settings WHERE key = $k";
                cmd.Parameters.AddWithValue("$k", key);
                var result = cmd.ExecuteScalar();
                return result is string s ? s : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Seeds default services with proper vehicle-type categories so the
        /// service dropdown in BillingForm filters correctly by vehicle type.
        /// Uses INSERT OR IGNORE so existing rows are never overwritten.
        /// </summary>
        private static void SeedDefaultServices(SqliteConnection conn)
        {
            try
            {
                var defaults = new (string Category, string Name)[]
                {
                    ("Car", "Engine Oil Change"),
                    ("Car", "Full Service / Body Wash"),
                    ("Car", "Interior Cleaning"),
                    ("Car", "Vacuum"),
                    ("Car", "Tune-up"),
                    ("Car", "Filters Replacement"),
                    ("Car", "Labor Charges"),
                    ("Car", "Custom Item"),
                    ("Bike", "Engine Oil Change"),
                    ("Bike", "Tune-up"),
                    ("Bike", "Custom Item"),
                    ("Van", "Engine Oil Change"),
                    ("Van", "Full Service / Body Wash"),
                    ("Van", "Tune-up"),
                    ("Van", "Custom Item"),
                    ("Bus", "Engine Oil Change"),
                    ("Bus", "Full Service / Body Wash"),
                    ("Bus", "Tune-up"),
                    ("Bus", "Custom Item"),
                    ("Lorry", "Engine Oil Change"),
                    ("Lorry", "Full Service / Body Wash"),
                    ("Lorry", "Tune-up"),
                    ("Lorry", "Custom Item"),
                    ("Others", "Custom Item"),
                };

                using var tx = conn.BeginTransaction();
                foreach (var (cat, name) in defaults)
                {
                    using var ins = conn.CreateCommand();
                    ins.CommandText = "INSERT OR IGNORE INTO services (category, name, price) VALUES ($cat, $n, 0)";
                    ins.Parameters.AddWithValue("$cat", cat);
                    ins.Parameters.AddWithValue("$n", name);
                    ins.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch { /* ignore seeding errors */ }

            // Separate step: reassign old "Others" services to the correct
            // vehicle-type categories so filtering works properly.
            try
            {
                var reassign = new (string Name, string CorrectCategory)[]
                {
                    ("Engine Oil Change", "Car"),
                    ("Full Service / Body Wash", "Car"),
                    ("Interior Cleaning", "Car"),
                    ("Vacuum", "Car"),
                    ("Tune-up", "Car"),
                    ("Filters Replacement", "Car"),
                    ("Labor Charges", "Car"),
                };

                foreach (var (name, cat) in reassign)
                {
                    // Delete "Others" row if a vehicle-specific row already exists
                    using var del = conn.CreateCommand();
                    del.CommandText = @"DELETE FROM services WHERE category = 'Others' AND name = $n
                        AND EXISTS (SELECT 1 FROM services WHERE category <> 'Others' AND name = $n)";
                    del.Parameters.AddWithValue("$n", name);
                    del.ExecuteNonQuery();
                }
            }
            catch { /* ignore cleanup errors */ }
        }
    }
}
