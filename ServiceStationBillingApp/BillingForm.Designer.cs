using System.Drawing;
using System.Windows.Forms;

namespace ServiceStationBillingApp
{
    partial class BillingForm
    {
        private System.ComponentModel.IContainer components = null;

        // Controls
        private GroupBox groupCustomer;
        private TextBox txtCustomerName;
        private TextBox txtVehicleNo;
        private TextBox txtContact;
        private ComboBox cmbVehicleType;
        private Label lblCustomerName;
        private Label lblVehicleNo;
        private Label lblContact;
        private Label lblVehicleType;

        private Panel panelGridCard;
        private Label lblItemsTitle;
        private DataGridView dgvItems;

        private Label lblSubtotal;
        private Label lblDiscount;
        private Label lblTotal;

        private TextBox txtSubtotal;
        private TextBox txtDiscount;
        private TextBox txtTotal;
        private Label lblPayment;
        private ComboBox cmbPayment;

        private Button btnSave;
        private Button btnPrint;
        private Button btnClear;
        // Removed standalone All Invoices button; using Actions menu instead

        // Menu for History, Inventory, Services, Logout
        private MenuStrip menuStripActions;
        private ToolStripMenuItem menuActions;
        private ToolStripMenuItem menuAllInvoices;
        private ToolStripMenuItem menuHistory;
        private ToolStripMenuItem menuInventory;
        private ToolStripMenuItem menuServices;
        private ToolStripMenuItem menuCustomers;
        private ToolStripMenuItem menuSuppliers;
        private ToolStripMenuItem menuSupplyInvoices;
        private ToolStripMenuItem menuProfitStatement;
        private ToolStripMenuItem menuSettings;
        private ToolStripMenuItem menuLogout;

        // Advanced UI panels
        private Panel panelHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;

