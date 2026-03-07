using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Drawing.Printing;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public partial class BillingForm : Form
    {
        private readonly string _userRole; // "Admin" or "User"
        private readonly string _dbPath;
        private readonly string _connectionString;

        // For hover effect on grid rows
        private int _hoveredRowIndex = -1;

        public BillingForm(string userRole = "Admin")
        {
            InitializeComponent();
            _userRole = string.IsNullOrWhiteSpace(userRole) ? "Admin" : userRole;

            // Use centralized Database connection
            _dbPath = Database.GetDbPath();
            _connectionString = Database.ConnectionString;

            // Keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += BillingForm_KeyDown;

            // Customer autocomplete
            ConfigureCustomerAutoComplete();
            this.txtCustomerName.TextChanged += TxtCustomerName_TextChanged;
            this.txtCustomerName.KeyDown += TxtCustomerName_KeyDown;

            // Ensure DB is initialized
            Database.Ensure();
            SetupItemsGrid();
            HookEvents();

            // Style main action buttons (menu items are already styled by designer)
            StyleButton(btnSave, primary: true);
            StyleButton(btnPrint);
            StyleButton(btnClear);

            // Keep Inventory accessible for all; enforce read-only inside the form for users
        }

        // =======================
        //   UI HELPERS
        // =======================

        private void StyleButton(Button btn, bool primary = false)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            if (primary)
            {
                // Primary Blue
                btn.BackColor = Color.FromArgb(30, 136, 229);
                btn.ForeColor = Color.White;
            }
            else
            {
                // Light neutral
                btn.BackColor = Color.FromArgb(236, 239, 241);
                btn.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Configure the columns of the dgvItems grid.
        /// </summary>
        private void SetupItemsGrid()
        {
            dgvItems.Columns.Clear();

            // --- Service dropdown column ---
            var colService = new DataGridViewComboBoxColumn
            {
                Name = "Service",
                HeaderText = "Service",
                Width = 180,
                FlatStyle = FlatStyle.Flat
            };

            // Populate services from DB
            LoadServiceDropdown(colService);

            // Item ID column
            var colItemId = new DataGridViewTextBoxColumn
            {
                Name = "ItemId",
                HeaderText = "Item ID",
                Width = 140
            };

            // Description column
            var colDescription = new DataGridViewTextBoxColumn
            {
                Name = "Description",
                HeaderText = "Description",
                Width = 200
            };

            // Quantity column
            var colQty = new DataGridViewTextBoxColumn
            {
                Name = "Qty",
                HeaderText = "Qty",
                Width = 60
            };

            // Rate column
            var colRate = new DataGridViewTextBoxColumn
            {
                Name = "Rate",
                HeaderText = "Rate",
                Width = 80
            };

            // Amount column
            var colAmount = new DataGridViewTextBoxColumn
            {
                Name = "Amount",
                HeaderText = "Amount",
                Width = 100,
                ReadOnly = true
            };

            dgvItems.Columns.AddRange(colService, colItemId, colDescription, colQty, colRate, colAmount);

            dgvItems.AllowUserToAddRows = true;
            dgvItems.AllowUserToDeleteRows = true;
            dgvItems.RowHeadersVisible = false;
            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItems.MultiSelect = true;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.DataError += (s, err) => { err.ThrowException = false; };
            this.KeyPreview = true; // ensure form receives key combos like Ctrl+A
        }

        private void LoadServiceDropdown(DataGridViewComboBoxColumn? col = null)
        {
            var services = new System.Collections.Generic.List<string>();
            try
            {
                // Map vehicle type to service category
                string vehicleType = cmbVehicleType.SelectedItem?.ToString() ?? "";
                // Normalize "Other" to "Others" to match service categories
                if (string.Equals(vehicleType, "Other", StringComparison.OrdinalIgnoreCase))
                    vehicleType = "Others";

                using var conn = Database.OpenConnection();
                using var cmd = conn.CreateCommand();
                if (!string.IsNullOrEmpty(vehicleType) && !string.Equals(vehicleType, "Others", StringComparison.OrdinalIgnoreCase))
                {
                    cmd.CommandText = "SELECT DISTINCT name FROM services WHERE (LOWER(category) = LOWER($cat) OR LOWER(category) = 'others') AND name IS NOT NULL AND TRIM(name) <> '' ORDER BY name";
                    cmd.Parameters.AddWithValue("$cat", vehicleType);
                }
                else
                {
                    cmd.CommandText = "SELECT DISTINCT name FROM services WHERE name IS NOT NULL AND TRIM(name) <> '' ORDER BY name";
                }
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var svcName = reader.GetString(0);
                    if (!services.Contains(svcName))
                        services.Add(svcName);
                }
            }
            catch { /* table may not exist yet */ }

            // Fallback defaults if table empty or missing
            if (services.Count == 0)
            {
                services.AddRange(new[]
                {
                    "Engine Oil Change",
                    "Full Service / Body Wash",
                    "Interior Cleaning",
                    "Vacuum",
                    "Tune-up",
                    "Filters Replacement",
                    "Labor Charges",
                    "Custom Item"
                });
            }

            // Apply to existing column or find by name
            DataGridViewComboBoxColumn? target = col;
            if (target == null && dgvItems.Columns.Contains("Service") && dgvItems.Columns["Service"] is DataGridViewComboBoxColumn svcCol)
                target = svcCol;

            if (target != null)
            {
                // Preserve values already chosen in existing rows so they remain valid
                foreach (DataGridViewRow row in dgvItems.Rows)
                {
                    if (row.IsNewRow) continue;
                    var val = row.Cells["Service"].Value?.ToString();
                    if (!string.IsNullOrEmpty(val) && !services.Contains(val))
                        services.Add(val);
                }

                target.Items.Clear();
                target.Items.AddRange(services.ToArray());
            }
        }

        private void HookEvents()
        {
            // Grid + totals
            dgvItems.CellEndEdit += DgvItems_CellEndEdit;
            dgvItems.RowsRemoved += DgvItems_RowsRemoved;
            dgvItems.UserDeletedRow += DgvItems_UserDeletedRow;
            txtDiscount.TextChanged += TxtDiscount_TextChanged;
            dgvItems.KeyDown += DgvItems_KeyDown;

            // Reload service dropdown when vehicle type changes
            cmbVehicleType.SelectedIndexChanged += (_, __) => LoadServiceDropdown();

            // Hover effect
            dgvItems.CellMouseEnter += DgvItems_CellMouseEnter;
            dgvItems.CellMouseLeave += DgvItems_CellMouseLeave;

            // Buttons
            btnSave.Click += BtnSave_Click;
            btnClear.Click += BtnClear_Click;
            btnPrint.Click += BtnPrint_Click;
            // Contact number: restrict to digits and length
            txtContact.KeyPress += TxtContact_KeyPress;
            txtContact.TextChanged += TxtContact_TextChanged;
            // Menu items are wired in the designer; avoid double wiring
            // to prevent opening dialogs twice on click.
            // Apply role-based restrictions
            ApplyRoleRestrictions();
        }

        private void ConfigureCustomerAutoComplete()
        {
            try
            {
                var source = new AutoCompleteStringCollection();
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT name FROM customers WHERE name IS NOT NULL AND TRIM(name) <> '' GROUP BY name";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var name = reader[0]?.ToString();
                    if (!string.IsNullOrWhiteSpace(name)) source.Add(name);
                }
                txtCustomerName.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txtCustomerName.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtCustomerName.AutoCompleteCustomSource = source;
            }
            catch { /* ignore autocomplete failures */ }
        }

        private void TxtCustomerName_TextChanged(object? sender, EventArgs e)
        {
            // Only populate contact; do not auto-suggest vehicle number or type
            var name = txtCustomerName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                txtContact.Clear();
                txtVehicleNo.Clear();
                cmbVehicleType.SelectedIndex = -1;
                return;
            }

            try
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT phone FROM customers WHERE LOWER(name) = LOWER($n) LIMIT 1";
                    cmd.Parameters.AddWithValue("$n", name);
                    var result = cmd.ExecuteScalar();
                    txtContact.Text = result?.ToString() ?? string.Empty;
                }

                // Clear vehicle fields for manual entry
                txtVehicleNo.Clear();
                cmbVehicleType.SelectedIndex = -1;
            }
            catch { /* ignore transient db errors during typing */ }
        }

        // Populate saved customer details when pressing Enter on the name field
        private void TxtCustomerName_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            var name = txtCustomerName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT phone FROM customers WHERE LOWER(name) = LOWER($n) LIMIT 1";
                    cmd.Parameters.AddWithValue("$n", name);
                    var result = cmd.ExecuteScalar();
                    txtContact.Text = result?.ToString() ?? string.Empty;
                }

                // Keep vehicle details manual per latest requirement
                txtVehicleNo.Focus();
                e.Handled = true;
            }
            catch
            {
                // Ignore lookup errors; allow manual entry
            }
        }

        private void MenuAllInvoices_Click(object? sender, EventArgs e)
        {
            using var form = new InvoiceListForm();
            form.ShowDialog(this);
        }

        private void MenuCustomers_Click(object? sender, EventArgs e)
        {
            using var form = new CustomersForm(_userRole);
            form.ShowDialog(this);
        }

        private void MenuSuppliers_Click(object? sender, EventArgs e)
        {
            using var form = new SuppliersForm(_userRole);
            form.ShowDialog(this);
        }

        private void MenuSupplyInvoices_Click(object? sender, EventArgs e)
        {
            using var form = new SupplyInvoicesForm(_userRole);
            form.ShowDialog(this);
        }

        private void MenuProfitStatement_Click(object? sender, EventArgs e)
        {
            using var form = new ProfitStatementForm();
            form.ShowDialog(this);
        }

        private void MenuSettings_Click(object? sender, EventArgs e)
        {
            using var form = new SettingsForm();
            form.ShowDialog(this);
        }

        private bool IsAdminUser()
        {
            // Assuming there's a current user role stored in the form or a service
            // Replace this with your actual role check logic
            return _userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
        private void ApplyRoleRestrictions()
        {
            bool isOtherUser = string.Equals(_userRole, "User", StringComparison.OrdinalIgnoreCase);
            if (menuServices != null)
                menuServices.Enabled = true;
            if (menuAllInvoices != null)
                menuAllInvoices.Visible = true;
            if (menuCustomers != null)
            {
                menuCustomers.Visible = true;
                menuCustomers.Enabled = true;
            }
            if (menuSuppliers != null)
            {
                menuSuppliers.Visible = true;
                menuSuppliers.Enabled = true;
            }
            if (menuProfitStatement != null)
            {
                menuProfitStatement.Enabled = !isOtherUser;
            }
            if (menuSettings != null)
            {
                menuSettings.Enabled = !isOtherUser;
            }
        }
        // =======================
        //   KEYBOARD SHORTCUTS
        // =======================

        private void BillingForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                // Ctrl + S -> Save bill
                BtnSave_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                // F2 -> New row
                AddNewItemRow();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.H)
            {
                // Ctrl + H -> History
                BtnHistory_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Esc -> if a single service row selected in grid, delete only that row; otherwise clear form
                    if (dgvItems.Focused)
                    {
                        bool deleted = false;
                        // Delete all selected non-new rows
                        if (dgvItems.SelectedRows.Count > 0)
                        {
                            foreach (DataGridViewRow sel in dgvItems.SelectedRows)
                            {
                                if (!sel.IsNewRow) { dgvItems.Rows.Remove(sel); deleted = true; }
                            }
                        }
                        else if (dgvItems.CurrentRow != null && !dgvItems.CurrentRow.IsNewRow)
                        {
                            dgvItems.Rows.Remove(dgvItems.CurrentRow); deleted = true;
                        }
                        if (deleted)
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                    // If grid not focused or nothing to delete, fall back to clearing form
                    ClearForm();
                    e.Handled = true;
            }
        }

        private void DgvItems_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                foreach (DataGridViewRow r in dgvItems.Rows)
                {
                    if (!r.IsNewRow) r.Selected = true;
                }
                // Keep focus on grid
                if (dgvItems.Rows.Count > 0)
                {
                    var firstDataRow = dgvItems.Rows[0];
                    if (!firstDataRow.IsNewRow && dgvItems.Columns.Count > 0)
                        dgvItems.CurrentCell = firstDataRow.Cells[0];
                }
                e.Handled = true;
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                    bool deleted = false;
                    if (dgvItems.SelectedRows.Count > 0)
                    {
                        foreach (DataGridViewRow sel in dgvItems.SelectedRows)
                        {
                            if (!sel.IsNewRow) { dgvItems.Rows.Remove(sel); deleted = true; }
                        }
                    }
                    else if (dgvItems.CurrentRow != null && !dgvItems.CurrentRow.IsNewRow)
                    {
                        dgvItems.Rows.Remove(dgvItems.CurrentRow); deleted = true;
                    }
                    if (deleted) { e.Handled = true; }
            }
        }

        // Ensure Ctrl+A works even if DataGridView consumes the key
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.A) && dgvItems.Focused)
            {
                foreach (DataGridViewRow r in dgvItems.Rows)
                {
                    if (!r.IsNewRow) r.Selected = true;
                }
                if (dgvItems.Rows.Count > 0 && dgvItems.Columns.Count > 0)
                {
                    var firstDataRow = dgvItems.Rows[0];
                    if (!firstDataRow.IsNewRow)
                        dgvItems.CurrentCell = firstDataRow.Cells[0];
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void AddNewItemRow()
        {
            int rowIndex = dgvItems.Rows.Add();
            if (rowIndex >= 0)
            {
                dgvItems.CurrentCell = dgvItems.Rows[rowIndex].Cells["Service"];
                dgvItems.BeginEdit(true);
            }
        }

        // =======================
        //   GRID & TOTALS
        // =======================

        // Hover effect handlers
        private void DgvItems_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvItems.Rows.Count)
                return;

            if (_hoveredRowIndex == e.RowIndex)
                return;

            // Reset previous hovered row
            if (_hoveredRowIndex >= 0 && _hoveredRowIndex < dgvItems.Rows.Count)
            {
                var oldRow = dgvItems.Rows[_hoveredRowIndex];
                oldRow.DefaultCellStyle.BackColor = Color.Empty; // fall back to default/alternating style
            }

            _hoveredRowIndex = e.RowIndex;
            var row = dgvItems.Rows[e.RowIndex];

            if (!row.Selected)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(239, 246, 255);
            }
        }

        private void DgvItems_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvItems.Rows.Count)
                return;

            var row = dgvItems.Rows[e.RowIndex];
            if (!row.Selected)
            {
                row.DefaultCellStyle.BackColor = Color.Empty;
            }
        }

        private void DgvItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var columnName = dgvItems.Columns[e.ColumnIndex].Name;

            // Auto-fill rate from Services when service name selected/edited
            var row = dgvItems.Rows[e.RowIndex];
            var colName = dgvItems.Columns[e.ColumnIndex].Name;
            if (string.Equals(colName, "Service", StringComparison.OrdinalIgnoreCase))
            {
                var serviceNameObj = row.Cells["Service"].Value;
                var serviceName = serviceNameObj?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(serviceName))
                {
                    // If service is inventory-rated type and Item ID present, pull rate from Inventory
                    if (IsInventoryRatedService(serviceName))
                    {
                        var itemId = row.Cells["ItemId"].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(itemId) && TryGetInventoryPriceByItemId(itemId, out var invPrice, out var invBrand))
                        {
                            row.Cells["Rate"].Value = invPrice.ToString("0.00");
                            if (!string.IsNullOrEmpty(invBrand))
                                row.Cells["Description"].Value = invBrand;
                            var qtyTextInv = row.Cells["Qty"].Value?.ToString();
                            if (string.IsNullOrWhiteSpace(qtyTextInv))
                                row.Cells["Qty"].Value = "1";
                            RecalculateRowAmount(e.RowIndex);
                            UpdateTotals();
                            return;
                        }
                    }
                    // Fallback to fixed service price
                    if (TryGetServicePrice(serviceName, out var price))
                    {
                        row.Cells["Rate"].Value = price.ToString("0.00");
                        var qtyText = row.Cells["Qty"].Value?.ToString();
                        if (string.IsNullOrWhiteSpace(qtyText))
                            row.Cells["Qty"].Value = "1";
                        RecalculateRowAmount(e.RowIndex);
                        UpdateTotals();
                    }
                }
            }
            else if (string.Equals(colName, "ItemId", StringComparison.OrdinalIgnoreCase))
            {
                var serviceName = row.Cells["Service"].Value?.ToString()?.Trim();
                var itemId = row.Cells["ItemId"].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(itemId))
                {
                    if (TryGetInventoryPriceByItemId(itemId, out var invPrice, out var invBrand))
                    {
                        if (!string.IsNullOrEmpty(invBrand))
                            row.Cells["Description"].Value = invBrand;
                        if (!string.IsNullOrEmpty(serviceName) && IsInventoryRatedService(serviceName))
                        {
                            row.Cells["Rate"].Value = invPrice.ToString("0.00");
                            var qtyText = row.Cells["Qty"].Value?.ToString();
                            if (string.IsNullOrWhiteSpace(qtyText))
                                row.Cells["Qty"].Value = "1";
                            RecalculateRowAmount(e.RowIndex);
                            UpdateTotals();
                        }
                        return;
                    }
                }
            }
            if (columnName == "Qty" || columnName == "Rate")
            {
                RecalculateRowAmount(e.RowIndex);
                UpdateTotals();
            }
        }

        private static bool IsInventoryRatedService(string serviceName)
        {
            return string.Equals(serviceName, "Engine Oil Change", StringComparison.OrdinalIgnoreCase)
                || string.Equals(serviceName, "Filters Replacement", StringComparison.OrdinalIgnoreCase)
                || string.Equals(serviceName, "Custom Item", StringComparison.OrdinalIgnoreCase);
        }

        private bool TryGetInventoryPriceByItemId(string itemId, out decimal price, out string brand)
        {
            price = 0m;
            brand = string.Empty;
            try
            {
                using var conn = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT unit_price, COALESCE(brand, '') FROM inventory WHERE item_id = $id LIMIT 1";
                cmd.Parameters.AddWithValue("$id", itemId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var priceVal = reader.GetValue(0);
                    if (priceVal is double d)
                        price = (decimal)d;
                    else if (decimal.TryParse(priceVal?.ToString(), out var dec))
                        price = dec;
                    else
                        return false;

                    brand = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private void DgvItems_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            UpdateTotals();
        }

        private void DgvItems_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            UpdateTotals();
        }

        private void TxtDiscount_TextChanged(object sender, EventArgs e)
        {
            UpdateTotals();
        }

        // Lookup fixed price from Services table by service name
        private bool TryGetServicePrice(string serviceName, out decimal price)
        {
            price = 0m;
            try
            {
                // Get current vehicle type so we return the price for the correct category
                string vehicleType = cmbVehicleType.SelectedItem?.ToString() ?? "";
                if (string.Equals(vehicleType, "Other", StringComparison.OrdinalIgnoreCase))
                    vehicleType = "Others";

                using var conn = Database.OpenConnection();
                using var cmd = conn.CreateCommand();
                if (!string.IsNullOrEmpty(vehicleType))
                {
                    cmd.CommandText = "SELECT price FROM services WHERE LOWER(name) = LOWER($n) AND LOWER(category) = LOWER($cat) LIMIT 1";
                    cmd.Parameters.AddWithValue("$n", serviceName);
                    cmd.Parameters.AddWithValue("$cat", vehicleType);
                }
                else
                {
                    cmd.CommandText = "SELECT price FROM services WHERE LOWER(name) = LOWER($n) LIMIT 1";
                    cmd.Parameters.AddWithValue("$n", serviceName);
                }
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    if (result is double d)
                    {
                        price = (decimal)d;
                        return true;
                    }
                    if (decimal.TryParse(result.ToString(), out var dec))
                    {
                        price = dec;
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private void RecalculateRowAmount(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvItems.Rows.Count)
                return;

            var row = dgvItems.Rows[rowIndex];
            if (row.IsNewRow) return;

            decimal qty = GetCellDecimal(row, "Qty");
            decimal rate = GetCellDecimal(row, "Rate");
            decimal amount = qty * rate;

            row.Cells["Amount"].Value = amount.ToString("0.00");
        }

        private decimal GetCellDecimal(DataGridViewRow row, string columnName)
        {
            var cellValue = row.Cells[columnName].Value;
            if (cellValue == null) return 0m;

            if (decimal.TryParse(
                    cellValue.ToString(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal result))
            {
                return result;
            }

            if (decimal.TryParse(
                    cellValue.ToString(),
                    NumberStyles.Any,
                    CultureInfo.CurrentCulture,
                    out result))
            {
                return result;
            }

            return 0m;
        }

        private void UpdateTotals()
        {
            decimal subtotal = 0m;

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;
                subtotal += GetCellDecimal(row, "Amount");
            }

            txtSubtotal.Text = subtotal.ToString("0.00");

            decimal discount = 0m;
            if (!string.IsNullOrWhiteSpace(txtDiscount.Text))
            {
                decimal.TryParse(
                    txtDiscount.Text,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out discount
                );
            }

            decimal total = subtotal - discount;
            if (total < 0) total = 0m;

            txtTotal.Text = total.ToString("0.00");
        }

        // =======================
        //   DATABASE INIT
        // =======================

        // Database schema is ensured via Database.Ensure()

        // =======================
        //   SAVE BILL
        // =======================

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateBill())
                    return;
                var saved = SaveBill(out long invoiceId);
                if (!saved) return;
                MessageBox.Show("Bill saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving bill:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool SaveBill(out long invoiceId)
        {
            invoiceId = 0;
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            long customerId = GetOrCreateCustomer(conn);
            long vehicleId = GetOrCreateVehicle(conn, customerId);
            invoiceId = InsertInvoice(conn, customerId, vehicleId);
            InsertInvoiceItems(conn, invoiceId);
            tx.Commit();
            return true;
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateBill())
                    return;
                var saved = SaveBill(out long invoiceId);
                if (!saved) return;

                // A4 invoice layout printing (matches requested format)
                PrintDocument pd = new PrintDocument();
                pd.DocumentName = "ServiceStationInvoice";
                try
                {
                    // Use printer defaults; apply comfortable margins for A4
                    pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);
                    pd.DefaultPageSettings.Landscape = false;
                }
                catch { /* fallback on defaults */ }

                pd.PrintPage += (s, ev) => PrintInvoicePage(ev, invoiceId);

                using var dlg = new PrintDialog { Document = pd, UseEXDialog = true };
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    pd.Print();
                }

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving/printing bill:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintInvoicePage(PrintPageEventArgs ev, long invoiceId)
        {
            var g = ev.Graphics;
            float y = 20f;
            float left = 20f;
            float right = ev.PageBounds.Width - 20f;
            var font = new Font("Segoe UI", 10, FontStyle.Regular);
            var bold = new Font("Segoe UI", 10, FontStyle.Bold);
            var title = new Font("Segoe UI", 12, FontStyle.Bold);

            // Optional logo (assets/logo.png), scaled into 90x60 box
            Image? logoImg = null;
            try
            {
                string logoPath = Path.Combine(AppContext.BaseDirectory, "assets", "logo.png");
                if (File.Exists(logoPath))
                {
                    byte[] bytes = File.ReadAllBytes(logoPath);
                    var ms = new System.IO.MemoryStream(bytes);
                    logoImg = Image.FromStream(ms);
                }
            }
            catch { }

            // Company header (match screenshot)
            if (logoImg != null)
            {
                float maxW = 90f, maxH = 60f;
                float scale = Math.Min(maxW / logoImg.Width, maxH / logoImg.Height);
                float w = logoImg.Width * scale;
                float h = logoImg.Height * scale;
                g.DrawImage(logoImg, left, y, w, h);
            }
            float headerX = (logoImg != null) ? left + 100f : left;
            g.DrawString("Isuru Service Center", title, Brushes.Black, headerX, y); y += 20f;
            g.DrawString("Your Address Here", font, Brushes.Black, headerX, y); y += 18f;
            g.DrawString("Tel: +94-000-000000", font, Brushes.Black, headerX, y); y += 26f;
            g.DrawLine(Pens.Black, left, y, right, y); y += 10f;

            // Load header/invoice data
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string billNo = ""; string dateStr = ""; decimal totalAmt = 0m;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT bill_no, date_time, total_amount FROM invoices WHERE id=$id";
                cmd.Parameters.AddWithValue("$id", invoiceId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    billNo = reader.GetString(0);
                    dateStr = reader.GetString(1);
                    totalAmt = reader.GetDecimal(2);
                }
            }

            g.DrawString("Invoice", title, Brushes.Black, left, y); y += 24f;

            // Customer / Vehicle info
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT c.name, c.phone, v.vehicle_no, v.vehicle_type
                                     FROM invoices i
                                     JOIN customers c ON i.customer_id = c.id
                                     JOIN vehicles v ON i.vehicle_id = v.id
                                     WHERE i.id=$id";
                cmd.Parameters.AddWithValue("$id", invoiceId);
                using var rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    string custName = rdr.IsDBNull(0) ? "" : rdr.GetString(0);
                    string phone = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                    string vehNo = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                    string vehType = rdr.IsDBNull(3) ? "" : rdr.GetString(3);
                    g.DrawString($"Bill No: {billNo}", font, Brushes.Black, left, y); y += 20f;
                    g.DrawString($"Date/Time: {dateStr}", font, Brushes.Black, left, y); y += 20f;
                    g.DrawString($"Customer: {custName} ({phone})", font, Brushes.Black, left, y); y += 20f;
                    g.DrawString($"Vehicle: {vehNo} ({vehType})", font, Brushes.Black, left, y); y += 20f;
                }
            }

            // Separator before table
            g.DrawLine(Pens.Black, left, y, right, y); y += 10f;

            // Table headers (match positions from screenshot/dialog)
            float colService = left;
            float colDesc = 200f;
            float colQty = 530f;
            float colAmt = 610f;
            g.DrawString("Service", bold, Brushes.Black, colService, y);
            g.DrawString("Description", bold, Brushes.Black, colDesc, y);
            g.DrawString("Qty", bold, Brushes.Black, colQty, y);
            g.DrawString("Amount", bold, Brushes.Black, colAmt, y);
            y += 22f;

            // Line under header
            g.DrawLine(Pens.Black, left, y, right, y); y += 6f;

            decimal subtotal = 0m;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT service, description, qty, amount FROM invoice_items WHERE invoice_id=$iid";
                cmd.Parameters.AddWithValue("$iid", invoiceId);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string svc = rdr.IsDBNull(0) ? "" : rdr.GetString(0);
                    string desc = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                    decimal qty = rdr.IsDBNull(2) ? 0m : rdr.GetDecimal(2);
                    decimal amt = rdr.IsDBNull(3) ? 0m : rdr.GetDecimal(3);
                    subtotal += amt;

                    g.DrawString(svc, font, Brushes.Black, colService, y);
                    g.DrawString(desc, font, Brushes.Black, colDesc, y);
                    g.DrawString(qty.ToString("0.##"), font, Brushes.Black, colQty, y);
                    g.DrawString(amt.ToString("0.00"), font, Brushes.Black, colAmt, y);
                    y += 20f;
                }
            }

            // Totals block aligned to right edge like screenshot
            g.DrawLine(Pens.Black, left, y, right, y); y += 6f;
            string subText = $"Subtotal: {subtotal:0.00}";
            decimal discount = Math.Max(0m, subtotal - totalAmt);
            string discText = $"Discount: {discount:0.00}";
            string totText = $"Total: {totalAmt:0.00}";
            SizeF wSub = g.MeasureString(subText, bold);
            SizeF wDisc = g.MeasureString(discText, bold);
            SizeF wTot = g.MeasureString(totText, bold);
            g.DrawString(subText, bold, Brushes.Black, right - wSub.Width, y); y += 18f;
            g.DrawString(discText, bold, Brushes.Black, right - wDisc.Width, y); y += 18f;
            g.DrawString(totText, bold, Brushes.Black, right - wTot.Width, y); y += 24f;

            ev.HasMorePages = false;
        }

        // Rollback-format printer page: monospace text, narrow width columns
        private void PrintInvoiceRollback(PrintPageEventArgs ev, long invoiceId)
        {
            var g = ev.Graphics;
            float x = ev.MarginBounds.Left;
            float y = ev.MarginBounds.Top;
            // Compact font for thermal (58mm)
            float lineHeight = 13f;
            using var font = new Font("Courier New", 7.5f, FontStyle.Regular);
            using var fontBold = new Font("Courier New", 7.5f, FontStyle.Bold);

            // Helper to draw a line and advance
            void DrawLine(string text, bool bold = false)
            {
                g.DrawString(text, bold ? fontBold : font, Brushes.Black, x, y);
                y += lineHeight;
            }

            // Load header/invoice data
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            string billNo = ""; string dateStr = ""; decimal totalAmt = 0m;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT bill_no, date_time, total_amount FROM invoices WHERE id=$id";
                cmd.Parameters.AddWithValue("$id", invoiceId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    billNo = reader.GetString(0);
                    dateStr = reader.GetString(1);
                    totalAmt = reader.GetDecimal(2);
                }
            }

            string cust = ""; string phone = ""; string veh = ""; string vtype = "";
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT c.name, c.phone, v.vehicle_no, v.vehicle_type
                                     FROM invoices i
                                     JOIN customers c ON i.customer_id = c.id
                                     JOIN vehicles v ON i.vehicle_id = v.id
                                     WHERE i.id=$id";
                cmd.Parameters.AddWithValue("$id", invoiceId);
                using var rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    cust = rdr.GetString(0);
                    phone = rdr.GetString(1);
                    veh = rdr.GetString(2);
                    vtype = rdr.GetString(3);
                }
            }

            // Header
            DrawLine("ISURU SERVICE STATION", bold: true);
            DrawLine("INVOICE", bold: true);
            // Separator tuned for narrow roll width
            DrawLine(new string('-', 40));
            DrawLine($"Bill: {billNo}");
            DrawLine($"Date: {dateStr}");
            if (!string.IsNullOrWhiteSpace(cust)) DrawLine($"Cust: {cust}");
            if (!string.IsNullOrWhiteSpace(phone)) DrawLine($"Phone: {phone}");
            if (!string.IsNullOrWhiteSpace(veh)) DrawLine($"Vehicle: {veh} ({vtype})");
            DrawLine(new string('-', 40));

            // Rollback columns without RATE: align Qty and Amt left within the bill width
            string HeaderRow() => PadRight("Item", 22) + PadRight("Qty", 6) + PadRight("Amt", 12);
            DrawLine(HeaderRow(), bold: true);
            DrawLine(new string('-', 40));

            decimal subtotal = 0m;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT service, qty, rate, amount FROM invoice_items WHERE invoice_id=$iid";
                cmd.Parameters.AddWithValue("$iid", invoiceId);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string svc = rdr.GetString(0);
                    decimal qty = rdr.GetDecimal(1);
                    decimal rate = rdr.GetDecimal(2); // kept if needed for future logic
                    decimal amt = rdr.GetDecimal(3);
                    subtotal += amt;

                    // Compose row without rate column; left-align Qty and Amt
                    string line = PadRight(Truncate(svc, 22), 22)
                                 + PadRight(qty.ToString("0.##"), 6)
                                 + PadRight(amt.ToString("0.00"), 12);
                    DrawLine(line);
                }
            }

            DrawLine(new string('-', 40));

            decimal discount = 0m;
            decimal.TryParse(txtDiscount.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out discount);
            decimal grandTotal = subtotal - discount;

            // Totals under Amount column in rollback: left-aligned amounts for consistent left layout
            DrawLine(PadRight("Sub Total", 28) + PadRight(subtotal.ToString("0.00"), 12), bold: true);
            DrawLine(PadRight("(-) Discount", 28) + PadRight(discount.ToString("0.00"), 12), bold: true);
            DrawLine(PadRight("TOTAL", 28) + PadRight(grandTotal.ToString("0.00"), 12), bold: true);
            DrawLine(new string('-', 40));
            DrawLine("Thank you!", bold: true);

            ev.HasMorePages = false;

            // Local helpers for fixed-width formatting
            static string PadRight(string? s, int w)
            {
                var t = s ?? string.Empty;
                return t.Length >= w ? t.Substring(0, w) : t.PadRight(w);
            }
            static string PadLeft(string? s, int w)
            {
                var t = s ?? string.Empty;
                return t.Length >= w ? t.Substring(0, w) : t.PadLeft(w);
            }
            static string Truncate(string? s, int w)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                return s!.Length > w ? s.Substring(0, w) : s;
            }
        }

        private bool ValidateBill()
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Please enter customer name.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtVehicleNo.Text))
            {
                MessageBox.Show("Please enter vehicle number.");
                return false;
            }

            bool hasItem = false;
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;
                var service = row.Cells["Service"].Value?.ToString();
                decimal amount = GetCellDecimal(row, "Amount");

                if (!string.IsNullOrWhiteSpace(service) && amount > 0)
                {
                    hasItem = true;
                    break;
                }
            }

            if (!hasItem)
            {
                MessageBox.Show("Please add at least one service/item.");
                return false;
            }

            // Validate contact number: exactly 10 digits
            var phoneDigits = new string((txtContact.Text ?? string.Empty).Where(char.IsDigit).ToArray());
            if (phoneDigits.Length != 10)
            {
                MessageBox.Show("Please enter a valid 10-digit contact number (digits only).");
                txtContact.Focus();
                return false;
            }

            return true;
        }

        private long GetOrCreateCustomer(SqliteConnection conn)
        {
            string name = txtCustomerName.Text.Trim();
            string phone = new string((txtContact.Text ?? string.Empty).Where(char.IsDigit).ToArray());

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT id FROM customers WHERE name = $name AND phone = $phone LIMIT 1;";
                cmd.Parameters.AddWithValue("$name", name);
                cmd.Parameters.AddWithValue("$phone", phone);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    return (long)result;
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO customers (name, phone) VALUES ($name, $phone);
                                    SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("$name", name);
                cmd.Parameters.AddWithValue("$phone", phone);

                var id = (long)cmd.ExecuteScalar();
                return id;
            }
        }

        // Enforce numeric-only input and max length for contact number
        private void TxtContact_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void TxtContact_TextChanged(object? sender, EventArgs e)
        {
            // Keep only digits and limit to 10
            var digits = new string((txtContact.Text ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length > 10) digits = digits.Substring(0, 10);
            if (digits != txtContact.Text)
            {
                int pos = txtContact.SelectionStart;
                txtContact.Text = digits;
                txtContact.SelectionStart = Math.Min(pos, txtContact.Text.Length);
            }
        }

        private long GetOrCreateVehicle(SqliteConnection conn, long customerId)
        {
            string vehicleNo = txtVehicleNo.Text.Trim();
            string vehicleType = cmbVehicleType.SelectedItem?.ToString() ?? "";

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT id FROM vehicles 
                                    WHERE customer_id = $cid AND vehicle_no = $vno 
                                    LIMIT 1;";
                cmd.Parameters.AddWithValue("$cid", customerId);
                cmd.Parameters.AddWithValue("$vno", vehicleNo);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    return (long)result;
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO vehicles (customer_id, vehicle_no, vehicle_type)
                                    VALUES ($cid, $vno, $vtype);
                                    SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("$cid", customerId);
                cmd.Parameters.AddWithValue("$vno", vehicleNo);
                cmd.Parameters.AddWithValue("$vtype", vehicleType);

                var id = (long)cmd.ExecuteScalar();
                return id;
            }
        }

        private long InsertInvoice(SqliteConnection conn, long customerId, long vehicleId)
        {
            decimal total = 0m;
            decimal.TryParse(
                txtTotal.Text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out total
            );

            string billNo = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string paymentMethod = (cmbPayment?.SelectedItem?.ToString() ?? "Cash");
            string paymentStatus = "paid";
            if (string.Equals(paymentMethod, "Credit", StringComparison.OrdinalIgnoreCase))
                paymentStatus = "unpaid";
            else if (string.Equals(paymentMethod, "Cheque", StringComparison.OrdinalIgnoreCase))
                paymentStatus = "pending";

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO invoices 
                    (bill_no, customer_id, vehicle_id, date_time, total_amount, payment_method, payment_status)
                VALUES
                    ($billno, $cid, $vid, $dt, $total, $pmethod, $pstatus);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("$billno", billNo);
            cmd.Parameters.AddWithValue("$cid", customerId);
            cmd.Parameters.AddWithValue("$vid", vehicleId);
            cmd.Parameters.AddWithValue("$dt", dateTime);
            cmd.Parameters.AddWithValue("$total", total);
            cmd.Parameters.AddWithValue("$pmethod", paymentMethod);
            cmd.Parameters.AddWithValue("$pstatus", paymentStatus);

            var id = (long)cmd.ExecuteScalar();
            return id;
        }

        private void InsertInvoiceItems(SqliteConnection conn, long invoiceId)
        {
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;

                var service = row.Cells["Service"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(service)) continue;

                var itemId = row.Cells["ItemId"].Value?.ToString() ?? "";
                var description = row.Cells["Description"].Value?.ToString() ?? "";
                decimal qty = GetCellDecimal(row, "Qty");
                decimal rate = GetCellDecimal(row, "Rate");
                decimal amount = GetCellDecimal(row, "Amount");

                // Look up current buying_price from inventory to snapshot at sale time
                decimal buyingPrice = 0;
                if (!string.IsNullOrWhiteSpace(itemId))
                {
                    try
                    {
                        using var bpCmd = conn.CreateCommand();
                        bpCmd.CommandText = "SELECT COALESCE(buying_price, 0) FROM inventory WHERE item_id = $itemid";
                        bpCmd.Parameters.AddWithValue("$itemid", itemId);
                        var bpResult = bpCmd.ExecuteScalar();
                        if (bpResult != null && bpResult != DBNull.Value)
                            buyingPrice = Convert.ToDecimal(bpResult);
                    }
                    catch { }
                }

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO invoice_items
                        (invoice_id, item_id, service, description, qty, rate, amount, buying_price)
                    VALUES
                        ($iid, $itemid, $service, $desc, $qty, $rate, $amount, $bp);";

                cmd.Parameters.AddWithValue("$iid", invoiceId);
                cmd.Parameters.AddWithValue("$itemid", itemId);
                cmd.Parameters.AddWithValue("$service", service);
                cmd.Parameters.AddWithValue("$desc", description);
                cmd.Parameters.AddWithValue("$qty", qty);
                cmd.Parameters.AddWithValue("$rate", rate);
                cmd.Parameters.AddWithValue("$amount", amount);
                cmd.Parameters.AddWithValue("$bp", buyingPrice);

                cmd.ExecuteNonQuery();

                // Real-time stock deduction for inventory-rated services when Item ID is provided
                if (!string.IsNullOrWhiteSpace(itemId) && IsInventoryRatedService(service) && qty > 0)
                {
                    try
                    {
                        using var stockCmd = conn.CreateCommand();
                        // Ensure the item exists; then deduct quantity without going below zero
                        stockCmd.CommandText = @"
                            UPDATE inventory
                            SET quantity = CASE
                                WHEN quantity - $deduct < 0 THEN 0
                                ELSE quantity - $deduct
                            END,
                                updated_at = $ts
                            WHERE item_id = $itemid;";
                        stockCmd.Parameters.AddWithValue("$deduct", (double)qty);
                        stockCmd.Parameters.AddWithValue("$ts", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        stockCmd.Parameters.AddWithValue("$itemid", itemId);
                        stockCmd.ExecuteNonQuery();
                    }
                    catch { /* swallow inventory update errors to not block billing */ }
                }
            }
        }

        // =======================
        //   HISTORY / INVENTORY
        // =======================

        private void BtnHistory_Click(object? sender, EventArgs e)
        {
            using var form = new BillHistoryForm();
            form.ShowDialog(this);
        }

        private void BtnInventory_Click(object? sender, EventArgs e)
        {
            bool readOnly = string.Equals(_userRole, "User", StringComparison.OrdinalIgnoreCase);
            using var form = new InventoryForm(readOnly);
            form.ShowDialog(this);
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            try
            {
                this.Hide();
                using (var login = new LoginForm())
                {
                    var result = login.ShowDialog(this);
                    if (result == DialogResult.OK && login.IsAuthenticated)
                    {
                        // Update role after re-login
                        var newRole = string.IsNullOrWhiteSpace(login.UserRole) ? "Admin" : login.UserRole;
                        typeof(BillingForm)
                            .GetField("_userRole", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                            ?.SetValue(this, newRole);
                        // Successful re-login; show the form again
                        this.Show();
                        this.Activate();
                        ApplyRoleRestrictions();
                        ClearForm();
                    }
                    else
                    {
                        // Not authenticated; close the main form and exit
                        this.Close();
                        Application.Exit();
                    }
                }
            }
            catch
            {
                // Fallback: ensure app closes on unexpected errors
                this.Close();
                Application.Exit();
            }
        }

        // =======================
        //       SERVICES MENU
        // =======================
        private void MenuServices_Click(object? sender, EventArgs e)
        {
            using var form = new ServicesForm(_userRole);
            form.ShowDialog(this);
            // Refresh services dropdown after changes
            LoadServiceDropdown();
        }

        // =======================
        //   CLEAR FORM
        // =======================

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtCustomerName.Clear();
            txtVehicleNo.Clear();
            txtContact.Clear();
            cmbVehicleType.SelectedIndex = -1;

            dgvItems.Rows.Clear();

            txtSubtotal.Text = "0.00";
            txtDiscount.Text = "";
            txtTotal.Text = "0.00";

            txtCustomerName.Focus();
        }
    }
}
