using System;
using System.Drawing;
using System.Windows.Forms;

namespace ServiceStationBillingApp
{
    partial class InventoryForm
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel mainLayout;
        private TableLayoutPanel formLayout;
        private FlowLayoutPanel buttonPanel;

        private Label lblCategory, lblSearch, lblQuantity, lblUnitPrice, lblBrand, lblSupplierId, lblPayment, lblBuyingPrice;
        private ComboBox cmbCategory, cmbPayment;
        private TextBox txtSearch, txtQuantity, txtUnitPrice, txtBrand, txtSupplierId, txtBuyingPrice;

        private Button btnAdd, btnUpdate, btnDelete, btnImportCsv, btnExportPdf;

        private DataGridView dgvInventory;
        private ContextMenuStrip ctxGrid;
        private ToolStripMenuItem mnuEdit, mnuDelete;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ===== Main Layout =====
            mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // ===== Form Layout (Inputs) =====
            formLayout = new TableLayoutPanel();
            formLayout.ColumnCount = 6;
            formLayout.Dock = DockStyle.Fill;
            formLayout.Padding = new Padding(10);
            formLayout.AutoSize = true;

            for (int i = 0; i < 6; i++)
                formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));

            // Labels & Inputs
            lblCategory = new Label() { Text = "Category", Anchor = AnchorStyles.Left };
            cmbCategory = new ComboBox() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new object[]
            {
                "Engine Oils & Fluids",
                "Cleaning & Detailing Items",
                "Spare Parts",
                "Tools & Equipment",
                "Lubricants & Grease",
                "Service Consumables",
                "Battery & Electrical Items",
                "Tires & Accessories",
                "Safety & Misc Items"
            });

            lblSearch = new Label() { Text = "Search", Anchor = AnchorStyles.Left };
            txtSearch = new TextBox() { Dock = DockStyle.Fill };

            lblQuantity = new Label() { Text = "Quantity", Anchor = AnchorStyles.Left };
            txtQuantity = new TextBox() { Dock = DockStyle.Fill };

            lblUnitPrice = new Label() { Text = "Unit Price", Anchor = AnchorStyles.Left };
            txtUnitPrice = new TextBox() { Dock = DockStyle.Fill };

            lblBrand = new Label() { Text = "Brand", Anchor = AnchorStyles.Left };
            txtBrand = new TextBox() { Dock = DockStyle.Fill };

            lblSupplierId = new Label() { Text = "Supplier ID", Anchor = AnchorStyles.Left };
            txtSupplierId = new TextBox() { Dock = DockStyle.Fill };

            lblBuyingPrice = new Label() { Text = "Buying Price", Anchor = AnchorStyles.Left };
            txtBuyingPrice = new TextBox() { Dock = DockStyle.Fill };

            lblPayment = new Label() { Text = "Payment", Anchor = AnchorStyles.Left };
            cmbPayment = new ComboBox() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPayment.Items.AddRange(new object[] { "Cash", "Credit", "Cheque" });

            // Add Inputs to Form Layout
            formLayout.Controls.Add(lblCategory, 0, 0);
            formLayout.Controls.Add(cmbCategory, 1, 0);
            formLayout.Controls.Add(lblSearch, 2, 0);
            formLayout.Controls.Add(txtSearch, 3, 0);
            formLayout.Controls.Add(lblQuantity, 4, 0);
            formLayout.Controls.Add(txtQuantity, 5, 0);

            formLayout.Controls.Add(lblUnitPrice, 0, 1);
            formLayout.Controls.Add(txtUnitPrice, 1, 1);
            formLayout.Controls.Add(lblBrand, 2, 1);
            formLayout.Controls.Add(txtBrand, 3, 1);
            formLayout.Controls.Add(lblSupplierId, 4, 1);
            formLayout.Controls.Add(txtSupplierId, 5, 1);

            formLayout.Controls.Add(lblBuyingPrice, 0, 2);
            formLayout.Controls.Add(txtBuyingPrice, 1, 2);
            formLayout.Controls.Add(lblPayment, 2, 2);
            formLayout.Controls.Add(cmbPayment, 3, 2);

            // ===== Buttons Panel =====
            buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Padding = new Padding(10);
            buttonPanel.BackColor = Color.WhiteSmoke;
            buttonPanel.AutoSize = true;

            var btnFont = new Font("Segoe UI", 10F, FontStyle.Bold);

            btnAdd = new Button()
            {
                Text = "Add",
                Width = 110,
                Height = 36,
                Font = btnFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                UseVisualStyleBackColor = false,
                Margin = new Padding(8, 6, 8, 6)
            };
            btnAdd.FlatAppearance.BorderColor = Color.DimGray;
            btnAdd.FlatAppearance.BorderSize = 1;
            btnAdd.FlatAppearance.MouseOverBackColor = Color.Gainsboro;
            btnAdd.FlatAppearance.MouseDownBackColor = Color.Silver;

            btnUpdate = new Button()
            {
                Text = "Update",
                Width = 110,
                Height = 36,
                Font = btnFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                UseVisualStyleBackColor = false,
                Margin = new Padding(8, 6, 8, 6),
                Enabled = false
            };
            btnUpdate.FlatAppearance.BorderColor = Color.DimGray;
            btnUpdate.FlatAppearance.BorderSize = 1;
            btnUpdate.FlatAppearance.MouseOverBackColor = Color.Gainsboro;
            btnUpdate.FlatAppearance.MouseDownBackColor = Color.Silver;

            btnDelete = new Button()
            {
                Text = "Delete",
                Width = 110,
                Height = 36,
                Font = btnFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                UseVisualStyleBackColor = false,
                Margin = new Padding(8, 6, 8, 6)
            };
            btnDelete.FlatAppearance.BorderColor = Color.DimGray;
            btnDelete.FlatAppearance.BorderSize = 1;
            btnDelete.FlatAppearance.MouseOverBackColor = Color.Gainsboro;
            btnDelete.FlatAppearance.MouseDownBackColor = Color.Silver;

            btnImportCsv = new Button()
            {
                Text = "Import CSV",
                Width = 140,
                Height = 36,
                Font = btnFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                UseVisualStyleBackColor = false,
                Margin = new Padding(8, 6, 8, 6)
            };
            btnImportCsv.FlatAppearance.BorderColor = Color.DimGray;
            btnImportCsv.FlatAppearance.BorderSize = 1;
            btnImportCsv.FlatAppearance.MouseOverBackColor = Color.Gainsboro;
            btnImportCsv.FlatAppearance.MouseDownBackColor = Color.Silver;

            btnExportPdf = new Button()
            {
                Text = "Export PDF",
                Width = 140,
                Height = 36,
                Font = btnFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                UseVisualStyleBackColor = false,
                Margin = new Padding(8, 6, 8, 6)
            };
            btnExportPdf.FlatAppearance.BorderColor = Color.DimGray;
            btnExportPdf.FlatAppearance.BorderSize = 1;
            btnExportPdf.FlatAppearance.MouseOverBackColor = Color.Gainsboro;
            btnExportPdf.FlatAppearance.MouseDownBackColor = Color.Silver;

            buttonPanel.Controls.AddRange(new Control[]
            {
                btnExportPdf, btnImportCsv, btnDelete, btnUpdate, btnAdd
            });

            // ===== DataGridView =====
            dgvInventory = new DataGridView();
            dgvInventory.Dock = DockStyle.Fill;
            dgvInventory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInventory.AllowUserToAddRows = false;
            dgvInventory.AllowUserToDeleteRows = false;

            ctxGrid = new ContextMenuStrip();
            mnuEdit = new ToolStripMenuItem("Edit");
            mnuDelete = new ToolStripMenuItem("Delete");
            ctxGrid.Items.AddRange(new ToolStripItem[] { mnuEdit, mnuDelete });
            dgvInventory.ContextMenuStrip = ctxGrid;

            // ===== Add to Main Layout =====
            mainLayout.Controls.Add(formLayout, 0, 0);
            mainLayout.Controls.Add(buttonPanel, 0, 1);
            mainLayout.Controls.Add(dgvInventory, 0, 2);

            // ===== Form Settings =====
            this.Controls.Add(mainLayout);
            this.Text = "Inventory Management";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 500);
        }
    }
}