        private Panel panelSummary;
        private Label lblSummaryTitle;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyleHeader = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyleDefault = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyleAlt = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupCustomer = new System.Windows.Forms.GroupBox();
            this.txtCustomerName = new System.Windows.Forms.TextBox();
            this.txtVehicleNo = new System.Windows.Forms.TextBox();
            this.txtContact = new System.Windows.Forms.TextBox();
            this.cmbVehicleType = new System.Windows.Forms.ComboBox();
            this.lblCustomerName = new System.Windows.Forms.Label();
            this.lblVehicleNo = new System.Windows.Forms.Label();
            this.lblContact = new System.Windows.Forms.Label();
            this.lblVehicleType = new System.Windows.Forms.Label();
            this.panelGridCard = new System.Windows.Forms.Panel();
            this.dgvItems = new System.Windows.Forms.DataGridView();
            this.lblItemsTitle = new System.Windows.Forms.Label();
            this.lblSubtotal = new System.Windows.Forms.Label();
            this.lblDiscount = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.txtSubtotal = new System.Windows.Forms.TextBox();
            this.txtDiscount = new System.Windows.Forms.TextBox();
            this.txtTotal = new System.Windows.Forms.TextBox();
            this.lblPayment = new System.Windows.Forms.Label();
            this.cmbPayment = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            // removed btnAllInvoices initialization
            this.menuStripActions = new System.Windows.Forms.MenuStrip();
            this.menuActions = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAllInvoices = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProfitStatement = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.menuInventory = new System.Windows.Forms.ToolStripMenuItem();
            this.menuServices = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCustomers = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSuppliers = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSupplyInvoices = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.lblHeaderSubtitle = new System.Windows.Forms.Label();
            this.panelSummary = new System.Windows.Forms.Panel();
            this.lblSummaryTitle = new System.Windows.Forms.Label();
            this.groupCustomer.SuspendLayout();
            this.panelGridCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).BeginInit();
            this.menuStripActions.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.panelSummary.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStripActions
            // 
            this.menuStripActions.BackColor = Color.FromArgb(30, 136, 229); // Primary Blue
            this.menuStripActions.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.menuStripActions.GripStyle = ToolStripGripStyle.Visible;
            this.menuStripActions.ImageScalingSize = new Size(20, 20);
            this.menuStripActions.Items.AddRange(new ToolStripItem[] {
                this.menuActions
            });
            this.menuStripActions.Location = new System.Drawing.Point(0, 0);
            this.menuStripActions.Name = "menuStripActions";
            this.menuStripActions.Padding = new Padding(6, 3, 0, 3);
            this.menuStripActions.Size = new System.Drawing.Size(900, 25);
            this.menuStripActions.TabIndex = 0;
            this.menuStripActions.Text = "menuStripActions";
            // 
            // menuActions
            // 
            this.menuActions.ForeColor = Color.White;
            this.menuActions.Name = "menuActions";
            this.menuActions.Size = new System.Drawing.Size(63, 19);
            this.menuActions.Text = "Actions";
            this.menuActions.DropDownItems.AddRange(new ToolStripItem[] {
                this.menuAllInvoices,
                this.menuHistory,
                this.menuInventory,
                this.menuServices,
                this.menuCustomers,
                this.menuSuppliers,
                this.menuSupplyInvoices,
                this.menuProfitStatement,
                this.menuSettings,
                this.menuLogout
            });
                        // menuAllInvoices
                        // 
                        this.menuAllInvoices.Name = "menuAllInvoices";
                        this.menuAllInvoices.Size = new System.Drawing.Size(180, 22);
                        this.menuAllInvoices.Text = "All Invoices";
                        this.menuAllInvoices.Click += new System.EventHandler(this.MenuAllInvoices_Click);
            // 
            // menuHistory
            // 
            this.menuHistory.Name = "menuHistory";
            this.menuHistory.Size = new System.Drawing.Size(125, 22);
            this.menuHistory.Text = "History";
            this.menuHistory.Click += new System.EventHandler(this.BtnHistory_Click);
            // 
            // menuInventory
            // 
            this.menuInventory.Name = "menuInventory";
            this.menuInventory.Size = new System.Drawing.Size(125, 22);
            this.menuInventory.Text = "Inventory";
            this.menuInventory.Click += new System.EventHandler(this.BtnInventory_Click);
            // 
            // menuServices
            // 
            this.menuServices.Name = "menuServices";
            this.menuServices.Size = new System.Drawing.Size(125, 22);
            this.menuServices.Text = "Services";
            this.menuServices.Click += new System.EventHandler(this.MenuServices_Click);
            // 
            // menuCustomers
            // 
            this.menuCustomers.Name = "menuCustomers";
            this.menuCustomers.Size = new System.Drawing.Size(125, 22);
            this.menuCustomers.Text = "Customers";
            this.menuCustomers.Click += new System.EventHandler(this.MenuCustomers_Click);
            // 
            // menuSuppliers
            // 
            this.menuSuppliers.Name = "menuSuppliers";
            this.menuSuppliers.Size = new System.Drawing.Size(125, 22);
            this.menuSuppliers.Text = "Suppliers";
            this.menuSuppliers.Click += new System.EventHandler(this.MenuSuppliers_Click);
            //
            // menuSupplyInvoices
            //
            this.menuSupplyInvoices.Name = "menuSupplyInvoices";
            this.menuSupplyInvoices.Size = new System.Drawing.Size(160, 22);
            this.menuSupplyInvoices.Text = "Supply Invoices";
            this.menuSupplyInvoices.Click += new System.EventHandler(this.MenuSupplyInvoices_Click);
            // 
            // menuProfitStatement
            // 
            this.menuProfitStatement.Name = "menuProfitStatement";
            this.menuProfitStatement.Size = new System.Drawing.Size(125, 22);
            this.menuProfitStatement.Text = "Profit Statement";
            this.menuProfitStatement.Click += new System.EventHandler(this.MenuProfitStatement_Click);
            //
            // menuSettings
            //
            this.menuSettings.Name = "menuSettings";
            this.menuSettings.Size = new System.Drawing.Size(125, 22);
            this.menuSettings.Text = "Settings";
            this.menuSettings.Click += new System.EventHandler(this.MenuSettings_Click);
            //
            // menuLogout
            // 
            this.menuLogout.Name = "menuLogout";
            this.menuLogout.Size = new System.Drawing.Size(125, 22);
            this.menuLogout.Text = "Logout";
            this.menuLogout.Click += new System.EventHandler(this.BtnLogout_Click);
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = Color.FromArgb(13, 71, 161); // Dark Navy
            this.panelHeader.Controls.Add(this.lblHeaderTitle);
            this.panelHeader.Controls.Add(this.lblHeaderSubtitle);
            this.panelHeader.Dock = DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 25);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(900, 60);
            this.panelHeader.TabIndex = 1;
            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.AutoSize = true;
            this.lblHeaderTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblHeaderTitle.ForeColor = Color.White;
            this.lblHeaderTitle.Location = new System.Drawing.Point(18, 10);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(195, 25);
            this.lblHeaderTitle.TabIndex = 0;
            this.lblHeaderTitle.Text = "Isuru Service Center";
            // 
            // lblHeaderSubtitle
            // 
            this.lblHeaderSubtitle.AutoSize = true;
            this.lblHeaderSubtitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblHeaderSubtitle.ForeColor = Color.FromArgb(198, 255, 0); // Accent Lime
            this.lblHeaderSubtitle.Location = new System.Drawing.Point(20, 35);
            this.lblHeaderSubtitle.Name = "lblHeaderSubtitle";
            this.lblHeaderSubtitle.Size = new System.Drawing.Size(273, 17);
            this.lblHeaderSubtitle.TabIndex = 1;
            this.lblHeaderSubtitle.Text = "Quick billing · Customer history · Inventory view";
            // 
            // groupCustomer
            // 
            this.groupCustomer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.groupCustomer.BackColor = Color.White;
            this.groupCustomer.Controls.Add(this.txtCustomerName);
            this.groupCustomer.Controls.Add(this.txtVehicleNo);
            this.groupCustomer.Controls.Add(this.txtContact);
            this.groupCustomer.Controls.Add(this.cmbVehicleType);
            this.groupCustomer.Controls.Add(this.lblCustomerName);
            this.groupCustomer.Controls.Add(this.lblVehicleNo);
            this.groupCustomer.Controls.Add(this.lblContact);
            this.groupCustomer.Controls.Add(this.lblVehicleType);
            this.groupCustomer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.groupCustomer.Location = new System.Drawing.Point(12, 95);
            this.groupCustomer.Name = "groupCustomer";
            this.groupCustomer.Padding = new System.Windows.Forms.Padding(10, 8, 10, 10);
            this.groupCustomer.Size = new System.Drawing.Size(876, 105);
            this.groupCustomer.TabIndex = 2;
            this.groupCustomer.TabStop = false;
            this.groupCustomer.Text = "Customer & Vehicle Details";
            // 
            // lblCustomerName
            // 
            this.lblCustomerName.AutoSize = true;
            this.lblCustomerName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblCustomerName.Location = new System.Drawing.Point(20, 30);
            this.lblCustomerName.Name = "lblCustomerName";
            this.lblCustomerName.Size = new System.Drawing.Size(97, 15);
            this.lblCustomerName.TabIndex = 0;
            this.lblCustomerName.Text = "Customer Name";
            // 
            // txtCustomerName
            // 
            this.txtCustomerName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.txtCustomerName.Location = new System.Drawing.Point(130, 27);
            this.txtCustomerName.Name = "txtCustomerName";
            this.txtCustomerName.Size = new System.Drawing.Size(240, 23);
            this.txtCustomerName.TabIndex = 1;
            // 
            // lblVehicleNo
            // 
            this.lblVehicleNo.AutoSize = true;
            this.lblVehicleNo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.lblVehicleNo.Location = new System.Drawing.Point(410, 30);
            this.lblVehicleNo.Name = "lblVehicleNo";
            this.lblVehicleNo.Size = new System.Drawing.Size(69, 15);
            this.lblVehicleNo.TabIndex = 2;
            this.lblVehicleNo.Text = "Vehicle No.";
            // 
            // txtVehicleNo
            // 
            this.txtVehicleNo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.txtVehicleNo.Location = new System.Drawing.Point(495, 27);
            this.txtVehicleNo.Name = "txtVehicleNo";
            this.txtVehicleNo.Size = new System.Drawing.Size(180, 23);
            this.txtVehicleNo.TabIndex = 3;
            // 
            // lblContact
            // 
            this.lblContact.AutoSize = true;
            this.lblContact.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.lblContact.Location = new System.Drawing.Point(20, 66);
            this.lblContact.Name = "lblContact";
            this.lblContact.Size = new System.Drawing.Size(98, 15);
            this.lblContact.TabIndex = 4;
            this.lblContact.Text = "Contact Number";
            // 
            // txtContact
            // 
            this.txtContact.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.txtContact.Location = new System.Drawing.Point(130, 63);
            this.txtContact.Name = "txtContact";
            this.txtContact.Size = new System.Drawing.Size(240, 23);
            this.txtContact.TabIndex = 5;
            // 
            // lblVehicleType
            // 
            this.lblVehicleType.AutoSize = true;
            this.lblVehicleType.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.lblVehicleType.Location = new System.Drawing.Point(410, 66);
            this.lblVehicleType.Name = "lblVehicleType";
            this.lblVehicleType.Size = new System.Drawing.Size(75, 15);
            this.lblVehicleType.TabIndex = 6;
            this.lblVehicleType.Text = "Vehicle Type";
            // 
            // cmbVehicleType
            // 
            this.cmbVehicleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVehicleType.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.cmbVehicleType.FormattingEnabled = true;
            this.cmbVehicleType.Items.AddRange(new object[] {
            "Car",
            "Bike",
            "Van",
            "Bus",
            "Lorry",
            "Other"});
            this.cmbVehicleType.Location = new System.Drawing.Point(495, 63);
            this.cmbVehicleType.Name = "cmbVehicleType";
            this.cmbVehicleType.Size = new System.Drawing.Size(180, 23);
            this.cmbVehicleType.TabIndex = 7;
            this.cmbVehicleType.SelectedIndex = 0;
            // 
            // panelGridCard
            // 
            this.panelGridCard.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                    | System.Windows.Forms.AnchorStyles.Left)
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.panelGridCard.BackColor = Color.White;
            this.panelGridCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelGridCard.Controls.Add(this.dgvItems);
            this.panelGridCard.Controls.Add(this.lblItemsTitle);
            this.panelGridCard.Location = new System.Drawing.Point(12, 210);
            this.panelGridCard.Name = "panelGridCard";
            this.panelGridCard.Padding = new System.Windows.Forms.Padding(10, 8, 10, 10);
            this.panelGridCard.Size = new System.Drawing.Size(876, 260);
            this.panelGridCard.TabIndex = 3;
            // 
            // lblItemsTitle
            // 
            this.lblItemsTitle.AutoSize = true;
            this.lblItemsTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
            this.lblItemsTitle.Location = new System.Drawing.Point(13, 10);
            this.lblItemsTitle.Name = "lblItemsTitle";
            this.lblItemsTitle.Size = new System.Drawing.Size(80, 15);
            this.lblItemsTitle.TabIndex = 0;
            this.lblItemsTitle.Text = "Service Items";
            // 
            // dgvItems
            // 
            this.dgvItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                    | System.Windows.Forms.AnchorStyles.Left)
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvItems.AllowUserToAddRows = true;
            this.dgvItems.AllowUserToDeleteRows = true;
            this.dgvItems.BackgroundColor = Color.White;
            this.dgvItems.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvItems.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvItems.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyleHeader.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyleHeader.BackColor = Color.FromArgb(13, 71, 161);   // Dark Navy
            dataGridViewCellStyleHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
            dataGridViewCellStyleHeader.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyleHeader.SelectionBackColor = Color.FromArgb(13, 71, 161);
            dataGridViewCellStyleHeader.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyleHeader.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvItems.ColumnHeadersDefaultCellStyle = dataGridViewCellStyleHeader;
            this.dgvItems.ColumnHeadersHeight = 32;
            this.dgvItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyleDefault.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyleDefault.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyleDefault.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyleDefault.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyleDefault.SelectionBackColor = Color.FromArgb(30, 136, 229); // Primary Blue
            dataGridViewCellStyleDefault.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyleDefault.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvItems.DefaultCellStyle = dataGridViewCellStyleDefault;
            dataGridViewCellStyleAlt.BackColor = Color.FromArgb(236, 239, 241);  // Cool Gray
            this.dgvItems.AlternatingRowsDefaultCellStyle = dataGridViewCellStyleAlt;
            this.dgvItems.EnableHeadersVisualStyles = false;
            this.dgvItems.GridColor = Color.FromArgb(144, 164, 174); // Medium Gray
            this.dgvItems.Location = new System.Drawing.Point(13, 32);
            this.dgvItems.MultiSelect = true;
            this.dgvItems.Name = "dgvItems";
            this.dgvItems.RowHeadersVisible = false;
            this.dgvItems.RowTemplate.Height = 26;
            this.dgvItems.RowTemplate.DefaultCellStyle.Padding = new Padding(2, 2, 2, 2);
            this.dgvItems.Size = new System.Drawing.Size(848, 213);
            this.dgvItems.TabIndex = 1;
            this.dgvItems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            // 
            // panelSummary
            // 
            this.panelSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.panelSummary.BackColor = Color.White;
            this.panelSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSummary.Controls.Add(this.lblSummaryTitle);
            this.panelSummary.Controls.Add(this.btnSave);
            this.panelSummary.Controls.Add(this.btnPrint);
            this.panelSummary.Controls.Add(this.btnClear);
            // removed btnAllInvoices from panelSummary
            this.panelSummary.Controls.Add(this.lblSubtotal);
            this.panelSummary.Controls.Add(this.txtSubtotal);
            this.panelSummary.Controls.Add(this.lblDiscount);
            this.panelSummary.Controls.Add(this.txtDiscount);
            this.panelSummary.Controls.Add(this.lblPayment);
            this.panelSummary.Controls.Add(this.cmbPayment);
            this.panelSummary.Controls.Add(this.lblTotal);
            this.panelSummary.Controls.Add(this.txtTotal);
            this.panelSummary.Location = new System.Drawing.Point(12, 480);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Padding = new Padding(10);
            this.panelSummary.Size = new System.Drawing.Size(876, 100);
            this.panelSummary.TabIndex = 4;
            // 
            // lblSummaryTitle
            // 
            this.lblSummaryTitle.AutoSize = true;
            this.lblSummaryTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblSummaryTitle.Location = new System.Drawing.Point(13, 10);
            this.lblSummaryTitle.Name = "lblSummaryTitle";
            this.lblSummaryTitle.Size = new System.Drawing.Size(62, 15);
            this.lblSummaryTitle.TabIndex = 0;
            this.lblSummaryTitle.Text = "Summary";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnSave.Location = new System.Drawing.Point(16, 37);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 32);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save Bill";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.BackColor = Color.FromArgb(30, 136, 229); // Primary Blue
            this.btnSave.ForeColor = Color.White;
            this.btnSave.FlatStyle = FlatStyle.Flat;
            this.btnSave.FlatAppearance.BorderSize = 0;
            // 
            // btnPrint
            // 
            this.btnPrint.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnPrint.Location = new System.Drawing.Point(136, 37);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(110, 32);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = false;
            this.btnPrint.BackColor = Color.FromArgb(198, 255, 0);  // Accent Lime
            this.btnPrint.ForeColor = Color.Black;
            this.btnPrint.FlatStyle = FlatStyle.Flat;
            this.btnPrint.FlatAppearance.BorderSize = 0;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnClear.Location = new System.Drawing.Point(256, 37);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(110, 32);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.BackColor = Color.FromArgb(144, 164, 174); // Medium Gray
            this.btnClear.ForeColor = Color.White;
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.FlatAppearance.BorderSize = 0;
            // 
            // using menuAllInvoices in Actions menu
            // 
            // lblSubtotal
            // 
            this.lblSubtotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSubtotal.AutoSize = true;
            this.lblSubtotal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.lblSubtotal.Location = new System.Drawing.Point(470, 18);
            this.lblSubtotal.Name = "lblSubtotal";
            this.lblSubtotal.Size = new System.Drawing.Size(56, 15);
            this.lblSubtotal.TabIndex = 4;
            this.lblSubtotal.Text = "Subtotal :";
            // 
            // txtSubtotal
            // 
            this.txtSubtotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSubtotal.BackColor = Color.FromArgb(236, 239, 241); // Cool Gray
            this.txtSubtotal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.txtSubtotal.Location = new System.Drawing.Point(545, 15);
            this.txtSubtotal.Name = "txtSubtotal";
            this.txtSubtotal.ReadOnly = true;
            this.txtSubtotal.Size = new System.Drawing.Size(110, 23);
            this.txtSubtotal.TabIndex = 5;
            // 
            // lblDiscount
            // 
            this.lblDiscount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDiscount.AutoSize = true;
            this.lblDiscount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.lblDiscount.Location = new System.Drawing.Point(470, 47);
            this.lblDiscount.Name = "lblDiscount";
            this.lblDiscount.Size = new System.Drawing.Size(59, 15);
            this.lblDiscount.TabIndex = 6;
            this.lblDiscount.Text = "Discount :";
            // 
            // txtDiscount
            // 
            this.txtDiscount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDiscount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.txtDiscount.Location = new System.Drawing.Point(545, 44);
            this.txtDiscount.Name = "txtDiscount";
            this.txtDiscount.Size = new System.Drawing.Size(110, 23);
            this.txtDiscount.TabIndex = 7;
            // 
            // lblPayment
            // 
            this.lblPayment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPayment.AutoSize = true;
            this.lblPayment.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.lblPayment.Location = new System.Drawing.Point(470, 76);
            this.lblPayment.Name = "lblPayment";
            this.lblPayment.Size = new System.Drawing.Size(63, 15);
            this.lblPayment.TabIndex = 8;
            this.lblPayment.Text = "Payment :";
            // 
            // cmbPayment
            // 
            this.cmbPayment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPayment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPayment.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.cmbPayment.FormattingEnabled = true;
            this.cmbPayment.Items.AddRange(new object[] { "Cash", "Cheque", "Credit" });
            this.cmbPayment.Location = new System.Drawing.Point(545, 73);
            this.cmbPayment.Name = "cmbPayment";
            this.cmbPayment.Size = new System.Drawing.Size(110, 23);
            this.cmbPayment.TabIndex = 9;
            this.cmbPayment.SelectedIndex = 0;
            // 
            // lblTotal
            // 
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
            this.lblTotal.Location = new System.Drawing.Point(675, 44);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(49, 19);
            this.lblTotal.TabIndex = 10;
            this.lblTotal.Text = "TOTAL:";
            // 
            // txtTotal
            // 
            this.txtTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTotal.BackColor = Color.FromArgb(236, 239, 241); // Cool Gray
            this.txtTotal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
            this.txtTotal.Location = new System.Drawing.Point(730, 41);
            this.txtTotal.Name = "txtTotal";
            this.txtTotal.ReadOnly = true;
            this.txtTotal.Size = new System.Drawing.Size(110, 25);
            this.txtTotal.TabIndex = 11;
            // 
            // BillingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(236, 239, 241); // Cool Gray
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.panelSummary);
            this.Controls.Add(this.panelGridCard);
            this.Controls.Add(this.groupCustomer);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.menuStripActions);
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            this.MainMenuStrip = this.menuStripActions;
            this.MinimumSize = new System.Drawing.Size(820, 500);
            this.Name = "BillingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Service Station Billing System";
            this.groupCustomer.ResumeLayout(false);
            this.groupCustomer.PerformLayout();
            this.panelGridCard.ResumeLayout(false);
            this.panelGridCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).EndInit();
            this.menuStripActions.ResumeLayout(false);
            this.menuStripActions.PerformLayout();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelSummary.ResumeLayout(false);
            this.panelSummary.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
