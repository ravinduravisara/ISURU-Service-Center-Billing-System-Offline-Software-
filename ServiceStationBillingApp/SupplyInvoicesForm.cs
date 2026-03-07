using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public class SupplyInvoicesForm : Form
    {
        private readonly string _userRole;

        private Label lblTitle;
        private TextBox txtSearch;
        private DataGridView dgvInvoices;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnCashSummary;
        private Button btnCreditSummary;
        private Button btnChequeSummary;
        private Label lblStatus;

        public SupplyInvoicesForm(string userRole = "Admin")
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
                if (e.Control && e.KeyCode == Keys.A) { dgvInvoices.SelectAll(); e.Handled = true; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Delete && !txtSearch.Focused) { BtnDelete_Click(s, e); e.Handled = true; }
                else if (e.KeyCode == Keys.Escape) { this.Close(); e.Handled = true; }
            };
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            txtSearch = new TextBox();
            dgvInvoices = new DataGridView();
            btnAdd = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            btnCashSummary = new Button();
            btnCreditSummary = new Button();
            btnChequeSummary = new Button();
            lblStatus = new Label();

            SuspendLayout();

            // lblTitle
            lblTitle.Text = "Supply Invoices";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(16, 12);
            lblTitle.AutoSize = true;

            // txtSearch
            txtSearch.Location = new Point(16, 48);
            txtSearch.Size = new Size(280, 25);
            txtSearch.PlaceholderText = "Search...";
            txtSearch.TextChanged += (s, e) => LoadInvoices();

            // btnAdd
            btnAdd.Text = "Add";
            btnAdd.Size = new Size(80, 30);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Location = new Point(700, 45);
            btnAdd.Click += BtnAdd_Click;

            // btnEdit
            btnEdit.Text = "Edit";
            btnEdit.Size = new Size(80, 30);
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEdit.Location = new Point(790, 45);
            btnEdit.Click += BtnEdit_Click;

            // btnDelete
            btnDelete.Text = "Delete";
            btnDelete.Size = new Size(80, 30);
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Location = new Point(880, 45);
            btnDelete.Click += BtnDelete_Click;

            // btnCashSummary
            btnCashSummary.Text = "Cash (0.00)";
            btnCashSummary.Size = new Size(160, 35);
            btnCashSummary.Anchor = AnchorStyles.Bottom;
            btnCashSummary.Location = new Point(240, 480);
            btnCashSummary.FlatStyle = FlatStyle.System;
            btnCashSummary.Click += (s, e) => OpenPaymentDetails("Cash");

            // btnCreditSummary
            btnCreditSummary.Text = "Credit (0.00)";
            btnCreditSummary.Size = new Size(160, 35);
            btnCreditSummary.Anchor = AnchorStyles.Bottom;
            btnCreditSummary.Location = new Point(410, 480);
            btnCreditSummary.FlatStyle = FlatStyle.System;
            btnCreditSummary.Click += (s, e) => OpenPaymentDetails("Credit");
            btnChequeSummary.Text = "Cheque (0.00)";
            btnChequeSummary.Size = new Size(160, 35);
            btnChequeSummary.Anchor = AnchorStyles.Bottom;
            btnChequeSummary.Location = new Point(580, 480);
            btnChequeSummary.FlatStyle = FlatStyle.System;
            btnChequeSummary.Click += (s, e) => OpenPaymentDetails("Cheque");

            // dgvInvoices
            dgvInvoices.Location = new Point(16, 85);
            dgvInvoices.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvInvoices.Size = new Size(948, 385);
            dgvInvoices.ReadOnly = true;
            dgvInvoices.AllowUserToAddRows = false;
            dgvInvoices.AllowUserToDeleteRows = false;
            dgvInvoices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInvoices.MultiSelect = true;
            dgvInvoices.RowHeadersVisible = false;
            dgvInvoices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInvoices.BackgroundColor = Color.FromArgb(180, 180, 180);
            dgvInvoices.ColumnHeadersDefaultCellStyle.BackColor = Color.DeepSkyBlue;
            dgvInvoices.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvInvoices.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvInvoices.EnableHeadersVisualStyles = false;
            dgvInvoices.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 144, 255);
            dgvInvoices.DefaultCellStyle.SelectionForeColor = Color.White;

            // lblStatus
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 28;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.Padding = new Padding(12, 0, 0, 0);

            // Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(980, 560);
            Controls.Add(dgvInvoices);
            Controls.Add(btnCashSummary);
            Controls.Add(btnCreditSummary);
            Controls.Add(btnChequeSummary);
            Controls.Add(btnDelete);
            Controls.Add(btnEdit);
            Controls.Add(btnAdd);
            Controls.Add(txtSearch);
            Controls.Add(lblTitle);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Supply Invoices";
            Load += (s, e) => LoadInvoices();

            ResumeLayout(false);
            PerformLayout();
        }

        private void LoadInvoices()
        {
            var dt = new DataTable();
            dt.Columns.Add("ID", typeof(long));
            dt.Columns.Add("Invoice");
            dt.Columns.Add("Supplier ID");
            dt.Columns.Add("Item ID");
            dt.Columns.Add("Category");
            dt.Columns.Add("Brand");
            dt.Columns.Add("Buying Price", typeof(double));
            dt.Columns.Add("Quantity", typeof(double));
            dt.Columns.Add("Amount", typeof(double));
            dt.Columns.Add("Payment Status");
            dt.Columns.Add("Date");

            using var conn = Database.OpenConnection();
            using var cmd = conn.CreateCommand();

            var search = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(search))
            {
                cmd.CommandText = @"SELECT si.id, si.invoice_no, si.supplier_code, si.item_id,
                                           COALESCE(inv.item_name,''), COALESCE(inv.brand,''),
                                           si.buying_price, si.quantity,
                                           si.buying_price * si.quantity,
                                           si.payment_status, si.created_at
                                    FROM supply_invoices si
                                    LEFT JOIN inventory inv ON inv.item_id = si.item_id
                                    ORDER BY si.created_at DESC";
            }
            else
            {
                cmd.CommandText = @"SELECT si.id, si.invoice_no, si.supplier_code, si.item_id,
                                           COALESCE(inv.item_name,''), COALESCE(inv.brand,''),
                                           si.buying_price, si.quantity,
                                           si.buying_price * si.quantity,
                                           si.payment_status, si.created_at
                                    FROM supply_invoices si
                                    LEFT JOIN inventory inv ON inv.item_id = si.item_id
                                    LEFT JOIN suppliers sup ON sup.supplier_code = si.supplier_code
                                    WHERE si.invoice_no LIKE @s
                                       OR si.supplier_code LIKE @s
                                       OR si.item_id LIKE @s
                                       OR si.payment_status LIKE @s
                                       OR COALESCE(sup.name,'') LIKE @s
                                    ORDER BY si.created_at DESC";
                cmd.Parameters.AddWithValue("@s", $"%{search}%");
            }

            using var reader = cmd.ExecuteReader();
            double totalAmount = 0;
            int count = 0;
            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                var invoiceNo = reader.IsDBNull(1) ? "" : reader.GetString(1);
                var supplierCode = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var itemId = reader.IsDBNull(3) ? "" : reader.GetString(3);
                var category = reader.IsDBNull(4) ? "" : reader.GetString(4);
                var brand = reader.IsDBNull(5) ? "" : reader.GetString(5);
                var buyingPrice = reader.IsDBNull(6) ? 0.0 : reader.GetDouble(6);
                var quantity = reader.IsDBNull(7) ? 0.0 : reader.GetDouble(7);
                var amount = reader.IsDBNull(8) ? 0.0 : reader.GetDouble(8);
                var paymentStatus = reader.IsDBNull(9) ? "" : reader.GetString(9);
                var date = reader.IsDBNull(10) ? "" : reader.GetString(10);

                dt.Rows.Add(id, invoiceNo, supplierCode, itemId, category, brand, buyingPrice, quantity, amount, paymentStatus, date);
                totalAmount += amount;
                count++;
            }

            dgvInvoices.DataSource = dt;

            // Hide ID column
            if (dgvInvoices.Columns.Contains("ID"))
                dgvInvoices.Columns["ID"]!.Visible = false;

            // Format numeric columns
            if (dgvInvoices.Columns.Contains("Buying Price"))
            {
                dgvInvoices.Columns["Buying Price"]!.DefaultCellStyle.Format = "0.00";
                dgvInvoices.Columns["Buying Price"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvInvoices.Columns.Contains("Quantity"))
            {
                dgvInvoices.Columns["Quantity"]!.DefaultCellStyle.Format = "0.##";
                dgvInvoices.Columns["Quantity"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvInvoices.Columns.Contains("Amount"))
            {
                dgvInvoices.Columns["Amount"]!.DefaultCellStyle.Format = "0.00";
                dgvInvoices.Columns["Amount"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            lblStatus.Text = $"Total Records: {count}    Total Amount: {totalAmount:0.00}";

            UpdateSummaryButtons();
        }

        private void UpdateSummaryButtons()
        {
            double cashTotal = 0, creditTotal = 0, chequeTotal = 0;
            using var conn = Database.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT
                                    CASE
                                        WHEN LOWER(TRIM(COALESCE(payment_status,''))) LIKE '%credit%' THEN 'credit'
                                        WHEN LOWER(TRIM(COALESCE(payment_status,''))) LIKE '%cheque%' THEN 'cheque'
                                        ELSE 'cash'
                                    END AS pm,
                                    SUM(buying_price * quantity) AS total
                                FROM supply_invoices
                                GROUP BY pm";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var pm = reader.IsDBNull(0) ? "" : reader.GetString(0);
                var total = reader.IsDBNull(1) ? 0.0 : reader.GetDouble(1);
                if (pm == "credit") creditTotal += total;
                else if (pm == "cheque") chequeTotal += total;
                else cashTotal += total;
            }

            btnCashSummary.Text = $"Cash ({cashTotal:0.00})";
            btnCreditSummary.Text = $"Credit ({creditTotal:0.00})";
            btnChequeSummary.Text = $"Cheque ({chequeTotal:0.00})";
            lblStatus.Text += $"     Cash: {cashTotal:0.00}    Credit: {creditTotal:0.00}    Cheque: {chequeTotal:0.00}";
        }

        private DataRow? GetSelectedRow()
        {
            if (dgvInvoices.CurrentRow == null) return null;
            if (dgvInvoices.CurrentRow.DataBoundItem is DataRowView drv)
                return drv.Row;
            return null;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var dlg = new SupplyInvoiceEditDialog(null);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                LoadInvoices();
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            var row = GetSelectedRow();
            if (row == null) { MessageBox.Show("Select an invoice to edit."); return; }
            long id = Convert.ToInt64(row["ID"]);
            using var dlg = new SupplyInvoiceEditDialog(id);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                LoadInvoices();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvInvoices.SelectedRows.Count == 0) { MessageBox.Show("Select an invoice to delete."); return; }

            // Collect data from all selected rows
            var invoices = new List<(long Id, string InvoiceNo, string ItemId, double Qty, string SupplierCode)>();
            foreach (DataGridViewRow dgvRow in dgvInvoices.SelectedRows)
            {
                if (dgvRow.DataBoundItem is DataRowView drv)
                {
                    var row = drv.Row;
                    invoices.Add((
                        Convert.ToInt64(row["ID"]),
                        Convert.ToString(row["Invoice"]) ?? "",
                        Convert.ToString(row["Item ID"]) ?? "",
                        Convert.ToDouble(row["Quantity"]),
                        Convert.ToString(row["Supplier"]) ?? ""
                    ));
                }
            }
            if (invoices.Count == 0) return;

            string msg = invoices.Count == 1
                ? $"Delete supply invoice {invoices[0].InvoiceNo}?\nThis will also remove {invoices[0].Qty} from inventory item {invoices[0].ItemId}."
                : $"Delete {invoices.Count} supply invoices?\nThis will also adjust inventory quantities.";
            if (MessageBox.Show(msg, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            using var conn = Database.OpenConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var inv in invoices)
                {
                    // Subtract quantity from inventory
                    using (var updInv = conn.CreateCommand())
                    {
                        updInv.Transaction = tx;
                        updInv.CommandText = @"UPDATE inventory SET quantity = MAX(0, COALESCE(quantity,0) - @qty),
                                                                   updated_at = @ts
                                              WHERE item_id = @itemId";
                        updInv.Parameters.AddWithValue("@qty", inv.Qty);
                        updInv.Parameters.AddWithValue("@ts", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        updInv.Parameters.AddWithValue("@itemId", inv.ItemId);
                        updInv.ExecuteNonQuery();
                    }

                    // Delete matching stock_movement (before deleting invoice so supplier_code is available)
                    using (var delMov = conn.CreateCommand())
                    {
                        delMov.Transaction = tx;
                        delMov.CommandText = @"DELETE FROM stock_movements WHERE id IN (
                            SELECT id FROM stock_movements
                            WHERE item_id = @itemId AND supplier_code = @supplierCode
                            ORDER BY moved_at DESC LIMIT 1)";
                        delMov.Parameters.AddWithValue("@itemId", inv.ItemId);
                        delMov.Parameters.AddWithValue("@supplierCode", inv.SupplierCode);
                        delMov.ExecuteNonQuery();
                    }

                    // Delete the supply invoice record
                    using (var del = conn.CreateCommand())
                    {
                        del.Transaction = tx;
                        del.CommandText = "DELETE FROM supply_invoices WHERE id = @id";
                        del.Parameters.AddWithValue("@id", inv.Id);
                        del.ExecuteNonQuery();
                    }
                }

                tx.Commit();
                LoadInvoices();
            }
            catch (Exception ex)
            {
                tx.Rollback();
                MessageBox.Show($"Failed to delete: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenPaymentDetails(string paymentMethod)
        {
            using var dlg = new SupplyInvoicePaymentDetailsForm(paymentMethod);
            dlg.ShowDialog(this);
            LoadInvoices(); // Refresh totals after potential settlement
        }
    }

    // ====== Payment Details Dialog (Cash / Credit / Cheque) ======
    public class SupplyInvoicePaymentDetailsForm : Form
    {
        private readonly string _paymentMethod;
        private Label lblTitle;
        private DataGridView dgvDetails;
        private Label lblTotal;
        private Button btnSettle;
        private Button btnClose;

        public SupplyInvoicePaymentDetailsForm(string paymentMethod)
        {
            _paymentMethod = paymentMethod;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            dgvDetails = new DataGridView();
            lblTotal = new Label();
            btnSettle = new Button();
            btnClose = new Button();

            SuspendLayout();

            // lblTitle
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 42;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.Padding = new Padding(12, 0, 12, 0);
            lblTitle.Text = $"Supply Invoice - {_paymentMethod} Details";

            // dgvDetails
            dgvDetails.Dock = DockStyle.Fill;
            dgvDetails.ReadOnly = true;
            dgvDetails.AllowUserToAddRows = false;
            dgvDetails.AllowUserToDeleteRows = false;
            dgvDetails.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetails.MultiSelect = true;
            dgvDetails.RowHeadersVisible = false;
            dgvDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDetails.BackgroundColor = Color.FromArgb(180, 180, 180);
            dgvDetails.ColumnHeadersDefaultCellStyle.BackColor = Color.DeepSkyBlue;
            dgvDetails.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDetails.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvDetails.EnableHeadersVisualStyles = false;
            dgvDetails.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 144, 255);
            dgvDetails.DefaultCellStyle.SelectionForeColor = Color.White;

            // Bottom panel
            var bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 44;
            bottomPanel.Padding = new Padding(12, 8, 12, 8);

            // lblTotal
            lblTotal.Dock = DockStyle.Left;
            lblTotal.AutoSize = false;
            lblTotal.Width = 450;
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;
            lblTotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            // btnSettle
            btnSettle.Dock = DockStyle.Right;
            btnSettle.Width = 130;
            btnSettle.Text = "Mark Settled";
            btnSettle.FlatStyle = FlatStyle.System;
            btnSettle.Click += BtnSettle_Click;
            btnSettle.Visible = (_paymentMethod == "Credit" || _paymentMethod == "Cheque");

            // btnClose
            btnClose.Dock = DockStyle.Right;
            btnClose.Width = 100;
            btnClose.Text = "Close";
            btnClose.FlatStyle = FlatStyle.System;
            btnClose.Click += (s, e) => Close();

            bottomPanel.Controls.Add(btnClose);
            bottomPanel.Controls.Add(btnSettle);
            bottomPanel.Controls.Add(lblTotal);

            // Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 520);
            Controls.Add(dgvDetails);
            Controls.Add(bottomPanel);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = $"Supply Invoice - {_paymentMethod} Details";
            Load += (s, e) => LoadData();

            ResumeLayout(false);
        }

        private void LoadData()
        {
            var dt = new DataTable();
            dt.Columns.Add("ID", typeof(long));
            dt.Columns.Add("Invoice No");
            dt.Columns.Add("Supplier ID");
            dt.Columns.Add("Supplier Name");
            dt.Columns.Add("Item ID");
            dt.Columns.Add("Category");
            dt.Columns.Add("Quantity", typeof(double));
            dt.Columns.Add("Buying Price", typeof(double));
            dt.Columns.Add("Total Amount", typeof(double));

            if (_paymentMethod == "Cheque")
            {
                dt.Columns.Add("Cheque No");
                dt.Columns.Add("Branch Code");
                dt.Columns.Add("Bank Code");
                dt.Columns.Add("Value Date");
            }

            dt.Columns.Add("Date");
            dt.Columns.Add("Status");

            using var conn = Database.OpenConnection();
            using var cmd = conn.CreateCommand();

            string payFilter;
            if (_paymentMethod == "Credit")
                payFilter = "LOWER(TRIM(COALESCE(si.payment_status,''))) LIKE '%credit%'";
            else if (_paymentMethod == "Cheque")
                payFilter = "LOWER(TRIM(COALESCE(si.payment_status,''))) LIKE '%cheque%'";
            else
                payFilter = "(TRIM(COALESCE(si.payment_status,'')) = '' OR LOWER(TRIM(COALESCE(si.payment_status,''))) = 'cash')";

            cmd.CommandText = $@"
                SELECT si.id, si.invoice_no, si.supplier_code,
                       COALESCE(sup.name, si.supplier_code),
                       si.item_id, COALESCE(inv.item_name,''),
                       si.quantity, si.buying_price,
                       si.quantity * si.buying_price,
                       si.cheque_no, si.branch_code, si.bank_code, si.value_date,
                       si.created_at, si.settled_at
                FROM supply_invoices si
                LEFT JOIN suppliers sup ON sup.supplier_code = si.supplier_code
                LEFT JOIN inventory inv ON inv.item_id = si.item_id
                WHERE {payFilter}
                ORDER BY si.created_at DESC";

            using var reader = cmd.ExecuteReader();
            double grandTotal = 0, unsettledTotal = 0;
            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                var invoiceNo = reader.IsDBNull(1) ? "" : reader.GetString(1);
                var supplierCode = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var supplierName = reader.IsDBNull(3) ? "" : reader.GetString(3);
                var itemId = reader.IsDBNull(4) ? "" : reader.GetString(4);
                var category = reader.IsDBNull(5) ? "" : reader.GetString(5);
                var qty = reader.IsDBNull(6) ? 0.0 : reader.GetDouble(6);
                var bp = reader.IsDBNull(7) ? 0.0 : reader.GetDouble(7);
                var total = reader.IsDBNull(8) ? 0.0 : reader.GetDouble(8);
                var chequeNo = reader.IsDBNull(9) ? "" : reader.GetString(9);
                var branchCode = reader.IsDBNull(10) ? "" : reader.GetString(10);
                var bankCode = reader.IsDBNull(11) ? "" : reader.GetString(11);
                var valueDate = reader.IsDBNull(12) ? "" : reader.GetString(12);
                var createdAt = reader.IsDBNull(13) ? "" : reader.GetString(13);
                var settledAt = reader.IsDBNull(14) ? (string?)null : reader.GetString(14);
                var status = string.IsNullOrWhiteSpace(settledAt) ? "Unsettled" : "Settled";

                if (_paymentMethod == "Cheque")
                    dt.Rows.Add(id, invoiceNo, supplierCode, supplierName, itemId, category, qty, bp, total, chequeNo, branchCode, bankCode, valueDate, createdAt, status);
                else
                    dt.Rows.Add(id, invoiceNo, supplierCode, supplierName, itemId, category, qty, bp, total, createdAt, status);

                grandTotal += total;
                if (status == "Unsettled") unsettledTotal += total;
            }

            dgvDetails.DataSource = dt;

            // Hide ID
            if (dgvDetails.Columns.Contains("ID"))
                dgvDetails.Columns["ID"]!.Visible = false;

            // Format numeric columns
            foreach (var col in new[] { "Buying Price", "Total Amount", "Quantity" })
            {
                if (dgvDetails.Columns.Contains(col))
                {
                    dgvDetails.Columns[col]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvDetails.Columns[col]!.DefaultCellStyle.Format = col == "Quantity" ? "0.##" : "0.00";
                }
            }

            // Color-code Status
            dgvDetails.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (dgvDetails.Columns[e.ColumnIndex].Name == "Status")
                {
                    var val = Convert.ToString(e.Value);
                    if (val == "Settled")
                    {
                        e.CellStyle.ForeColor = Color.Green;
                        e.CellStyle.Font = new Font(dgvDetails.Font, FontStyle.Bold);
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font(dgvDetails.Font, FontStyle.Bold);
                    }
                }
            };

            if (_paymentMethod == "Credit" || _paymentMethod == "Cheque")
                lblTotal.Text = $"Total: {grandTotal:0.00}  |  Unsettled: {unsettledTotal:0.00}";
            else
                lblTotal.Text = $"Total: {grandTotal:0.00}";
        }

        private void BtnSettle_Click(object? sender, EventArgs e)
        {
            if (dgvDetails.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select one or more rows to mark as settled.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var unsettledIds = new List<long>();
            foreach (DataGridViewRow row in dgvDetails.SelectedRows)
            {
                var status = Convert.ToString(row.Cells["Status"].Value);
                if (status == "Unsettled")
                    unsettledIds.Add(Convert.ToInt64(row.Cells["ID"].Value));
            }

            if (unsettledIds.Count == 0)
            {
                MessageBox.Show("All selected rows are already settled.", "Already Settled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Mark {unsettledIds.Count} item(s) as settled?", "Confirm Settlement",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using var conn = Database.OpenConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var id in unsettledIds)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = "UPDATE supply_invoices SET settled_at = @ts WHERE id = @id";
                    cmd.Parameters.AddWithValue("@ts", now);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
                MessageBox.Show($"{unsettledIds.Count} item(s) marked as settled.", "Settled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                tx.Rollback();
                MessageBox.Show($"Failed to settle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ====== Edit/Add Dialog ======
    public class SupplyInvoiceEditDialog : Form
    {
        private readonly long? _editId;

        private Label lblInvoice;
        private TextBox txtInvoice;
        private Label lblSupplier;
        private TextBox txtSupplier;
        private Label lblItemId;
        private ComboBox cmbItemId;
        private Label lblBuyingPrice;
        private TextBox txtBuyingPrice;
        private Label lblQuantity;
        private TextBox txtQuantity;
        private Label lblAmount;
        private TextBox txtAmount;
        private Label lblPayment;
        private ComboBox cmbPayment;

        // Cheque detail fields
        private Panel pnlChequeDetails;
        private Label lblChequeNo;
        private TextBox txtChequeNo;
        private Label lblBranchCode;
        private TextBox txtBranchCode;
        private Label lblBankCode;
        private TextBox txtBankCode;
        private Label lblValueDate;
        private DateTimePicker dtpValueDate;

        private Button btnSave;
        private Button btnCancel;

        private double _originalQty = 0;
        private string _originalItemId = "";

        public SupplyInvoiceEditDialog(long? editId)
        {
            _editId = editId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            lblInvoice = new Label { Text = "Invoice No:", Location = new Point(20, 20), AutoSize = true };
            txtInvoice = new TextBox { Location = new Point(150, 17), Size = new Size(230, 25) };

            lblSupplier = new Label { Text = "Supplier ID:", Location = new Point(20, 55), AutoSize = true };
            txtSupplier = new TextBox { Location = new Point(150, 52), Size = new Size(230, 25) };

            lblItemId = new Label { Text = "Item ID:", Location = new Point(20, 90), AutoSize = true };
            cmbItemId = new ComboBox { Location = new Point(150, 87), Size = new Size(230, 25), DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems };

            lblBuyingPrice = new Label { Text = "Buying Price:", Location = new Point(20, 125), AutoSize = true };
            txtBuyingPrice = new TextBox { Location = new Point(150, 122), Size = new Size(230, 25) };

            lblQuantity = new Label { Text = "Quantity:", Location = new Point(20, 160), AutoSize = true };
            txtQuantity = new TextBox { Location = new Point(150, 157), Size = new Size(230, 25) };

            lblAmount = new Label { Text = "Amount:", Location = new Point(20, 195), AutoSize = true };
            txtAmount = new TextBox { Location = new Point(150, 192), Size = new Size(230, 25), ReadOnly = true, BackColor = Color.FromArgb(240, 240, 240) };

            lblPayment = new Label { Text = "Payment Status:", Location = new Point(20, 230), AutoSize = true };
            cmbPayment = new ComboBox { Location = new Point(150, 227), Size = new Size(230, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPayment.Items.AddRange(new object[] { "Cash", "Credit", "Cheque" });
            cmbPayment.SelectedIndex = 0;
            cmbPayment.SelectedIndexChanged += CmbPayment_SelectedIndexChanged;

            // Cheque details panel
            pnlChequeDetails = new Panel { Location = new Point(10, 260), Size = new Size(390, 145), Visible = false };
            pnlChequeDetails.BorderStyle = BorderStyle.FixedSingle;
            pnlChequeDetails.BackColor = Color.FromArgb(245, 245, 255);

            lblChequeNo = new Label { Text = "Cheque No:", Location = new Point(10, 12), AutoSize = true };
            txtChequeNo = new TextBox { Location = new Point(140, 9), Size = new Size(230, 25) };

            lblBranchCode = new Label { Text = "Branch Code:", Location = new Point(10, 42), AutoSize = true };
            txtBranchCode = new TextBox { Location = new Point(140, 39), Size = new Size(230, 25) };

            lblBankCode = new Label { Text = "Bank Code:", Location = new Point(10, 72), AutoSize = true };
            txtBankCode = new TextBox { Location = new Point(140, 69), Size = new Size(230, 25) };

            lblValueDate = new Label { Text = "Value Date:", Location = new Point(10, 102), AutoSize = true };
            dtpValueDate = new DateTimePicker { Location = new Point(140, 99), Size = new Size(230, 25), Format = DateTimePickerFormat.Short };

            pnlChequeDetails.Controls.AddRange(new Control[] { lblChequeNo, txtChequeNo, lblBranchCode, txtBranchCode, lblBankCode, txtBankCode, lblValueDate, dtpValueDate });

            btnSave = new Button { Text = "Save", Location = new Point(150, 275), Size = new Size(100, 32), DialogResult = DialogResult.None };
            btnCancel = new Button { Text = "Cancel", Location = new Point(260, 275), Size = new Size(100, 32), DialogResult = DialogResult.Cancel };

            btnSave.Click += BtnSave_Click;

            // Auto-calculate amount
            txtBuyingPrice.TextChanged += (s, e) => CalcAmount();
            txtQuantity.TextChanged += (s, e) => CalcAmount();

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(410, 325);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = _editId.HasValue ? "Edit Supply Invoice" : "Add Supply Invoice";
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] { lblInvoice, txtInvoice, lblSupplier, txtSupplier, lblItemId, cmbItemId,
                lblBuyingPrice, txtBuyingPrice, lblQuantity, txtQuantity, lblAmount, txtAmount,
                lblPayment, cmbPayment, pnlChequeDetails, btnSave, btnCancel });

            Load += SupplyInvoiceEditDialog_Load;

            ResumeLayout(false);
            PerformLayout();
        }

        private void CmbPayment_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool isCheque = cmbPayment.SelectedItem?.ToString() == "Cheque";
            pnlChequeDetails.Visible = isCheque;
            if (isCheque)
            {
                ClientSize = new Size(410, 475);
                btnSave.Location = new Point(150, 420);
                btnCancel.Location = new Point(260, 420);
            }
            else
            {
                ClientSize = new Size(410, 325);
                btnSave.Location = new Point(150, 275);
                btnCancel.Location = new Point(260, 275);
            }
        }

        private void CalcAmount()
        {
            double.TryParse(txtBuyingPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var bp);
            double.TryParse(txtQuantity.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var q);
            txtAmount.Text = (bp * q).ToString("0.00", CultureInfo.InvariantCulture);
        }

        private void SupplyInvoiceEditDialog_Load(object? sender, EventArgs e)
        {
            LoadItemIds();

            if (_editId.HasValue)
            {
                using var conn = Database.OpenConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT invoice_no, supplier_code, item_id, buying_price, quantity, payment_status, cheque_no, branch_code, bank_code, value_date FROM supply_invoices WHERE id=@id";
                cmd.Parameters.AddWithValue("@id", _editId.Value);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtInvoice.Text = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    txtSupplier.Text = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var itemId = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    cmbItemId.Text = itemId;
                    _originalItemId = itemId;
                    txtBuyingPrice.Text = reader.IsDBNull(3) ? "0" : reader.GetDouble(3).ToString(CultureInfo.InvariantCulture);
                    var qty = reader.IsDBNull(4) ? 0.0 : reader.GetDouble(4);
                    txtQuantity.Text = qty.ToString(CultureInfo.InvariantCulture);
                    _originalQty = qty;
                    var payment = reader.IsDBNull(5) ? "Cash" : reader.GetString(5);
                    int idx = cmbPayment.Items.IndexOf(payment);
                    cmbPayment.SelectedIndex = idx >= 0 ? idx : 0;

                    // Load cheque details
                    txtChequeNo.Text = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    txtBranchCode.Text = reader.IsDBNull(7) ? "" : reader.GetString(7);
                    txtBankCode.Text = reader.IsDBNull(8) ? "" : reader.GetString(8);
                    var vd = reader.IsDBNull(9) ? "" : reader.GetString(9);
                    if (DateTime.TryParse(vd, out var vdate)) dtpValueDate.Value = vdate;
                }
                CalcAmount();
            }
        }

        private void LoadItemIds()
        {
            cmbItemId.Items.Clear();
            using var conn = Database.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT item_id, item_name FROM inventory ORDER BY item_id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var itemId = reader.IsDBNull(0) ? "" : reader.GetString(0);
                if (!string.IsNullOrWhiteSpace(itemId))
                    cmbItemId.Items.Add(itemId);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var invoiceNo = txtInvoice.Text.Trim();
            var supplierCode = txtSupplier.Text.Trim();
            var itemId = cmbItemId.Text.Trim();
            var paymentStatus = cmbPayment.SelectedItem?.ToString() ?? "Cash";

            if (string.IsNullOrWhiteSpace(invoiceNo))
            { MessageBox.Show("Invoice No is required."); return; }
            if (string.IsNullOrWhiteSpace(supplierCode))
            { MessageBox.Show("Supplier ID is required."); return; }
            if (string.IsNullOrWhiteSpace(itemId))
            { MessageBox.Show("Item ID is required."); return; }
            if (!double.TryParse(txtBuyingPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var buyingPrice) || buyingPrice < 0)
            { MessageBox.Show("Enter a valid buying price."); return; }
            if (!double.TryParse(txtQuantity.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var qty) || qty <= 0)
            { MessageBox.Show("Enter a valid quantity."); return; }

            // Validate cheque fields when Cheque is selected
            if (paymentStatus == "Cheque")
            {
                if (string.IsNullOrWhiteSpace(txtChequeNo.Text))
                { MessageBox.Show("Cheque No is required."); return; }
                if (string.IsNullOrWhiteSpace(txtBranchCode.Text))
                { MessageBox.Show("Branch Code is required."); return; }
                if (string.IsNullOrWhiteSpace(txtBankCode.Text))
                { MessageBox.Show("Bank Code is required."); return; }
            }

            using var conn = Database.OpenConnection();

            // Verify item exists in inventory
            using (var chk = conn.CreateCommand())
            {
                chk.CommandText = "SELECT COUNT(*) FROM inventory WHERE item_id = @id";
                chk.Parameters.AddWithValue("@id", itemId);
                var cnt = Convert.ToInt64(chk.ExecuteScalar());
                if (cnt == 0)
                {
                    MessageBox.Show($"Item ID '{itemId}' not found in inventory. Please add the item to inventory first.", "Item Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            using var tx = conn.BeginTransaction();
            try
            {
                string ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (_editId.HasValue)
                {
                    // Reverse old quantity from inventory
                    using (var rev = conn.CreateCommand())
                    {
                        rev.Transaction = tx;
                        rev.CommandText = @"UPDATE inventory SET quantity = MAX(0, COALESCE(quantity,0) - @oqty), updated_at = @ts
                                           WHERE item_id = @oid";
                        rev.Parameters.AddWithValue("@oqty", _originalQty);
                        rev.Parameters.AddWithValue("@ts", ts);
                        rev.Parameters.AddWithValue("@oid", _originalItemId);
                        rev.ExecuteNonQuery();
                    }

                    // Update supply invoice record
                    using (var upd = conn.CreateCommand())
                    {
                        upd.Transaction = tx;
                        upd.CommandText = @"UPDATE supply_invoices SET invoice_no=@inv, supplier_code=@sup, item_id=@item,
                                               buying_price=@bp, quantity=@qty, payment_status=@pay, created_at=@ts,
                                               cheque_no=@chqNo, branch_code=@brCode, bank_code=@bkCode, value_date=@vDate
                                           WHERE id=@id";
                        upd.Parameters.AddWithValue("@inv", invoiceNo);
                        upd.Parameters.AddWithValue("@sup", supplierCode);
                        upd.Parameters.AddWithValue("@item", itemId);
                        upd.Parameters.AddWithValue("@bp", buyingPrice);
                        upd.Parameters.AddWithValue("@qty", qty);
                        upd.Parameters.AddWithValue("@pay", paymentStatus);
                        upd.Parameters.AddWithValue("@ts", ts);
                        upd.Parameters.AddWithValue("@chqNo", paymentStatus == "Cheque" ? txtChequeNo.Text.Trim() : (object)DBNull.Value);
                        upd.Parameters.AddWithValue("@brCode", paymentStatus == "Cheque" ? txtBranchCode.Text.Trim() : (object)DBNull.Value);
                        upd.Parameters.AddWithValue("@bkCode", paymentStatus == "Cheque" ? txtBankCode.Text.Trim() : (object)DBNull.Value);
                        upd.Parameters.AddWithValue("@vDate", paymentStatus == "Cheque" ? dtpValueDate.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
                        upd.Parameters.AddWithValue("@id", _editId.Value);
                        upd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Insert new supply invoice
                    using (var ins = conn.CreateCommand())
                    {
                        ins.Transaction = tx;
                        ins.CommandText = @"INSERT INTO supply_invoices (invoice_no, supplier_code, item_id, buying_price, quantity, payment_status, created_at, cheque_no, branch_code, bank_code, value_date)
                                           VALUES (@inv, @sup, @item, @bp, @qty, @pay, @ts, @chqNo, @brCode, @bkCode, @vDate)";
                        ins.Parameters.AddWithValue("@inv", invoiceNo);
                        ins.Parameters.AddWithValue("@sup", supplierCode);
                        ins.Parameters.AddWithValue("@item", itemId);
                        ins.Parameters.AddWithValue("@bp", buyingPrice);
                        ins.Parameters.AddWithValue("@qty", qty);
                        ins.Parameters.AddWithValue("@pay", paymentStatus);
                        ins.Parameters.AddWithValue("@ts", ts);
                        ins.Parameters.AddWithValue("@chqNo", paymentStatus == "Cheque" ? txtChequeNo.Text.Trim() : (object)DBNull.Value);
                        ins.Parameters.AddWithValue("@brCode", paymentStatus == "Cheque" ? txtBranchCode.Text.Trim() : (object)DBNull.Value);
                        ins.Parameters.AddWithValue("@bkCode", paymentStatus == "Cheque" ? txtBankCode.Text.Trim() : (object)DBNull.Value);
                        ins.Parameters.AddWithValue("@vDate", paymentStatus == "Cheque" ? dtpValueDate.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
                        ins.ExecuteNonQuery();
                    }
                }

                // Add new quantity to inventory
                using (var addInv = conn.CreateCommand())
                {
                    addInv.Transaction = tx;
                    addInv.CommandText = @"UPDATE inventory SET quantity = COALESCE(quantity,0) + @qty,
                                                              buying_price = @bp,
                                                              supplier_code = @sup,
                                                              payment_method = @pay,
                                                              updated_at = @ts
                                          WHERE item_id = @item";
                    addInv.Parameters.AddWithValue("@qty", qty);
                    addInv.Parameters.AddWithValue("@bp", buyingPrice);
                    addInv.Parameters.AddWithValue("@sup", supplierCode);
                    addInv.Parameters.AddWithValue("@pay", paymentStatus);
                    addInv.Parameters.AddWithValue("@ts", ts);
                    addInv.Parameters.AddWithValue("@item", itemId);
                    addInv.ExecuteNonQuery();
                }

                // Insert stock movement
                using (var mv = conn.CreateCommand())
                {
                    mv.Transaction = tx;
                    mv.CommandText = @"INSERT INTO stock_movements (item_id, supplier_code, quantity, unit_price, buying_price, payment_method, moved_at)
                                      VALUES (@item, @sup, @qty, @bp, @bp, @pay, @ts)";
                    mv.Parameters.AddWithValue("@item", itemId);
                    mv.Parameters.AddWithValue("@sup", supplierCode);
                    mv.Parameters.AddWithValue("@qty", qty);
                    mv.Parameters.AddWithValue("@bp", buyingPrice);
                    mv.Parameters.AddWithValue("@pay", paymentStatus);
                    mv.Parameters.AddWithValue("@ts", ts);
                    mv.ExecuteNonQuery();
                }

                // Ensure supplier exists
                using (var ensSup = conn.CreateCommand())
                {
                    ensSup.Transaction = tx;
                    ensSup.CommandText = @"INSERT OR IGNORE INTO suppliers (supplier_code, name, contact, cash_total, credit_total, invoice)
                                          VALUES (@c, @c, '', 0, 0, @inv)";
                    ensSup.Parameters.AddWithValue("@c", supplierCode);
                    ensSup.Parameters.AddWithValue("@inv", invoiceNo);
                    ensSup.ExecuteNonQuery();
                }

                tx.Commit();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                tx.Rollback();
                MessageBox.Show($"Failed to save: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
