using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public partial class InvoiceListForm : Form
    {
        private DataGridView dgv;
        private Label lblTotals;
        private Button btnCash;
        private Button btnCheque;
        private Button btnCredit;
        private Button btnCreditors;
        private Button btnMarkSettled;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private string _currentMethodFilter = string.Empty;

        public InvoiceListForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "All Invoices";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(900, 650);

            // --- Date range filter panel ---
            var datePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(12, 8, 12, 4)
            };

            var lblFrom = new Label { Text = "From:", AutoSize = true, Left = 12, Top = 12, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            dtpFrom = new DateTimePicker { Left = 60, Top = 8, Width = 180, Format = DateTimePickerFormat.Short };
            dtpFrom.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // default: first of current month

            var lblTo = new Label { Text = "To:", AutoSize = true, Left = 260, Top = 12, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            dtpTo = new DateTimePicker { Left = 290, Top = 8, Width = 180, Format = DateTimePickerFormat.Short };
            dtpTo.Value = DateTime.Now;

            var btnFilter = new Button
            {
                Text = "Filter",
                Left = 490,
                Top = 7,
                Width = 80,
                Height = 30,
                BackColor = Color.FromArgb(30, 136, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += (s, e) => { LoadData(); };

            var btnShowAll = new Button
            {
                Text = "Show All",
                Left = 580,
                Top = 7,
                Width = 90,
                Height = 30,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnShowAll.FlatAppearance.BorderSize = 0;
            btnShowAll.Click += (s, e) =>
            {
                dtpFrom.Value = new DateTime(2000, 1, 1);
                dtpTo.Value = DateTime.Now;
                _currentMethodFilter = string.Empty;
                LoadData();
            };

            datePanel.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, btnFilter, btnShowAll });

            dgv = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 480,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgv.Columns.Add("BillNo", "Bill No");
            dgv.Columns.Add("Customer", "Customer");
            dgv.Columns.Add("Date", "Date/Time");
            dgv.Columns.Add("PaymentMethod", "Payment Method");
            dgv.Columns.Add("PaymentStatus", "Status");
            dgv.Columns.Add("Total", "Total Amount");

            // Bottom panel with totals and filter buttons
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };
            lblTotals = new Label
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Text = ""
            };

            btnCash = new Button
            {
                Text = "Cash",
                Width = 140,
                Height = 34,
                Left = 12,
                Top = 40
            };
            btnCash.Click += (s, e) => { _currentMethodFilter = "Cash"; LoadData(); };

            btnCheque = new Button
            {
                Text = "Cheque",
                Width = 140,
                Height = 34,
                Left = 162,
                Top = 40
            };
            btnCheque.Click += (s, e) => { _currentMethodFilter = "Cheque"; LoadData(); };

            btnCredit = new Button
            {
                Text = "Credit",
                Width = 140,
                Height = 34,
                Left = 312,
                Top = 40
            };
            btnCredit.Click += (s, e) => { _currentMethodFilter = "Credit"; LoadData(); };

            btnCreditors = new Button
            {
                Text = "Creditors",
                Width = 140,
                Height = 34,
                Left = 462,
                Top = 40
            };
            btnCreditors.Click += (s, e) => { ShowCreditors(); };

            bottomPanel.Controls.Add(lblTotals);
            bottomPanel.Controls.Add(btnCash);
            bottomPanel.Controls.Add(btnCheque);
            bottomPanel.Controls.Add(btnCredit);
            bottomPanel.Controls.Add(btnCreditors);

            btnMarkSettled = new Button
            {
                Text = "Mark as Settled",
                Width = 160,
                Height = 34,
                Left = 612,
                Top = 40
            };
            btnMarkSettled.Click += (s, e) => { MarkSelectedInvoicesSettled(); };
            bottomPanel.Controls.Add(btnMarkSettled);

            this.Controls.Add(bottomPanel);
            this.Controls.Add(dgv);
            this.Controls.Add(datePanel);
        }

        private void ShowCreditors()
        {
            dgv.Rows.Clear();

            using var conn = Database.OpenConnection();
            conn.Open();

            EnsurePaymentSettledColumn(conn);

            string fromDate = dtpFrom.Value.ToString("yyyy-MM-dd") + " 00:00:00";
            string toDate = dtpTo.Value.ToString("yyyy-MM-dd") + " 23:59:59";

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT i.bill_no, c.name AS customer_name, i.date_time, i.payment_status, i.payment_settled_at, i.total_amount
                FROM invoices i
                LEFT JOIN customers c ON c.id = i.customer_id
                WHERE LOWER(i.payment_method) = 'credit'
                  AND i.date_time >= $from AND i.date_time <= $to
                ORDER BY i.date_time DESC;";
            cmd.Parameters.AddWithValue("$from", fromDate);
            cmd.Parameters.AddWithValue("$to", toDate);

            using var reader = cmd.ExecuteReader();
            dgv.Rows.Clear();

            // Adjust columns for creditors view if needed
            if (dgv.Columns.Count < 7)
            {
                // Ensure basic columns exist
                if (!dgv.Columns.Contains("BillNo")) dgv.Columns.Add("BillNo", "Bill No");
                if (!dgv.Columns.Contains("Customer")) dgv.Columns.Add("Customer", "Customer");
                if (!dgv.Columns.Contains("Date")) dgv.Columns.Add("Date", "Date/Time");
                if (!dgv.Columns.Contains("PaymentStatus")) dgv.Columns.Add("PaymentStatus", "Status");
                if (!dgv.Columns.Contains("SettledAt")) dgv.Columns.Add("SettledAt", "Settled At");
                if (!dgv.Columns.Contains("PeriodDays")) dgv.Columns.Add("PeriodDays", "Creditor Period (days)");
                if (!dgv.Columns.Contains("Total")) dgv.Columns.Add("Total", "Total Amount");
            }

            while (reader.Read())
            {
                var billNo = reader[0]?.ToString() ?? "";
                var cust = reader[1]?.ToString() ?? "";
                var dtStr = reader[2]?.ToString() ?? "";
                var status = reader[3]?.ToString() ?? "";
                var settledStr = reader[4]?.ToString() ?? "";
                var totalObj = reader[5];
                decimal total = 0m;
                if (totalObj != null && totalObj != DBNull.Value)
                {
                    decimal.TryParse(totalObj.ToString(), out total);
                }

                DateTime.TryParse(dtStr, out var creditDate);
                DateTime settledAt;
                var hasSettled = DateTime.TryParse(settledStr, out settledAt);
                var periodDays = (hasSettled ? settledAt : DateTime.Now) - creditDate;

                dgv.Rows.Add(
                    billNo,
                    cust,
                    dtStr,
                    status,
                    hasSettled ? settledAt.ToString("yyyy-MM-dd HH:mm") : "",
                    Math.Max(0, (int)periodDays.TotalDays),
                    total.ToString("0.00")
                );
            }

            // Enable/disable Mark Settled based on selection availability
            dgv.SelectionChanged += (s, e) => { btnMarkSettled.Enabled = dgv.SelectedRows.Count > 0; };
            btnMarkSettled.Enabled = dgv.SelectedRows.Count > 0;
        }

        private void MarkSelectedInvoicesSettled()
        {
            if (dgv.SelectedRows.Count == 0) return;

            using var conn = Database.OpenConnection();
            conn.Open();
            EnsurePaymentSettledColumn(conn);

            foreach (DataGridViewRow row in dgv.SelectedRows)
            {
                var billNo = row.Cells["BillNo"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(billNo)) continue;

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE invoices SET payment_status = 'paid', payment_settled_at = $settled WHERE bill_no = $billno;";
                cmd.Parameters.AddWithValue("$settled", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("$billno", billNo);
                cmd.ExecuteNonQuery();
            }

            // Refresh creditors view after update
            ShowCreditors();
        }

        private void EnsurePaymentSettledColumn(SqliteConnection conn)
        {
            try
            {
                using var check = conn.CreateCommand();
                check.CommandText = "PRAGMA table_info(invoices);";
                using var r = check.ExecuteReader();
                bool hasColumn = false;
                while (r.Read())
                {
                    var name = r[1]?.ToString();
                    if (string.Equals(name, "payment_settled_at", StringComparison.OrdinalIgnoreCase))
                    {
                        hasColumn = true;
                        break;
                    }
                }
                if (!hasColumn)
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = "ALTER TABLE invoices ADD COLUMN payment_settled_at TEXT";
                    alter.ExecuteNonQuery();
                }
            }
            catch
            {
                // Ignore issues; view will work without the column by showing empty settled date.
            }
        }

        private string GetDbPath()
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return System.IO.Path.Combine(docs, "ServiceStationBilling", "billing.db");
        }

        private void LoadData()
        {
            dgv.Rows.Clear();
            decimal totalCash = 0m, totalCheque = 0m, totalCredit = 0m;

            using var conn = Database.OpenConnection();
            conn.Open();

            // Date range boundaries
            string fromDate = dtpFrom.Value.ToString("yyyy-MM-dd") + " 00:00:00";
            string toDate = dtpTo.Value.ToString("yyyy-MM-dd") + " 23:59:59";

            using var cmd = conn.CreateCommand();
            if (string.IsNullOrEmpty(_currentMethodFilter))
            {
                cmd.CommandText = @"
                    SELECT i.bill_no, c.name AS customer_name, i.date_time, i.payment_method, i.payment_status, i.total_amount
                    FROM invoices i
                    LEFT JOIN customers c ON c.id = i.customer_id
                    WHERE i.date_time >= $from AND i.date_time <= $to
                    ORDER BY i.date_time DESC;";
            }
            else
            {
                cmd.CommandText = @"
                    SELECT i.bill_no, c.name AS customer_name, i.date_time, i.payment_method, i.payment_status, i.total_amount
                    FROM invoices i
                    LEFT JOIN customers c ON c.id = i.customer_id
                    WHERE i.date_time >= $from AND i.date_time <= $to
                      AND LOWER(i.payment_method) = LOWER($method)
                    ORDER BY i.date_time DESC;";
                cmd.Parameters.AddWithValue("$method", _currentMethodFilter);
            }
            cmd.Parameters.AddWithValue("$from", fromDate);
            cmd.Parameters.AddWithValue("$to", toDate);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var billNo = reader[0]?.ToString() ?? "";
                var cust = reader[1]?.ToString() ?? "";
                var dt = reader[2]?.ToString() ?? "";
                var method = reader[3]?.ToString() ?? "";
                var status = reader[4]?.ToString() ?? "";
                var totalObj = reader[5];
                decimal total = 0m;
                if (totalObj != null && totalObj != DBNull.Value)
                {
                    decimal.TryParse(totalObj.ToString(), out total);
                }

                dgv.Rows.Add(billNo, cust, dt, method, status, total.ToString("0.00"));

                if (string.Equals(method, "Cash", StringComparison.OrdinalIgnoreCase)) totalCash += total;
                else if (string.Equals(method, "Cheque", StringComparison.OrdinalIgnoreCase)) totalCheque += total;
                else if (string.Equals(method, "Credit", StringComparison.OrdinalIgnoreCase)) totalCredit += total;
            }

            lblTotals.Text = $"Cash: {totalCash:0.00}    Cheque: {totalCheque:0.00}    Credit: {totalCredit:0.00}";
            // Update button text to reflect totals
            btnCash.Text = $"Cash ({totalCash:0.00})";
            btnCheque.Text = $"Cheque ({totalCheque:0.00})";
            btnCredit.Text = $"Credit ({totalCredit:0.00})";
        }
    }
}
