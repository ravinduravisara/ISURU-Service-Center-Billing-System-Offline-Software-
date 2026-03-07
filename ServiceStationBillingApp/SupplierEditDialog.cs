using System;
using System.Drawing;
using System.Windows.Forms;

namespace ServiceStationBillingApp
{
    public class SupplierEditDialog : Form
    {
        private TextBox txtName = null!;
        private TextBox txtContact = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;
        private TextBox txtInvoice = null!;

        public string NameValue => txtName.Text.Trim();
        public string? ContactValue => txtContact.Text.Trim();
        public string? InvoiceValue => txtInvoice.Text.Trim();

        public SupplierEditDialog(string name = "", string contact = "", string invoice = "")
        {
            InitializeComponent();
            txtName.Text = name ?? string.Empty;
            txtContact.Text = contact ?? string.Empty;
            txtInvoice.Text = invoice ?? string.Empty;
        }

        private void InitializeComponent()
        {
            this.Text = "Supplier";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(380, 220);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblName = new Label
            {
                Text = "Name",
                Location = new Point(12, 18),
                AutoSize = true
            };

            txtName = new TextBox
            {
                Location = new Point(90, 15),
                Width = 240
            };

            var lblContact = new Label
            {
                Text = "Contact",
                Location = new Point(12, 56),
                AutoSize = true
            };

            txtContact = new TextBox
            {
                Location = new Point(90, 53),
                Width = 240
            };

            var lblInvoice = new Label
            {
                Text = "Invoice",
                Location = new Point(12, 94),
                AutoSize = true
            };

            txtInvoice = new TextBox
            {
                Location = new Point(90, 91),
                Width = 240
            };

            btnOk = new Button
            {
                Text = "OK",
                Location = new Point(190, 140),
                DialogResult = DialogResult.OK
            };
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Name is required.");
                    this.DialogResult = DialogResult.None;
                }
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(275, 140),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblContact);
            this.Controls.Add(txtContact);
            this.Controls.Add(lblInvoice);
            this.Controls.Add(txtInvoice);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
