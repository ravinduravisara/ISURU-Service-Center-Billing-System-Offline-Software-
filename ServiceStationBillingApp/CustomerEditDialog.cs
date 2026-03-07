using System;
using System.Drawing;
using System.Windows.Forms;

namespace ServiceStationBillingApp
{
    public class CustomerEditDialog : Form
    {
        private TextBox txtName = null!;
        private TextBox txtPhone = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;

        public string NameValue => txtName.Text.Trim();
        public string? PhoneValue => txtPhone.Text.Trim();

        public CustomerEditDialog(string name = "", string phone = "")
        {
            InitializeComponent();
            txtName.Text = name ?? string.Empty;
            txtPhone.Text = phone ?? string.Empty;
        }

        private void InitializeComponent()
        {
            this.Text = "Customer";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(360, 180);
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

            var lblPhone = new Label
            {
                Text = "Phone",
                Location = new Point(12, 56),
                AutoSize = true
            };

            txtPhone = new TextBox
            {
                Location = new Point(90, 53),
                Width = 240
            };

            btnOk = new Button
            {
                Text = "OK",
                Location = new Point(170, 95),
                DialogResult = DialogResult.OK
            };
            btnOk.Click += (s, e) => { if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Name is required."); this.DialogResult = DialogResult.None; } };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(255, 95),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblPhone);
            this.Controls.Add(txtPhone);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
