using System.Drawing;
using System.Windows.Forms;

namespace ServiceStationBillingApp
{
    partial class BillHistoryForm
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblSearchName;
        private TextBox txtSearchName;
        private Button btnSearch;

        private Label lblFrom;
        private DateTimePicker dtpFrom;
        private Label lblTo;
        private DateTimePicker dtpTo;

        private Label lblCustomersTitle;
        private Label lblVehiclesTitle;
        private Label lblInvoicesTitle;

        private DataGridView dgvCustomers;
        private DataGridView dgvVehicles;
        private DataGridView dgvInvoices;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle customerHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle customerCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle customerAltStyle = new System.Windows.Forms.DataGridViewCellStyle();

            System.Windows.Forms.DataGridViewCellStyle vehicleHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle vehicleCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle vehicleAltStyle = new System.Windows.Forms.DataGridViewCellStyle();

            System.Windows.Forms.DataGridViewCellStyle invoiceHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle invoiceCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle invoiceAltStyle = new System.Windows.Forms.DataGridViewCellStyle();

            this.lblSearchName = new System.Windows.Forms.Label();
            this.txtSearchName = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();

            this.lblFrom = new System.Windows.Forms.Label();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.lblTo = new System.Windows.Forms.Label();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();

            this.lblCustomersTitle = new System.Windows.Forms.Label();
            this.lblVehiclesTitle = new System.Windows.Forms.Label();
            this.lblInvoicesTitle = new System.Windows.Forms.Label();

