using System;
using System.Windows.Forms;
using System.Drawing;

namespace ServiceStationBillingApp
{
    partial class SuppliersForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private TextBox txtSearch;
        private DataGridView dgvSuppliers;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusCash;
        private ToolStripStatusLabel statusCredit;
        private ToolStripStatusLabel statusCheque;
        private Button btnCashSummary;
        private Button btnCreditSummary;
        private Button btnChequeSummary;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblTitle = new Label();
            this.txtSearch = new TextBox();
            this.dgvSuppliers = new DataGridView();
            this.btnAdd = new Button();
            this.btnEdit = new Button();
            this.btnDelete = new Button();
            this.statusStrip = new StatusStrip();
            this.statusCash = new ToolStripStatusLabel();
            this.statusCredit = new ToolStripStatusLabel();
            this.statusCheque = new ToolStripStatusLabel();
            this.btnCashSummary = new Button();
            this.btnCreditSummary = new Button();
            this.btnChequeSummary = new Button();

            ((System.ComponentModel.ISupportInitialize)(this.dgvSuppliers)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(108, 21);
            this.lblTitle.Text = "All Suppliers";

            // txtSearch
            this.txtSearch.Location = new Point(12, 40);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "Search...";
            this.txtSearch.Size = new Size(260, 23);
            this.txtSearch.TextChanged += (s, e) => ApplyFilter();

            // dgvSuppliers
            this.dgvSuppliers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.dgvSuppliers.Location = new Point(12, 75);
            this.dgvSuppliers.Name = "dgvSuppliers";
            this.dgvSuppliers.Size = new Size(926, 340);
            this.dgvSuppliers.AllowUserToAddRows = false;
            this.dgvSuppliers.AllowUserToDeleteRows = false;
            this.dgvSuppliers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSuppliers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSuppliers.MultiSelect = true;
            this.dgvSuppliers.RowHeadersVisible = false;
            this.dgvSuppliers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            // btnAdd
            this.btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnAdd.Location = new Point(686, 40);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new Size(75, 27);
            this.btnAdd.Text = "Add";
            this.btnAdd.Click += (s, e) => AddSupplier();

            // btnEdit
            this.btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnEdit.Location = new Point(767, 40);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new Size(75, 27);
            this.btnEdit.Text = "Edit";
            this.btnEdit.Click += (s, e) => EditSupplier();

            // btnDelete
            this.btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnDelete.Location = new Point(848, 40);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new Size(75, 27);
            this.btnDelete.Text = "Delete";
            this.btnDelete.Click += (s, e) => DeleteSupplier();

            // statusStrip
            this.statusStrip.SizingGrip = false;
            this.statusStrip.Items.AddRange(new ToolStripItem[] { this.statusCash, this.statusCredit, this.statusCheque });
            this.statusStrip.Location = new Point(0, 485);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new Size(950, 24);
            this.statusStrip.BackColor = Color.FromArgb(245, 245, 245);

            // statusCash
            this.statusCash.Text = "Cash: 0.00";
            this.statusCash.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            this.statusCash.Margin = new Padding(8, 3, 16, 3);

            // statusCredit
            this.statusCredit.Text = "Credit: 0.00";
            this.statusCredit.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            this.statusCredit.Margin = new Padding(8, 3, 8, 3);

            // statusCheque
            this.statusCheque.Text = "Cheque: 0.00";
            this.statusCheque.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            this.statusCheque.Margin = new Padding(8, 3, 8, 3);

            // btnCashSummary (bottom-right)
            this.btnCashSummary.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCashSummary.Location = new Point(565, 450);
            this.btnCashSummary.Name = "btnCashSummary";
            this.btnCashSummary.Size = new Size(120, 30);
            this.btnCashSummary.Text = "Cash (0.00)";
            this.btnCashSummary.FlatStyle = FlatStyle.System;
            this.btnCashSummary.UseVisualStyleBackColor = true;
            this.btnCashSummary.Click += (s, e) => OpenPaymentDetails("Cash");

            // btnCreditSummary (bottom-right)
            this.btnCreditSummary.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCreditSummary.Location = new Point(695, 450);
            this.btnCreditSummary.Name = "btnCreditSummary";
            this.btnCreditSummary.Size = new Size(120, 30);
            this.btnCreditSummary.Text = "Credit (0.00)";
            this.btnCreditSummary.FlatStyle = FlatStyle.System;
            this.btnCreditSummary.UseVisualStyleBackColor = true;
            this.btnCreditSummary.Click += (s, e) => OpenPaymentDetails("Credit");

            // btnChequeSummary (bottom-right)
            this.btnChequeSummary.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnChequeSummary.Location = new Point(825, 450);
            this.btnChequeSummary.Name = "btnChequeSummary";
            this.btnChequeSummary.Size = new Size(120, 30);
            this.btnChequeSummary.Text = "Cheque (0.00)";
            this.btnChequeSummary.FlatStyle = FlatStyle.System;
            this.btnChequeSummary.UseVisualStyleBackColor = true;
            this.btnChequeSummary.Click += (s, e) => OpenPaymentDetails("Cheque");

            // SuppliersForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(950, 507);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.dgvSuppliers);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.btnCashSummary);
            this.Controls.Add(this.btnCreditSummary);
            this.Controls.Add(this.btnChequeSummary);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Name = "SuppliersForm";
            this.Text = "Suppliers";
            this.Load += (s, e) => LoadSuppliers();

            ((System.ComponentModel.ISupportInitialize)(this.dgvSuppliers)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
