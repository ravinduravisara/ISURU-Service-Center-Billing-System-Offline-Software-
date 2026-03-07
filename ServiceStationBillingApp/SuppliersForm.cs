using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public partial class SuppliersForm : Form
    {
        private readonly string _userRole;

        public SuppliersForm(string userRole = "Admin")
        {
            _userRole = string.IsNullOrWhiteSpace(userRole) ? "Admin" : userRole;
            InitializeComponent();

            bool isAdmin = _userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            btnAdd.Enabled = isAdmin;
            btnEdit.Enabled = isAdmin;
            btnDelete.Enabled = isAdmin;

            // Keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.A) { dgvSuppliers.SelectAll(); e.Handled = true; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Delete && !txtSearch.Focused) { DeleteSupplier(); e.Handled = true; }
                else if (e.KeyCode == Keys.Escape) { this.Close(); e.Handled = true; }
            };
        }

        private void LoadSuppliers()
        {
            SyncSupplierTotalsFromInventory();

            var table = new DataTable();
            table.Columns.Add("Supplier ID");
            table.Columns.Add("Name");
            table.Columns.Add("Contact Number");
            table.Columns.Add("Cash", typeof(double));
            table.Columns.Add("Credit", typeof(double));
            table.Columns.Add("Cheque", typeof(double));
            table.Columns.Add("Invoice");

            using var conn = Database.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT supplier_code, name, contact, cash_total, credit_total, COALESCE(cheque_total,0), invoice FROM suppliers ORDER BY name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var code = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var contact = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                var cash = reader.IsDBNull(3) ? 0.0 : reader.GetDouble(3);
                var credit = reader.IsDBNull(4) ? 0.0 : reader.GetDouble(4);
                var cheque = reader.IsDBNull(5) ? 0.0 : reader.GetDouble(5);
                var invoice = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                table.Rows.Add(code, name, contact, cash, credit, cheque, invoice);
            }

            dgvSuppliers.DataSource = table;
            UpdateTotals(table);

            // Ensure column order and widths roughly match screenshot
            dgvSuppliers.ReadOnly = false;
            dgvSuppliers.EditMode = DataGridViewEditMode.EditOnEnter;
            if (dgvSuppliers.Columns.Count >= 3)
            {
                dgvSuppliers.Columns["Supplier ID"].Width = 180;
                dgvSuppliers.Columns["Name"].Width = 240;
                dgvSuppliers.Columns["Contact Number"].Width = 180;
                if (dgvSuppliers.Columns.Contains("Cash"))
                {
                    dgvSuppliers.Columns["Cash"].Visible = true;
                    dgvSuppliers.Columns["Cash"].Width = 120;
                    dgvSuppliers.Columns["Cash"].DefaultCellStyle.Format = "0.00";
                    dgvSuppliers.Columns["Cash"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (dgvSuppliers.Columns.Contains("Credit"))
                {
                    dgvSuppliers.Columns["Credit"].Visible = true;
                    dgvSuppliers.Columns["Credit"].Width = 120;
                    dgvSuppliers.Columns["Credit"].DefaultCellStyle.Format = "0.00";
                    dgvSuppliers.Columns["Credit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (dgvSuppliers.Columns.Contains("Cheque"))
                {
                    dgvSuppliers.Columns["Cheque"].Visible = true;
                    dgvSuppliers.Columns["Cheque"].Width = 120;
                    dgvSuppliers.Columns["Cheque"].DefaultCellStyle.Format = "0.00";
                    dgvSuppliers.Columns["Cheque"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (dgvSuppliers.Columns.Contains("Invoice"))
                {
                    dgvSuppliers.Columns["Invoice"].HeaderText = "Invoice";
                    dgvSuppliers.Columns["Invoice"].Width = 160;
                    dgvSuppliers.Columns["Invoice"].ReadOnly = false;
                }
            }

            // Persist inline edits for Invoice
            dgvSuppliers.CellEndEdit -= DgvSuppliers_CellEndEdit;
            dgvSuppliers.CellEndEdit += DgvSuppliers_CellEndEdit;
        }

        private void DgvSuppliers_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var grid = dgvSuppliers;
            if (grid.Columns[e.ColumnIndex].HeaderText.Equals("Invoice", StringComparison.OrdinalIgnoreCase))
            {
                var code = Convert.ToString(grid.Rows[e.RowIndex].Cells["Supplier ID"].Value) ?? string.Empty;
                var invoiceText = Convert.ToString(grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(code))
                {
                    using var conn = Database.OpenConnection();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE suppliers SET invoice=@inv WHERE supplier_code=@code";
                    cmd.Parameters.AddWithValue("@inv", invoiceText);
                    cmd.Parameters.AddWithValue("@code", code);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ApplyFilter()
        {
            if (dgvSuppliers.DataSource is DataTable dt)
            {
                var text = txtSearch.Text.Trim().Replace("'", "''");
                dt.DefaultView.RowFilter = string.IsNullOrEmpty(text)
                    ? string.Empty
                    : $"[Supplier ID] LIKE '%{text}%' OR [Name] LIKE '%{text}%' OR [Contact Number] LIKE '%{text}%'";
                UpdateTotals(dt.DefaultView.ToTable());
            }
        }

        private void AddSupplier()
        {
            using var dialog = new SupplierEditDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                using var conn = Database.OpenConnection();
                using var tx = conn.BeginTransaction();
                try
                {
                    string code = GenerateNextSupplierCode(conn);
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = "INSERT INTO suppliers(supplier_code, name, contact, cash_total, credit_total, invoice) VALUES(@c,@n,@p,0,0,@inv)";
                    cmd.Parameters.AddWithValue("@c", code);
                    cmd.Parameters.AddWithValue("@n", dialog.NameValue);
                    cmd.Parameters.AddWithValue("@p", dialog.ContactValue ?? "");
                    cmd.Parameters.AddWithValue("@inv", dialog.InvoiceValue ?? "");
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                    LoadSuppliers();
                }
                catch
                {
                    tx.Rollback();
                    MessageBox.Show("Failed to add supplier.");
                }
            }
        }

        private void EditSupplier()
        {
            var row = GetSelectedRow();
            if (row == null) return;
            string code = Convert.ToString(row["Supplier ID"]) ?? string.Empty;
            string name = Convert.ToString(row["Name"]) ?? string.Empty;
            string contact = Convert.ToString(row["Contact Number"]) ?? string.Empty;
            string invoice = Convert.ToString(row["Invoice"]) ?? string.Empty;

            using var dialog = new SupplierEditDialog(name, contact, invoice);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                using var conn = Database.OpenConnection();
                using var tx = conn.BeginTransaction();
                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = "UPDATE suppliers SET name=@n, contact=@p, invoice=@inv WHERE supplier_code=@c";
                    cmd.Parameters.AddWithValue("@n", dialog.NameValue);
                    cmd.Parameters.AddWithValue("@p", dialog.ContactValue ?? "");
                    cmd.Parameters.AddWithValue("@inv", dialog.InvoiceValue ?? "");
                    cmd.Parameters.AddWithValue("@c", code);
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                    LoadSuppliers();
                }
                catch
                {
                    tx.Rollback();
                    MessageBox.Show("Failed to update supplier.");
                }
            }
        }

        private void DeleteSupplier()
        {
            if (dgvSuppliers.SelectedRows.Count == 0) return;

            var codes = new List<string>();
            foreach (DataGridViewRow r in dgvSuppliers.SelectedRows)
            {
                if (r.DataBoundItem is DataRowView drv)
                {
                    string code = Convert.ToString(drv.Row["Supplier ID"]) ?? string.Empty;
                    if (!string.IsNullOrEmpty(code)) codes.Add(code);
                }
            }
            if (codes.Count == 0) return;

            string msg = codes.Count == 1 ? $"Delete supplier {codes[0]}?" : $"Delete {codes.Count} suppliers?";
            if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using var conn = Database.OpenConnection();
                using var tx = conn.BeginTransaction();
                try
                {
                    foreach (var code in codes)
                    {
                        using (var delSI = conn.CreateCommand())
                        {
                            delSI.Transaction = tx;
                            delSI.CommandText = "DELETE FROM supply_invoices WHERE supplier_code=@c";
                            delSI.Parameters.AddWithValue("@c", code);
                            delSI.ExecuteNonQuery();
                        }
                        using (var delMov = conn.CreateCommand())
                        {
                            delMov.Transaction = tx;
                            delMov.CommandText = "DELETE FROM stock_movements WHERE supplier_code=@c";
                            delMov.Parameters.AddWithValue("@c", code);
                            delMov.ExecuteNonQuery();
                        }
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = "DELETE FROM suppliers WHERE supplier_code=@c";
                        cmd.Parameters.AddWithValue("@c", code);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                    LoadSuppliers();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    MessageBox.Show($"Failed to delete supplier: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private DataRow? GetSelectedRow()
        {
            if (dgvSuppliers.CurrentRow == null) return null;
            if (dgvSuppliers.CurrentRow.DataBoundItem is DataRowView drv)
            {
                return drv.Row;
            }
            return null;
        }

        private string GenerateNextSupplierCode(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT supplier_code FROM suppliers WHERE supplier_code IS NOT NULL ORDER BY supplier_code DESC LIMIT 1";
            var last = cmd.ExecuteScalar() as string;
            int nextNum = 1;
            if (!string.IsNullOrWhiteSpace(last) && last.StartsWith("SUP-", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(last.Substring(4), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                    nextNum = n + 1;
            }
            return $"SUP-{nextNum.ToString("D4", CultureInfo.InvariantCulture)}";
        }

        private void UpdateTotals(DataTable dt)
        {
            double cash = 0, credit = 0, cheque = 0;
            foreach (DataRow r in dt.Rows)
            {
                if (r["Cash"] is double c) cash += c;
                if (r["Credit"] is double cr) credit += cr;
                if (r["Cheque"] is double ch) cheque += ch;
            }
            statusCash.Text = $"Cash: {cash:0.00}";
            statusCredit.Text = $"Credit: {credit:0.00}";
            statusCheque.Text = $"Cheque: {cheque:0.00}";
            btnCashSummary.Text = $"Cash ({cash:0.00})";
            btnCreditSummary.Text = $"Credit ({credit:0.00})";
            btnChequeSummary.Text = $"Cheque ({cheque:0.00})";
        }

        private void SyncSupplierTotalsFromInventory()
        {
            using var conn = Database.OpenConnection();
            using var tx = conn.BeginTransaction();

            // Ensure supplier rows exist for any supplier_code present in inventory
            using (var ensure = conn.CreateCommand())
            {
                ensure.Transaction = tx;
                ensure.CommandText = @"SELECT DISTINCT TRIM(COALESCE(supplier_code,'')) AS code
                                        FROM inventory
                                        WHERE TRIM(COALESCE(supplier_code,'')) <> ''";
                using var r = ensure.ExecuteReader();
                var newCodes = new List<string>();
                while (r.Read())
                {
                    var code = r.GetString(0);
                    if (!string.IsNullOrWhiteSpace(code)) newCodes.Add(code);
                }
                foreach (var code in newCodes)
                {
                    using var ins = conn.CreateCommand();
                    ins.Transaction = tx;
                    ins.CommandText = @"INSERT OR IGNORE INTO suppliers (supplier_code, name, contact, cash_total, credit_total, invoice)
                                        VALUES (@c, @n, '', 0, 0, '')";
                    ins.Parameters.AddWithValue("@c", code);
                    ins.Parameters.AddWithValue("@n", code);
                    ins.ExecuteNonQuery();
                }
            }

            using (var reset = conn.CreateCommand())
            {
                reset.Transaction = tx;
                reset.CommandText = "UPDATE suppliers SET cash_total=0, credit_total=0, cheque_total=0";
                reset.ExecuteNonQuery();
            }

            // Compute totals from current inventory state (not accumulated stock_movements)
            var totals = new Dictionary<string, (double cash, double credit, double cheque)>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"SELECT
                                            supplier_code,
                                            CASE
                                                WHEN LOWER(TRIM(COALESCE(payment_method, ''))) LIKE '%credit%' THEN 'credit'
                                                WHEN LOWER(TRIM(COALESCE(payment_method, ''))) LIKE '%cheque%' THEN 'cheque'
                                                ELSE 'cash'
                                            END AS pm,
                                            SUM(quantity * COALESCE(buying_price, 0)) AS total
                                        FROM inventory
                                        WHERE supplier_code IS NOT NULL AND TRIM(supplier_code) <> ''
                                        GROUP BY supplier_code, pm";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var code = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                    if (string.IsNullOrWhiteSpace(code)) continue;

                    var pm = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    var total = reader.IsDBNull(2) ? 0.0 : reader.GetDouble(2);

                    totals.TryGetValue(code, out var existing);
                    if (pm == "credit")
                    {
                        existing = (existing.cash, existing.credit + total, existing.cheque);
                    }
                    else if (pm == "cheque")
                    {
                        existing = (existing.cash, existing.credit, existing.cheque + total);
                    }
                    else
                    {
                        existing = (existing.cash + total, existing.credit, existing.cheque);
                    }
                    totals[code] = existing;
                }
            }

            foreach (var kvp in totals)
            {
                using var upd = conn.CreateCommand();
                upd.Transaction = tx;
                upd.CommandText = "UPDATE suppliers SET cash_total=@c, credit_total=@cr, cheque_total=@ch WHERE supplier_code=@code";
                upd.Parameters.AddWithValue("@c", kvp.Value.cash);
                upd.Parameters.AddWithValue("@cr", kvp.Value.credit);
                upd.Parameters.AddWithValue("@ch", kvp.Value.cheque);
                upd.Parameters.AddWithValue("@code", kvp.Key);
                upd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        private void OpenPaymentDetails(string paymentMethod)
        {
            using var dlg = new SupplierPaymentDetailsForm(paymentMethod);
            dlg.ShowDialog(this);
        }

        private void HighlightCash()
        {
            // Simple visual: sort by cash descending
            if (dgvSuppliers.DataSource is DataTable dt)
            {
                dt.DefaultView.Sort = "Cash DESC";
            }
        }

        private void HighlightCredit()
        {
            // Simple visual: sort by credit descending
            if (dgvSuppliers.DataSource is DataTable dt)
            {
                dt.DefaultView.Sort = "Credit DESC";
            }
        }
    }
}
