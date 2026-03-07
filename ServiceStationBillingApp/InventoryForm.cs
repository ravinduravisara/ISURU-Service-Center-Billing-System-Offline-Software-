using System;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using Microsoft.Data.Sqlite;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace ServiceStationBillingApp
{
    public partial class InventoryForm : Form
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly bool _readOnly;
        public InventoryForm(bool readOnly = false)
        {
            InitializeComponent();
            _readOnly = readOnly;
            // Use centralized Database helpers for path and initialization
            _dbPath = Database.GetDbPath();
            _connectionString = Database.ConnectionString;

            Database.Ensure();
            SetupGrid();
            HookEvents();
            LoadInventory();
            LoadSearchAutoComplete();

            ApplyReadOnlyMode();
        }

        private void SetupGrid()
        {
            dgvInventory.Columns.Clear();
            dgvInventory.ReadOnly = false;
            dgvInventory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInventory.MultiSelect = true;
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.Columns.Add("Id", "ID");
            dgvInventory.Columns["Id"].Visible = false;
            dgvInventory.Columns.Add("ItemId", "Item ID");
            dgvInventory.Columns.Add("ItemName", "Category");
            dgvInventory.Columns.Add("Brand", "Brand");
            dgvInventory.Columns.Add("Quantity", "Quantity");
            dgvInventory.Columns.Add("UnitPrice", "Unit Price");
            dgvInventory.Columns.Add("BuyingPrice", "Buying Price");
            dgvInventory.Columns.Add("Amount", "Amount");
            dgvInventory.Columns.Add("UpdatedAt", "Updated At");

            // Supplier ID (editable)
            dgvInventory.Columns.Add("SupplierId", "Supplier ID");
            dgvInventory.Columns["SupplierId"].ReadOnly = false;

            // Payment dropdown
            var colPayment = new DataGridViewComboBoxColumn
            {
                Name = "Payment",
                HeaderText = "Payment",
                FlatStyle = FlatStyle.Flat
            };
            colPayment.Items.AddRange(new object[] { "Cash", "Credit", "Cheque" });
            dgvInventory.Columns.Add(colPayment);

            // Keep core columns read-only
            dgvInventory.Columns["ItemId"].ReadOnly = true;
            dgvInventory.Columns["ItemName"].ReadOnly = true;
            dgvInventory.Columns["Brand"].ReadOnly = true;
            dgvInventory.Columns["Quantity"].ReadOnly = true;
            dgvInventory.Columns["UnitPrice"].ReadOnly = true;
            dgvInventory.Columns["BuyingPrice"].ReadOnly = true;
            dgvInventory.Columns["Amount"].ReadOnly = true;
            dgvInventory.Columns["UpdatedAt"].ReadOnly = true;
            dgvInventory.EditMode = DataGridViewEditMode.EditOnEnter;
        }

        private void HookEvents()
        {
            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;
            btnUpdate.Click += BtnUpdate_Click;
            
            btnExportPdf.Click += BtnExportPdf_Click;
            btnImportCsv.Click += BtnImportCsv_Click;
            mnuEdit.Click += MnuEdit_Click;
            mnuDelete.Click += MnuDelete_Click;
            dgvInventory.CellMouseDown += DgvInventory_CellMouseDown;
            dgvInventory.CellEndEdit += DgvInventory_CellEndEdit;
            dgvInventory.SelectionChanged += DgvInventory_SelectionChanged;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtQuantity.KeyPress += NumericTextBox_KeyPress;
            txtUnitPrice.KeyPress += NumericTextBox_KeyPress;
            cmbCategory.SelectedIndex = 0;

            // Keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.A) { dgvInventory.SelectAll(); e.Handled = true; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Delete && !txtSearch.Focused && !txtQuantity.Focused && !txtUnitPrice.Focused && !txtBuyingPrice.Focused && !txtBrand.Focused && !txtSupplierId.Focused) { BtnDelete_Click(s, e); e.Handled = true; }
                else if (e.KeyCode == Keys.Escape) { this.Close(); e.Handled = true; }
            };
        }

        private void DgvInventory_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var row = dgvInventory.Rows[e.RowIndex];
            if (row.Cells["Id"].Value == null) return;
            long id = Convert.ToInt64(row.Cells["Id"].Value);

            string colName = dgvInventory.Columns[e.ColumnIndex].Name;
            if (colName == "SupplierId" || colName == "Payment")
            {
                string supplierCode = Convert.ToString(row.Cells["SupplierId"].Value) ?? string.Empty;
                string payment = Convert.ToString(row.Cells["Payment"].Value) ?? string.Empty;
                if (payment != "Cash" && payment != "Credit" && payment != "Cheque") payment = "Cash";

                using var conn = Database.OpenConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE inventory SET supplier_code=$sup, payment_method=$pay WHERE id=$id";
                cmd.Parameters.AddWithValue("$sup", supplierCode);
                cmd.Parameters.AddWithValue("$pay", payment);
                cmd.Parameters.AddWithValue("$id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private void BtnImportCsv_Click(object? sender, EventArgs e)
        {
            // Expect CSV with headers: ItemId,Price (any brand). We'll prompt for Category and Brand first.
            // Pop-up to choose Category and Brand applied to all imported rows.
            if (!PromptCategoryAndBrand(out string chosenCategory, out string chosenBrand))
                return;

            using var ofd = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Import CSV"
            };
            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            int imported = 0;
            try
            {
                var lines = File.ReadAllLines(ofd.FileName);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Selected file is empty.");
                    return;
                }

                // Detect header
                int startIdx = 0;
                if (lines[0].IndexOf("OE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    lines[0].IndexOf("Part", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    startIdx = 1;
                }

                using var conn = Database.OpenConnection();
                using var tx = conn.BeginTransaction();
                for (int i = startIdx; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Simple CSV split (handle quoted commas minimally)
                    var parts = SplitCsvLine(line);
                    if (parts.Count < 2) continue;

                    string itemId = parts[0].Trim().Trim('"');
                    string priceStr = parts[1].Trim().Trim('"');
                    if (string.IsNullOrWhiteSpace(itemId)) continue;
                    if (!double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                    {
                        // Try removing non-digits (e.g., LKR, commas)
                        var cleaned = new string(priceStr.Where(ch => char.IsDigit(ch) || ch == '.' || ch == ',').ToArray());
                        cleaned = cleaned.Replace(",", "");
                        if (!double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
                            continue;
                    }

                    // Insert with selected category/brand, item_id from CSV
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = @"INSERT OR IGNORE INTO inventory (item_id, item_name, brand, quantity, unit_price, updated_at)
                                        VALUES ($id, $name, $brand, $qty, $price, $ts)";
                    cmd.Parameters.AddWithValue("$id", itemId);
                    cmd.Parameters.AddWithValue("$name", chosenCategory);
                    cmd.Parameters.AddWithValue("$brand", chosenBrand);
                    cmd.Parameters.AddWithValue("$qty", 0);
                    cmd.Parameters.AddWithValue("$price", price);
                    cmd.Parameters.AddWithValue("$ts", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                    imported++;
                }
                tx.Commit();

                LoadInventory();
                LoadSearchAutoComplete();
                MessageBox.Show($"Imported {imported} item(s) into Inventory.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to import CSV: {ex.Message}");
            }
        }

        private bool PromptCategoryAndBrand(out string category, out string brand)
        {
            category = string.Empty;
            brand = string.Empty;

            using var dlg = new Form();
            dlg.Text = "Import CSV - Set Category & Brand";
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MinimizeBox = false;
            dlg.MaximizeBox = false;
            dlg.Width = 420;
            dlg.Height = 200;

            var lblCat = new Label { Left = 15, Top = 20, Width = 100, Text = "Category:" };
            var cmbCat = new ComboBox { Left = 120, Top = 16, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
            // Reuse same categories as main form
            foreach (var item in cmbCategory.Items)
                cmbCat.Items.Add(item);
            if (cmbCat.Items.Count > 0) cmbCat.SelectedIndex = 0;

            var lblBrand = new Label { Left = 15, Top = 60, Width = 100, Text = "Brand:" };
            var txtBrandLocal = new TextBox { Left = 120, Top = 56, Width = 260 };

            var btnOk = new Button { Left = 220, Top = 100, Width = 75, Text = "OK", DialogResult = DialogResult.OK };
            var btnCancel = new Button { Left = 305, Top = 100, Width = 75, Text = "Cancel", DialogResult = DialogResult.Cancel };

            dlg.Controls.Add(lblCat);
            dlg.Controls.Add(cmbCat);
            dlg.Controls.Add(lblBrand);
            dlg.Controls.Add(txtBrandLocal);
            dlg.Controls.Add(btnOk);
            dlg.Controls.Add(btnCancel);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                var catSel = cmbCat.SelectedItem?.ToString() ?? string.Empty;
                var brandSel = txtBrandLocal.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(catSel))
                {
                    MessageBox.Show("Please select a category.");
                    return false;
                }
                category = catSel;
                brand = brandSel;
                return true;
            }
            return false;
        }

        private static List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result;
        }

        private void ApplyReadOnlyMode()
        {
            if (_readOnly)
            {
                // Hide modification controls
                btnAdd.Visible = false;
                btnDelete.Visible = false;
                btnUpdate.Visible = false;
                btnImportCsv.Visible = false;
                
                mnuEdit.Visible = false;
                mnuDelete.Visible = false;

                // Disable item entry inputs
                cmbCategory.Enabled = false;
                txtQuantity.Enabled = false;
                txtUnitPrice.Enabled = false;
                txtBrand.Enabled = false;

                // Ensure grid remains read-only and selection-only for search
                dgvInventory.ReadOnly = true;
                dgvInventory.ContextMenuStrip = null; // remove context menu to avoid edits
            }
            else
            {
                // Admin: show full controls
                btnAdd.Visible = true;
                btnDelete.Visible = true;
                btnUpdate.Visible = true;
                btnImportCsv.Visible = true;
                
                mnuEdit.Visible = true;
                mnuDelete.Visible = true;

                cmbCategory.Enabled = true;
                txtQuantity.Enabled = true;
                txtUnitPrice.Enabled = true;
                txtBrand.Enabled = true;

                dgvInventory.ReadOnly = true; // keep grid read-only; edits via dialog/context
            }
        }

        private void LoadInventory()
        {
            dgvInventory.Rows.Clear();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id, item_id, item_name, brand, quantity, unit_price, updated_at, supplier_code, payment_method, COALESCE(buying_price, 0) FROM inventory ORDER BY item_name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int idx = dgvInventory.Rows.Add();
                var row = dgvInventory.Rows[idx];
                row.Cells["Id"].Value = reader.GetInt64(0);
                row.Cells["ItemId"].Value = reader.IsDBNull(1) ? "" : reader.GetString(1);
                row.Cells["ItemName"].Value = reader.GetString(2);
                row.Cells["Brand"].Value = reader.IsDBNull(3) ? "" : reader.GetString(3);
                row.Cells["Quantity"].Value = reader.GetDouble(4).ToString("0.##");
                row.Cells["UnitPrice"].Value = reader.GetDouble(5).ToString("0.00");
                row.Cells["BuyingPrice"].Value = reader.GetDouble(9).ToString("0.00");
                var amt = reader.GetDouble(4) * reader.GetDouble(5);
                row.Cells["Amount"].Value = amt.ToString("0.00");
                row.Cells["UpdatedAt"].Value = reader.GetString(6);
                row.Cells["SupplierId"].Value = reader.IsDBNull(7) ? "" : reader.GetString(7);
                var payRaw = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
                string payNorm;
                if (string.IsNullOrWhiteSpace(payRaw))
                    payNorm = "Cash";
                else if (payRaw.Trim().IndexOf("credit", StringComparison.OrdinalIgnoreCase) >= 0)
                    payNorm = "Credit";
                else if (payRaw.Trim().IndexOf("cheque", StringComparison.OrdinalIgnoreCase) >= 0)
                    payNorm = "Cheque";
                else
                    payNorm = "Cash";
                row.Cells["Payment"].Value = payNorm;
            }
        }

        private void LoadSearchAutoComplete()
        {
            var acs = new AutoCompleteStringCollection();
            using var conn = Database.OpenConnection();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT DISTINCT item_name FROM inventory WHERE item_name IS NOT NULL AND TRIM(item_name) <> '' ORDER BY item_name";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    acs.Add(reader.GetString(0));
                }
            }
            using (var cmd2 = conn.CreateCommand())
            {
                cmd2.CommandText = @"SELECT DISTINCT brand FROM inventory WHERE brand IS NOT NULL AND TRIM(brand) <> '' ORDER BY brand";
                using var reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    acs.Add(reader2.GetString(0));
                }
            }
            txtSearch.AutoCompleteCustomSource = acs;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var itemName = cmbCategory.SelectedItem?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(itemName))
            {
                MessageBox.Show("Please select a category.");
                return;
            }

            if (!double.TryParse(txtQuantity.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var qty))
            {
                MessageBox.Show("Please enter a valid quantity.");
                return;
            }

            if (!double.TryParse(txtUnitPrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            {
                MessageBox.Show("Please enter a valid unit price.");
                return;
            }

            double buyingPrice = 0;
            if (!string.IsNullOrWhiteSpace(txtBuyingPrice.Text))
            {
                if (!double.TryParse(txtBuyingPrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out buyingPrice) || buyingPrice < 0)
                {
                    MessageBox.Show("Please enter a valid buying price.");
                    return;
                }
            }

            var supplierCode = txtSupplierId.Text.Trim();
            var paymentMethod = (cmbPayment.SelectedItem?.ToString() ?? "Cash").Trim();
            if (paymentMethod != "Cash" && paymentMethod != "Credit" && paymentMethod != "Cheque") paymentMethod = "Cash";

            using var conn = Database.OpenConnection();
            string brand = txtBrand.Text.Trim();
            string ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        // Add quantity to an existing item when possible:
                        // 1) If user typed an Item ID (in Search box), update that exact item.
                        // 2) Otherwise match by Category + Brand + Unit Price (price-related item id).
                        long? existingId = null;
                        string searchItemId = txtSearch.Text.Trim();

                        if (!string.IsNullOrWhiteSpace(searchItemId))
                        {
                                using var findById = conn.CreateCommand();
                                findById.CommandText = @"SELECT id
                                                                                FROM inventory
                                                                                WHERE LOWER(TRIM(item_id)) = LOWER(TRIM($itemId))
                                                                                LIMIT 1";
                                findById.Parameters.AddWithValue("$itemId", searchItemId);
                                var found = findById.ExecuteScalar();
                                if (found != null && found != DBNull.Value)
                                        existingId = Convert.ToInt64(found);
                        }

                        if (!existingId.HasValue)
                        {
                                using var findByPrice = conn.CreateCommand();
                                findByPrice.CommandText = @"SELECT id
                                                                                     FROM inventory
                                                                                     WHERE LOWER(TRIM(item_name)) = LOWER(TRIM($name))
                                                                                         AND LOWER(TRIM(COALESCE(brand,''))) = LOWER(TRIM($brand))
                                                                                         AND ROUND(COALESCE(unit_price, 0), 2) = ROUND($price, 2)
                                                                                     ORDER BY updated_at DESC
                                                                                     LIMIT 1";
                                findByPrice.Parameters.AddWithValue("$name", itemName);
                                findByPrice.Parameters.AddWithValue("$brand", brand);
                                findByPrice.Parameters.AddWithValue("$price", price);
                                var found2 = findByPrice.ExecuteScalar();
                                if (found2 != null && found2 != DBNull.Value)
                                        existingId = Convert.ToInt64(found2);
                        }

            if (existingId.HasValue)
            {
                using var upd = conn.CreateCommand();
                upd.CommandText = @"UPDATE inventory
                                    SET quantity = COALESCE(quantity, 0) + $qty,
                                        unit_price = $price,
                                        buying_price = $bprice,
                                        updated_at = $ts,
                                        supplier_code = $sup,
                                        payment_method = $pay
                                    WHERE id = $id";
                upd.Parameters.AddWithValue("$qty", qty);
                upd.Parameters.AddWithValue("$price", price);
                upd.Parameters.AddWithValue("$bprice", buyingPrice);
                upd.Parameters.AddWithValue("$ts", ts);
                upd.Parameters.AddWithValue("$sup", supplierCode);
                upd.Parameters.AddWithValue("$pay", paymentMethod);
                upd.Parameters.AddWithValue("$id", existingId.Value);
                upd.ExecuteNonQuery();

                // Insert stock movement entry referencing the item's item_id
                string existingItemId;
                using (var getItem = conn.CreateCommand())
                {
                    getItem.CommandText = "SELECT item_id FROM inventory WHERE id=$id";
                    getItem.Parameters.AddWithValue("$id", existingId.Value);
                    existingItemId = Convert.ToString(getItem.ExecuteScalar()) ?? string.Empty;
                }
                using (var mv = conn.CreateCommand())
                {
                    mv.CommandText = @"INSERT INTO stock_movements(item_id, supplier_code, quantity, unit_price, buying_price, payment_method, moved_at)
                                       VALUES($itemId, $sup, $qty, $price, $bprice, $pay, $ts)";
                    mv.Parameters.AddWithValue("$itemId", existingItemId);
                    mv.Parameters.AddWithValue("$sup", supplierCode);
                    mv.Parameters.AddWithValue("$qty", qty);
                    mv.Parameters.AddWithValue("$price", price);
                    mv.Parameters.AddWithValue("$bprice", buyingPrice);
                    mv.Parameters.AddWithValue("$pay", paymentMethod);
                    mv.Parameters.AddWithValue("$ts", ts);
                    mv.ExecuteNonQuery();
                }
            }
            else
            {
                using var cmd = conn.CreateCommand();
                var newItemId = GenerateItemId(itemName);
                cmd.CommandText = @"INSERT INTO inventory (item_id, item_name, brand, quantity, unit_price, buying_price, updated_at, supplier_code, payment_method)
                                    VALUES ($itemId, $name, $brand, $qty, $price, $bprice, $ts, $sup, $pay)";
                cmd.Parameters.AddWithValue("$itemId", newItemId);
                cmd.Parameters.AddWithValue("$name", itemName);
                cmd.Parameters.AddWithValue("$brand", brand);
                cmd.Parameters.AddWithValue("$qty", qty);
                cmd.Parameters.AddWithValue("$price", price);
                cmd.Parameters.AddWithValue("$bprice", buyingPrice);
                cmd.Parameters.AddWithValue("$ts", ts);
                cmd.Parameters.AddWithValue("$sup", supplierCode);
                cmd.Parameters.AddWithValue("$pay", paymentMethod);
                cmd.ExecuteNonQuery();

                // Insert stock movement for this new item
                using (var mv = conn.CreateCommand())
                {
                    mv.CommandText = @"INSERT INTO stock_movements(item_id, supplier_code, quantity, unit_price, buying_price, payment_method, moved_at)
                                       VALUES($itemId, $sup, $qty, $price, $bprice, $pay, $ts)";
                    mv.Parameters.AddWithValue("$itemId", newItemId);
                    mv.Parameters.AddWithValue("$sup", supplierCode);
                    mv.Parameters.AddWithValue("$qty", qty);
                    mv.Parameters.AddWithValue("$price", price);
                    mv.Parameters.AddWithValue("$bprice", buyingPrice);
                    mv.Parameters.AddWithValue("$pay", paymentMethod);
                    mv.Parameters.AddWithValue("$ts", ts);
                    mv.ExecuteNonQuery();
                }
            }

            cmbCategory.SelectedIndex = 0;
            txtQuantity.Clear();
            txtUnitPrice.Clear();
            txtBuyingPrice.Clear();
            txtSupplierId.Clear();
            cmbPayment.SelectedIndex = 0;
            LoadInventory();
            LoadSearchAutoComplete();
        }

        private string GenerateItemId(string category)
        {
            string prefix = new string((category ?? string.Empty)
                .ToUpperInvariant()
                .Replace(" ", "")
                .Where(char.IsLetter)
                .Take(3)
                .ToArray());
            if (string.IsNullOrEmpty(prefix)) prefix = "CAT";

            using var conn = Database.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT item_id FROM inventory WHERE item_name = $cat AND item_id LIKE $pref || '-%' ORDER BY item_id DESC LIMIT 1";
            cmd.Parameters.AddWithValue("$cat", category);
            cmd.Parameters.AddWithValue("$pref", prefix);
            var last = cmd.ExecuteScalar() as string;
            int next = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var parts = last.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out var num))
                {
                    next = num + 1;
                }
            }
            return $"{prefix}-{next.ToString("0000")}";
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (_readOnly)
            {
                return;
            }

            if (dgvInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an item to update.");
                return;
            }

            var row = dgvInventory.SelectedRows[0];
            if (row.Cells["Id"].Value == null)
            {
                MessageBox.Show("Invalid selection.");
                return;
            }

            int id = Convert.ToInt32(row.Cells["Id"].Value);
            string category = cmbCategory.SelectedItem?.ToString() ?? string.Empty;
            string brand = txtBrand.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(category))
            {
                MessageBox.Show("Category is required.");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Enter a valid quantity.");
                return;
            }
            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Enter a valid unit price.");
                return;
            }

            decimal buyingPriceUpd = 0;
            if (!string.IsNullOrWhiteSpace(txtBuyingPrice.Text))
            {
                if (!decimal.TryParse(txtBuyingPrice.Text, out buyingPriceUpd) || buyingPriceUpd < 0)
                {
                    MessageBox.Show("Enter a valid buying price.");
                    return;
                }
            }

            using var conn = Database.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE inventory SET item_name = $name, brand = $brand, quantity = $qty, unit_price = $price, buying_price = $bprice, updated_at = CURRENT_TIMESTAMP, supplier_code=$sup, payment_method=$pay WHERE id = $id";
            cmd.Parameters.AddWithValue("$name", category);
            cmd.Parameters.AddWithValue("$brand", brand);
            cmd.Parameters.AddWithValue("$qty", quantity);
            cmd.Parameters.AddWithValue("$price", unitPrice);
            cmd.Parameters.AddWithValue("$bprice", buyingPriceUpd);
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$sup", txtSupplierId.Text.Trim());
            var payUpd = (cmbPayment.SelectedItem?.ToString() ?? "Cash");
            if (payUpd != "Cash" && payUpd != "Credit" && payUpd != "Cheque") payUpd = "Cash";
            cmd.Parameters.AddWithValue("$pay", payUpd);
            cmd.ExecuteNonQuery();

            LoadInventory();
            MessageBox.Show("Item updated.");
        }

        private void DgvInventory_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                dgvInventory.ClearSelection();
                dgvInventory.Rows[e.RowIndex].Selected = true;
            }
        }

        private void DgvInventory_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0) return;
            var row = dgvInventory.SelectedRows[0];
                var nameValSel = Convert.ToString(row.Cells["ItemName"].Value) ?? string.Empty;
                if (!string.IsNullOrEmpty(nameValSel))
                {
                    var idxSel = cmbCategory.Items.IndexOf(nameValSel);
                    if (idxSel >= 0) cmbCategory.SelectedIndex = idxSel;
                }
            txtBrand.Text = Convert.ToString(row.Cells["Brand"].Value) ?? string.Empty;
            txtQuantity.Text = Convert.ToString(row.Cells["Quantity"].Value) ?? string.Empty;
            txtUnitPrice.Text = Convert.ToString(row.Cells["UnitPrice"].Value) ?? string.Empty;
            txtBuyingPrice.Text = Convert.ToString(row.Cells["BuyingPrice"].Value) ?? string.Empty;
            txtSupplierId.Text = Convert.ToString(row.Cells["SupplierId"].Value) ?? string.Empty;
            var payVal = Convert.ToString(row.Cells["Payment"].Value) ?? "Cash";
            var payIdx = cmbPayment.Items.IndexOf(payVal);
            cmbPayment.SelectedIndex = payIdx >= 0 ? payIdx : 0;
            if (!_readOnly)
            {
                btnUpdate.Enabled = true;
            }
        }

        private void MnuEdit_Click(object? sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0) return;
            var row = dgvInventory.SelectedRows[0];
            long id = Convert.ToInt64(row.Cells["Id"].Value);

            // Simple inline edit: load values to inputs
            var nameValEdit = Convert.ToString(row.Cells["ItemName"].Value) ?? string.Empty;
            if (!string.IsNullOrEmpty(nameValEdit))
            {
                var idxEdit = cmbCategory.Items.IndexOf(nameValEdit);
                if (idxEdit >= 0) cmbCategory.SelectedIndex = idxEdit;
            }
            txtQuantity.Text = Convert.ToString(row.Cells["Quantity"].Value) ?? string.Empty;
            txtUnitPrice.Text = Convert.ToString(row.Cells["UnitPrice"].Value) ?? string.Empty;

            var result = MessageBox.Show("Update this item with the current input values?", "Confirm Update", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                if (cmbCategory.SelectedItem == null)
                {
                    MessageBox.Show("Please select a category.");
                    return;
                }
                if (!double.TryParse(txtQuantity.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var qty))
                {
                    MessageBox.Show("Please enter a valid quantity.");
                    return;
                }
                if (!double.TryParse(txtUnitPrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    MessageBox.Show("Please enter a valid unit price.");
                    return;
                }

                using var conn = Database.OpenConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE inventory SET item_name=$name, brand=$brand, quantity=$qty, unit_price=$price, updated_at=$ts WHERE id=$id";
                cmd.Parameters.AddWithValue("$name", cmbCategory.SelectedItem?.ToString() ?? string.Empty);
                cmd.Parameters.AddWithValue("$brand", txtBrand.Text.Trim());
                cmd.Parameters.AddWithValue("$qty", qty);
                cmd.Parameters.AddWithValue("$price", price);
                cmd.Parameters.AddWithValue("$ts", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("$id", id);
                cmd.ExecuteNonQuery();

                cmbCategory.SelectedIndex = 0;
                txtQuantity.Clear();
                txtUnitPrice.Clear();
                LoadInventory();
                LoadSearchAutoComplete();
            }
        }

        private void MnuDelete_Click(object? sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0) return;
            int count = dgvInventory.SelectedRows.Count;
            var result = MessageBox.Show($"Delete {count} selected item(s)?", "Confirm Delete", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                using var conn = Database.OpenConnection();
                using var tx = conn.BeginTransaction();
                foreach (DataGridViewRow r in dgvInventory.SelectedRows)
                {
                    long id = Convert.ToInt64(r.Cells["Id"].Value);
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM inventory WHERE id=$id";
                    cmd.Parameters.AddWithValue("$id", id);
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
                LoadInventory();
                LoadSearchAutoComplete();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to delete.");
                return;
            }
            int count = dgvInventory.SelectedRows.Count;
            var result = MessageBox.Show($"Delete {count} selected item(s)?", "Confirm Delete", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                using var conn = Database.OpenConnection();
                using var tx = conn.BeginTransaction();
                foreach (DataGridViewRow r in dgvInventory.SelectedRows)
                {
                    long id = Convert.ToInt64(r.Cells["Id"].Value);
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = @"DELETE FROM inventory WHERE id=$id";
                    cmd.Parameters.AddWithValue("$id", id);
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
                LoadInventory();
                LoadSearchAutoComplete();
            }
            // Disable update if selection cleared after delete
            if (dgvInventory.SelectedRows.Count == 0)
            {
                btnUpdate.Enabled = false;
            }
        }

        // Invert selection removed per request; multi-select + Delete remains

        

        private void BtnExportPdf_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                FileName = "inventory.pdf"
            };
            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            QuestPDF.Settings.License = LicenseType.Community;

            var items = new List<(string ItemId, string Name, string Brand, string Qty, string Price, string Updated)>();
            using (var conn = Database.OpenConnection())
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT item_id, item_name, brand, quantity, unit_price, updated_at FROM inventory ORDER BY item_name";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    var name = reader.GetString(1);
                    var brand = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var qty = reader.GetDouble(3).ToString("0.##", CultureInfo.InvariantCulture);
                    var price = reader.GetDouble(4).ToString("0.00", CultureInfo.InvariantCulture);
                    var ts = reader.GetString(5);
                    items.Add((id, name, brand, qty, price, ts));
                }
            }

            // Optional logo - standardized path: assets/logo.png
            byte[]? logoBytes = null;
            try
            {
                string logoPath = Path.Combine(AppContext.BaseDirectory, "assets", "logo.png");
                if (File.Exists(logoPath))
                {
                    logoBytes = File.ReadAllBytes(logoPath);
                }
            }
            catch { /* ignore logo errors */ }

            string businessName = "Isuru Service Station";
            string reportTitle = "Inventory Report";
            string reportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string filterText = string.IsNullOrWhiteSpace(txtSearch.Text) ? "All Items" : $"Filter: {txtSearch.Text.Trim()}";

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            if (logoBytes != null)
                                row.ConstantItem(64).Image(logoBytes);
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(businessName).SemiBold().FontSize(18);
                                col.Item().Text(reportTitle).SemiBold().FontSize(14);
                                col.Item().Text($"Generated: {reportDate}").FontSize(10).FontColor("#666");
                                col.Item().Text(filterText).FontSize(10).FontColor("#666");
                            });
                        });
                    });
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Item ID
                            columns.RelativeColumn(3); // Category
                            columns.RelativeColumn(3); // Brand
                            columns.RelativeColumn(2); // Quantity
                            columns.RelativeColumn(2); // Unit Price
                            columns.RelativeColumn(3); // Updated At
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeader).Text("Item ID");
                            header.Cell().Element(CellHeader).Text("Category");
                            header.Cell().Element(CellHeader).Text("Brand");
                            header.Cell().Element(CellHeader).AlignRight().Text("Quantity");
                            header.Cell().Element(CellHeader).AlignRight().Text("Unit Price");
                            header.Cell().Element(CellHeader).Text("Updated At");

                            static IContainer CellHeader(IContainer container)
                            {
                                return container.PaddingVertical(6).PaddingHorizontal(4).Background("#EEEEEE");
                            }
                        });

                        foreach (var it in items)
                        {
                            table.Cell().Element(CellBody).Text(it.ItemId);
                            table.Cell().Element(CellBody).Text(it.Name);
                            table.Cell().Element(CellBody).Text(it.Brand);
                            table.Cell().Element(CellBody).AlignRight().Text(it.Qty);
                            table.Cell().Element(CellBody).AlignRight().Text(it.Price);
                            table.Cell().Element(CellBody).Text(it.Updated);
                        }

                        static IContainer CellBody(IContainer container)
                        {
                            return container.PaddingVertical(4).PaddingHorizontal(4);
                        }
                    });
                    page.Footer().AlignRight().Text(t =>
                    {
                        t.Span("Page ");
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
                });
            });

            try
            {
                doc.GeneratePdf(sfd.FileName);
                MessageBox.Show("Exported inventory to PDF.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export PDF: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                LoadInventory();
                // Restore selection to first row so Edit/Update works immediately
                if (dgvInventory.Rows.Count > 0)
                {
                    dgvInventory.ClearSelection();
                    dgvInventory.Rows[0].Selected = true;
                    if (!_readOnly)
                        btnUpdate.Enabled = true;
                }
                return;
            }

            dgvInventory.Rows.Clear();
            using var conn = Database.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id, item_id, item_name, brand, quantity, unit_price, updated_at FROM inventory 
                                WHERE item_id LIKE '%' || $q || '%' OR item_name LIKE '%' || $q || '%' OR brand LIKE '%' || $q || '%' 
                                ORDER BY item_name";
            cmd.Parameters.AddWithValue("$q", query);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int idx = dgvInventory.Rows.Add();
                var row = dgvInventory.Rows[idx];
                row.Cells["Id"].Value = reader.GetInt64(0);
                row.Cells["ItemId"].Value = reader.IsDBNull(1) ? "" : reader.GetString(1);
                row.Cells["ItemName"].Value = reader.GetString(2);
                row.Cells["Brand"].Value = reader.IsDBNull(3) ? "" : reader.GetString(3);
                row.Cells["Quantity"].Value = reader.GetDouble(4).ToString("0.##");
                row.Cells["UnitPrice"].Value = reader.GetDouble(5).ToString("0.00");
                var amt = reader.GetDouble(4) * reader.GetDouble(5);
                row.Cells["Amount"].Value = amt.ToString("0.00");
                row.Cells["UpdatedAt"].Value = reader.GetString(6);
            }

            // Select first result to allow immediate edit/update while searching (admin only)
            if (dgvInventory.Rows.Count > 0)
            {
                dgvInventory.ClearSelection();
                dgvInventory.Rows[0].Selected = true;
                if (!_readOnly)
                    btnUpdate.Enabled = true;
            }
        }

        // Unit Price fills manually per request; removed auto-fill on item name

        private void NumericTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow digits, one decimal separator, backspace
            if (char.IsControl(e.KeyChar)) return;
            var tb = sender as TextBox;
            char sep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            if (char.IsDigit(e.KeyChar)) return;
            if (e.KeyChar == sep && tb != null && !tb.Text.Contains(sep)) return;
            e.Handled = true;
        }

        private void EnsureInventoryTable()
        {
            // No-op: Inventory table creation handled by Database.Ensure()
        }
    }
}