            this.dgvCustomers = new System.Windows.Forms.DataGridView();
            this.dgvVehicles = new System.Windows.Forms.DataGridView();
            this.dgvInvoices = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCustomers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVehicles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInvoices)).BeginInit();
            this.SuspendLayout();
            // 
            // BillHistoryForm (form-level styling)
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 650);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(820, 550);
            this.Text = "Bill History";

            // Overall soft background (Cool Gray: #ECEFF1)
            this.BackColor = Color.FromArgb(236, 239, 241);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            // 
            // lblSearchName
            // 
            this.lblSearchName.AutoSize = true;
            this.lblSearchName.Location = new System.Drawing.Point(18, 18);
            this.lblSearchName.Name = "lblSearchName";
            this.lblSearchName.Size = new System.Drawing.Size(101, 17);
            this.lblSearchName.TabIndex = 0;
            this.lblSearchName.Text = "Customer Name:";
            // 
            // txtSearchName
            // 
            this.txtSearchName.Location = new System.Drawing.Point(125, 15);
            this.txtSearchName.Name = "txtSearchName";
            this.txtSearchName.PlaceholderText = "Type customer name and press Search...";
            this.txtSearchName.Size = new System.Drawing.Size(280, 24);
            this.txtSearchName.TabIndex = 1;
            // AutoComplete settings
            this.txtSearchName.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.txtSearchName.AutoCompleteSource = AutoCompleteSource.CustomSource;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(420, 13);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(95, 28);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = false;
            // Primary Blue button: #1E88E5 / #1E88E5-ish
            this.btnSearch.BackColor = Color.FromArgb(30, 136, 229);
            this.btnSearch.ForeColor = Color.White;
            this.btnSearch.FlatStyle = FlatStyle.Flat;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSearch.Cursor = Cursors.Hand;
            //
            // lblFrom
            //
            this.lblFrom.AutoSize = true;
            this.lblFrom.Location = new System.Drawing.Point(530, 18);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Text = "From:";
            this.lblFrom.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            //
            // dtpFrom
            //
            this.dtpFrom.Location = new System.Drawing.Point(575, 14);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(130, 24);
            this.dtpFrom.Format = DateTimePickerFormat.Short;
            this.dtpFrom.Value = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
            //
            // lblTo
            //
            this.lblTo.AutoSize = true;
            this.lblTo.Location = new System.Drawing.Point(715, 18);
            this.lblTo.Name = "lblTo";
            this.lblTo.Text = "To:";
            this.lblTo.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            //
            // dtpTo
            //
            this.dtpTo.Location = new System.Drawing.Point(740, 14);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(130, 24);
            this.dtpTo.Format = DateTimePickerFormat.Short;
            this.dtpTo.Value = System.DateTime.Now;
            // 
            // lblCustomersTitle
            // 
            this.lblCustomersTitle.AutoSize = true;
            this.lblCustomersTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblCustomersTitle.ForeColor = Color.FromArgb(55, 71, 79); // dark grey-blue
            this.lblCustomersTitle.Location = new System.Drawing.Point(18, 55);
            this.lblCustomersTitle.Name = "lblCustomersTitle";
            this.lblCustomersTitle.Size = new System.Drawing.Size(131, 15);
            this.lblCustomersTitle.TabIndex = 3;
            this.lblCustomersTitle.Text = "Customers (Search list)";
            // 
            // dgvCustomers
            // 
            this.dgvCustomers.AllowUserToAddRows = false;
            this.dgvCustomers.AllowUserToDeleteRows = false;
            this.dgvCustomers.AllowUserToResizeRows = false;
            this.dgvCustomers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCustomers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCustomers.BackgroundColor = Color.White;
            this.dgvCustomers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvCustomers.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvCustomers.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvCustomers.ColumnHeadersHeight = 32;
            this.dgvCustomers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvCustomers.EnableHeadersVisualStyles = false;
            this.dgvCustomers.GridColor = Color.FromArgb(144, 164, 174); // Medium Gray
            this.dgvCustomers.Location = new System.Drawing.Point(18, 75);
            this.dgvCustomers.MultiSelect = false;
            this.dgvCustomers.Name = "dgvCustomers";
            this.dgvCustomers.ReadOnly = true;
            this.dgvCustomers.RowHeadersVisible = false;
            this.dgvCustomers.RowTemplate.Height = 28;
            this.dgvCustomers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCustomers.Size = new System.Drawing.Size(864, 115);
            this.dgvCustomers.TabIndex = 4;
            // header style – Dark Navy: #0D47A1
            customerHeaderStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            customerHeaderStyle.BackColor = Color.FromArgb(13, 71, 161);
            customerHeaderStyle.ForeColor = Color.White;
            customerHeaderStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            customerHeaderStyle.SelectionBackColor = Color.FromArgb(13, 71, 161);
            customerHeaderStyle.SelectionForeColor = Color.White;
            customerHeaderStyle.WrapMode = DataGridViewTriState.True;
            this.dgvCustomers.ColumnHeadersDefaultCellStyle = customerHeaderStyle;
            // default cell style
            customerCellStyle.BackColor = Color.White;
            customerCellStyle.ForeColor = Color.Black;
            customerCellStyle.SelectionBackColor = Color.FromArgb(30, 136, 229); // Primary Blue
            customerCellStyle.SelectionForeColor = Color.White;
            customerCellStyle.Font = new Font("Segoe UI", 9.25F, FontStyle.Regular, GraphicsUnit.Point);
            this.dgvCustomers.DefaultCellStyle = customerCellStyle;
            // alternating rows – Cool Gray
            customerAltStyle.BackColor = Color.FromArgb(236, 239, 241);
            customerAltStyle.SelectionBackColor = Color.FromArgb(30, 136, 229);
            customerAltStyle.SelectionForeColor = Color.White;
            this.dgvCustomers.AlternatingRowsDefaultCellStyle = customerAltStyle;
            // 
            // lblVehiclesTitle
            // 
            this.lblVehiclesTitle.AutoSize = true;
            this.lblVehiclesTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblVehiclesTitle.ForeColor = Color.FromArgb(55, 71, 79);
            this.lblVehiclesTitle.Location = new System.Drawing.Point(18, 200);
            this.lblVehiclesTitle.Name = "lblVehiclesTitle";
            this.lblVehiclesTitle.Size = new System.Drawing.Size(172, 15);
            this.lblVehiclesTitle.TabIndex = 5;
            this.lblVehiclesTitle.Text = "Vehicles (for selected customer)";
            // 
            // dgvVehicles
            // 
            this.dgvVehicles.AllowUserToAddRows = false;
            this.dgvVehicles.AllowUserToDeleteRows = false;
            this.dgvVehicles.AllowUserToResizeRows = false;
            this.dgvVehicles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvVehicles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvVehicles.BackgroundColor = Color.White;
            this.dgvVehicles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvVehicles.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvVehicles.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvVehicles.ColumnHeadersHeight = 30;
            this.dgvVehicles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvVehicles.EnableHeadersVisualStyles = false;
            this.dgvVehicles.GridColor = Color.FromArgb(144, 164, 174); // Medium Gray
            this.dgvVehicles.Location = new System.Drawing.Point(18, 220);
            this.dgvVehicles.MultiSelect = false;
            this.dgvVehicles.Name = "dgvVehicles";
            this.dgvVehicles.ReadOnly = true;
            this.dgvVehicles.RowHeadersVisible = false;
            this.dgvVehicles.RowTemplate.Height = 26;
            this.dgvVehicles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvVehicles.Size = new System.Drawing.Size(864, 115);
            this.dgvVehicles.TabIndex = 6;
            // header style – Primary Blue: #1E88E5
            vehicleHeaderStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            vehicleHeaderStyle.BackColor = Color.FromArgb(30, 136, 229);
            vehicleHeaderStyle.ForeColor = Color.White;
            vehicleHeaderStyle.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold, GraphicsUnit.Point);
            vehicleHeaderStyle.SelectionBackColor = Color.FromArgb(30, 136, 229);
            vehicleHeaderStyle.SelectionForeColor = Color.White;
            vehicleHeaderStyle.WrapMode = DataGridViewTriState.True;
            this.dgvVehicles.ColumnHeadersDefaultCellStyle = vehicleHeaderStyle;
            // default cell style
            vehicleCellStyle.BackColor = Color.White;
            vehicleCellStyle.ForeColor = Color.Black;
            vehicleCellStyle.SelectionBackColor = Color.FromArgb(30, 136, 229);
            vehicleCellStyle.SelectionForeColor = Color.White;
            vehicleCellStyle.Font = new Font("Segoe UI", 9.25F, FontStyle.Regular, GraphicsUnit.Point);
            this.dgvVehicles.DefaultCellStyle = vehicleCellStyle;
            // alternating rows – Cool Gray
            vehicleAltStyle.BackColor = Color.FromArgb(236, 239, 241);
            vehicleAltStyle.SelectionBackColor = Color.FromArgb(30, 136, 229);
            vehicleAltStyle.SelectionForeColor = Color.White;
            this.dgvVehicles.AlternatingRowsDefaultCellStyle = vehicleAltStyle;
            // 
            // lblInvoicesTitle
            // 
            this.lblInvoicesTitle.AutoSize = true;
            this.lblInvoicesTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblInvoicesTitle.ForeColor = Color.FromArgb(55, 71, 79);
            this.lblInvoicesTitle.Location = new System.Drawing.Point(18, 345);
            this.lblInvoicesTitle.Name = "lblInvoicesTitle";
            this.lblInvoicesTitle.Size = new System.Drawing.Size(176, 15);
            this.lblInvoicesTitle.TabIndex = 7;
            this.lblInvoicesTitle.Text = "Invoices (for selected vehicle)";
            // 
            // dgvInvoices
            // 
            this.dgvInvoices.AllowUserToAddRows = false;
            this.dgvInvoices.AllowUserToDeleteRows = false;
            this.dgvInvoices.AllowUserToResizeRows = false;
            this.dgvInvoices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvInvoices.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvInvoices.BackgroundColor = Color.White;
            this.dgvInvoices.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvInvoices.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvInvoices.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvInvoices.ColumnHeadersHeight = 32;
            this.dgvInvoices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvInvoices.EnableHeadersVisualStyles = false;
            this.dgvInvoices.GridColor = Color.FromArgb(144, 164, 174); // Medium Gray
            this.dgvInvoices.Location = new System.Drawing.Point(18, 365);
            this.dgvInvoices.MultiSelect = false;
            this.dgvInvoices.Name = "dgvInvoices";
            this.dgvInvoices.ReadOnly = true;
            this.dgvInvoices.RowHeadersVisible = false;
            this.dgvInvoices.RowTemplate.Height = 26;
            this.dgvInvoices.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvInvoices.Size = new System.Drawing.Size(864, 205);
            this.dgvInvoices.TabIndex = 8;
            // header style – Dark Navy
            invoiceHeaderStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            invoiceHeaderStyle.BackColor = Color.FromArgb(13, 71, 161);   // Dark Navy
            invoiceHeaderStyle.ForeColor = Color.White;
            invoiceHeaderStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            invoiceHeaderStyle.SelectionBackColor = Color.FromArgb(13, 71, 161);
            invoiceHeaderStyle.SelectionForeColor = Color.White;
            invoiceHeaderStyle.WrapMode = DataGridViewTriState.True;
            this.dgvInvoices.ColumnHeadersDefaultCellStyle = invoiceHeaderStyle;
            // default cell style
            invoiceCellStyle.BackColor = Color.White;
            invoiceCellStyle.ForeColor = Color.Black;
            invoiceCellStyle.SelectionBackColor = Color.FromArgb(30, 136, 229); // Primary Blue
            invoiceCellStyle.SelectionForeColor = Color.White;
            invoiceCellStyle.Font = new Font("Segoe UI", 9.25F, FontStyle.Regular, GraphicsUnit.Point);
            this.dgvInvoices.DefaultCellStyle = invoiceCellStyle;
            // alternating rows
            invoiceAltStyle.BackColor = Color.FromArgb(236, 239, 241);
            invoiceAltStyle.SelectionBackColor = Color.FromArgb(30, 136, 229);
            invoiceAltStyle.SelectionForeColor = Color.White;
            this.dgvInvoices.AlternatingRowsDefaultCellStyle = invoiceAltStyle;

            // 
            // AcceptButton (Enter triggers Search)
            // 
            this.AcceptButton = this.btnSearch;

            // 
            // Add controls to form
            // 
            this.Controls.Add(this.dgvInvoices);
            this.Controls.Add(this.lblInvoicesTitle);
            this.Controls.Add(this.dgvVehicles);
            this.Controls.Add(this.lblVehiclesTitle);
            this.Controls.Add(this.dgvCustomers);
            this.Controls.Add(this.lblCustomersTitle);
            this.Controls.Add(this.dtpTo);
            this.Controls.Add(this.lblTo);
            this.Controls.Add(this.dtpFrom);
            this.Controls.Add(this.lblFrom);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtSearchName);
            this.Controls.Add(this.lblSearchName);

            ((System.ComponentModel.ISupportInitialize)(this.dgvCustomers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVehicles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInvoices)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
