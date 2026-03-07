using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public partial class SupplierPaymentDetailsForm : Form
    {
        private readonly string _paymentMethod;

        private Label lblTitle;
        private DataGridView dgvDetails;
        private Label lblTotal;
        private Button btnClose;
        private Button btnSettle;

        public SupplierPaymentDetailsForm(string paymentMethod)
        {
            _paymentMethod = NormalizePaymentMethod(paymentMethod);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            dgvDetails = new DataGridView();
            lblTotal = new Label();
            btnClose = new Button();
            btnSettle = new Button();

            SuspendLayout();

            // lblTitle
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 42;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Padding = new Padding(12, 0, 12, 0);

            // dgvDetails
            dgvDetails.Dock = DockStyle.Fill;
            dgvDetails.ReadOnly = true;
            dgvDetails.AllowUserToAddRows = false;
            dgvDetails.AllowUserToDeleteRows = false;
            dgvDetails.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetails.MultiSelect = true;
            dgvDetails.RowHeadersVisible = false;
            dgvDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // bottom panel
            var bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 44;
            bottomPanel.Padding = new Padding(12, 8, 12, 8);

            // lblTotal
            lblTotal.Dock = DockStyle.Left;
            lblTotal.AutoSize = false;
            lblTotal.Width = 300;
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;
            lblTotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblTotal.Text = "Total: 0.00";

            // btnSettle
            btnSettle.Dock = DockStyle.Right;
            btnSettle.Width = 130;
            btnSettle.Text = "Mark Settled";
            btnSettle.FlatStyle = FlatStyle.System;
            btnSettle.Margin = new Padding(0, 0, 8, 0);
            btnSettle.Click += BtnSettle_Click;
            // Only show settle button for Credit/Cheque
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
            ClientSize = new Size(1050, 520);
            Controls.Add(dgvDetails);
            Controls.Add(bottomPanel);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Load += SupplierPaymentDetailsForm_Load;

            ResumeLayout(false);
        }

        private static string NormalizePaymentMethod(string? paymentMethod)
        {
            var pm = (paymentMethod ?? string.Empty).Trim();
            if (pm.IndexOf("credit", StringComparison.OrdinalIgnoreCase) >= 0) return "Credit";
            if (pm.IndexOf("cheque", StringComparison.OrdinalIgnoreCase) >= 0) return "Cheque";
            return "Cash";
        }

        private void LoadData()
        {
            var dt = new DataTable();
            dt.Columns.Add("ID", typeof(long));    // hidden, for settlement
            dt.Columns.Add("Date/Time");
            dt.Columns.Add("Supplier ID");
            dt.Columns.Add("Supplier Name");
            dt.Columns.Add("Item ID");
            dt.Columns.Add("Category");
            dt.Columns.Add("Brand");
            dt.Columns.Add("Quantity", typeof(double));
            dt.Columns.Add("Buying Price", typeof(double));
            dt.Columns.Add("Total", typeof(double));
            dt.Columns.Add("Payment");
            dt.Columns.Add("Invoice");
            dt.Columns.Add("Status");

            using var conn = Database.OpenConnection();
            using var cmd = conn.CreateCommand();

            string paymentFilter;
            if (_paymentMethod == "Credit")
            {
                paymentFilter = "LOWER(TRIM(COALESCE(m.payment_method, ''))) LIKE '%credit%'";
            }
            else if (_paymentMethod == "Cheque")
            {
                paymentFilter = "LOWER(TRIM(COALESCE(m.payment_method, ''))) LIKE '%cheque%'";
            }
            else
            {
                paymentFilter = @"(
                    TRIM(COALESCE(m.payment_method, '')) = ''
                    OR LOWER(TRIM(COALESCE(m.payment_method, ''))) = 'cash'
                )";
            }

            cmd.CommandText = $@"
                SELECT
                    m.id,
                    m.moved_at,
                    m.supplier_code,
                    COALESCE(s.name, ''),
                    COALESCE(m.item_id, ''),
                    COALESCE(inv.item_name, ''),
                    COALESCE(inv.brand, ''),
                    COALESCE(m.quantity, 0),
                    COALESCE(m.buying_price, 0),
                    COALESCE(m.quantity, 0) * COALESCE(m.buying_price, 0),
                    COALESCE(m.payment_method, ''),
                    COALESCE(s.invoice, ''),
                    m.settled_at
                FROM stock_movements m
                LEFT JOIN suppliers s ON s.supplier_code = m.supplier_code
                LEFT JOIN inventory inv ON inv.item_id = m.item_id
                WHERE TRIM(COALESCE(m.supplier_code, '')) <> ''
                    AND {paymentFilter}
                ORDER BY m.moved_at DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                var movedAt = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var supplierCode = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                var supplierName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                var itemId = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                var category = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                var brand = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                var qty = reader.IsDBNull(7) ? 0.0 : reader.GetDouble(7);
                var unitPrice = reader.IsDBNull(8) ? 0.0 : reader.GetDouble(8);
                var total = reader.IsDBNull(9) ? 0.0 : reader.GetDouble(9);
                var payment = reader.IsDBNull(10) ? string.Empty : reader.GetString(10);
                var invoice = reader.IsDBNull(11) ? string.Empty : reader.GetString(11);
                var settledAt = reader.IsDBNull(12) ? (string?)null : reader.GetString(12);
                var status = string.IsNullOrWhiteSpace(settledAt) ? "Unsettled" : "Settled";

                dt.Rows.Add(id, movedAt, supplierCode, supplierName, itemId, category, brand, qty, unitPrice, total, payment, invoice, status);
            }

            dgvDetails.DataSource = dt;
            dgvDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Hide the ID column
            if (dgvDetails.Columns.Contains("ID"))
                dgvDetails.Columns["ID"].Visible = false;

            // Color-code the Status column
            dgvDetails.CellFormatting += DgvDetails_CellFormatting;

            double grandTotal = 0;
            double unsettledTotal = 0;
            foreach (DataRow r in dt.Rows)
            {
                if (r["Total"] is double t)
                {
                    grandTotal += t;
                    if (Convert.ToString(r["Status"]) == "Unsettled")
                        unsettledTotal += t;
                }
            }

            if (_paymentMethod == "Credit" || _paymentMethod == "Cheque")
                lblTotal.Text = $"Total: {grandTotal:0.00}  |  Unsettled: {unsettledTotal:0.00}";
            else
                lblTotal.Text = $"Total: {grandTotal:0.00}";
        }

        private void DgvDetails_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
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
        }

        private void BtnSettle_Click(object? sender, EventArgs e)
        {
            if (dgvDetails.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select one or more rows to mark as settled.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Check if any selected row is already settled
            var unsettledIds = new System.Collections.Generic.List<long>();
            foreach (DataGridViewRow row in dgvDetails.SelectedRows)
            {
                var status = Convert.ToString(row.Cells["Status"].Value);
                if (status == "Unsettled")
                {
                    unsettledIds.Add(Convert.ToInt64(row.Cells["ID"].Value));
                }
            }

            if (unsettledIds.Count == 0)
            {
                MessageBox.Show("All selected rows are already settled.", "Already Settled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Mark {unsettledIds.Count} item(s) as settled?",
                "Confirm Settlement",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using var conn = Database.OpenConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var id in unsettledIds)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = "UPDATE stock_movements SET settled_at = @ts WHERE id = @id";
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

        private void SupplierPaymentDetailsForm_Load(object? sender, EventArgs e)
        {
            Text = $"Supplier {_paymentMethod} Details";
            lblTitle.Text = $"Supplier {_paymentMethod} Details";
            LoadData();
        }
    }
}
