using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public class CustomersForm : Form
    {
        private readonly string _connectionString;
        private readonly string _userRole;
        private DataGridView dgvCustomers = null!;
        private TextBox txtSearch = null!;
        private Label lblTitle = null!;
        private Button btnAdd = null!;
        private Button btnEdit = null!;
        private Button btnDelete = null!;

        private DataTable? _table;

        public CustomersForm(string userRole = "Admin")
        {
            // Use centralized Database connection
            _connectionString = Database.ConnectionString;
            _userRole = string.IsNullOrWhiteSpace(userRole) ? "Admin" : userRole;

            InitializeComponent();
            ApplyRoleRestrictions();
        }

        private void ApplyRoleRestrictions()
        {
            bool isUser = string.Equals(_userRole, "User", StringComparison.OrdinalIgnoreCase);
            if (isUser)
            {
                btnAdd.Enabled = false;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Customers";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(700, 480);
            this.MinimumSize = new Size(600, 420);
            this.BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "All Customers",
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(12, 9)
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "Search name or phone...",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Location = new Point(12, 36),
                Width = 300
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            btnAdd = new Button
            {
                Text = "Add",
                Location = new Point(450, 34),
                Size = new Size(65, 26)
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button
            {
                Text = "Edit",
                Location = new Point(520, 34),
                Size = new Size(65, 26),
                Enabled = false
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(590, 34),
                Size = new Size(65, 26),
                Enabled = false
            };
            btnDelete.Click += BtnDelete_Click;


            dgvCustomers = new DataGridView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(12, 70),
                Size = new Size(660, 360),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                MultiSelect = true,
            };
            dgvCustomers.SelectionChanged += DgvCustomers_SelectionChanged;

            this.Controls.Add(lblTitle);
            this.Controls.Add(txtSearch);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
            this.Controls.Add(dgvCustomers);

            // Keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.A) { dgvCustomers.SelectAll(); e.Handled = true; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Delete && !txtSearch.Focused) { BtnDelete_Click(s, e); e.Handled = true; }
                else if (e.KeyCode == Keys.Escape) { this.Close(); e.Handled = true; }
            };

            this.Load += CustomersForm_Load;
        }

        private void CustomersForm_Load(object? sender, EventArgs e)
        {
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                using var conn = Database.OpenConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        c.id AS Id,
                        c.name AS Name,
                        c.phone AS Phone,
                        (SELECT COUNT(1) FROM vehicles v WHERE v.customer_id = c.id) AS Vehicles
                    FROM customers c
                    ORDER BY LOWER(c.name);
                ";
                using var reader = cmd.ExecuteReader();
                _table = new DataTable();
                _table.Load(reader);
                dgvCustomers.DataSource = _table;
                if (dgvCustomers.Columns.Contains("Id"))
                {
                    dgvCustomers.Columns["Id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load customers:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            if (_table == null) return;
            string q = txtSearch.Text.Replace("'", "''").Trim();
            if (string.IsNullOrEmpty(q))
            {
                (_table.DefaultView).RowFilter = string.Empty;
            }
            else
            {
                (_table.DefaultView).RowFilter = $"Convert(Name, 'System.String') LIKE '%{q}%' OR Convert(Phone, 'System.String') LIKE '%{q}%'";
            }
        }

        private void DgvCustomers_SelectionChanged(object? sender, EventArgs e)
        {
            bool hasSel = GetSelectedCustomerId() != null;
            bool isUser = string.Equals(_userRole, "User", StringComparison.OrdinalIgnoreCase);
            btnEdit.Enabled = hasSel && !isUser;
            btnDelete.Enabled = hasSel && !isUser;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var dlg = new CustomerEditDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(dlg.NameValue))
                {
                    MessageBox.Show("Name is required.");
                    return;
                }
                try
                {
                    using var conn = Database.OpenConnection();
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO customers(name, phone) VALUES($n, $p);";
                    cmd.Parameters.AddWithValue("$n", dlg.NameValue.Trim());
                    cmd.Parameters.AddWithValue("$p", (dlg.PhoneValue ?? string.Empty).Trim());
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to add customer:\n" + ex.Message);
                }
                LoadCustomers();
                TxtSearch_TextChanged(null, EventArgs.Empty);
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            var id = GetSelectedCustomerId();
            if (id == null) return;
            string currName = string.Empty;
            string currPhone = string.Empty;
            try
            {
                using var conn = Database.OpenConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT name, phone FROM customers WHERE id=$id";
                cmd.Parameters.AddWithValue("$id", id.Value);
                using var rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    currName = rdr[0]?.ToString() ?? string.Empty;
                    currPhone = rdr[1]?.ToString() ?? string.Empty;
                }
            }
            catch { }

            using var dlg = new CustomerEditDialog(currName, currPhone);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(dlg.NameValue))
                {
                    MessageBox.Show("Name is required.");
                    return;
                }
                try
                {
                    using var conn = new SqliteConnection(_connectionString);
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE customers SET name=$n, phone=$p WHERE id=$id";
                    cmd.Parameters.AddWithValue("$n", dlg.NameValue.Trim());
                    cmd.Parameters.AddWithValue("$p", (dlg.PhoneValue ?? string.Empty).Trim());
                    cmd.Parameters.AddWithValue("$id", id.Value);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update customer:\n" + ex.Message);
                }
                LoadCustomers();
                TxtSearch_TextChanged(null, EventArgs.Empty);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            var ids = new System.Collections.Generic.List<long>();
            foreach (DataGridViewRow row in dgvCustomers.SelectedRows)
            {
                if (row.DataBoundItem is DataRowView drv)
                {
                    var val = drv.Row["Id"];
                    if (val != null && val != DBNull.Value)
                        ids.Add(Convert.ToInt64(val));
                }
            }
            if (ids.Count == 0) return;

            string msg = ids.Count == 1
                ? "Are you sure you want to delete this customer?\nThis will also delete all related vehicles and invoices."
                : $"Are you sure you want to delete {ids.Count} customers?\nThis will also delete all related vehicles and invoices.";
            if (MessageBox.Show(msg, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using var conn = Database.OpenConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();

                foreach (var id in ids)
                {
                    using (var cmd = conn.CreateCommand()) { cmd.Transaction = tx; cmd.CommandText = "DELETE FROM invoice_items WHERE invoice_id IN (SELECT id FROM invoices WHERE customer_id=$id)"; cmd.Parameters.AddWithValue("$id", id); cmd.ExecuteNonQuery(); }
                    using (var cmd = conn.CreateCommand()) { cmd.Transaction = tx; cmd.CommandText = "DELETE FROM invoices WHERE customer_id=$id"; cmd.Parameters.AddWithValue("$id", id); cmd.ExecuteNonQuery(); }
                    using (var cmd = conn.CreateCommand()) { cmd.Transaction = tx; cmd.CommandText = "DELETE FROM vehicles WHERE customer_id=$id"; cmd.Parameters.AddWithValue("$id", id); cmd.ExecuteNonQuery(); }
                    using (var cmd = conn.CreateCommand()) { cmd.Transaction = tx; cmd.CommandText = "DELETE FROM customers WHERE id=$id"; cmd.Parameters.AddWithValue("$id", id); cmd.ExecuteNonQuery(); }
                }

                tx.Commit();
                string successMsg = ids.Count == 1 ? "Customer deleted successfully." : $"{ids.Count} customers deleted successfully.";
                MessageBox.Show(successMsg, "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete customer:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadCustomers();
            TxtSearch_TextChanged(null, EventArgs.Empty);
        }

        private long? GetSelectedCustomerId()
        {
            if (dgvCustomers.CurrentRow == null || dgvCustomers.CurrentRow.DataBoundItem == null)
                return null;
            if (dgvCustomers.CurrentRow.DataBoundItem is System.Data.DataRowView drv)
            {
                try
                {
                    var val = drv.Row["Id"];
                    if (val == null || val == DBNull.Value) return null;
                    return Convert.ToInt64(val);
                }
                catch { return null; }
            }
            return null;
        }
    }
}
