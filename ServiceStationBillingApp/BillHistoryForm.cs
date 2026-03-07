using System;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public partial class BillHistoryForm : Form
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public BillHistoryForm()
        {
            InitializeComponent();

            // Use centralized Database helpers
            _dbPath = Database.GetDbPath();
            _connectionString = Database.ConnectionString;

            SetupGrids();
            HookEvents();
            InitializeAutoComplete(); // load customer names into suggestions
        }

        /// <summary>
        /// Load all customer names from DB into the search textbox autocomplete.
        /// </summary>
        private void InitializeAutoComplete()
        {
            try
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT DISTINCT name
                    FROM customers
                    WHERE name IS NOT NULL AND TRIM(name) <> ''
                    ORDER BY name;
                ";

                using var reader = cmd.ExecuteReader();
                var autoSource = new AutoCompleteStringCollection();

                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        autoSource.Add(name);
                    }
                }

                // Attach collection to textbox (designer already set mode/source)
                txtSearchName.AutoCompleteCustomSource = autoSource;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customer name suggestions:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // -----------------
        //   UI CONFIG
        // -----------------
        private void SetupGrids()
        {
            // Customers grid
            dgvCustomers.Columns.Clear();
            dgvCustomers.ReadOnly = true;
            dgvCustomers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCustomers.MultiSelect = false;
            dgvCustomers.RowHeadersVisible = false;

            dgvCustomers.Columns.Add("CustomerId", "ID");
            dgvCustomers.Columns["CustomerId"]!.Visible = false;

            dgvCustomers.Columns.Add("Name", "Name");
            dgvCustomers.Columns.Add("Phone", "Phone");

            dgvCustomers.Columns["Name"]!.Width = 300;
            dgvCustomers.Columns["Phone"]!.Width = 150;

            // Vehicles grid
            dgvVehicles.Columns.Clear();
            dgvVehicles.ReadOnly = true;
            dgvVehicles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvVehicles.MultiSelect = false;
            dgvVehicles.RowHeadersVisible = false;

            dgvVehicles.Columns.Add("VehicleId", "ID");
            dgvVehicles.Columns["VehicleId"]!.Visible = false;

            dgvVehicles.Columns.Add("VehicleNo", "Vehicle No");
            dgvVehicles.Columns.Add("VehicleType", "Type");

            dgvVehicles.Columns["VehicleNo"]!.Width = 200;
            dgvVehicles.Columns["VehicleType"]!.Width = 100;

            // Invoices grid
            dgvInvoices.Columns.Clear();
            dgvInvoices.ReadOnly = true;
            dgvInvoices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInvoices.MultiSelect = false;
            dgvInvoices.RowHeadersVisible = false;

            dgvInvoices.Columns.Add("InvoiceId", "ID");
            dgvInvoices.Columns["InvoiceId"]!.Visible = false;

            dgvInvoices.Columns.Add("BillNo", "Bill No");
            dgvInvoices.Columns.Add("DateTime", "Date & Time");
            dgvInvoices.Columns.Add("VehicleNo", "Vehicle No");
            dgvInvoices.Columns.Add("VehicleType", "Type");
            dgvInvoices.Columns.Add("Total", "Total Amount");
            dgvInvoices.Columns.Add("PaymentMethod", "Payment Method");
            dgvInvoices.Columns.Add("PaymentStatus", "Payment Status");

            dgvInvoices.Columns["BillNo"]!.Width = 120;
            dgvInvoices.Columns["DateTime"]!.Width = 150;
            dgvInvoices.Columns["VehicleNo"]!.Width = 120;
            dgvInvoices.Columns["Total"]!.Width = 100;
        }

        private void HookEvents()
        {
            btnSearch.Click += BtnSearch_Click;
            dgvCustomers.SelectionChanged += DgvCustomers_SelectionChanged;
            dgvInvoices.CellDoubleClick += DgvInvoices_CellDoubleClick;

            // Optional: if you want Enter inside the textbox to trigger search
            // even without AcceptButton, you could also do:
            // txtSearchName.KeyDown += (s, e) =>
            // {
            //     if (e.KeyCode == Keys.Enter)
            //     {
            //         BtnSearch_Click(s, EventArgs.Empty);
            //         e.Handled = true;
            //         e.SuppressKeyPress = true;
            //     }
            // };
        }

        // -----------------
        //   SEARCH LOGIC
        // -----------------
        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            string nameFilter = txtSearchName.Text.Trim();

            dgvCustomers.Rows.Clear();
            dgvVehicles.Rows.Clear();
            dgvInvoices.Rows.Clear();

            if (string.IsNullOrWhiteSpace(nameFilter))
            {
                MessageBox.Show("Please enter a customer name to search.");
                return;
            }

            try
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT id, name, phone 
                    FROM customers
                    WHERE name LIKE '%' || $name || '%'
                    ORDER BY name;
                ";
                cmd.Parameters.AddWithValue("$name", nameFilter);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int rowIndex = dgvCustomers.Rows.Add();
                    var row = dgvCustomers.Rows[rowIndex];
                    row.Cells["CustomerId"].Value = reader.GetInt64(0);
                    row.Cells["Name"].Value = reader.GetString(1);
                    row.Cells["Phone"].Value = reader.IsDBNull(2) ? "" : reader.GetString(2);
                }

                if (dgvCustomers.Rows.Count == 0)
                {
                    MessageBox.Show("No customers found with that name.");
                }
                else
                {
                    // Select the first result and load related data immediately
                    dgvCustomers.ClearSelection();
                    var firstRow = dgvCustomers.Rows[0];
                    firstRow.Selected = true;
                    dgvCustomers.CurrentCell = firstRow.Cells["Name"]; // ensure current cell set

                    if (firstRow.Cells["CustomerId"].Value != null)
                    {
                        long firstCustomerId = Convert.ToInt64(firstRow.Cells["CustomerId"].Value);
                        LoadVehicles(firstCustomerId);
                        LoadInvoices(firstCustomerId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // When user selects a customer, load their vehicles + bills
        private void DgvCustomers_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0) return;

            var row = dgvCustomers.SelectedRows[0];
            if (row.Cells["CustomerId"].Value == null) return;

            long customerId = Convert.ToInt64(row.Cells["CustomerId"].Value);

            LoadVehicles(customerId);
            LoadInvoices(customerId);
        }

        private void LoadVehicles(long customerId)
        {
            dgvVehicles.Rows.Clear();

            try
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT id, vehicle_no, vehicle_type
                    FROM vehicles
                    WHERE customer_id = $cid;
                ";
                cmd.Parameters.AddWithValue("$cid", customerId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int rowIndex = dgvVehicles.Rows.Add();
                    var row = dgvVehicles.Rows[rowIndex];

                    row.Cells["VehicleId"].Value = reader.GetInt64(0);
                    row.Cells["VehicleNo"].Value = reader.GetString(1);
                    row.Cells["VehicleType"].Value = reader.IsDBNull(2) ? "" : reader.GetString(2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading vehicles:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadInvoices(long customerId)
        {
            dgvInvoices.Rows.Clear();

            try
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT i.id,
                           i.bill_no,
                           i.date_time,
                           v.vehicle_no,
                           v.vehicle_type,
                           i.total_amount,
                           i.payment_method,
                           i.payment_status
                    FROM invoices i
                    JOIN vehicles v ON i.vehicle_id = v.id
                    WHERE i.customer_id = $cid
                      AND i.date_time >= $from AND i.date_time <= $to
                    ORDER BY i.date_time DESC;
                ";
                cmd.Parameters.AddWithValue("$cid", customerId);
                cmd.Parameters.AddWithValue("$from", dtpFrom.Value.ToString("yyyy-MM-dd") + " 00:00:00");
                cmd.Parameters.AddWithValue("$to", dtpTo.Value.ToString("yyyy-MM-dd") + " 23:59:59");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int rowIndex = dgvInvoices.Rows.Add();
                    var row = dgvInvoices.Rows[rowIndex];

                    row.Cells["InvoiceId"].Value = reader.GetInt64(0);
                    row.Cells["BillNo"].Value = reader.GetString(1);
                    row.Cells["DateTime"].Value = reader.GetString(2);
                    row.Cells["VehicleNo"].Value = reader.GetString(3);
                    row.Cells["VehicleType"].Value = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    row.Cells["Total"].Value = reader.GetDouble(5).ToString("0.00");
                    row.Cells["PaymentMethod"].Value = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    row.Cells["PaymentStatus"].Value = reader.IsDBNull(7) ? "" : reader.GetString(7);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading invoices:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // -----------------
        //   VIEW INVOICE
        // -----------------
        private void DgvInvoices_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvInvoices.Rows[e.RowIndex];

            if (row.Cells["InvoiceId"].Value == null) return;
            long invoiceId = Convert.ToInt64(row.Cells["InvoiceId"].Value);

            // Open print/export dialog
            using var dlg = new InvoicePrintDialog(invoiceId);
            dlg.ShowDialog(this);
        }
    }
}
