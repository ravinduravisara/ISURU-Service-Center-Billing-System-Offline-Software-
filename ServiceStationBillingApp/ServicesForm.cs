using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public partial class ServicesForm : Form
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly string _userRole;
        private long? _selectedId;

        public ServicesForm(string userRole = "Admin")
        {
            InitializeComponent();

            // Use centralized Database helpers
            _dbPath = Database.GetDbPath();
            _connectionString = Database.ConnectionString;
            _userRole = string.IsNullOrWhiteSpace(userRole) ? "Admin" : userRole;

            HookEvents();
            EnsureServicesTable();
            TryMigrateServicesFromOldDb();
            SeedFromInvoiceItemsIfEmpty();
            EnsureDefaultServices();
            LoadServices();
            ApplyRoleRestrictions();
        }

        private void ApplyRoleRestrictions()
        {
            bool isUser = string.Equals(_userRole, "User", StringComparison.OrdinalIgnoreCase);
            if (isUser)
            {
                btnAdd.Enabled = false;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void HookEvents()
        {
            dgvServices.SelectionChanged += DgvServices_SelectionChanged;
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (_, __) => ClearInputs();
            btnClose.Click += (_, __) => this.Close();
            txtPrice.KeyPress += NumericTextBox_KeyPress;
            cmbCategory.SelectedIndexChanged += (_, __) => LoadServices();

            // Keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.A) { dgvServices.SelectAll(); e.Handled = true; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Delete && !txtServiceName.Focused && !txtPrice.Focused && !txtDescription.Focused) { BtnDelete_Click(s, e); e.Handled = true; }
                else if (e.KeyCode == Keys.Escape) { this.Close(); e.Handled = true; }
            };
        }

        private void EnsureServicesTable()
        {
            using var conn = Database.OpenConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS services (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    category TEXT NOT NULL DEFAULT 'Others',
                    name TEXT NOT NULL,
                    price REAL NOT NULL DEFAULT 0,
                    description TEXT,
                    UNIQUE(category, name)
                );";
            cmd.ExecuteNonQuery();

            // Migration: add category column if missing
            try
            {
                using var pragma = conn.CreateCommand();
                pragma.CommandText = "PRAGMA table_info(services)";
                using var r = pragma.ExecuteReader();
                bool hasCategory = false;
                while (r.Read())
                {
                    if (string.Equals(r[1]?.ToString(), "category", StringComparison.OrdinalIgnoreCase))
                    { hasCategory = true; break; }
                }
                if (!hasCategory)
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = "ALTER TABLE services ADD COLUMN category TEXT NOT NULL DEFAULT 'Others'";
                    alter.ExecuteNonQuery();
                }
            }
            catch { /* column already exists */ }
        }

        private void TryMigrateServicesFromOldDb()
        {
            try
            {
                string oldDb = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "billing.db");
                if (!File.Exists(oldDb)) return;

                using var connNew = Database.OpenConnection();
                connNew.Open();
                // If new already has rows, skip
                using (var checkNew = connNew.CreateCommand())
                {
                    checkNew.CommandText = "SELECT COUNT(1) FROM services";
                    long cnt = (long)(checkNew.ExecuteScalar() ?? 0);
                    if (cnt > 0) return;
                }

                using var connOld = new SqliteConnection($"Data Source={oldDb}");
                connOld.Open();
                // Ensure old has services table
                using (var pragma = connOld.CreateCommand())
                {
                    pragma.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='services'";
                    var exists = pragma.ExecuteScalar();
                    if (exists == null) return;
                }

                using (var read = connOld.CreateCommand())
                {
                    read.CommandText = "SELECT name, price, description FROM services";
                    using var rdr = read.ExecuteReader();
                    using var tx = connNew.BeginTransaction();
                    while (rdr.Read())
                    {
                        string name = rdr.IsDBNull(0) ? "" : rdr.GetString(0);
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        double price = rdr.IsDBNull(1) ? 0 : rdr.GetDouble(1);
                        string? desc = rdr.IsDBNull(2) ? null : rdr.GetString(2);
                        using var ins = connNew.CreateCommand();
                        ins.CommandText = "INSERT OR IGNORE INTO services (name, price, description) VALUES ($n,$p,$d)";
                        ins.Parameters.AddWithValue("$n", name);
                        ins.Parameters.AddWithValue("$p", price);
                        ins.Parameters.AddWithValue("$d", (object?)desc ?? DBNull.Value);
                        ins.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
            catch { /* ignore migration errors */ }
        }

        private void SeedFromInvoiceItemsIfEmpty()
        {
            try
            {
                using var conn = Database.OpenConnection();
                conn.Open();
                using (var check = conn.CreateCommand())
                {
                    check.CommandText = "SELECT COUNT(1) FROM services";
                    long cnt = (long)(check.ExecuteScalar() ?? 0);
                    if (cnt > 0) return; // already has services
                }

                // Seed from invoice_items distinct services with a representative price
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT service, MAX(rate) AS price
                                     FROM invoice_items
                                     WHERE service IS NOT NULL AND TRIM(service) <> ''
                                     GROUP BY service";
                var list = new System.Collections.Generic.List<(string Name, double Price)>();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string name = rdr.IsDBNull(0) ? "" : rdr.GetString(0);
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        double price = rdr.IsDBNull(1) ? 0 : rdr.GetDouble(1);
                        list.Add((name, price));
                    }
                }

                using var tx = conn.BeginTransaction();
                foreach (var it in list)
                {
                    using var ins = conn.CreateCommand();
                    ins.CommandText = "INSERT OR IGNORE INTO services (name, price) VALUES ($n,$p)";
                    ins.Parameters.AddWithValue("$n", it.Name);
                    ins.Parameters.AddWithValue("$p", it.Price);
                    ins.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch { /* ignore seeding errors */ }
        }

        private void EnsureDefaultServices()
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
                    ("Others", "Custom Item")
                };

                using var conn = Database.OpenConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();
                foreach (var (cat, name) in defaults)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT OR IGNORE INTO services (category, name, price) VALUES ($cat, $n, $p)";
                    cmd.Parameters.AddWithValue("$cat", cat);
                    cmd.Parameters.AddWithValue("$n", name);
                    cmd.Parameters.AddWithValue("$p", 0);
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch { /* ignore default seeding errors */ }
        }

        private void LoadServices()
        {
            dgvServices.Columns.Clear();
            dgvServices.Rows.Clear();

            dgvServices.Columns.Add("Id", "Id");
            dgvServices.Columns["Id"].Visible = false;
            dgvServices.Columns.Add("Category", "Vehicle Type");
            dgvServices.Columns["Category"].Width = 100;
            dgvServices.Columns.Add("Name", "Service");
            dgvServices.Columns.Add("Price", "Price");
            dgvServices.Columns.Add("Description", "Description");

            string filterCategory = cmbCategory.SelectedItem?.ToString() ?? "";

            using var conn = Database.OpenConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            if (string.IsNullOrEmpty(filterCategory) || filterCategory == "All")
            {
                cmd.CommandText = "SELECT id, category, name, price, description FROM services ORDER BY category, name";
            }
            else
            {
                cmd.CommandText = "SELECT id, category, name, price, description FROM services WHERE category = $cat ORDER BY name";
                cmd.Parameters.AddWithValue("$cat", filterCategory);
            }
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int idx = dgvServices.Rows.Add();
                var row = dgvServices.Rows[idx];
                row.Cells["Id"].Value = reader.GetInt64(0);
                row.Cells["Category"].Value = reader.IsDBNull(1) ? "Others" : reader.GetString(1);
                row.Cells["Name"].Value = reader.GetString(2);
                row.Cells["Price"].Value = reader.GetDouble(3).ToString("0.00", CultureInfo.InvariantCulture);
                row.Cells["Description"].Value = reader.IsDBNull(4) ? "" : reader.GetString(4);
            }
        }

        private void DgvServices_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvServices.SelectedRows.Count == 0)
            {
                _selectedId = null;
                return;
            }
            var row = dgvServices.SelectedRows[0];
            _selectedId = row.Cells["Id"].Value is long l ? l : Convert.ToInt64(row.Cells["Id"].Value);
            cmbCategoryInput.SelectedItem = row.Cells["Category"].Value?.ToString() ?? "Others";
            txtServiceName.Text = row.Cells["Name"].Value?.ToString() ?? "";
            txtPrice.Text = row.Cells["Price"].Value?.ToString() ?? "";
            txtDescription.Text = row.Cells["Description"].Value?.ToString() ?? "";
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            string category = cmbCategoryInput.SelectedItem?.ToString() ?? "Others";
            string name = txtServiceName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { MessageBox.Show("Enter service name."); return; }
            if (!decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            { MessageBox.Show("Enter valid price."); return; }
            string desc = txtDescription.Text.Trim();

            using var conn = Database.OpenConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO services (category, name, price, description) VALUES ($cat, $n, $p, $d)";
            cmd.Parameters.AddWithValue("$cat", category);
            cmd.Parameters.AddWithValue("$n", name);
            cmd.Parameters.AddWithValue("$p", price);
            cmd.Parameters.AddWithValue("$d", string.IsNullOrEmpty(desc) ? (object)DBNull.Value : desc);
            try
            {
                cmd.ExecuteNonQuery();
                ClearInputs();
                LoadServices();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint
            {
                MessageBox.Show("This service name already exists in this category.");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (_selectedId == null) { MessageBox.Show("Select a service to update."); return; }
            string category = cmbCategoryInput.SelectedItem?.ToString() ?? "Others";
            string name = txtServiceName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { MessageBox.Show("Enter service name."); return; }
            if (!decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            { MessageBox.Show("Enter valid price."); return; }
            string desc = txtDescription.Text.Trim();

            using var conn = Database.OpenConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE services SET category=$cat, name=$n, price=$p, description=$d WHERE id=$id";
            cmd.Parameters.AddWithValue("$cat", category);
            cmd.Parameters.AddWithValue("$n", name);
            cmd.Parameters.AddWithValue("$p", price);
            cmd.Parameters.AddWithValue("$d", string.IsNullOrEmpty(desc) ? (object)DBNull.Value : desc);
            cmd.Parameters.AddWithValue("$id", _selectedId.Value);
            try
            {
                cmd.ExecuteNonQuery();
                ClearInputs();
                LoadServices();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                MessageBox.Show("This service name already exists in this category.");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvServices.SelectedRows.Count == 0) { MessageBox.Show("Select a service to delete."); return; }

            int count = dgvServices.SelectedRows.Count;
            string msg = count == 1 ? "Delete selected service?" : $"Delete {count} selected services?";
            if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            using var conn = Database.OpenConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            foreach (DataGridViewRow row in dgvServices.SelectedRows)
            {
                var idVal = row.Cells["Id"].Value;
                if (idVal == null) continue;
                long id = Convert.ToInt64(idVal);
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM services WHERE id=$id";
                cmd.Parameters.AddWithValue("$id", id);
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
            ClearInputs();
            LoadServices();
        }

        private void ClearInputs()
        {
            _selectedId = null;
            cmbCategoryInput.SelectedIndex = 0;
            txtServiceName.Clear();
            txtPrice.Clear();
            txtDescription.Clear();
            dgvServices.ClearSelection();
        }

        private void NumericTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            var tb = sender as TextBox;
            char sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            if (char.IsDigit(e.KeyChar)) return;
            if (e.KeyChar == sep && tb != null && !tb.Text.Contains(sep)) return;
            e.Handled = true;
        }
    }
}
